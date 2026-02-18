using ImbueDurationManager.Configuration;
using NUnit.Framework;

namespace ImbueDurationManager.Tests
{
    [TestFixture]
    public class IDMModOptionsTests
    {
        [SetUp]
        public void SetUp()
        {
            IDMModOptions.EnableMod = true;
            IDMModOptions.PresetDurationExperience = IDMModOptions.PresetDefaultPlus;
            IDMModOptions.PresetContextProfile = IDMModOptions.PresetContextUniform;
            IDMModOptions.GlobalDrainMultiplier = 0.85f;
            IDMModOptions.PlayerHeldDrainMultiplier = 1.0f;
            IDMModOptions.PlayerThrownDrainMultiplier = 1.0f;
            IDMModOptions.NpcHeldDrainMultiplier = 1.0f;
            IDMModOptions.NpcThrownDrainMultiplier = 1.0f;
            IDMModOptions.WorldDrainMultiplier = 1.0f;
            IDMModOptions.EnableBasicLogging = true;
            IDMModOptions.EnableDiagnosticsLogging = false;
            IDMModOptions.EnableVerboseLogging = false;
        }

        [Test]
        public void Version_IsSemverLike()
        {
            Assert.That(IDMModOptions.VERSION, Is.Not.Null.And.Not.Empty);
            Assert.That(IDMModOptions.VERSION, Does.Match("^\\d+\\.\\d+\\.\\d+"));
        }

        [Test]
        public void Logging_DefaultsAreSensible()
        {
            Assert.That(IDMModOptions.EnableBasicLogging, Is.True);
            Assert.That(IDMModOptions.EnableDiagnosticsLogging, Is.False);
            Assert.That(IDMModOptions.EnableVerboseLogging, Is.False);
        }

        [Test]
        public void PresetNormalization_MapsAliases()
        {
            Assert.That(IDMModOptions.NormalizeDurationPreset("infinite"), Is.EqualTo(IDMModOptions.PresetInfinite));
            Assert.That(IDMModOptions.NormalizeDurationPreset("Way Less"), Is.EqualTo(IDMModOptions.PresetWayLess));
            Assert.That(IDMModOptions.NormalizeContextPreset("player power"), Is.EqualTo(IDMModOptions.PresetContextPlayerFavored));
        }

        [Test]
        public void ApplyingInfinitePreset_BatchWritesSourceOfTruthFields()
        {
            IDMModOptions.PresetDurationExperience = IDMModOptions.PresetInfinite;
            IDMModOptions.PresetContextProfile = IDMModOptions.PresetContextUniform;

            int presetHash = IDMModOptions.GetPresetSelectionHash();

            Assert.That(presetHash, Is.Not.EqualTo(0));
        }

        [Test]
        public void PresetNormalization_DefaultsWhenUnknown()
        {
            Assert.That(IDMModOptions.NormalizeDurationPreset("nonsense"), Is.EqualTo(IDMModOptions.PresetDefaultPlus));
            Assert.That(IDMModOptions.NormalizeContextPreset("nonsense"), Is.EqualTo(IDMModOptions.PresetContextUniform));
        }
    }
}
