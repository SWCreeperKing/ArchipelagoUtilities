using FluentAssertions;

namespace KaitoKid.ArchipelagoUtilities.Net.Tests
{
    internal class LocationCheckerTests
    {
        private LocationChecker _locationChecker;

        [SetUp]
        public void SetUp()
        {
            _locationChecker = new LocationChecker(null, null, new List<string>());
        }

        [Test]
        public void TryGetCloseEnoughName_CanFindEqual()
        {
            // Arrange
            var allLocations = new[] { "LocA", "Locb", "Loc C", "Loc d" };
            var desiredLocation = "LocA";

            // Act
            var success = _locationChecker.TryGetCloseEnoughName(allLocations, desiredLocation, out var foundLocation);

            // Assert
            success.Should().BeTrue();
            foundLocation.Should().Be("LocA");
        }

        [Test]
        public void TryGetCloseEnoughName_CanFindDifferentCase()
        {
            // Arrange
            var allLocations = new[] { "LocA", "Locb", "Loc C", "Loc d" };
            var desiredLocation = "LocB";

            // Act
            var success = _locationChecker.TryGetCloseEnoughName(allLocations, desiredLocation, out var foundLocation);

            // Assert
            success.Should().BeTrue();
            foundLocation.Should().Be("Locb");
        }

        [Test]
        public void TryGetCloseEnoughName_CanFindDifferentSpacing()
        {
            // Arrange
            var allLocations = new[] { "LocA", "Locb", "Loc C", "Loc d" };
            var desiredLocation = "LocC";

            // Act
            var success = _locationChecker.TryGetCloseEnoughName(allLocations, desiredLocation, out var foundLocation);

            // Assert
            success.Should().BeTrue();
            foundLocation.Should().Be("Loc C");
        }

        [Test]
        public void TryGetCloseEnoughName_CanFindDifferentSpacingAndDifferentCase()
        {
            // Arrange
            var allLocations = new[] { "LocA", "Locb", "Loc C", "Loc d" };
            var desiredLocation = "LocD";

            // Act
            var success = _locationChecker.TryGetCloseEnoughName(allLocations, desiredLocation, out var foundLocation);

            // Assert
            success.Should().BeTrue();
            foundLocation.Should().Be("Loc d");
        }

        [Test]
        public void TryGetCloseEnoughName_CanNotFindDifferentLetters()
        {
            // Arrange
            var allLocations = new[] { "LocA", "Locb", "Loc C", "Loc d" };
            var desiredLocation = "LocE";

            // Act
            var success = _locationChecker.TryGetCloseEnoughName(allLocations, desiredLocation, out var foundLocation);

            // Assert
            success.Should().BeFalse();
        }
    }
}
