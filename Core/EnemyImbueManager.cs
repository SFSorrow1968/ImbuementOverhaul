using System;
using System.Collections.Generic;
using System.Reflection;
using EnemyImbuePresets.Configuration;
using ThunderRoad;
using UnityEngine;

namespace EnemyImbuePresets.Core
{
    public sealed class EnemyImbueManager
    {
        private sealed class TrackedCreatureState
        {
            public Creature Creature;
            public int FactionId;
            public int ProfileHash;
            public bool IsCasterType;
            public bool RollPassed;
            public float RollPercent;
            public int SelectedSlot;
            public float SelectedChancePercent;
            public string BaseSpellId;
            public float StrengthRatio;
            public float LastApplyTime;
        }

        private static readonly string[] DaggerKeywords = { "dagger", "knife", "dirk", "stiletto" };
        private static readonly string[] SpearKeywords = { "spear", "pike", "halberd", "glaive", "lance", "trident", "polearm" };
        private static readonly string[] StaffKeywords = { "staff", "wand", "rod", "cane" };
        private static readonly string[] AxeKeywords = { "axe", "hatchet", "tomahawk" };
        private static readonly string[] MaceKeywords = { "mace", "hammer", "club", "morningstar", "flail", "maul" };
        private static readonly string[] SwordKeywords = { "sword", "blade", "katana", "rapier", "sabre", "saber", "falchion", "scimitar" };
        private static readonly string[] ArrowKeywords = { "arrow", "bolt" };
        private static readonly string[] BowKeywords = { "bow", "crossbow" };
        private static readonly string[] ShieldKeywords = { "shield", "buckler" };
        private static readonly string[] ThrowingKeywords = { "throw", "javelin", "kunai", "shuriken", "star" };
        private static readonly string[] CasterIdentityKeywords = { "mage", "wizard", "sorcer", "sorcery", "warlock", "witch", "druid", "shaman", "caster", "spell", "magic", "arcane", "pyro", "electro", "cryo" };
        private static readonly string[] CasterWeaponKeywords = { "staff", "wand", "rod", "cane", "scepter", "sceptre", "orb", "focus", "grimoire", "tome", "spell" };
        private const float CasterNegativeRecheckSeconds = 1.5f;
        private const float CasterSpellRefreshSeconds = 0.2f;
        private const float ApplyLogCleanupIntervalSeconds = 15f;
        private const float ApplyLogEntryExpirySeconds = 8f;
        private const int ApplyLogSoftLimit = 512;
        private const int ApplyLogHardLimit = 2048;

        private readonly Dictionary<int, TrackedCreatureState> tracked = new Dictionary<int, TrackedCreatureState>();
        private readonly Dictionary<string, SpellCastCharge> spellCache = new Dictionary<string, SpellCastCharge>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EIPModOptions.WeaponFilterBucket> itemBucketCache = new Dictionary<string, EIPModOptions.WeaponFilterBucket>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> missingSpellIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> transferFailureKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> applyLogTimes = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<int> casterPositiveCache = new HashSet<int>();
        private readonly Dictionary<int, float> casterNegativeRetryUntil = new Dictionary<int, float>();
        private readonly Dictionary<int, CachedCasterSpell> casterSpellCache = new Dictionary<int, CachedCasterSpell>();
        private readonly List<int> removeBuffer = new List<int>();
        private readonly List<string> removeLogKeyBuffer = new List<string>(64);

        private float nextUpdateTime;
        private float nextRescanTime;
        private float nextApplyLogCleanupTime;
        private int lastOptionsHash;

        private struct CachedCasterSpell
        {
            public string SpellId;
            public float RefreshAt;
        }

        public static EnemyImbueManager Instance { get; } = new EnemyImbueManager();

        private EnemyImbueManager()
        {
        }

        public void Initialize()
        {
            tracked.Clear();
            spellCache.Clear();
            itemBucketCache.Clear();
            missingSpellIds.Clear();
            transferFailureKeys.Clear();
            applyLogTimes.Clear();
            casterPositiveCache.Clear();
            casterNegativeRetryUntil.Clear();
            casterSpellCache.Clear();
            nextUpdateTime = 0f;
            nextRescanTime = 0f;
            nextApplyLogCleanupTime = 0f;
            lastOptionsHash = EIPModOptions.GetOptionsStateHash();

            TrackCurrentCreatures(forceRefresh: true, Time.unscaledTime);
            EIPLog.Info("Enemy imbue manager initialized.");
        }

        public void Shutdown()
        {
            tracked.Clear();
            spellCache.Clear();
            itemBucketCache.Clear();
            missingSpellIds.Clear();
            transferFailureKeys.Clear();
            applyLogTimes.Clear();
            casterPositiveCache.Clear();
            casterNegativeRetryUntil.Clear();
            casterSpellCache.Clear();
            lastOptionsHash = int.MinValue;
            EIPLog.Info("Enemy imbue manager shut down.");
        }

