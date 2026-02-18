$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$outputPath = Join-Path $repoRoot "Configuration\ImbuementModOptions.cs"

$factions = @(
    @{ Id = 1; Code = "F01"; Short = "Combat"; Category = "Combat Practice (Mixed Enemies)"; Display = "Mixed Enemies"; DefaultId = 3; Keywords = @("mixed", "combat") },
    @{ Id = 2; Code = "F02"; Short = "Outlaws"; Category = "Outlaws (T0)"; Display = "Outlaws"; DefaultId = 4; Keywords = @("outlaw", "bandit", "t0") },
    @{ Id = 3; Code = "F03"; Short = "Wildfolk"; Category = "Wildfolk (T1)"; Display = "Wildfolk"; DefaultId = 5; Keywords = @("wildfolk", "wild", "t1") },
    @{ Id = 4; Code = "F04"; Short = "Eraden"; Category = "Eraden Kingdom (T2)"; Display = "Eraden Kingdom"; DefaultId = 6; Keywords = @("eraden", "kingdom", "t2") },
    @{ Id = 5; Code = "F05"; Short = "Eye"; Category = "The Eye (T3)"; Display = "The Eye"; DefaultId = 7; Keywords = @("eye", "t3") },
    @{ Id = 6; Code = "F06"; Short = "Rakta"; Category = "Rakta"; Display = "Rakta"; DefaultId = 8; Keywords = @("rakta") },
    @{ Id = 7; Code = "F07"; Short = "Special"; Category = "Special / Rogue"; Display = "Special / Rogue"; DefaultId = 9; Keywords = @("special", "rogue") },
    @{ Id = 8; Code = "F08"; Short = "Fallback"; Category = "Fallback (Any Enemy)"; Display = "Any Enemy"; DefaultId = -1; Keywords = @() }
)

$loreSpells = @(
    @("Fire", "Lightning", "Gravity"),
    @("Fire", "Fire", "Lightning"),
    @("Gravity", "Lightning", "Fire"),
    @("Lightning", "Fire", "Gravity"),
    @("Gravity", "Lightning", "Fire"),
    @("Fire", "Gravity", "Lightning"),
    @("Lightning", "Gravity", "Fire"),
    @("Fire", "Lightning", "Gravity")
)

$loreChances = @(
    @(25, 10, 5),
    @(45, 20, 10),
    @(30, 25, 15),
    @(35, 30, 20),
    @(40, 35, 15),
    @(50, 25, 15),
    @(45, 30, 20),
    @(20, 15, 10)
)

$loreStrengths = @(
    @(65, 50, 40),
    @(75, 60, 50),
    @(70, 60, 45),
    @(80, 65, 55),
    @(85, 70, 60),
    @(80, 65, 55),
    @(85, 70, 60),
    @(60, 50, 40)
)

$spellDefaultIndex = @{
    Fire = 0
    Lightning = 1
    Gravity = 2
}

$sb = New-Object System.Text.StringBuilder
function L([string]$line = "") { [void]$sb.AppendLine($line) }

L "using System;"
L "using System.Collections.Generic;"
L "using System.Reflection;"
L "using ThunderRoad;"
L "using UnityEngine;"
L ""
L "namespace ImbuementOverhaul.Configuration"
L "{"
L "    public static class ImbuementModOptions"
L "    {"
L "        public const string VERSION = ""0.3.0"";"
L "        public const int FactionCount = 8;"
L "        public const int ImbueSlotsPerFaction = 3;"
L ""
L "        private const string CategoryPresets = ""Imbuement Overhaul"";"
L "        private const string CategoryWeaponFilters = ""Weapon Type Filters"";"
L "        private const string CategoryDiagnostics = ""Advanced"";"
L ""
foreach ($f in $factions) {
    L ("        private const string Category{0} = ""{1}"";" -f $f.Code, $f.Category)
}
L ""
L "        private const string OptionEnableMod = ""Enable Mod"";"
L "        private const string OptionPresetImbue = ""Imbue Experience Preset"";"
L "        private const string OptionPresetChance = ""Chance Experience Preset"";"
L "        private const string OptionPresetStrength = ""Strength Experience Preset"";"
L ""
L "        private const string OptionWeaponFilterAll = ""All Weapons Filter"";"
L "        private const string OptionWeaponFilterArrow = ""Arrows Filter"";"
L "        private const string OptionWeaponFilterDagger = ""Daggers Filter"";"
L "        private const string OptionWeaponFilterSword = ""Swords Filter"";"
L "        private const string OptionWeaponFilterAxe = ""Axes Filter"";"
L "        private const string OptionWeaponFilterMace = ""Maces Filter"";"
L "        private const string OptionWeaponFilterSpear = ""Spears Filter"";"
L "        private const string OptionWeaponFilterStaff = ""Staves Filter"";"
L "        private const string OptionWeaponFilterBow = ""Bows Filter"";"
L "        private const string OptionWeaponFilterShield = ""Shields Filter"";"
L "        private const string OptionWeaponFilterThrowing = ""Throwables Filter"";"
L "        private const string OptionWeaponFilterOther = ""Other Weapons Filter"";"
L ""
L "        private const string OptionEnableBasicLogging = ""Basic Logs"";"
L "        private const string OptionEnableDiagnosticsLogging = ""Diagnostics Logs"";"
L "        private const string OptionEnableVerboseLogging = ""Verbose Logs"";"
L "        private const string OptionSessionDiagnostics = ""Session Diagnostics"";"
L "        private const string OptionUpdateInterval = ""Imbue Update Interval"";"
L "        private const string OptionRescanInterval = ""Enemy Rescan Interval"";"
L "        private const string OptionForceReapply = ""Force Reapply"";"
L ""

