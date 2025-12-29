using Archipelago.MultiClient.Net.Enums;
using FluentAssertions;
using KaitoKid.ArchipelagoUtilities.AssetDownloader;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;

namespace KaitoKid.ArchipelagoUtilities.Net.Tests
{
    internal class ArchipelagoItemSpritesTests
    {
        private ILogger _logger;
        private ArchipelagoItemSprites _itemSprites;

        [SetUp]
        public void SetUp()
        {
            _logger = new TestLogger();
        }

        [TestCase("Hollow Knight", "Desolate_Dive", true, true, null)]
        [TestCase("Stardew Valley", "Desolate_Dive", true, false, null)]
        [TestCase("Hollow Knight", "Combat Level", true, false, null)]
        [TestCase("Hollow Knight", "ABDJF", true, true, "")]
        [TestCase("ANOGF", "ABDJF", false, false, null)]
        [TestCase("A Hat in Time", "Relic (Red Crayon)", true, true, "Relic (Red Crayon)")]
        public void TestLoadsSpritesProperly(string gameName, string itemName, bool expectedSuccess, bool expectedCorrectGame, string expectedItem)
        {
            // Arrange
            if (expectedItem == null)
            {
                expectedItem = itemName;
            }
            _itemSprites = new ArchipelagoItemSprites(_logger, s => JsonConvert.DeserializeObject<ItemSpriteAliases>(s));
            var scoutedLocation = new ScoutedLocation("", itemName, "", gameName, 1, 2, 3, ItemFlags.Advancement);
            _itemSprites.PrepareGameAssets(gameName);

            // Act
            var success = _itemSprites.TryGetCustomAsset(scoutedLocation, "Stardew Valley", true, true, out var sprite);

            // Assert
            success.Should().Be(expectedSuccess);
            if (expectedSuccess)
            {
                sprite.Item.Should().Be(expectedItem);
                if (expectedCorrectGame)
                {
                    sprite.Game.Should().Be(gameName);
                    var expectedFilePath = $"{gameName}_{expectedItem}.png";
                    if (expectedItem == "")
                    {
                        expectedFilePath = $"{gameName}.png";
                    }
                    sprite.FilePath.Should().EndWith($@"Custom Assets\{gameName}\{expectedFilePath}");
                }
            }
        }

        [TestCase("A Hat in Time", "25 Pons", "Pons")]
        [TestCase("A Hat in Time", "50 Pons", "Pons")]
        [TestCase("A Hat in Time", "75 Pons", "")]
        [TestCase("A Hat in Time", "100 Pons", "Pons")]
        [TestCase("A Hat in Time", "Relic (Necklace Bust)", "Relic (Necklace Bust)")]
        [TestCase("A Hat in Time", "Relic (Necklace)", "Relic Necklace")]
        [TestCase("The Witness", "Desert Laser", "Lasers")]
        [TestCase("The Witness", "Colored Squares", "Symbols")]
        [TestCase("TUNIC", "Cyan Peril Ring", "Cards")]
        [TestCase("TUNIC", "Ladders to Frog's Domain", "Ladders")]
        [TestCase("Super Mario 64", "Cannon Unlock RR", "Cannon Unlock BoB")]
        [TestCase("Super Mario 64", "Second Floor Key", "Progressive Key")]
        [TestCase("Pokemon Red and Blue", "TM11", "TMs")]
        [TestCase("Pokemon Red and Blue", "TM47 Explosion", "TMs")]
        [TestCase("Lingo", "Color Hunt - Purple Barrier", "Color Hunt")]
        [TestCase("Lingo", "Orange Tower - Second Floor", "Orange Tower")]
        [TestCase("Lunacid", "Destroying Angel Mushroom", "Material")]
        [TestCase("Lunacid", "Fire Worm", "Fire Spell")]
        [TestCase("Paper Mario The Thousand Year Door", "Black Key (Plane Curse)", "Black Key")]
        [TestCase("Mario & Luigi Superstar Saga", "1-UP Super", "1-UP Mushroom")]
        public void TestAliasesLoadsSpritesProperly(string gameName, string itemName, string expectedItem)
        {
            // Arrange
            _itemSprites = new ArchipelagoItemSprites(_logger, s => JsonConvert.DeserializeObject<ItemSpriteAliases>(s));
            var scoutedLocation = new ScoutedLocation("", itemName, "", gameName, 1, 2, 3, ItemFlags.Advancement);
            _itemSprites.PrepareGameAssets(gameName);

            // Act
            var success = _itemSprites.TryGetCustomAsset(scoutedLocation, "Stardew Valley", true, true, out var sprite);

            // Assert
            success.Should().BeTrue();
            sprite.Item.Should().Be(expectedItem);
            sprite.Game.Should().Be(gameName);
            var expectedFilePath = $"{gameName}_{expectedItem}.png";
            if (expectedItem == "")
            {
                expectedFilePath = $"{gameName}.png";
            }
            sprite.FilePath.Should().EndWith($@"Custom Assets\{gameName}\{expectedFilePath}");
        }

        [TestCase("Balatro", "j_delayed_grat")]
        public void TestMissingEntireGameDownloadsProperly(string gameName, string itemName)
        {
            // Arrange
            _itemSprites = new ArchipelagoItemSprites(_logger, s => JsonConvert.DeserializeObject<ItemSpriteAliases>(s));
            var scoutedLocation = new ScoutedLocation("", itemName, "", gameName, 1, 2, 3, ItemFlags.Advancement);
            var zipPath = Path.Combine(Paths.CustomAssetsDirectory, $"{gameName}.zip");
            var folderPath = Path.Combine(Paths.CustomAssetsDirectory, gameName);
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            // Act
            var attemptDownload = _itemSprites.TryGetCustomAsset(scoutedLocation, "Stardew Valley", false, true, out var sprite);

            // Assert
            Thread.Sleep(50);
            File.Exists(zipPath).Should().BeTrue();
            Directory.Exists(folderPath).Should().BeTrue();
            var itemPath = Path.Combine(folderPath, $"{gameName}_{itemName}.png");
            File.Exists(itemPath).Should().BeTrue();

        }
    }
}
