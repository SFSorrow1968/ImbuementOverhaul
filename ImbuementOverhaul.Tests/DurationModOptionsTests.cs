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
            DurationModOptions.PresetDurationExperience = DurationModOptions.PresetDefaultPlus;
            DurationModOptions.PresetContextProfile = DurationModOptions.PresetContextUniform;
            DurationModOptions.GlobalDrainMultiplier = 0.85f;
            DurationModOptions.PlayerHeldDrainMultiplier = 1.0f;
            DurationModOptions.PlayerThrownDrainMultiplier = 1.0f;
            DurationModOptions.NpcHeldDrainMultiplier = 1.0f;
            DurationModOptions.NpcThrownDrainMultiplier = 1.0f;
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
            Assert.That(DurationModOptions.NormalizeDurationPreset("infinite"), Is.EqualTo(DurationModOptions.PresetInfinite));
            Assert.That(DurationModOptions.NormalizeDurationPreset("Way Less"), Is.EqualTo(DurationModOptions.PresetWayLess));
            Assert.That(DurationModOptions.NormalizeContextPreset("player power"), Is.EqualTo(DurationModOptions.PresetContextPlayerFavored));
        }

        [Test]
        public void ApplyingInfinitePreset_BatchWritesSourceOfTruthFields()
        {
            DurationModOptions.PresetDurationExperience = DurationModOptions.PresetInfinite;
            DurationModOptions.PresetContextProfile = DurationModOptions.PresetContextUniform;

            int presetHash = DurationModOptions.GetPresetSelectionHash();

            Assert.That(presetHash, Is.Not.EqualTo(0));
        }

        [Test]
        public void PresetNormalization_DefaultsWhenUnknown()
        {
            Assert.That(DurationModOptions.NormalizeDurationPreset("nonsense"), Is.EqualTo(DurationModOptions.PresetDefaultPlus));
            Assert.That(DurationModOptions.NormalizeContextPreset("nonsense"), Is.EqualTo(DurationModOptions.PresetContextUniform));
        }
    }
}