foreach ($f in $factions) {
    L ("        private const string Option{0}Enabled = ""{1} Enabled"";" -f $f.Code, $f.Short)
    for ($slot = 1; $slot -le 3; $slot++) {
        L ("        private const string Option{0}S{1}Spell = ""{2} Imbue {1}"";" -f $f.Code, $slot, $f.Short)
        L ("        private const string Option{0}S{1}Chance = ""{2} Chance {1}"";" -f $f.Code, $slot, $f.Short)
        L ("        private const string Option{0}S{1}Strength = ""{2} Strength {1}"";" -f $f.Code, $slot, $f.Short)
    }
    L ""
}

L "        public const string PresetImbueLore = ""LoreAccurate"";"
L "        public const string PresetImbueFactionIdentity = ""FactionIdentity"";"
L "        public const string PresetImbueArcaneSurge = ""ArcaneSurge"";"
L "        public const string PresetImbueElementalChaos = ""ElementalChaos"";"
L "        public const string PresetImbueRandomized = ""Randomized"";"
L ""
L "        public const string PresetChanceLow = ""LowIntensity"";"
L "        public const string PresetChanceBalanced = ""BalancedBattles"";"
L "        public const string PresetChanceAggressive = ""AggressiveWaves"";"
L "        public const string PresetChanceRelentless = ""RelentlessThreat"";"
L "        public const string PresetChanceOverflow = ""OverflowNormalized"";"
L ""
L "        public const string PresetStrengthFaint = ""FaintCharge"";"
L "        public const string PresetStrengthStandard = ""BattleReady"";"
L "        public const string PresetStrengthEmpowered = ""Empowered"";"
L "        public const string PresetStrengthOvercharged = ""Overcharged"";"
L "        public const string PresetStrengthCataclysmic = ""Cataclysmic"";"
L ""
L "        public const string WeaponFilterDefault = ""Default"";"
L "        public const string WeaponFilterNone = ""None"";"
L "        public const string WeaponFilterFire = ""Fire"";"
L "        public const string WeaponFilterGravity = ""Gravity"";"
L "        public const string WeaponFilterLightning = ""Lightning"";"
L ""
L "        private static readonly string[] FactionCategories ="
L "        {"
foreach ($f in $factions) {
    L ("            Category{0}," -f $f.Code)
}
L "        };"
L ""
L "        private static readonly int[] DefaultFactionIds ="
L "        {"
foreach ($f in $factions) {
    L ("            {0}," -f $f.DefaultId)
}
L "        };"
L ""
L "        private static readonly string[] FactionDisplayNames ="
L "        {"
foreach ($f in $factions) {
    L ("            ""{0}""," -f $f.Display)
}
L "        };"
L ""
L "        private static readonly string[] FactionShortNames ="
L "        {"
foreach ($f in $factions) {
    L ("            ""{0}""," -f $f.Short)
}
L "        };"
L ""
L "        private static readonly string[][] FactionKeywords ="
L "        {"
foreach ($f in $factions) {
    if ($f.Keywords.Count -eq 0) {
        L "            new string[0],"
    } else {
        $joined = ($f.Keywords | ForEach-Object { '"' + $_ + '"' }) -join ", "
        L ("            new[] {{ {0} }}," -f $joined)
    }
}
L "        };"
L ""
L "        private static readonly System.Random presetRandom = new System.Random();"
L "        private static readonly string[] BaseSpellPool = { ""Fire"", ""Lightning"", ""Gravity"" };"
L ""
L "        private static int[] resolvedFactionIds;"
L "        private static bool resolvedFromCatalog;"
L "        private static int resolvedFactionCount = -1;"
L ""
L "        public enum WeaponFilterBucket"
L "        {"
L "            All = 0,"
L "            Arrow = 1,"
L "            Dagger = 2,"
L "            Sword = 3,"
L "            Axe = 4,"
L "            Mace = 5,"
L "            Spear = 6,"
L "            Staff = 7,"
L "            Bow = 8,"
L "            Shield = 9,"
L "            Throwing = 10,"
L "            Other = 11"
L "        }"
L ""
L "        public struct ImbueSlotConfig"
L "        {"
L "            public int SlotIndex;"
L "            public string SpellId;"
L "            public float ChancePercent;"
L "            public float StrengthPercent;"
L "        }"
L ""
L "        public struct FactionProfile"
L "        {"
L "            public int FactionIndex;"
L "            public int FactionId;"
L "            public bool Enabled;"
L "            public ImbueSlotConfig Slot1;"
L "            public ImbueSlotConfig Slot2;"
L "            public ImbueSlotConfig Slot3;"
L "            public float TotalNormalizedChancePercent;"
L "            public int ProfileHash;"
L "        }"
L ""
L "        [ModOption(name = OptionEnableMod, order = 0, defaultValueIndex = 1, tooltip = ""Master switch for Imbuement Overhaul."")]"
L "        public static bool EnableMod = true;"
L ""
L "        [ModOption(name = OptionPresetImbue, category = CategoryPresets, categoryOrder = 0, order = 10, defaultValueIndex = 0, valueSourceName = nameof(ImbuePresetProvider), tooltip = ""Lore-oriented imbue experience profile. Applies values into every faction section."")]"
L "        public static string PresetImbue = PresetImbueLore;"
L ""
L "        [ModOption(name = OptionPresetChance, category = CategoryPresets, categoryOrder = 0, order = 20, defaultValueIndex = 1, valueSourceName = nameof(ChancePresetProvider), tooltip = ""Overall chance profile. Applies per-slot chances into every faction section."")]"
L "        public static string PresetChance = PresetChanceBalanced;"
L ""
L "        [ModOption(name = OptionPresetStrength, category = CategoryPresets, categoryOrder = 0, order = 30, defaultValueIndex = 1, valueSourceName = nameof(StrengthPresetProvider), tooltip = ""Overall strength profile. Applies per-slot strengths into every faction section."")]"
L "        public static string PresetStrength = PresetStrengthStandard;"
L ""
L "        [ModOption(name = OptionWeaponFilterAll, category = CategoryWeaponFilters, categoryOrder = 20, order = 0, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider), tooltip = ""Global fallback spell override for all weapon types."")]"
L "        public static string WeaponFilterAll = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterArrow, category = CategoryWeaponFilters, categoryOrder = 20, order = 10, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterArrow = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterDagger, category = CategoryWeaponFilters, categoryOrder = 20, order = 20, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterDagger = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterSword, category = CategoryWeaponFilters, categoryOrder = 20, order = 30, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterSword = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterAxe, category = CategoryWeaponFilters, categoryOrder = 20, order = 40, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterAxe = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterMace, category = CategoryWeaponFilters, categoryOrder = 20, order = 50, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterMace = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterSpear, category = CategoryWeaponFilters, categoryOrder = 20, order = 60, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterSpear = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterStaff, category = CategoryWeaponFilters, categoryOrder = 20, order = 70, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterStaff = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterBow, category = CategoryWeaponFilters, categoryOrder = 20, order = 80, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterBow = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterShield, category = CategoryWeaponFilters, categoryOrder = 20, order = 90, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterShield = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterThrowing, category = CategoryWeaponFilters, categoryOrder = 20, order = 100, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterThrowing = WeaponFilterDefault;"
L "        [ModOption(name = OptionWeaponFilterOther, category = CategoryWeaponFilters, categoryOrder = 20, order = 110, defaultValueIndex = 0, valueSourceName = nameof(WeaponFilterProvider))]"
L "        public static string WeaponFilterOther = WeaponFilterDefault;"
L ""

