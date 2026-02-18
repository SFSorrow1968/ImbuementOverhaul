using System;
using System.Collections.Generic;
using System.Reflection;
using ImbuementOverhaul.Configuration;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    public sealed class EnemyImbueManager
    {
        private sealed class TrackedCreatureState
        {
            public Creature Creature;
            public int FactionId;
            public int ProfileHash;
            public EIPModOptions.EnemyTypeArchetype EnemyArchetype;
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
        private static readonly string[] ArcherIdentityKeywords = { "archer", "bow", "crossbow", "ranger", "hunter", "marksman", "sniper", "ranged" };
        private static readonly string[] MeleeIdentityKeywords = { "melee", "warrior", "knight", "brute", "duelist", "swordsman", "guard" };
        private const float CasterNegativeRecheckSeconds = 1.5f;
        private const float EnemyTypeStabilizeSeconds = 1.75f;
        private const float CasterSpellRefreshSeconds = 0.2f;
        private const float ApplyLogCleanupIntervalSeconds = 15f;
        private const float ApplyLogEntryExpirySeconds = 8f;
        private const int ApplyLogSoftLimit = 512;
        private const int ApplyLogHardLimit = 2048;
        private const float DuplicateApplyWindowSeconds = 0.75f;
        private const float ApplyStateEntryExpirySeconds = 12f;
        private const int ApplyStateSoftLimit = 512;
        private const int ApplyStateHardLimit = 2048;

        private readonly Dictionary<int, TrackedCreatureState> tracked = new Dictionary<int, TrackedCreatureState>();
        private readonly Dictionary<string, SpellCastCharge> spellCache = new Dictionary<string, SpellCastCharge>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EIPModOptions.WeaponFilterBucket> itemBucketCache = new Dictionary<string, EIPModOptions.WeaponFilterBucket>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> missingSpellIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> transferFailureKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> applyLogTimes = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, CachedItemApplyState> itemApplyStateById = new Dictionary<int, CachedItemApplyState>();
        private readonly HashSet<int> casterPositiveCache = new HashSet<int>();
        private readonly Dictionary<int, float> casterNegativeRetryUntil = new Dictionary<int, float>();
        private readonly Dictionary<int, CachedCasterSpell> casterSpellCache = new Dictionary<int, CachedCasterSpell>();
        private readonly Dictionary<int, CachedEnemyTypeArchetype> enemyArchetypeCache = new Dictionary<int, CachedEnemyTypeArchetype>();
        private readonly List<int> removeBuffer = new List<int>();
        private readonly List<string> removeLogKeyBuffer = new List<string>(64);
        private readonly List<int> removeItemStateKeyBuffer = new List<int>(64);

        private float nextUpdateTime;
        private float nextRescanTime;
        private float nextApplyLogCleanupTime;
        private int lastAssignmentHash;

        private struct CachedCasterSpell
        {
            public string SpellId;
            public float RefreshAt;
        }

        private struct CachedItemApplyState
        {
            public string SpellId;
            public float StrengthRatio;
            public float LastWriteTime;
        }

        private struct CachedEnemyTypeArchetype
        {
            public EIPModOptions.EnemyTypeArchetype Archetype;
            public float StabilizeUntil;
        }

        private struct CasterDetectionDetail
        {
            public bool CachedPositive;
            public bool NegativeRetryBlocked;
            public bool ByIdentity;
            public bool ByHeldItems;
            public bool ByComponents;
        }

        private struct EnemyTypeEvidence
        {
            public CasterDetectionDetail Caster;
            public bool ArcherBySpawner;
            public bool ArcherByHeldItems;
            public bool ArcherByKeywords;
            public bool MeleeBySpawner;
            public bool MeleeByHeldItems;
            public bool MeleeByKeywords;

            public bool HasCasterEvidence => Caster.CachedPositive || Caster.ByIdentity || Caster.ByHeldItems || Caster.ByComponents;
            public bool HasArcherEvidence => ArcherBySpawner || ArcherByHeldItems || ArcherByKeywords;
            public bool HasMeleeEvidence => MeleeBySpawner || MeleeByHeldItems || MeleeByKeywords;
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
            itemApplyStateById.Clear();
            casterPositiveCache.Clear();
            casterNegativeRetryUntil.Clear();
            casterSpellCache.Clear();
            enemyArchetypeCache.Clear();
            nextUpdateTime = 0f;
            nextRescanTime = 0f;
            nextApplyLogCleanupTime = 0f;
            lastAssignmentHash = EIPModOptions.GetAssignmentStateHash();

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
            itemApplyStateById.Clear();
            casterPositiveCache.Clear();
            casterNegativeRetryUntil.Clear();
            casterSpellCache.Clear();
            enemyArchetypeCache.Clear();
            lastAssignmentHash = int.MinValue;
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

            int assignmentHash = EIPModOptions.GetAssignmentStateHash();
            if (assignmentHash != lastAssignmentHash)
            {
                int previousHash = lastAssignmentHash;
                lastAssignmentHash = assignmentHash;
                enemyArchetypeCache.Clear();
                TrackCurrentCreatures(forceRefresh: true, now);
                EIPTelemetry.RecordConfigRefresh(previousHash, assignmentHash);
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
            enemyArchetypeCache.Remove(key);
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
            itemApplyStateById.Clear();
            casterPositiveCache.Clear();
            casterNegativeRetryUntil.Clear();
            casterSpellCache.Clear();
            enemyArchetypeCache.Clear();
            lastAssignmentHash = int.MinValue;
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
            if (creature == null)
            {
                EIPTelemetry.RecordTrackSkip("null_creature", fromSpawnEvent);
                return;
            }

            if (!IsEligibleCreature(creature))
            {
                EIPTelemetry.RecordTrackSkip(GetIneligibleReason(creature), fromSpawnEvent);
                return;
            }

            if (!EIPModOptions.TryResolveFactionProfile(creature.factionId, out EIPModOptions.FactionProfile profile))
            {
                tracked.Remove(creature.GetInstanceID());
                EIPTelemetry.RecordTrackSkip("no_faction_profile", fromSpawnEvent);
                return;
            }

            int key = creature.GetInstanceID();
            if (!TryResolveEnemyTypeArchetype(creature, now, out EIPModOptions.EnemyTypeArchetype enemyArchetype, out bool uncertainSkipped, out bool stabilizedFromCache))
            {
                tracked.Remove(key);
                string reason = uncertainSkipped
                    ? "enemytype_uncertain_skipped"
                    : "enemytype_detection_failed";
                EIPTelemetry.RecordTrackSkip(reason, fromSpawnEvent);
                return;
            }

            if (!EIPModOptions.IsEnemyTypeEligible(enemyArchetype))
            {
                tracked.Remove(key);
                EIPTelemetry.RecordTrackSkip("enemytype_ineligible_" + EIPModOptions.GetEnemyTypeToken(enemyArchetype), fromSpawnEvent);
                return;
            }

            if (!profile.Enabled)
            {
                tracked.Remove(key);
                EIPTelemetry.RecordTrackSkip("faction_disabled", fromSpawnEvent);
                return;
            }

            bool hasExisting = tracked.TryGetValue(key, out TrackedCreatureState existing);
            bool shouldReroll = forceRefresh ||
                               !hasExisting ||
                               existing.FactionId != creature.factionId ||
                               existing.ProfileHash != profile.ProfileHash ||
                               existing.EnemyArchetype != enemyArchetype;
            if (!shouldReroll)
            {
                EIPTelemetry.RecordTrackReuse(fromSpawnEvent);
                return;
            }

            RollProfile(profile, out bool rollPassed, out int slotIndex, out float slotChancePercent, out string spellId, out float strengthPercent, out float rollPercent);

            TrackedCreatureState state = new TrackedCreatureState
            {
                Creature = creature,
                FactionId = creature.factionId,
                ProfileHash = profile.ProfileHash,
                EnemyArchetype = enemyArchetype,
                RollPassed = rollPassed,
                RollPercent = rollPercent,
                SelectedSlot = slotIndex,
                SelectedChancePercent = slotChancePercent,
                BaseSpellId = spellId ?? string.Empty,
                StrengthRatio = Mathf.Clamp01(strengthPercent / 100f),
                LastApplyTime = hasExisting ? existing.LastApplyTime : -1f
            };

            tracked[key] = state;
            EIPTelemetry.RecordTrackRollResult(fromSpawnEvent, rollPassed);

            if (EIPLog.VerboseEnabled)
            {
                string correlationId = BuildCorrelationId(creature, now);
                EIPLog.Info(
                    "Track " + BuildCreatureLabel(creature) +
                    " cid=" + correlationId +
                    " factionProfile=" + EIPModOptions.GetFactionShortName(profile.FactionIndex) +
                    " enemyType=" + EIPModOptions.GetEnemyTypeDisplayName(enemyArchetype) +
                    (stabilizedFromCache ? " stabilized=true" : string.Empty) +
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
                string correlationId = BuildCorrelationId(creature, now);
                EIPLog.Info(
                    "Spawn " + BuildCreatureLabel(creature) +
                    " cid=" + correlationId +
                    " result=" + (rollPassed ? "APPLY" : "SKIP") +
                    (rollPassed ? " slot=" + slotIndex + " spell=" + state.BaseSpellId : string.Empty) +
                    " enemyType=" + EIPModOptions.GetEnemyTypeDisplayName(enemyArchetype));
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

        private bool TryResolveEnemyTypeArchetype(
            Creature creature,
            float now,
            out EIPModOptions.EnemyTypeArchetype archetype,
            out bool uncertainSkipped,
            out bool stabilizedFromCache)
        {
            archetype = EIPModOptions.EnemyTypeArchetype.Melee;
            uncertainSkipped = false;
            stabilizedFromCache = false;

            if (creature == null)
            {
                return false;
            }

            EnemyTypeEvidence evidence = GatherEnemyTypeEvidence(creature, now);
            EIPModOptions.EnemyTypeArchetype candidate = ResolveArchetypeFromEvidence(evidence, out bool uncertain);
            int key = creature.GetInstanceID();

            if (enemyArchetypeCache.TryGetValue(key, out CachedEnemyTypeArchetype cached) &&
                cached.Archetype != candidate &&
                now < cached.StabilizeUntil &&
                !HasStrongArchetypeChangeEvidence(candidate, evidence))
            {
                candidate = cached.Archetype;
                uncertain = false;
                stabilizedFromCache = true;
            }

            archetype = candidate;
            enemyArchetypeCache[key] = new CachedEnemyTypeArchetype
            {
                Archetype = archetype,
                StabilizeUntil = now + EnemyTypeStabilizeSeconds
            };

            if (uncertain && EIPModOptions.ShouldSkipUncertainEnemyTypes())
            {
                uncertainSkipped = true;
                return false;
            }

            return true;
        }

        private EnemyTypeEvidence GatherEnemyTypeEvidence(Creature creature, float now)
        {
            EnemyTypeEvidence evidence = default;

            _ = IsRuntimeCaster(creature, now, out CasterDetectionDetail casterDetail);
            evidence.Caster = casterDetail;
            evidence.ArcherBySpawner = HasRangedSpawnerHint(creature);
            evidence.ArcherByHeldItems = LooksLikeRangedByHeldItems(creature);
            evidence.ArcherByKeywords = HasCreatureTypeKeyword(creature, ArcherIdentityKeywords);
            evidence.MeleeBySpawner = HasMeleeSpawnerHint(creature);
            evidence.MeleeByHeldItems = LooksLikeMeleeByHeldItems(creature);
            evidence.MeleeByKeywords = HasCreatureTypeKeyword(creature, MeleeIdentityKeywords);
            return evidence;
        }

        private static EIPModOptions.EnemyTypeArchetype ResolveArchetypeFromEvidence(EnemyTypeEvidence evidence, out bool uncertain)
        {
            bool isCaster = evidence.HasCasterEvidence;
            bool isArcher = evidence.HasArcherEvidence;
            bool isMelee = evidence.HasMeleeEvidence;
            uncertain = !isCaster && !isArcher && !isMelee;

            if (isCaster)
            {
                return EIPModOptions.EnemyTypeArchetype.Mage;
            }

            return isArcher
                ? EIPModOptions.EnemyTypeArchetype.Bow
                : EIPModOptions.EnemyTypeArchetype.Melee;
        }

        private static bool HasStrongArchetypeChangeEvidence(EIPModOptions.EnemyTypeArchetype candidate, EnemyTypeEvidence evidence)
        {
            switch (candidate)
            {
                case EIPModOptions.EnemyTypeArchetype.Mage:
                    return evidence.HasCasterEvidence;
                case EIPModOptions.EnemyTypeArchetype.Bow:
                    return evidence.HasArcherEvidence;
                case EIPModOptions.EnemyTypeArchetype.Melee:
                    return evidence.HasMeleeEvidence;
                default:
                    return false;
            }
        }

        private bool IsRuntimeArcher(Creature creature)
        {
            if (creature == null)
            {
                return false;
            }

            if (HasRangedSpawnerHint(creature))
            {
                return true;
            }

            if (LooksLikeRangedByHeldItems(creature))
            {
                return true;
            }

            return HasCreatureTypeKeyword(creature, ArcherIdentityKeywords);
        }

        private bool IsRuntimeMelee(Creature creature)
        {
            if (creature == null)
            {
                return false;
            }

            if (HasMeleeSpawnerHint(creature))
            {
                return true;
            }

            if (LooksLikeMeleeByHeldItems(creature))
            {
                return true;
            }

            return HasCreatureTypeKeyword(creature, MeleeIdentityKeywords);
        }

        private bool LooksLikeRangedByHeldItems(Creature creature)
        {
            Item left = creature?.handLeft?.grabbedHandle?.item;
            Item right = creature?.handRight?.grabbedHandle?.item;
            return IsBowItem(left) || IsBowItem(right);
        }

        private bool LooksLikeMeleeByHeldItems(Creature creature)
        {
            Item left = creature?.handLeft?.grabbedHandle?.item;
            Item right = creature?.handRight?.grabbedHandle?.item;
            return IsMeleeWeaponItem(left) || IsMeleeWeaponItem(right);
        }

        private bool IsBowItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            EIPModOptions.WeaponFilterBucket bucket = ResolveWeaponBucket(item);
            return bucket == EIPModOptions.WeaponFilterBucket.Bow || bucket == EIPModOptions.WeaponFilterBucket.Arrow;
        }

        private bool IsMeleeWeaponItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            EIPModOptions.WeaponFilterBucket bucket = ResolveWeaponBucket(item);
            switch (bucket)
            {
                case EIPModOptions.WeaponFilterBucket.Dagger:
                case EIPModOptions.WeaponFilterBucket.Sword:
                case EIPModOptions.WeaponFilterBucket.Axe:
                case EIPModOptions.WeaponFilterBucket.Mace:
                case EIPModOptions.WeaponFilterBucket.Spear:
                case EIPModOptions.WeaponFilterBucket.Shield:
                case EIPModOptions.WeaponFilterBucket.Throwing:
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasRangedSpawnerHint(Creature creature)
        {
            CreatureSpawner spawner = creature?.creatureSpawner;
            if (spawner != null)
            {
                switch (spawner.enemyConfigType)
                {
                    case CreatureSpawner.EnemyConfigType.PatrolRanged:
                    case CreatureSpawner.EnemyConfigType.AlertRanged:
                    case CreatureSpawner.EnemyConfigType.RareRanged:
                        return true;
                }

                if (ContainsAny((spawner.enemyConfigType.ToString() ?? string.Empty).ToLowerInvariant(), ArcherIdentityKeywords))
                {
                    return true;
                }

                string tableId = spawner.creatureTableID;
                if (!string.IsNullOrWhiteSpace(tableId) &&
                    ContainsAny(tableId.ToLowerInvariant(), ArcherIdentityKeywords))
                {
                    return true;
                }
            }

            WaveData.Group group = creature?.spawnGroup;
            if (group != null)
            {
                if (!string.IsNullOrWhiteSpace(group.referenceID) &&
                    ContainsAny(group.referenceID.ToLowerInvariant(), ArcherIdentityKeywords))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(group.creatureTableID) &&
                    ContainsAny(group.creatureTableID.ToLowerInvariant(), ArcherIdentityKeywords))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMeleeSpawnerHint(Creature creature)
        {
            CreatureSpawner spawner = creature?.creatureSpawner;
            if (spawner != null)
            {
                switch (spawner.enemyConfigType)
                {
                    case CreatureSpawner.EnemyConfigType.PatrolMelee:
                    case CreatureSpawner.EnemyConfigType.AlertMelee:
                    case CreatureSpawner.EnemyConfigType.RareMelee:
                        return true;
                }

                if (ContainsAny((spawner.enemyConfigType.ToString() ?? string.Empty).ToLowerInvariant(), MeleeIdentityKeywords))
                {
                    return true;
                }

                string tableId = spawner.creatureTableID;
                if (!string.IsNullOrWhiteSpace(tableId) &&
                    ContainsAny(tableId.ToLowerInvariant(), MeleeIdentityKeywords))
                {
                    return true;
                }
            }

            WaveData.Group group = creature?.spawnGroup;
            if (group != null)
            {
                if (!string.IsNullOrWhiteSpace(group.referenceID) &&
                    ContainsAny(group.referenceID.ToLowerInvariant(), MeleeIdentityKeywords))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(group.creatureTableID) &&
                    ContainsAny(group.creatureTableID.ToLowerInvariant(), MeleeIdentityKeywords))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCreatureTypeKeyword(Creature creature, string[] keywords)
        {
            if (creature == null || keywords == null || keywords.Length == 0)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(creature.data?.id) &&
                ContainsAny(creature.data.id.ToLowerInvariant(), keywords))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(creature.name) &&
                ContainsAny(creature.name.ToLowerInvariant(), keywords))
            {
                return true;
            }

            string brainId = creature.data?.brainId;
            if (!string.IsNullOrWhiteSpace(brainId) &&
                ContainsAny(brainId.ToLowerInvariant(), keywords))
            {
                return true;
            }

            return false;
        }

        private bool IsRuntimeCaster(Creature creature, float now)
        {
            return IsRuntimeCaster(creature, now, out _);
        }

        private bool IsRuntimeCaster(Creature creature, float now, out CasterDetectionDetail detail)
        {
            detail = default;
            if (creature == null)
            {
                return false;
            }

            int key = creature.GetInstanceID();
            if (casterPositiveCache.Contains(key))
            {
                detail.CachedPositive = true;
                return true;
            }

            if (casterNegativeRetryUntil.TryGetValue(key, out float retryUntil) && now < retryUntil)
            {
                detail.NegativeRetryBlocked = true;
                return false;
            }

            bool detected = DetectRuntimeCaster(creature, out bool byIdentity, out bool byHeldItems, out bool byComponents);
            detail.ByIdentity = byIdentity;
            detail.ByHeldItems = byHeldItems;
            detail.ByComponents = byComponents;

            if (detected)
            {
                casterPositiveCache.Add(key);
                casterNegativeRetryUntil.Remove(key);
                return true;
            }

            casterNegativeRetryUntil[key] = now + CasterNegativeRecheckSeconds;
            return false;
        }

        private static bool DetectRuntimeCaster(Creature creature, out bool byIdentity, out bool byHeldItems, out bool byComponents)
        {
            byIdentity = false;
            byHeldItems = false;
            byComponents = false;

            if (creature == null)
            {
                return false;
            }

            byIdentity = LooksLikeCasterByIdentity(creature);
            byHeldItems = LooksLikeCasterByHeldItems(creature);
            byComponents = LooksLikeCasterByComponents(creature);
            return byIdentity || byHeldItems || byComponents;
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
            return creature != null &&
                   creature.gameObject != null &&
                   creature.gameObject.activeInHierarchy &&
                   !IsPlayerControlledCreature(creature);
        }

        private static string GetIneligibleReason(Creature creature)
        {
            if (creature == null)
            {
                return "null_creature";
            }

            if (creature.gameObject == null)
            {
                return "missing_gameobject";
            }

            if (!creature.gameObject.activeInHierarchy)
            {
                return "inactive_creature";
            }

            if (IsPlayerControlledCreature(creature))
            {
                return "player_controlled";
            }

            return "ineligible";
        }

        private static bool IsPlayerControlledCreature(Creature creature)
        {
            if (creature == null)
            {
                return false;
            }

            if (creature.isPlayer)
            {
                return true;
            }

            if (Player.currentCreature != null && ReferenceEquals(creature, Player.currentCreature))
            {
                return true;
            }

            // ThunderRoad commonly maps the player faction to id 2.
            if (creature.factionId == 2)
            {
                return true;
            }

            string dataId = creature.data?.id;
            if (!string.IsNullOrWhiteSpace(dataId) &&
                dataId.IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
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
                int key = removeBuffer[i];
                tracked.Remove(key);
                casterPositiveCache.Remove(key);
                casterNegativeRetryUntil.Remove(key);
                casterSpellCache.Remove(key);
                enemyArchetypeCache.Remove(key);
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
                EIPTelemetry.RecordApplyNoHeldItems();
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

            EIPTelemetry.RecordApplyAttempt();
            EIPModOptions.WeaponFilterBucket bucket = ResolveWeaponBucket(item);
            string spellId = DetermineSpellIdForItem(state, creature, now, out string baseSpellId, out bool casterOverrideUsed);
            if (casterOverrideUsed)
            {
                EIPTelemetry.RecordCasterSpellOverride();
                if (EIPLog.VerboseEnabled)
                {
                    string itemId = item.data?.id ?? item.itemId ?? item.name;
                    EIPLog.Info(
                        "Caster spell override for " + itemId +
                        " cid=" + BuildCorrelationId(creature, now) +
                        " baseSpell=" + (string.IsNullOrWhiteSpace(baseSpellId) ? "None" : baseSpellId) +
                        " resolvedSpell=" + (string.IsNullOrWhiteSpace(spellId) ? "None" : spellId),
                        true);
                }
            }

            if (string.IsNullOrWhiteSpace(spellId))
            {
                bool cleared = ClearItemImbues(item);
                if (cleared)
                {
                    EIPTelemetry.RecordItemClear();
                }
                EIPTelemetry.RecordApplyOutcome(cleared);
                return cleared;
            }

            SpellCastCharge spellData = GetSpellData(spellId);
            if (spellData == null)
            {
                EIPTelemetry.RecordApplySkipReason("spell_lookup_failed");
                EIPTelemetry.RecordApplyOutcome(changed: false);
                return false;
            }

            bool applied = ApplySpellToItem(item, creature, spellData, state.StrengthRatio, bucket, force, now);
            if (applied)
            {
                EIPTelemetry.RecordItemWrite();
            }
            EIPTelemetry.RecordApplyOutcome(applied);
            return applied;
        }

        private string DetermineSpellIdForItem(TrackedCreatureState state, Creature creature, float now, out string baseSpellId, out bool casterOverrideUsed)
        {
            string fallbackSpellId = EIPModOptions.CanonicalSpellId(state?.BaseSpellId);
            baseSpellId = fallbackSpellId;
            casterOverrideUsed = false;
            if (state == null || creature == null)
            {
                return fallbackSpellId;
            }

            bool loreFriendly = EIPModOptions.IsLoreFriendlyProfileSelected();
            if (!loreFriendly || !EIPModOptions.IsCasterArchetype(state.EnemyArchetype))
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
                string resolvedSpellId = casterSpellData.id;
                casterOverrideUsed = !string.Equals(resolvedSpellId, fallbackSpellId, StringComparison.OrdinalIgnoreCase);
                return resolvedSpellId;
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

            if (changed)
            {
                itemApplyStateById.Remove(item.GetInstanceID());
            }

            return changed;
        }

        private bool ApplySpellToItem(Item item, Creature creature, SpellCastCharge spellData, float strengthRatio, EIPModOptions.WeaponFilterBucket bucket, bool force, float now)
        {
            if (item == null || item.imbues == null || item.imbues.Count == 0)
            {
                EIPTelemetry.RecordApplySkipReason("item_has_no_imbue_slots");
                return false;
            }

            int itemInstanceId = item.GetInstanceID();
            float clampedStrengthRatio = Mathf.Clamp01(strengthRatio);
            if (!force &&
                IsDuplicateApplyWindowActive(itemInstanceId, spellData.id, clampedStrengthRatio, now) &&
                ItemAlreadyMatchesDesiredState(item, spellData.id, clampedStrengthRatio))
            {
                EIPTelemetry.RecordApplySkipReason("duplicate_apply_window");
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

                float targetEnergy = clampedStrengthRatio * imbue.maxEnergy;
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
                            EIPTelemetry.RecordTransferFailure();
                            EIPTelemetry.RecordApplySkipReason("imbue_transfer_blocked");
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
                        " at " + (clampedStrengthRatio * 100f).ToString("F0") + "% (bucket=" + bucket + ").",
                        true);
                }
            }

            if (changed)
            {
                itemApplyStateById[itemInstanceId] = new CachedItemApplyState
                {
                    SpellId = spellData.id,
                    StrengthRatio = clampedStrengthRatio,
                    LastWriteTime = now
                };
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

        private bool IsDuplicateApplyWindowActive(int itemInstanceId, string spellId, float strengthRatio, float now)
        {
            if (!itemApplyStateById.TryGetValue(itemInstanceId, out CachedItemApplyState cached))
            {
                return false;
            }

            if (now - cached.LastWriteTime > DuplicateApplyWindowSeconds)
            {
                return false;
            }

            if (!string.Equals(cached.SpellId, spellId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return Mathf.Abs(cached.StrengthRatio - strengthRatio) <= 0.01f;
        }

        private static bool ItemAlreadyMatchesDesiredState(Item item, string spellId, float strengthRatio)
        {
            if (item == null || item.imbues == null || item.imbues.Count == 0)
            {
                return false;
            }

            bool inspectedAnyImbue = false;
            for (int i = 0; i < item.imbues.Count; i++)
            {
                Imbue imbue = item.imbues[i];
                if (imbue == null || imbue.colliderGroup == null || imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.None)
                {
                    continue;
                }

                inspectedAnyImbue = true;
                if (imbue.spellCastBase == null || !string.Equals(imbue.spellCastBase.id, spellId, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                float targetEnergy = strengthRatio * imbue.maxEnergy;
                float tolerance = Mathf.Max(0.05f, imbue.maxEnergy * 0.02f);
                if (Mathf.Abs(imbue.energy - targetEnergy) > tolerance)
                {
                    return false;
                }
            }

            return inspectedAnyImbue;
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
            if (now < nextApplyLogCleanupTime &&
                applyLogTimes.Count < ApplyLogSoftLimit &&
                itemApplyStateById.Count < ApplyStateSoftLimit)
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

            if (itemApplyStateById.Count == 0)
            {
                return;
            }

            removeItemStateKeyBuffer.Clear();
            foreach (KeyValuePair<int, CachedItemApplyState> pair in itemApplyStateById)
            {
                if (now - pair.Value.LastWriteTime > ApplyStateEntryExpirySeconds)
                {
                    removeItemStateKeyBuffer.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeItemStateKeyBuffer.Count; i++)
            {
                itemApplyStateById.Remove(removeItemStateKeyBuffer[i]);
            }

            if (itemApplyStateById.Count > ApplyStateHardLimit)
            {
                itemApplyStateById.Clear();
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
            if (EIPModOptions.ForceReapply)
            {
                EIPModOptions.ForceReapply = false;
                ForceReapply();
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

        private static string BuildCorrelationId(Creature creature, float now)
        {
            if (creature == null)
            {
                return "none";
            }

            int bucket = Mathf.FloorToInt(now * 2f);
            int hash;
            unchecked
            {
                hash = (creature.GetInstanceID() * 397) ^ bucket;
            }

            uint stable = (uint)hash;
            return stable.ToString("X8").Substring(2);
        }
    }
}