        public void Update()
        {
            HandleDiagnostics();

            if (!EIPModOptions.EnableMod)
            {
                return;
            }

            float now = Time.unscaledTime;
            float updateInterval = Mathf.Max(0.05f, EIPModOptions.UpdateInterval);
            if (now < nextUpdateTime)
            {
                return;
            }
            nextUpdateTime = now + updateInterval;
            CleanupApplyLogTimes(now);

            int optionsHash = EIPModOptions.GetOptionsStateHash();
            if (optionsHash != lastOptionsHash)
            {
                lastOptionsHash = optionsHash;
                TrackCurrentCreatures(forceRefresh: true, now);
                EIPLog.Info("Configuration changed, refreshed tracked assignments.", true);
            }

            if (now >= nextRescanTime)
            {
                TrackCurrentCreatures(forceRefresh: false, now);
                nextRescanTime = now + Mathf.Max(0.5f, EIPModOptions.RescanInterval);
            }

            ProcessTrackedCreatures(now);
        }

        public void OnCreatureSpawn(Creature creature)
        {
            EnsureTracked(creature, fromSpawnEvent: true, forceRefresh: false, Time.unscaledTime);
        }

        public void OnCreatureDespawn(Creature creature, EventTime eventTime)
        {
            if (creature == null)
            {
                return;
            }

            int key = creature.GetInstanceID();
            if (tracked.Remove(key) && EIPLog.VerboseEnabled)
            {
                EIPLog.Info("Creature despawned, removed tracking: " + BuildCreatureLabel(creature), true);
            }
            casterPositiveCache.Remove(key);
            casterNegativeRetryUntil.Remove(key);
            casterSpellCache.Remove(key);
        }

        public void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
        {
            if (eventTime != EventTime.OnEnd)
            {
                return;
            }

            tracked.Clear();
            spellCache.Clear();
            itemBucketCache.Clear();
            missingSpellIds.Clear();
            transferFailureKeys.Clear();
            applyLogTimes.Clear();
            casterPositiveCache.Clear();
            casterNegativeRetryUntil.Clear();
            casterSpellCache.Clear();
            lastOptionsHash = int.MinValue;
            EIPLog.Info("Level unload detected, cleared tracked states.");
        }

        private void TrackCurrentCreatures(bool forceRefresh, float now)
        {
            List<Creature> allActive = Creature.allActive;
            if (allActive == null)
            {
                return;
            }

            for (int i = 0; i < allActive.Count; i++)
            {
                Creature creature = allActive[i];
                if (!IsEligibleCreature(creature))
                {
                    continue;
                }

                EnsureTracked(creature, fromSpawnEvent: false, forceRefresh: forceRefresh, now);
            }
        }

        private void EnsureTracked(Creature creature, bool fromSpawnEvent, bool forceRefresh, float now)
        {
            if (!IsEligibleCreature(creature))
            {
                return;
            }

            if (!EIPModOptions.TryResolveFactionProfile(creature.factionId, out EIPModOptions.FactionProfile profile))
            {
                tracked.Remove(creature.GetInstanceID());
                return;
            }

            int key = creature.GetInstanceID();
            bool isCasterType = IsRuntimeCaster(creature, now);
            if (!EIPModOptions.IsEnemyTypeEligible(isCasterType))
            {
                tracked.Remove(key);
                return;
            }

            if (!profile.Enabled)
            {
                tracked.Remove(key);
                return;
            }

            bool hasExisting = tracked.TryGetValue(key, out TrackedCreatureState existing);
            bool shouldReroll = forceRefresh || !hasExisting || existing.FactionId != creature.factionId || existing.ProfileHash != profile.ProfileHash;
            if (!shouldReroll)
            {
                return;
            }

            RollProfile(profile, out bool rollPassed, out int slotIndex, out float slotChancePercent, out string spellId, out float strengthPercent, out float rollPercent);

            TrackedCreatureState state = new TrackedCreatureState
            {
                Creature = creature,
                FactionId = creature.factionId,
                ProfileHash = profile.ProfileHash,
                IsCasterType = isCasterType,
                RollPassed = rollPassed,
                RollPercent = rollPercent,
                SelectedSlot = slotIndex,
                SelectedChancePercent = slotChancePercent,
                BaseSpellId = spellId ?? string.Empty,
                StrengthRatio = Mathf.Clamp01(strengthPercent / 100f),
                LastApplyTime = hasExisting ? existing.LastApplyTime : -1f
            };

            tracked[key] = state;

            if (EIPLog.VerboseEnabled)
            {
                EIPLog.Info(
                    "Track " + BuildCreatureLabel(creature) +
                    " factionProfile=" + EIPModOptions.GetFactionShortName(profile.FactionIndex) +
                    " enemyType=" + (isCasterType ? "Caster" : "NonCaster") +
                    " slot=" + (rollPassed ? slotIndex.ToString() : "none") +
                    " roll=" + rollPercent.ToString("F1") +
                    " result=" + (rollPassed ? "APPLY" : "SKIP") +
                    (rollPassed
                        ? " chance=" + slotChancePercent.ToString("F1") + "% spell=" + state.BaseSpellId + " strength=" + (state.StrengthRatio * 100f).ToString("F0") + "%"
                        : string.Empty),
                    true);
            }
            else if (fromSpawnEvent)
            {
                EIPLog.Info(
                    "Spawn " + BuildCreatureLabel(creature) +
                    " result=" + (rollPassed ? "APPLY" : "SKIP") +
                    (rollPassed ? " slot=" + slotIndex + " spell=" + state.BaseSpellId : string.Empty) +
                    " enemyType=" + (isCasterType ? "Caster" : "NonCaster"));
            }

            if (rollPassed)
            {
                ApplyToCreature(state, force: true, now);
            }
        }
        private static void RollProfile(
            EIPModOptions.FactionProfile profile,
            out bool rollPassed,
            out int slotIndex,
            out float slotChancePercent,
            out string spellId,
            out float strengthPercent,
            out float rollPercent)
        {
            rollPassed = false;
            slotIndex = 0;
            slotChancePercent = 0f;
            spellId = string.Empty;
            strengthPercent = 0f;
            rollPercent = UnityEngine.Random.Range(0f, 100f);

            if (profile.TotalNormalizedChancePercent <= 0f)
            {
                return;
            }

            float cumulative = 0f;
            if (TryResolveSlotRoll(profile.Slot1, rollPercent, ref cumulative, out slotIndex, out slotChancePercent, out spellId, out strengthPercent))
            {
                rollPassed = true;
                return;
            }
            if (TryResolveSlotRoll(profile.Slot2, rollPercent, ref cumulative, out slotIndex, out slotChancePercent, out spellId, out strengthPercent))
            {
                rollPassed = true;
                return;
            }
            if (TryResolveSlotRoll(profile.Slot3, rollPercent, ref cumulative, out slotIndex, out slotChancePercent, out spellId, out strengthPercent))
            {
                rollPassed = true;
            }
        }

