using ImbuementOverhaul.Configuration;
using NUnit.Framework;

namespace ImbuementOverhaul.Tests
{
    [TestFixture]
    public class DurationModOptionsTests
    {
        [SetUp]
        public void SetUp()
        {
            DurationModOptions.EnableMod = true;
            DurationModOptions.PresetDrainProfile = DurationModOptions.PresetDrainBalanced;
            DurationModOptions.PlayerHeldDrainMultiplier = 1.0f;
            DurationModOptions.NpcHeldDrainMultiplier = 1.0f;
            DurationModOptions.WorldDrainMultiplier = 1.0f;
            DurationModOptions.EnableBasicLogging = true;
            DurationModOptions.EnableDiagnosticsLogging = false;
            DurationModOptions.EnableVerboseLogging = false;
        }

        [Test]
        public void Version_IsSemverLike()
        {
            Assert.That(DurationModOptions.VERSION, Is.Not.Null.And.Not.Empty);
            Assert.That(DurationModOptions.VERSION, Does.Match("^\\d+\\.\\d+\\.\\d+"));
        }

        [Test]
        public void Logging_DefaultsAreSensible()
        {
            Assert.That(DurationModOptions.EnableBasicLogging, Is.True);
            Assert.That(DurationModOptions.EnableDiagnosticsLogging, Is.False);
            Assert.That(DurationModOptions.EnableVerboseLogging, Is.False);
        }

        [Test]
        public void PresetNormalization_MapsAliases()
        {
            Assert.That(DurationModOptions.NormalizeDrainPreset("player dominant"), Is.EqualTo(DurationModOptions.PresetDrainPlayerDominant));
            Assert.That(DurationModOptions.NormalizeDrainPreset("npc favored"), Is.EqualTo(DurationModOptions.PresetDrainEnemyFavored));
            Assert.That(DurationModOptions.NormalizeDrainPreset("balanced"), Is.EqualTo(DurationModOptions.PresetDrainBalanced));
        }

        [Test]
        public void ApplyingPreset_BatchWritesSourceOfTruthFields()
        {
            DurationModOptions.PresetDrainProfile = DurationModOptions.PresetDrainEnemyDominant;

            bool changed = DurationModOptions.ApplySelectedPresets();

            Assert.That(changed, Is.True);
            Assert.That(DurationModOptions.PlayerHeldDrainMultiplier, Is.EqualTo(1.60f));
            Assert.That(DurationModOptions.NpcHeldDrainMultiplier, Is.EqualTo(0.50f));
            Assert.That(DurationModOptions.WorldDrainMultiplier, Is.EqualTo(1.60f));
        }

        [Test]
        public void PresetNormalization_DefaultsWhenUnknown()
        {
            Assert.That(DurationModOptions.NormalizeDrainPreset("nonsense"), Is.EqualTo(DurationModOptions.PresetDrainBalanced));
        }

        [Test]
        public void BalancedPreset_KeepsAllThreeContextsEqual()
        {
            DurationModOptions.PresetDrainProfile = DurationModOptions.PresetDrainBalanced;
            DurationModOptions.ApplySelectedPresets();

            Assert.That(DurationModOptions.PlayerHeldDrainMultiplier, Is.EqualTo(1.00f));
            Assert.That(DurationModOptions.NpcHeldDrainMultiplier, Is.EqualTo(1.00f));
            Assert.That(DurationModOptions.WorldDrainMultiplier, Is.EqualTo(1.00f));
        }
    }
}

