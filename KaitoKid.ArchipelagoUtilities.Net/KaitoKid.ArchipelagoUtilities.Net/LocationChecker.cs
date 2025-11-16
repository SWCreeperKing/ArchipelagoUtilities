using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;

namespace KaitoKid.ArchipelagoUtilities.Net
{
    public class LocationChecker
    {
        private static ILogger _logger;
        private ArchipelagoClient _archipelago;
        private Dictionary<string, long> _checkedLocations;

        public LocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked)
        {
            _logger = logger;
            _archipelago = archipelago;
            _checkedLocations = locationsAlreadyChecked.ToDictionary(x => x, x => (long)-1);
        }

        public List<string> GetAllLocationsAlreadyChecked()
        {
            return _checkedLocations.Keys.ToList();
        }

        public bool IsLocationChecked(string locationName)
        {
            return _checkedLocations.ContainsKey(locationName);
        }

        public bool IsLocationNotChecked(string locationName)
        {
            return !IsLocationChecked(locationName);
        }

        public bool IsLocationMissing(string locationName)
        {
            return LocationExists(locationName) && IsLocationNotChecked(locationName);
        }

        public bool LocationExists(string locationName, bool allowCloseEnough = false)
        {
            if (_archipelago.LocationExists(locationName))
            {
                return true;
            }

            if (!allowCloseEnough)
            {
                return false;
            }

            if (TryGetCloseEnoughNameFromAllLocations(locationName, out _))
            {
                return true;
            }

            return false;
        }

        public IReadOnlyCollection<long> GetAllMissingLocations()
        {
            return _archipelago.GetAllMissingLocations();
        }

        public IReadOnlyCollection<string> GetAllMissingLocationNames()
        {
            return _archipelago.GetAllMissingLocations().Select(x => _archipelago.GetMyLocationName(x)).ToArray();
        }

        public virtual void AddCheckedLocations(string[] locationNames)
        {
            RememberCheckedLocations(locationNames);
            SendAllLocationChecks();
        }

        public virtual void AddCheckedLocation(string locationName)
        {
            RememberCheckedLocation(locationName);
            SendAllLocationChecks();
        }

        public virtual void RememberCheckedLocations(string[] locationNames)
        {
            foreach (var locationName in locationNames)
            {
                RememberCheckedLocation(locationName);
            }
        }

        public virtual void RememberCheckedLocation(string locationName)
        {
            if (_checkedLocations.ContainsKey(locationName))
            {
                return;
            }

            if (!TryGetLocationId(locationName, out var locationId))
            {
                return;
            }

            _checkedLocations.Add(locationName, locationId);
            ClearCache();
            return;
        }

        private bool TryGetLocationId(string locationName, out long locationId)
        {
            locationId = _archipelago.GetLocationId(locationName);

            if (locationId != -1)
            {
                return true;
            }
            
            if (!TryGetCloseEnoughNameFromAllLocations(locationName, out var alternateName))
            {
                _logger.LogError($"Location \"{locationName}\" could not be converted to an Archipelago id");
                return false;
            }

            locationId = _archipelago.GetLocationId(alternateName);
            _logger.LogWarning($"Location \"{locationName}\" not found, using \"{alternateName}\" instead");

            return true;
        }

        public bool TryGetCloseEnoughNameFromUncheckedLocations(string locationName, out string closeEnoughName)
        {
            var allLocationsNotChecked = new HashSet<string>(GetAllLocationsNotChecked());

            return TryGetCloseEnoughName(allLocationsNotChecked, locationName, out closeEnoughName);
        }

        public bool TryGetCloseEnoughNameFromAllLocations(string locationName, out string closeEnoughName)
        {
            var allLocations = new HashSet<string>(GetAllLocations());

            return TryGetCloseEnoughName(allLocations, locationName, out closeEnoughName);
        }

        public bool TryGetCloseEnoughName(ICollection<string> validNames, string locationName, out string closeEnoughName)
        {
            var locationNameNoSpace = locationName.Replace(" ", "");
            foreach (var validName in validNames)
            {
                if (locationName.Equals(validName, StringComparison.InvariantCultureIgnoreCase))
                {
                    closeEnoughName = validName;
                    return true;
                }

                var validNameNoSpace = validName.Replace(" ", "");
                if (locationNameNoSpace.Equals(validNameNoSpace, StringComparison.InvariantCultureIgnoreCase))
                {
                    closeEnoughName = validName;
                    return true;
                }
            }

            closeEnoughName = "";
            return false;
        }

        public virtual void SendAllLocationChecks()
        {
            if (!_archipelago.IsConnected)
            {
                return;
            }

            TryToIdentifyUnknownLocationNames();

            var allCheckedLocations = new List<long>();
            allCheckedLocations.AddRange(_checkedLocations.Values);

            allCheckedLocations = allCheckedLocations.Distinct().Where(x => x > -1).ToList();

            _archipelago.ReportCheckedLocations(allCheckedLocations.ToArray());
        }

        public virtual void VerifyNewLocationChecksWithArchipelago()
        {
            var allCheckedLocations = _archipelago.GetAllCheckedLocations();
            foreach (var checkedLocation in allCheckedLocations)
            {
                if (!_checkedLocations.ContainsKey(checkedLocation.Key))
                {
                    _checkedLocations.Add(checkedLocation.Key, checkedLocation.Value);
                    ClearCache();
                }
            }
        }

        public virtual void ClearCache()
        {
        }

        private void TryToIdentifyUnknownLocationNames()
        {
            foreach (var locationName in _checkedLocations.Keys)
            {
                if (_checkedLocations[locationName] > -1)
                {
                    continue;
                }

                var locationId = _archipelago.GetLocationId(locationName);
                if (locationId == -1)
                {
                    continue;
                }

                _checkedLocations[locationName] = locationId;
            }
        }

        public void ForgetLocations(IEnumerable<string> locations)
        {
            foreach (var location in locations)
            {
                if (!_checkedLocations.ContainsKey(location))
                {
                    continue;
                }

                _checkedLocations.Remove(location);
                ClearCache();
            }
        }

        public IEnumerable<string> GetAllLocationsNotChecked()
        {
            if (!_archipelago.IsConnected)
            {
                return Enumerable.Empty<string>();
            }

            return _archipelago.GetSession().Locations.AllMissingLocations.Select(_archipelago.GetMyLocationName)
                .Where(x => x != null && !_checkedLocations.ContainsKey(x));
        }

        public IEnumerable<string> GetAllLocations()
        {
            if (!_archipelago.IsConnected)
            {
                return Enumerable.Empty<string>();
            }

            return _archipelago.GetSession().Locations.AllLocations.Select(_archipelago.GetMyLocationName).Where(x => x != null);
        }
    }
}