        private static bool TryResolveSlotRoll(
            EIPModOptions.ImbueSlotConfig slot,
            float rollPercent,
            ref float cumulative,
            out int slotIndex,
            out float slotChancePercent,
            out string spellId,
            out float strengthPercent)
        {
            slotIndex = 0;
            slotChancePercent = 0f;
            spellId = string.Empty;
            strengthPercent = 0f;

            if (slot.ChancePercent <= 0f || slot.StrengthPercent <= 0f || string.IsNullOrWhiteSpace(slot.SpellId))
            {
                return false;
            }

            cumulative += slot.ChancePercent;
            if (rollPercent > cumulative)
            {
                return false;
            }

            slotIndex = slot.SlotIndex;
            slotChancePercent = slot.ChancePercent;
            spellId = slot.SpellId;
            strengthPercent = slot.StrengthPercent;
            return true;
        }

        private bool IsRuntimeCaster(Creature creature, float now)
        {
            if (creature == null)
            {
                return false;
            }

            int key = creature.GetInstanceID();
            if (casterPositiveCache.Contains(key))
            {
                return true;
            }

            if (casterNegativeRetryUntil.TryGetValue(key, out float retryUntil) && now < retryUntil)
            {
                return false;
            }

            bool detected = DetectRuntimeCaster(creature);
            if (detected)
            {
                casterPositiveCache.Add(key);
                casterNegativeRetryUntil.Remove(key);
                return true;
            }

            casterNegativeRetryUntil[key] = now + CasterNegativeRecheckSeconds;
            return false;
        }

        private static bool DetectRuntimeCaster(Creature creature)
        {
            if (creature == null)
            {
                return false;
            }

            if (LooksLikeCasterByIdentity(creature))
            {
                return true;
            }

            if (LooksLikeCasterByHeldItems(creature))
            {
                return true;
            }

            if (LooksLikeCasterByComponents(creature))
            {
                return true;
            }

            return false;
        }

        private static bool LooksLikeCasterByIdentity(Creature creature)
        {
            string creatureDataId = creature.data?.id;
            if (!string.IsNullOrWhiteSpace(creatureDataId) &&
                ContainsAny(creatureDataId.ToLowerInvariant(), CasterIdentityKeywords))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(creature.name) &&
                ContainsAny(creature.name.ToLowerInvariant(), CasterIdentityKeywords))
            {
                return true;
            }

            if (ObjectHasCasterKeyword(creature.brain))
            {
                return true;
            }

            object brainData = ReadMemberValue(creature.brain, "data", "brainData", "instanceData", "instance");
            return ObjectHasCasterKeyword(brainData);
        }

        private static bool LooksLikeCasterByHeldItems(Creature creature)
        {
            Item left = creature.handLeft?.grabbedHandle?.item;
            Item right = creature.handRight?.grabbedHandle?.item;
            return IsCasterFocusItem(left) || IsCasterFocusItem(right);
        }

