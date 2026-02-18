using ImbuementOverhaul.Configuration;
using NUnit.Framework;

namespace ImbuementOverhaul.Tests
{
    [TestFixture]
    public class ImbuementModOptionsTests
    {
        [SetUp]
        public void SetUp()
        {
            ImbuementModOptions.EnableBasicLogging = true;
            ImbuementModOptions.EnableDiagnosticsLogging = false;
            ImbuementModOptions.EnableVerboseLogging = false;
        }

        [Test]
        public void Version_IsSemverLike()
        {
            Assert.That(ImbuementModOptions.VERSION, Is.Not.Null.And.Not.Empty);
            Assert.That(ImbuementModOptions.VERSION, Does.Match("^\\d+\\.\\d+\\.\\d+"));
        }

        [Test]
        public void EnemyTypePresets_UseThreeModeModel()
        {
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeProfilePreset("mage only"), Is.EqualTo(ImbuementModOptions.PresetEnemyTypeMageOnly));
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeProfilePreset("casters"), Is.EqualTo(ImbuementModOptions.PresetEnemyTypeMageOnly));
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeProfilePreset("ranged"), Is.EqualTo(ImbuementModOptions.PresetEnemyTypeRanged));
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeProfilePreset("all"), Is.EqualTo(ImbuementModOptions.PresetEnemyTypeAll));
        }

        [Test]
        public void BooleanLogging_DefaultsAreSensible()
        {
            Assert.That(ImbuementModOptions.EnableBasicLogging, Is.True);
            Assert.That(ImbuementModOptions.EnableDiagnosticsLogging, Is.False);
            Assert.That(ImbuementModOptions.EnableVerboseLogging, Is.False);
        }

        [Test]
        public void NormalizePresets_MapsKnownAliases()
        {
            Assert.That(ImbuementModOptions.NormalizeFactionProfilePreset("High Magic Conflict"), Is.EqualTo(ImbuementModOptions.PresetProfileHighMagic));
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeProfilePreset("mage melee"), Is.EqualTo(ImbuementModOptions.PresetEnemyTypeAll));
            Assert.That(ImbuementModOptions.NormalizeImbuePreset("random"), Is.EqualTo(ImbuementModOptions.PresetImbueRandomized));
            Assert.That(ImbuementModOptions.NormalizeChancePreset("Relentless Threat"), Is.EqualTo(ImbuementModOptions.PresetChanceRelentless));
            Assert.That(ImbuementModOptions.NormalizeStrengthPreset("cataclysmic"), Is.EqualTo(ImbuementModOptions.PresetStrengthCataclysmic));
        }

        [Test]
        public void NormalizeEnemyTypeFallback_DefaultsToMelee()
        {
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeFallbackMode("unknown"), Is.EqualTo(ImbuementModOptions.EnemyTypeFallbackMelee));
            Assert.That(ImbuementModOptions.NormalizeEnemyTypeFallbackMode("skip"), Is.EqualTo(ImbuementModOptions.EnemyTypeFallbackSkip));
        }

        [Test]
        public void PresetSelectionHash_ChangesWhenPresetSelectionChanges()
        {
            string prevImbue = ImbuementModOptions.PresetImbue;
            try
            {
                int before = ImbuementModOptions.GetPresetSelectionHash();
                ImbuementModOptions.PresetImbue = ImbuementModOptions.PresetImbueRandomized;
                int after = ImbuementModOptions.GetPresetSelectionHash();
                Assert.That(after, Is.Not.EqualTo(before));
            }
            finally
            {
                ImbuementModOptions.PresetImbue = prevImbue;
            }
        }
    }
}


