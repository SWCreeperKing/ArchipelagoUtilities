using Archipelago.MultiClient.Net.Enums;
using FluentAssertions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net.ItemSprites;

namespace KaitoKid.ArchipelagoUtilities.Net.Tests
{
    internal class ArchipelagoItemSpritesTests
    {
        private ILogger _logger;
        private ArchipelagoItemSprites _itemSprites;

        [SetUp]
        public void SetUp()
        {
            _logger = null;
        }

        [TestCase("Hollow Knight", "Desolate_Dive", true, true, true)]
        [TestCase("Stardew Valley", "Desolate_Dive", true, false, true)]
        [TestCase("Hollow Knight", "Combat Level", true, false, true)]
        [TestCase("Hollow Knight", "ABDJF", true, true, false)]
        [TestCase("ANOGF", "ABDJF", false, false, false)]
        public void Constructor_LoadsSpritesProperly(string gameName, string itemName, bool expectedSuccess, bool expectedCorrectGame, bool expectedCorrectItem)
        {
            // Arrange
            _itemSprites = new ArchipelagoItemSprites(_logger);
            var scoutedLocation = new ScoutedLocation("", itemName, "", gameName, 1, 2, 3, ItemFlags.Advancement);

            // Act
            var success = _itemSprites.TryGetCustomAsset(scoutedLocation, "Stardew Valley", out var sprite);

            // Assert
            success.Should().Be(expectedSuccess);
            if (expectedSuccess)
            {
                if (expectedCorrectGame) sprite.Game.Should().Be(gameName);
                if (expectedCorrectItem) sprite.Item.Should().Be(itemName);
                if (expectedCorrectGame && expectedCorrectItem) sprite.FilePath.Should().EndWith($@"Custom Assets\{gameName}\{gameName}_{itemName}.png");
            }
        }
    }
}