        private static bool IsCasterFocusItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            string descriptor = BuildItemDescriptor(item);
            return ContainsAny(descriptor, StaffKeywords) || ContainsAny(descriptor, CasterWeaponKeywords);
        }

        private static bool LooksLikeCasterByComponents(Creature creature)
        {
            Component[] components = creature.GetComponentsInChildren<Component>(true);
            if (components == null || components.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                string typeName = component.GetType().Name;
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                string token = typeName.ToLowerInvariant();
                if (token.Contains("spellcaster"))
                {
                    if (HasTruthyMember(component, "isCasting", "isFiring", "isSpraying", "isMerging", "casting"))
                    {
                        return true;
                    }

                    if (HasNonNullMember(component, "spellInstanceData", "currentSpell", "spellData", "loadedSpell", "spell"))
                    {
                        return true;
                    }
                }

                if (token == "mana" || token.Contains("mana"))
                {
                    if (HasNonNullMember(component, "currentSpell", "spellData", "mergedSpell", "activeSpell", "castSpell"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsEligibleCreature(Creature creature)
        {
            return creature != null && !creature.isPlayer && creature.gameObject.activeInHierarchy;
        }

        private void ProcessTrackedCreatures(float now)
        {
            removeBuffer.Clear();

            foreach (KeyValuePair<int, TrackedCreatureState> pair in tracked)
            {
                TrackedCreatureState state = pair.Value;
                if (!IsEligibleCreature(state.Creature) || state.Creature.isKilled)
                {
                    removeBuffer.Add(pair.Key);
                    continue;
                }

                if (!state.RollPassed)
                {
                    continue;
                }

                ApplyToCreature(state, force: false, now);
            }

            for (int i = 0; i < removeBuffer.Count; i++)
            {
                tracked.Remove(removeBuffer[i]);
            }
        }

        private void ApplyToCreature(TrackedCreatureState state, bool force, float now)
        {
            if (state == null || state.Creature == null)
            {
                return;
            }

            Item left = state.Creature.handLeft?.grabbedHandle?.item;
            Item right = state.Creature.handRight?.grabbedHandle?.item;
            if (left == null && right == null)
            {
                return;
            }

            bool changed = false;
            changed |= ApplyToItemWithFilters(left, state, state.Creature, force, now);

            if (right != null && right != left)
            {
                changed |= ApplyToItemWithFilters(right, state, state.Creature, force, now);
            }

            if (changed)
            {
                state.LastApplyTime = now;
            }
        }

        private bool ApplyToItemWithFilters(Item item, TrackedCreatureState state, Creature creature, bool force, float now)
        {
            if (item == null)
            {
                return false;
            }

            EIPModOptions.WeaponFilterBucket bucket = ResolveWeaponBucket(item);
            string spellId = DetermineSpellIdForItem(state, creature, now);

            if (string.IsNullOrWhiteSpace(spellId))
            {
                return ClearItemImbues(item);
            }

            SpellCastCharge spellData = GetSpellData(spellId);
            if (spellData == null)
            {
                return false;
            }

            return ApplySpellToItem(item, creature, spellData, state.StrengthRatio, bucket, force, now);
        }

        private string DetermineSpellIdForItem(TrackedCreatureState state, Creature creature, float now)
        {
            string fallbackSpellId = EIPModOptions.CanonicalSpellId(state?.BaseSpellId);
            if (state == null || creature == null)
            {
                return fallbackSpellId;
            }

            bool loreFriendly = EIPModOptions.IsLoreFriendlyProfileSelected();
            if (!loreFriendly || !state.IsCasterType)
            {
                return fallbackSpellId;
            }

            if (!TryGetCasterActiveSpellId(creature, now, out string casterSpellId))
            {
                return fallbackSpellId;
            }

            SpellCastCharge casterSpellData = GetSpellData(casterSpellId);
            if (casterSpellData != null)
            {
                return casterSpellData.id;
            }

            return fallbackSpellId;
        }

        private bool TryGetCasterActiveSpellId(Creature creature, float now, out string spellId)
        {
            spellId = string.Empty;
            if (creature == null)
            {
                return false;
            }

            int key = creature.GetInstanceID();
            if (casterSpellCache.TryGetValue(key, out CachedCasterSpell cached) && now < cached.RefreshAt)
            {
                spellId = cached.SpellId;
                return !string.IsNullOrWhiteSpace(spellId);
            }

            string detected = DetectCasterActiveSpellId(creature);
            string canonical = EIPModOptions.CanonicalSpellId(detected);
            casterSpellCache[key] = new CachedCasterSpell
            {
                SpellId = canonical ?? string.Empty,
                RefreshAt = now + CasterSpellRefreshSeconds
            };

            spellId = canonical;
            return !string.IsNullOrWhiteSpace(spellId);
        }

        private static string DetectCasterActiveSpellId(Creature creature)
        {
            if (creature == null)
            {
                return string.Empty;
            }

            object mana = creature.mana ?? ReadMemberValue(creature, "mana");
            if (mana == null)
            {
                return string.Empty;
            }

            object leftCaster = ReadMemberValue(mana, "casterLeft", "leftCaster", "leftHandCaster", "left");
            object rightCaster = ReadMemberValue(mana, "casterRight", "rightCaster", "rightHandCaster", "right");

            if (TryGetActivelyFiringSpell(leftCaster, out string leftActive) &&
                TryGetActivelyFiringSpell(rightCaster, out string rightActive))
            {
                return SelectPreferredSpellId(leftCaster, rightCaster, leftActive, rightActive);
            }

            if (TryGetActivelyFiringSpell(leftCaster, out leftActive))
            {
                return leftActive;
            }

            if (TryGetActivelyFiringSpell(rightCaster, out rightActive))
            {
                return rightActive;
            }

            bool leftLoaded = TryGetLoadedSpellId(leftCaster, out string leftLoadedId);
            bool rightLoaded = TryGetLoadedSpellId(rightCaster, out string rightLoadedId);
            if (leftLoaded && rightLoaded)
            {
                return SelectPreferredSpellId(leftCaster, rightCaster, leftLoadedId, rightLoadedId);
            }

            if (leftLoaded)
            {
                return leftLoadedId;
            }

            if (rightLoaded)
            {
                return rightLoadedId;
            }

            return string.Empty;
        }

        private static bool TryGetActivelyFiringSpell(object caster, out string spellId)
        {
            spellId = string.Empty;
            if (caster == null)
            {
                return false;
            }

            bool isCasting =
                HasTruthyMember(caster, "isFiring", "isSpraying", "isMerging", "isCasting", "firing", "casting") ||
                ReadFloatMember(caster, "intensity", "charge", "chargeIntensity") > 0.2f;

            if (!isCasting)
            {
                return false;
            }

            return TryGetLoadedSpellId(caster, out spellId);
        }

        private static bool TryGetLoadedSpellId(object caster, out string spellId)
        {
            spellId = string.Empty;
            if (caster == null)
            {
                return false;
            }

            object spell = ReadMemberValue(
                caster,
                "spellInstance",
                "currentSpell",
                "activeSpell",
                "loadedSpell",
                "spell",
                "spellData");

            string extracted = ExtractSpellId(spell);
            if (string.IsNullOrWhiteSpace(extracted))
            {
                return false;
            }

            spellId = extracted;
            return true;
        }

        private static string ExtractSpellId(object spell)
        {
            if (spell == null)
            {
                return string.Empty;
            }

            if (spell is string spellString)
            {
                return spellString;
            }

            object id = ReadMemberValue(spell, "id", "spellId", "name", "title");
            if (id is string idString && !string.IsNullOrWhiteSpace(idString))
            {
                return idString;
            }

            object data = ReadMemberValue(spell, "data", "spellData", "baseData");
            object nestedId = ReadMemberValue(data, "id", "spellId", "name");
            if (nestedId is string nestedIdString)
            {
                return nestedIdString;
            }

            return string.Empty;
        }

        private static string SelectPreferredSpellId(object leftCaster, object rightCaster, string leftSpellId, string rightSpellId)
        {
            if (string.IsNullOrWhiteSpace(leftSpellId))
            {
                return rightSpellId ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(rightSpellId))
            {
                return leftSpellId;
            }

            float leftIntensity = ReadFloatMember(leftCaster, "intensity", "charge", "chargeIntensity");
            float rightIntensity = ReadFloatMember(rightCaster, "intensity", "charge", "chargeIntensity");

            if (Mathf.Abs(leftIntensity - rightIntensity) > 0.01f)
            {
                return leftIntensity >= rightIntensity ? leftSpellId : rightSpellId;
            }

            return leftSpellId;
        }

        private bool ClearItemImbues(Item item)
        {
            if (item == null || item.imbues == null || item.imbues.Count == 0)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < item.imbues.Count; i++)
            {
                Imbue imbue = item.imbues[i];
                if (imbue == null)
                {
                    continue;
                }

                if (imbue.energy <= 0f && imbue.spellCastBase == null)
                {
                    continue;
                }

                if (imbue.spellCastBase != null)
                {
                    imbue.UnloadCurrentSpell(true);
                }
                if (imbue.energy > 0f)
                {
                    imbue.SetEnergyInstant(0f);
                }

                changed = true;
            }

            return changed;
        }

        private bool ApplySpellToItem(Item item, Creature creature, SpellCastCharge spellData, float strengthRatio, EIPModOptions.WeaponFilterBucket bucket, bool force, float now)
        {
            if (item == null || item.imbues == null || item.imbues.Count == 0)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < item.imbues.Count; i++)
            {
                Imbue imbue = item.imbues[i];
                if (imbue == null || imbue.colliderGroup == null || imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.None)
                {
                    continue;
                }

                float targetEnergy = Mathf.Clamp01(strengthRatio) * imbue.maxEnergy;
                bool spellMismatch = imbue.spellCastBase == null || !string.Equals(imbue.spellCastBase.id, spellData.id, StringComparison.OrdinalIgnoreCase);

                if (spellMismatch)
                {
                    imbue.UnloadCurrentSpell(true);
                    imbue.allowImbue = true;

                    float bootstrapEnergy = Mathf.Max(1f, targetEnergy * 0.2f);
                    imbue.Transfer(spellData, bootstrapEnergy, creature);

                    if (imbue.spellCastBase == null || !string.Equals(imbue.spellCastBase.id, spellData.id, StringComparison.OrdinalIgnoreCase))
                    {
                        string itemId = item.data?.id ?? item.itemId ?? item.name;
                        string key = itemId + "|" + spellData.id;
                        if (transferFailureKeys.Add(key))
                        {
                            string requiredSpell = imbue.colliderGroup?.imbueCustomSpellID;
                            EIPLog.Warn(
                                "Imbue transfer blocked for item " + itemId +
                                " spell=" + spellData.id +
                                " bucket=" + bucket +
                                (string.IsNullOrWhiteSpace(requiredSpell) ? string.Empty : " requiredSpell=" + requiredSpell));
                        }
                        continue;
                    }

                    changed = true;
                }

                if (targetEnergy <= 0f)
                {
                    if (imbue.energy > 0f)
                    {
                        imbue.SetEnergyInstant(0f);
                        changed = true;
                    }
                    continue;
                }

                if (force || spellMismatch || Mathf.Abs(imbue.energy - targetEnergy) > 0.01f)
                {
                    imbue.SetEnergyInstant(targetEnergy);
                    if (imbue.spellCastBase != null)
                    {
                        float normalized = imbue.maxEnergy > 0f ? Mathf.Clamp01(targetEnergy / imbue.maxEnergy) : 0f;
                        imbue.spellCastBase.UpdateImbue(normalized);
                        imbue.spellCastBase.SlowUpdateImbue();
                    }
                    changed = true;
                }
            }

            if (changed && EIPLog.VerboseEnabled)
            {
                string itemId = item.data?.id ?? item.itemId ?? item.name;
                string logKey = itemId + "|" + spellData.id + "|" + bucket;
                if (!applyLogTimes.TryGetValue(logKey, out float lastLogTime) || now - lastLogTime >= 1.5f)
                {
                    applyLogTimes[logKey] = now;
                    EIPLog.Info(
                        "Imbued item " + itemId +
                        " with " + spellData.id +
                        " at " + (strengthRatio * 100f).ToString("F0") + "% (bucket=" + bucket + ").",
                        true);
                }
            }

            return changed;
        }

        private SpellCastCharge GetSpellData(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return null;
            }

            string requestedId = spellId.Trim();
            if (spellCache.TryGetValue(requestedId, out SpellCastCharge cached))
            {
                return cached;
            }

            SpellCastCharge data = Catalog.GetData<SpellCastCharge>(requestedId, false);
            if (data == null)
            {
                if (missingSpellIds.Add(requestedId))
                {
                    EIPLog.Warn("Spell not found: " + requestedId + ". Use Fire, Lightning, Gravity, or another valid SpellCastCharge id.");
                }
                return null;
            }

            spellCache[requestedId] = data;
            return data;
        }
        private EIPModOptions.WeaponFilterBucket ResolveWeaponBucket(Item item)
        {
            string key = GetItemKey(item);
            if (string.IsNullOrWhiteSpace(key))
            {
                return ClassifyWeaponBucket(item);
            }

            if (itemBucketCache.TryGetValue(key, out EIPModOptions.WeaponFilterBucket cached))
            {
                return cached;
            }

            EIPModOptions.WeaponFilterBucket resolved = ClassifyWeaponBucket(item);
            itemBucketCache[key] = resolved;
            return resolved;
        }

        private static string GetItemKey(Item item)
        {
            if (item == null)
            {
                return null;
            }

            string id = item.data?.id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            if (!string.IsNullOrWhiteSpace(item.itemId))
            {
                return item.itemId;
            }

            return item.name;
        }

        private static EIPModOptions.WeaponFilterBucket ClassifyWeaponBucket(Item item)
        {
            ItemData data = item?.data;
            ItemModuleAI ai = data?.moduleAI;

            string descriptor = BuildItemDescriptor(item);
            if (HasClass(ai, ItemModuleAI.WeaponClass.Arrow) || HasClass(ai, ItemModuleAI.WeaponClass.Bolt) || ContainsAny(descriptor, ArrowKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Arrow;
            }
            if (HasClass(ai, ItemModuleAI.WeaponClass.Bow) || HasClass(ai, ItemModuleAI.WeaponClass.Crossbow) || ContainsAny(descriptor, BowKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Bow;
            }
            if (HasClass(ai, ItemModuleAI.WeaponClass.Shield) || data?.type == ItemData.Type.Shield || ContainsAny(descriptor, ShieldKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Shield;
            }
            if (HasClass(ai, ItemModuleAI.WeaponClass.Throwable) || ContainsAny(descriptor, ThrowingKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Throwing;
            }

            if (ContainsAny(descriptor, DaggerKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Dagger;
            }
            if (ContainsAny(descriptor, SpearKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Spear;
            }
            if (ContainsAny(descriptor, StaffKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Staff;
            }
            if (ContainsAny(descriptor, AxeKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Axe;
            }
            if (ContainsAny(descriptor, MaceKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Mace;
            }
            if (ContainsAny(descriptor, SwordKeywords))
            {
                return EIPModOptions.WeaponFilterBucket.Sword;
            }

            if (data?.type == ItemData.Type.Quiver)
            {
                return EIPModOptions.WeaponFilterBucket.Arrow;
            }
            if (data?.type == ItemData.Type.Weapon || HasClass(ai, ItemModuleAI.WeaponClass.Melee))
            {
                return EIPModOptions.WeaponFilterBucket.Sword;
            }

            return EIPModOptions.WeaponFilterBucket.Other;
        }

        private static bool HasClass(ItemModuleAI ai, ItemModuleAI.WeaponClass weaponClass)
        {
            return ai != null && (ai.primaryClass == weaponClass || ai.secondaryClass == weaponClass);
        }

        private static string BuildItemDescriptor(Item item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            ItemData data = item.data;
            string descriptor =
                (data?.id ?? string.Empty) + "|" +
                (data?.category ?? string.Empty) + "|" +
                (data?.displayName ?? string.Empty) + "|" +
                (item.itemId ?? string.Empty) + "|" +
                (item.name ?? string.Empty);

            return descriptor.ToLowerInvariant();
        }

        private static bool ContainsAny(string text, string[] keywords)
        {
            if (string.IsNullOrEmpty(text) || keywords == null)
            {
                return false;
            }

            for (int i = 0; i < keywords.Length; i++)
            {
                string keyword = keywords[i];
                if (!string.IsNullOrWhiteSpace(keyword) && text.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        private void CleanupApplyLogTimes(float now)
        {
            if (now < nextApplyLogCleanupTime && applyLogTimes.Count < ApplyLogSoftLimit)
            {
                return;
            }

            nextApplyLogCleanupTime = now + ApplyLogCleanupIntervalSeconds;
            if (applyLogTimes.Count == 0)
            {
                return;
            }

            removeLogKeyBuffer.Clear();
            foreach (KeyValuePair<string, float> pair in applyLogTimes)
            {
                if (now - pair.Value > ApplyLogEntryExpirySeconds)
                {
                    removeLogKeyBuffer.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeLogKeyBuffer.Count; i++)
            {
                applyLogTimes.Remove(removeLogKeyBuffer[i]);
            }

            if (applyLogTimes.Count > ApplyLogHardLimit)
            {
                applyLogTimes.Clear();
            }
        }

        private static bool ObjectHasCasterKeyword(object target)
        {
            if (target == null)
            {
                return false;
            }

            Type type = target.GetType();
            if (type != null)
            {
                string typeName = type.Name;
                if (!string.IsNullOrWhiteSpace(typeName) &&
                    ContainsAny(typeName.ToLowerInvariant(), CasterIdentityKeywords))
                {
                    return true;
                }
            }

            object id = ReadMemberValue(target, "id", "name", "displayName", "title");
            if (id is string s && !string.IsNullOrWhiteSpace(s))
            {
                return ContainsAny(s.ToLowerInvariant(), CasterIdentityKeywords);
            }

            return false;
        }

        private static object ReadMemberValue(object target, params string[] memberNames)
        {
            if (target == null || memberNames == null || memberNames.Length == 0)
            {
                return null;
            }

            Type type = target.GetType();
            for (int i = 0; i < memberNames.Length; i++)
            {
                string memberName = memberNames[i];
                if (string.IsNullOrWhiteSpace(memberName))
                {
                    continue;
                }

                try
                {
                    PropertyInfo property = type.GetProperty(
                        memberName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null && property.CanRead)
                    {
                        return property.GetValue(target);
                    }

                    FieldInfo field = type.GetField(
                        memberName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        return field.GetValue(target);
                    }
                }
                catch
                {
                    // Best-effort reflection for runtime compatibility across game versions.
                }
            }

            return null;
        }

        private static bool HasTruthyMember(object target, params string[] memberNames)
        {
            object value = ReadMemberValue(target, memberNames);
            if (value is bool b)
            {
                return b;
            }

            return false;
        }

        private static float ReadFloatMember(object target, params string[] memberNames)
        {
            object value = ReadMemberValue(target, memberNames);
            if (value == null)
            {
                return 0f;
            }

            if (value is float f)
            {
                return f;
            }

            if (value is double d)
            {
                return (float)d;
            }

            if (value is int i)
            {
                return i;
            }

            if (value is long l)
            {
                return l;
            }

            if (value is decimal m)
            {
                return (float)m;
            }

            try
            {
                return Convert.ToSingle(value);
            }
            catch
            {
                return 0f;
            }
        }

        private static bool HasNonNullMember(object target, params string[] memberNames)
        {
            return ReadMemberValue(target, memberNames) != null;
        }

        private void HandleDiagnostics()
        {
            if (EIPModOptions.DumpFactions)
            {
                EIPModOptions.DumpFactions = false;
                DumpFactions();
            }

            if (EIPModOptions.DumpState)
            {
                EIPModOptions.DumpState = false;
                DumpState();
            }

            if (EIPModOptions.DumpWaveMap)
            {
                EIPModOptions.DumpWaveMap = false;
                DumpWaveFactionMap();
            }

            if (EIPModOptions.ForceReapply)
            {
                EIPModOptions.ForceReapply = false;
                ForceReapply();
            }
        }

        private void DumpFactions()
        {
            List<GameData.Faction> factions = Catalog.gameData?.factions;
            if (factions == null || factions.Count == 0)
            {
                EIPLog.Warn("No factions available from Catalog.gameData.");
                return;
            }

            EIPLog.Info("Detected factions: " + factions.Count);
            for (int i = 0; i < factions.Count; i++)
            {
                GameData.Faction faction = factions[i];
                if (faction == null)
                {
                    continue;
                }

                EIPLog.Info("Faction " + faction.id + " -> " + EIPModOptions.GetFactionName(faction.id, faction.name));
            }

            for (int profileIndex = 1; profileIndex <= EIPModOptions.FactionCount; profileIndex++)
            {
                int mappedFactionId = EIPModOptions.GetResolvedFactionId(profileIndex);
                EIPModOptions.FactionProfile profile = EIPModOptions.GetFactionProfileByIndex(profileIndex);

                EIPLog.Info(
                    "Profile " + profileIndex.ToString("D2") +
                    " " + EIPModOptions.GetFactionShortName(profileIndex) +
                    " -> faction " + mappedFactionId +
                    " (" + EIPModOptions.GetFactionName(mappedFactionId) + ")" +
                    " enabled=" + profile.Enabled);

                EIPLog.Info("  Slot1 spell=" + profile.Slot1.SpellId + " chance=" + profile.Slot1.ChancePercent.ToString("F1") + "% strength=" + profile.Slot1.StrengthPercent.ToString("F0") + "%");
                EIPLog.Info("  Slot2 spell=" + profile.Slot2.SpellId + " chance=" + profile.Slot2.ChancePercent.ToString("F1") + "% strength=" + profile.Slot2.StrengthPercent.ToString("F0") + "%");
                EIPLog.Info("  Slot3 spell=" + profile.Slot3.SpellId + " chance=" + profile.Slot3.ChancePercent.ToString("F1") + "% strength=" + profile.Slot3.StrengthPercent.ToString("F0") + "%");
            }
        }

        private void DumpState()
        {
            EIPLog.Info("Tracked creatures: " + tracked.Count);
            foreach (KeyValuePair<int, TrackedCreatureState> pair in tracked)
            {
                TrackedCreatureState state = pair.Value;
                if (state == null || state.Creature == null)
                {
                    continue;
                }

                EIPLog.Info(
                    "State " + BuildCreatureLabel(state.Creature) +
                    " slot=" + (state.RollPassed ? state.SelectedSlot.ToString() : "none") +
                    " roll=" + state.RollPercent.ToString("F1") +
                    " result=" + (state.RollPassed ? "pass" : "fail") +
                    (state.RollPassed
                        ? " chance=" + state.SelectedChancePercent.ToString("F1") +
                          " spell=" + state.BaseSpellId +
                          " strength=" + (state.StrengthRatio * 100f).ToString("F0") + "%"
                        : string.Empty));
            }
        }
        private void DumpWaveFactionMap()
        {
            List<WaveData> waves;
            try
            {
                waves = Catalog.GetDataList<WaveData>();
            }
            catch
            {
                waves = null;
            }

            if (waves == null || waves.Count == 0)
            {
                EIPLog.Warn("No WaveData entries available to build a wave-faction map.");
                return;
            }

            var map = new Dictionary<int, HashSet<string>>();
            for (int i = 0; i < waves.Count; i++)
            {
                WaveData wave = waves[i];
                if (wave == null || wave.factions == null || wave.factions.Count == 0)
                {
                    continue;
                }

                string waveLabel = !string.IsNullOrWhiteSpace(wave.title) ? wave.title : wave.id;
                if (string.IsNullOrWhiteSpace(waveLabel))
                {
                    continue;
                }

                for (int j = 0; j < wave.factions.Count; j++)
                {
                    WaveData.WaveFaction faction = wave.factions[j];
                    if (faction == null)
                    {
                        continue;
                    }

                    if (!map.TryGetValue(faction.factionID, out HashSet<string> entries))
                    {
                        entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        map[faction.factionID] = entries;
                    }

                    entries.Add(waveLabel);
                }
            }

            if (map.Count == 0)
            {
                EIPLog.Warn("Wave-faction map is empty.");
                return;
            }

            var factionIds = new List<int>(map.Keys);
            factionIds.Sort();
            EIPLog.Info("Wave-faction map:");

            for (int i = 0; i < factionIds.Count; i++)
            {
                int factionId = factionIds[i];
                List<string> labels = new List<string>(map[factionId]);
                labels.Sort(StringComparer.OrdinalIgnoreCase);

                int max = Mathf.Min(8, labels.Count);
                string joined = string.Empty;
                for (int j = 0; j < max; j++)
                {
                    if (j > 0)
                    {
                        joined += ", ";
                    }
                    joined += labels[j];
                }
                if (labels.Count > max)
                {
                    joined += ", ...";
                }

                EIPLog.Info("Faction " + factionId + " (" + EIPModOptions.GetFactionName(factionId) + ") waves: " + joined);
            }
        }

        private void ForceReapply()
        {
            float now = Time.unscaledTime;
            int applied = 0;

            foreach (TrackedCreatureState state in tracked.Values)
            {
                if (state == null || !state.RollPassed)
                {
                    continue;
                }
                if (!IsEligibleCreature(state.Creature) || state.Creature.isKilled)
                {
                    continue;
                }

                ApplyToCreature(state, force: true, now);
                applied++;
            }

            EIPLog.Info("Force reapply finished. creatures=" + applied);
        }

        private static string BuildCreatureLabel(Creature creature)
        {
            if (creature == null)
            {
                return "null";
            }

            string name = creature.data?.id;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = creature.name;
            }

            return name + " [faction=" + creature.factionId + " " + EIPModOptions.GetFactionName(creature.factionId) + "]";
        }
    }
}