foreach ($f in $factions) {
    $catOrder = 100 + (($f.Id - 1) * 10)
    L ("        [ModOption(name = Option{0}Enabled, category = Category{0}, categoryOrder = {1}, order = 0, defaultValueIndex = 1)]" -f $f.Code, $catOrder)
    L ("        public static bool {0}Enabled = true;" -f $f.Code)
    L ""

    for ($slot = 1; $slot -le 3; $slot++) {
        $spell = $loreSpells[$f.Id - 1][$slot - 1]
        $chance = [int]$loreChances[$f.Id - 1][$slot - 1]
        $strength = [int]$loreStrengths[$f.Id - 1][$slot - 1]
        $spellIdx = $spellDefaultIndex[$spell]
        $chanceIdx = [int][Math]::Min(20, [Math]::Max(0, [Math]::Round($chance / 5)))
        $strengthIdx = [int][Math]::Min(20, [Math]::Max(0, [Math]::Round($strength / 5)))
        $baseOrder = 10 + (($slot - 1) * 30)

        L ("        [ModOption(name = Option{0}S{1}Spell, category = Category{0}, categoryOrder = {2}, order = {3}, defaultValueIndex = {4}, valueSourceName = nameof(SpellProvider))]" -f $f.Code, $slot, $catOrder, $baseOrder, $spellIdx)
        L ("        public static string {0}S{1}Spell = ""{2}"";" -f $f.Code, $slot, $spell)
        L ("        [ModOption(name = Option{0}S{1}Chance, category = Category{0}, categoryOrder = {2}, order = {3}, defaultValueIndex = {4}, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = ""If total slot chances exceed 100, they are normalized."")]" -f $f.Code, $slot, $catOrder, ($baseOrder + 10), $chanceIdx)
        L ("        public static float {0}S{1}Chance = {2}f;" -f $f.Code, $slot, $chance)
        L ("        [ModOption(name = Option{0}S{1}Strength, category = Category{0}, categoryOrder = {2}, order = {3}, defaultValueIndex = {4}, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]" -f $f.Code, $slot, $catOrder, ($baseOrder + 20), $strengthIdx)
        L ("        public static float {0}S{1}Strength = {2}f;" -f $f.Code, $slot, $strength)
        L ""
    }
}

L "        [ModOption(name = OptionEnableBasicLogging, category = CategoryDiagnostics, categoryOrder = 999, order = 0, defaultValueIndex = 1)]"
L "        public static bool EnableBasicLogging = true;"
L "        [ModOption(name = OptionEnableDiagnosticsLogging, category = CategoryDiagnostics, categoryOrder = 999, order = 5, defaultValueIndex = 0)]"
L "        public static bool EnableDiagnosticsLogging = false;"
L "        [ModOption(name = OptionEnableVerboseLogging, category = CategoryDiagnostics, categoryOrder = 999, order = 7, defaultValueIndex = 0)]"
L "        public static bool EnableVerboseLogging = false;"
L "        [ModOption(name = OptionSessionDiagnostics, category = CategoryDiagnostics, categoryOrder = 999, order = 8, defaultValueIndex = 0)]"
L "        public static bool SessionDiagnostics = false;"
L "        [ModOption(name = OptionUpdateInterval, category = CategoryDiagnostics, categoryOrder = 999, order = 10, defaultValueIndex = 4, valueSourceName = nameof(UpdateIntervalProvider), interactionType = (ModOption.InteractionType)2)]"
L "        public static float UpdateInterval = 0.25f;"
L "        [ModOption(name = OptionRescanInterval, category = CategoryDiagnostics, categoryOrder = 999, order = 20, defaultValueIndex = 3, valueSourceName = nameof(RescanIntervalProvider), interactionType = (ModOption.InteractionType)2)]"
L "        public static float RescanInterval = 2.0f;"
L ""
L "        [ModOption(name = OptionForceReapply, category = CategoryDiagnostics, categoryOrder = 999, order = 30, defaultValueIndex = 0)]"
L "        [ModOptionDontSave]"
L "        public static bool ForceReapply = false;"
L ""

# Emit methods from template file for maintainability
$templatePath = Join-Path $PSScriptRoot "ImbuementModOptions.template.txt"
if (-not (Test-Path $templatePath)) {
    throw "Template not found: $templatePath"
}
Get-Content $templatePath | ForEach-Object { L $_ }

L "    }"
L "}"

Set-Content -Path $outputPath -Value $sb.ToString() -Encoding UTF8
Write-Output "Generated $outputPath"


