using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;

// ReSharper disable UseArrayEmptyMethod

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public abstract class ArchipelagoClient : ISessionProvider
    {
        public const string MOVE_LINK_TAG = "MoveLink";
        private const string MISSING_LOCATION_NAME = "Thin Air";
        protected ILogger Logger;
        private ArchipelagoSession _session;
        private DeathLinkService _deathLinkService;
        private ArchipelagoConnectionInfo _connectionInfo;
        private DataPackageCache _localDataPackage;

        private Action<ReceivedItemsHelper> _itemReceivedFunction;
        public bool IsConnected { get; private set; }
        protected ISlotData _slotData;
        public Dictionary<string, ScoutedLocation> ScoutedLocations { get; set; }
        public HashSet<string> ScoutHintedLocations { get; set; }
        public bool DeathLink => _connectionInfo?.DeathLink == true;
        public abstract string GameName { get; }
        public abstract string ModName { get; }
        public abstract string ModVersion { get; }

        public ArchipelagoClient(ILogger logger, DataPackageCache dataPackageCache) : this(logger, dataPackageCache, (Action)null)
        {
        }

        public ArchipelagoClient(ILogger logger, DataPackageCache dataPackageCache, Action itemReceivedFunction)
        : this(logger, dataPackageCache, itemReceivedFunction != null ? (_) => itemReceivedFunction() : (Action<ReceivedItemsHelper>)((_) => {}))
        {
        }

        public ArchipelagoClient(ILogger logger, DataPackageCache dataPackageCache, Action<ReceivedItemsHelper> itemReceivedFunction)
        {
            Logger = logger;
            _localDataPackage = dataPackageCache;
            _itemReceivedFunction = itemReceivedFunction;

            IsConnected = false;
            ScoutedLocations = new Dictionary<string, ScoutedLocation>();
            ScoutHintedLocations = new HashSet<string>();
        }

        public virtual bool ConnectToMultiworld(ArchipelagoConnectionInfo connectionInfo, out string errorMessage)
        {
            DisconnectPermanently();
            var success = TryConnect(connectionInfo, out errorMessage);
            if (!success)
            {
                DisconnectPermanently();
                return false;
            }

            if (!IsMultiworldVersionSupported())
            {
                var genericVersion = _slotData.MultiworldVersion.Replace("0", "x");
                errorMessage = $"This Multiworld has been created for {ModName} version {genericVersion},\nbut this is {ModName} version {ModVersion}.\nPlease update to a compatible mod version.";
                DisconnectPermanently();
                return false;
            }

            return true;
        }

        private bool TryConnect(ArchipelagoConnectionInfo connectionInfo, out string errorMessage)
        {
            LoginResult result;
            try
            {
                InitSession(connectionInfo);
                var itemsHandling = ItemsHandlingFlags.AllItems;
                var apVersion = new Version(0, 6, 1);
                var tags = connectionInfo.DeathLink == true ? new[] { "AP", "DeathLink" } : new[] { "AP" };
                result = _session.TryConnectAndLogin(GameName, _connectionInfo.SlotName, itemsHandling, apVersion, tags, null, _connectionInfo.Password);
            }
            catch (Exception e)
            {
                var message = e.GetBaseException().Message;
                result = new LoginFailure(message);
                Logger.LogError($"An error occured trying to connect to archipelago. Message: {message}");
            }

            if (!result.Successful)
            {
                var failure = (LoginFailure)result;
                errorMessage = $"Failed to Connect to {_connectionInfo?.HostUrl}:{_connectionInfo?.Port} as {_connectionInfo?.SlotName}:";
                foreach (var error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }

                var detailedErrorMessage = errorMessage;
                foreach (var error in failure.ErrorCodes)
                {
                    detailedErrorMessage += $"\n    {error}";
                }

                Logger.LogError(detailedErrorMessage);
                DisconnectAndCleanup();
                return false; // Did not connect, show the user the contents of `errorMessage`
            }

            errorMessage = "";

            // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess._slotData`)
            var loginSuccess = (LoginSuccessful)result;
            var loginMessage = $"Connected to Archipelago server as {connectionInfo.SlotName} (Team {loginSuccess.Team}).";
            Logger.LogInfo(loginMessage);

            // Must go AFTER a successful connection attempt
            InitializeSlotData(connectionInfo.SlotName, loginSuccess.SlotData);
            if (connectionInfo.DeathLink == null)
            {
                connectionInfo.DeathLink = _slotData.DeathLink;
            }

            InitializeAfterConnection();
            return true;
        }

        protected virtual void InitializeAfterConnection()
        {
            IsConnected = true;

            _session.Items.ItemReceived += OnItemReceived;
            _session.MessageLog.OnMessageReceived += OnMessageReceived;
            _session.Socket.PacketReceived += OnPacketReceived;
            _session.Socket.ErrorReceived += SessionErrorReceived;
            _session.Socket.SocketClosed += SessionSocketClosed;

            InitializeDeathLink();
        }

        public void Sync()
        {
            if (!MakeSureConnected(0))
            {
                return;
            }

            _session.Socket.SendPacket(new SyncPacket());
        }

        protected virtual void InitializeDeathLink()
        {
            _deathLinkService = _session.CreateDeathLinkService();
            _deathLinkService.OnDeathLinkReceived += ReceiveDeathLink;
            if (_connectionInfo.DeathLink == true)
            {
                _deathLinkService.EnableDeathLink();
            }
            else
            {
                _deathLinkService.DisableDeathLink();
            }
        }

        public void ToggleDeathlink()
        {
            if (_connectionInfo.DeathLink == true)
            {
                _deathLinkService.DisableDeathLink();
                _connectionInfo.DeathLink = false;
            }
            else
            {
                _deathLinkService.EnableDeathLink();
                _connectionInfo.DeathLink = true;
            }
        }

        protected abstract void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields);

        private void InitSession(ArchipelagoConnectionInfo connectionInfo)
        {
            _session = ArchipelagoSessionFactory.CreateSession(connectionInfo.HostUrl, connectionInfo.Port);
            _connectionInfo = connectionInfo;
        }

        protected abstract void OnMessageReceived(LogMessage message);

        protected abstract void OnPacketReceived(ArchipelagoPacketBase packet);

        public void SendMessage(string text)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            var packet = new SayPacket()
            {
                Text = text,
            };

            _session.Socket.SendPacket(packet);
        }

        private void OnItemReceived(ReceivedItemsHelper receivedItemsHelper)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            _itemReceivedFunction(receivedItemsHelper);
        }

        public void ReportCheckedLocations(long[] locationIds)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            _session.Locations.CompleteLocationChecksAsync(locationIds);
            if (_session?.RoomState == null)
            {
                return;
            }
        }

        public int GetTeam()
        {
            if (!MakeSureConnected())
            {
                return -1;
            }

            return _session.ConnectionInfo.Team;
        }

        public string GetPlayerName()
        {
            return GetPlayerName(_session.ConnectionInfo.Slot);
        }

        public string GetPlayerName(int playerSlot)
        {
            if (!MakeSureConnected())
            {
                return "Archipelago Player";
            }

            return _session.Players.GetPlayerName(playerSlot) ?? "Archipelago Player";
        }

        public string GetPlayerAlias(string playerName)
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            var player = _session.Players.AllPlayers.FirstOrDefault(x => x.Name == playerName);

            return player?.Alias;
        }

        public bool PlayerExists(string playerName)
        {
            if (!MakeSureConnected())
            {
                return false;
            }

            return _session.Players.AllPlayers.Any(x => x.Name == playerName) || _session.Players.AllPlayers.Any(x => x.Alias == playerName);
        }

        public string GetPlayerGame(string playerName)
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            var player = _session.Players.AllPlayers.FirstOrDefault(x => x.Name == playerName) ??
                         _session.Players.AllPlayers.FirstOrDefault(x => x.Alias == playerName);

            return player?.Game;
        }

        public string GetPlayerGame(int playerSlot)
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            var player = _session.Players.AllPlayers.FirstOrDefault(x => x.Slot == playerSlot);
            return player?.Game;
        }

        public bool IsCurrentGamePlayer(string playerName)
        {
            var game = GetPlayerGame(playerName);
            return game != null && game == GameName;
        }

        public Dictionary<string, long> GetAllCheckedLocations()
        {
            if (!MakeSureConnected())
            {
                return new Dictionary<string, long>();
            }

            var allLocationsCheckedIds = _session.Locations.AllLocationsChecked;
            var allLocationsChecked = allLocationsCheckedIds.ToDictionary(GetLocationName, x => x);
            return allLocationsChecked;
        }

        public IReadOnlyCollection<long> GetAllMissingLocations()
        {
            if (!MakeSureConnected())
            {
                return new List<long>();
            }

            return _session.Locations.AllMissingLocations;
        }

        public IReadOnlyCollection<string> GetAllMissingLocationNames()
        {
            return GetAllMissingLocations().Select(GetLocationName).ToArray();
        }

        public Dictionary<string, long> GetAllLocations()
        {
            if (!MakeSureConnected())
            {
                return new Dictionary<string, long>();
            }

            var allLocationsCheckedIds = _session.Locations.AllLocations;
            var allLocationsChecked = allLocationsCheckedIds.ToDictionary(GetLocationName, x => x);
            return allLocationsChecked;
        }

        public List<string> GetAllLocationNames()
        {
            if (!MakeSureConnected())
            {
                return new List<string>();
            }

            var allLocationsCheckedIds = _session.Locations.AllLocations;
            var allLocationsChecked = allLocationsCheckedIds.Select(GetLocationName).ToList();
            return allLocationsChecked;
        }

        public List<ReceivedItem> GetAllReceivedItems()
        {
            if (!MakeSureConnected())
            {
                return new List<ReceivedItem>();
            }

            var allReceivedItems = new List<ReceivedItem>();
            var apItems = _session.Items.AllItemsReceived.ToArray();
            for (var itemIndex = 0; itemIndex < apItems.Length; itemIndex++)
            {
                var apItem = apItems[itemIndex];
                var itemName = GetItemName(apItem);
                var playerName = GetPlayerName(apItem.Player);
                var locationName = GetLocationName(apItem);

                var receivedItem = new ReceivedItem(locationName, itemName, playerName, apItem.LocationId, apItem.ItemId, apItem.Player, itemIndex);

                allReceivedItems.Add(receivedItem);
            }

            return allReceivedItems;
        }

        public Dictionary<string, int> GetAllReceivedItemNamesAndCounts()
        {
            if (!MakeSureConnected())
            {
                return new Dictionary<string, int>();
            }

            var receivedItemsGrouped = _session.Items.AllItemsReceived.GroupBy(x => x.ItemName);
            var receivedItemsWithCount = receivedItemsGrouped.ToDictionary(x => x.Key, x => x.Count());
            return receivedItemsWithCount;
        }

        public bool HasReceivedItem(string itemName)
        {
            return HasReceivedItem(itemName, false);
        }

        public bool HasReceivedItem(string itemName, bool ignoreCase)
        {
            return HasReceivedItem(itemName, ignoreCase, out _);
        }

        public bool HasReceivedItem(string itemName, bool ignoreCase, out string sendingPlayer)
        {
            sendingPlayer = "";
            if (!MakeSureConnected())
            {
                return false;
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.OrdinalIgnoreCase;
            foreach (var receivedItem in _session.Items.AllItemsReceived)
            {
                if (!GetItemName(receivedItem).Equals(itemName, stringComparison))
                {
                    continue;
                }

                sendingPlayer = _session.Players.GetPlayerName(receivedItem.Player);
                return true;
            }

            return false;
        }

        public int GetReceivedItemCount(string itemName)
        {
            if (!MakeSureConnected())
            {
                return 0;
            }

            return _session.Items.AllItemsReceived.Count(x => GetItemName(x) == itemName);
        }

        public Hint[] GetHints()
        {
            if (!MakeSureConnected())
            {
                return new Hint[0];
            }

            var hintTask = _session.DataStorage.GetHintsAsync();
            hintTask.Wait(2000);
            if (hintTask.IsCanceled || hintTask.IsFaulted || !hintTask.IsCompleted || hintTask.Status != TaskStatus.RanToCompletion)
            {
                return new Hint[0];
            }

            return hintTask.Result;
        }

        public Hint[] GetMyActiveHints()
        {
            if (!MakeSureConnected())
            {
                return new Hint[0];
            }

            return GetHints().Where(x => !x.Found && GetPlayerName(x.FindingPlayer) == _slotData.SlotName).ToArray();
        }

        private static readonly HintStatus[] _desiredHintStatus = { HintStatus.Priority };
        private static readonly HintStatus[] _avoidedHintStatus = { HintStatus.Avoid };

        public Hint[] GetMyActiveDesiredHints()
        {
            return GetMyActiveHintsMatchingStatus(_desiredHintStatus);
        }

        public Hint[] GetMyActiveAvoidedHints()
        {
            return GetMyActiveHintsMatchingStatus(_avoidedHintStatus);
        }

        public Hint[] GetMyActiveHintsMatchingStatus(HintStatus[] statusToMatch)
        {
            if (!MakeSureConnected())
            {
                return new Hint[0];
            }

            return GetHints().Where(x => !x.Found && GetPlayerName(x.FindingPlayer) == _slotData.SlotName && statusToMatch.Contains(x.Status)).ToArray();
        }

        public void ReportGoalCompletion()
        {
            if (!MakeSureConnected())
            {
                return;
            }

            var statusUpdatePacket = new StatusUpdatePacket
            {
                Status = ArchipelagoClientState.ClientGoal,
            };
            _session.Socket.SendPacket(statusUpdatePacket);
        }

        public string GetLocationName(ItemInfo item)
        {
            return item?.LocationName ?? GetLocationName(item.LocationId, true);
        }

        public string GetLocationName(long locationId)
        {
            return GetLocationName(locationId, true);
        }

        public string GetLocationName(long locationId, bool required)
        {
            if (!MakeSureConnected())
            {
                return _localDataPackage.GetLocalLocationName(locationId);
            }

            var locationName = _session.Locations.GetLocationNameFromId(locationId);
            if (string.IsNullOrWhiteSpace(locationName))
            {
                locationName = _localDataPackage.GetLocalLocationName(locationId);
            }

            if (string.IsNullOrWhiteSpace(locationName))
            {
                if (required)
                {
                    Logger.LogError($"Failed at getting the location name for location {locationId}. This is probably due to a corrupted datapackage. Unexpected behaviors may follow");
                }

                return MISSING_LOCATION_NAME;
            }

            return locationName;
        }

        public bool LocationExists(string locationName)
        {
            if (locationName == null || !MakeSureConnected())
            {
                return false;
            }

            var id = GetLocationId(locationName);
            return _session.Locations.AllLocations.Contains(id);
        }

        public long GetLocationId(string locationName)
        {
            return GetLocationId(locationName, GameName);
        }

        public long GetLocationId(string locationName, string gameName)
        {
            if (!MakeSureConnected())
            {
                return _localDataPackage.GetLocalLocationId(locationName);
            }

            var locationId = _session.Locations.GetLocationIdFromName(gameName, locationName);
            if (locationId <= 0)
            {
                locationId = _localDataPackage.GetLocalLocationId(locationName);
            }

            return locationId;
        }

        public string GetItemName(ItemInfo item)
        {
            return item?.ItemName ?? GetItemName(item.ItemId);
        }

        public string GetItemName(long itemId)
        {
            if (!MakeSureConnected())
            {
                return _localDataPackage.GetLocalItemName(itemId);
            }

            var itemName = _session.Items.GetItemName(itemId);
            if (string.IsNullOrWhiteSpace(itemName))
            {
                itemName = _localDataPackage.GetLocalItemName(itemId);
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                Logger.LogError($"Failed at getting the item name for item {itemId}. This is probably due to a corrupted datapackage. Unexpected behaviors may follow");
                return "Error Item";
            }

            return itemName;
        }

        public void SendDeathLinkAsync(string reason = "Unknown cause")
        {
            Task.Run(() => SendDeathLink(reason));
        }

        public void SendDeathLink(string reason = "Unknown cause")
        {
            if (!MakeSureConnected())
            {
                return;
            }

            try
            {
                Logger.LogMessage($"Sending a deathlink with reason [{reason}]");
                _deathLinkService.SendDeathLink(new DeathLink(GetPlayerName(), reason));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private void ReceiveDeathLink(DeathLink deathlink)
        {
            if (_connectionInfo.DeathLink != true)
            {
                return;
            }

            var deathLinkMessage = $"You have been killed by {deathlink.Source} ({deathlink.Cause})";
            Logger.LogInfo(deathLinkMessage);

            KillPlayerDeathLink(deathlink);
        }

        protected abstract void KillPlayerDeathLink(DeathLink deathlink);

        public Dictionary<string, ScoutedLocation> ScoutManyLocations(IEnumerable<string> locationNames, bool createAsHint = false)
        {
            var scoutResult = new Dictionary<string, ScoutedLocation>();
            if (!MakeSureConnected() || locationNames == null || !locationNames.Any())
            {
                Logger.LogWarning($"Could not scout locations {locationNames}");
                return scoutResult;
            }

            var namesToScout = new List<string>();
            var idsToScout = new List<long>();
            foreach (var locationName in locationNames)
            {
                if (!NeedToScout(locationName, createAsHint))
                {
                    scoutResult.Add(locationName, ScoutedLocations[locationName]);
                    continue;
                }

                var locationId = GetLocationId(locationName);
                if (locationId == -1)
                {
                    Logger.LogWarning($"Could get location id for \"{locationName}\".");
                    continue;
                }

                if (_session.Locations.AllLocationsChecked.Contains(locationId) || !_session.Locations.AllMissingLocations.Contains(locationId))
                {
                    Logger.LogInfo($"Skipping scout operation for location \"{locationName}\" that is missing or already checked");
                    continue;
                }

                namesToScout.Add(locationName);
                idsToScout.Add(locationId);
            }

            if (!idsToScout.Any())
            {
                return scoutResult;
            }

            ScoutedItemInfo[] scoutResponse;
            try
            {
                scoutResponse = ScoutLocations(idsToScout.ToArray(), createAsHint);
                if (scoutResponse.Length < 1)
                {
                    Logger.LogInfo($"Could not scout location ids \"{idsToScout}\".");
                    return scoutResult;
                }
            }
            catch (Exception e)
            {
                Logger.LogInfo($"Could not scout location ids \"{idsToScout}\". Message: {e.Message}");
                return scoutResult;
            }

            for (var i = 0; i < idsToScout.Count; i++)
            {
                if (scoutResponse.Length <= i)
                {
                    break;
                }

                var itemScouted = scoutResponse[i];
                var itemName = GetItemName(itemScouted);
                var playerSlotName = _session.Players.GetPlayerName(itemScouted.Player);
                var gameName = itemScouted.ItemGame;

                var scoutedLocation = new ScoutedLocation(namesToScout[i], itemName, playerSlotName, gameName, idsToScout[i], itemScouted.ItemId, itemScouted.Player, itemScouted.Flags);


                if (!ScoutedLocations.ContainsKey(namesToScout[i]))
                {
                    ScoutedLocations.Add(namesToScout[i], scoutedLocation);
                }
                if (createAsHint)
                {
                    ScoutHintedLocations.Add(namesToScout[i]);
                }
                scoutResult.Add(namesToScout[i], scoutedLocation);
            }

            return scoutResult;
        }

        public ScoutedLocation ScoutSingleLocation(long locationId, bool createAsHint = false)
        {
            var locationName = GetLocationName(locationId);
            return ScoutSingleLocation(locationName, createAsHint);
        }

        public ScoutedLocation ScoutSingleLocation(string locationName, bool createAsHint = false)
        {
            if (!NeedToScout(locationName, createAsHint))
            {
                return ScoutedLocations[locationName];
            }

            if (!MakeSureConnected())
            {
                Logger.LogWarning($"Could not find the id for location \"{locationName}\".");
                return null;
            }

            try
            {
                var locationId = GetLocationId(locationName);
                if (locationId == -1)
                {
                    Logger.LogWarning($"Could not find the id for location \"{locationName}\".");
                    return null;
                }

                if (_session.Locations.AllLocationsChecked.Contains(locationId) || !_session.Locations.AllMissingLocations.Contains(locationId))
                {
                    Logger.LogInfo($"Skipping scout operation for location \"{locationName}\" that is missing or already checked");
                    return null;
                }

                var scoutedItemInfo = ScoutLocation(locationId, createAsHint);
                if (scoutedItemInfo == null)
                {
                    Logger.LogWarning($"Could not scout location \"{locationName}\".");
                    return null;
                }

                var itemName = GetItemName(scoutedItemInfo);
                var playerSlotName = _session.Players.GetPlayerName(scoutedItemInfo.Player);
                var gameName = scoutedItemInfo.ItemGame;

                var scoutedLocation = new ScoutedLocation(locationName, itemName, playerSlotName, gameName, locationId,
                    scoutedItemInfo.ItemId, scoutedItemInfo.Player, scoutedItemInfo.Flags);

                if (!ScoutedLocations.ContainsKey(locationName))
                {
                    ScoutedLocations.Add(locationName, scoutedLocation);
                }
                if (createAsHint)
                {
                    ScoutHintedLocations.Add(locationName);
                }
                return scoutedLocation;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not scout location \"{locationName}\". Message: {e.Message}");
                return null;
            }
        }

        private bool NeedToScout(string locationName, bool createAsHint)
        {
            if (!ScoutedLocations.ContainsKey(locationName))
            {
                return true;
            }

            if (!createAsHint)
            {
                return false;
            }

            return !ScoutHintedLocations.Contains(locationName);
        }

        private ScoutedItemInfo ScoutLocation(long locationId, bool createAsHint)
        {
            var scoutTask = _session.Locations.ScoutLocationsAsync(GetHintPolicy(createAsHint), locationId);
            scoutTask.Wait();
            var scoutedItems = scoutTask.Result;
            if (scoutedItems == null || !scoutedItems.Any())
            {
                return null;
            }

            return scoutedItems.First().Value;
        }

        private ScoutedItemInfo[] ScoutLocations(long[] locationIds, bool createAsHint)
        {
            var scoutTask = _session.Locations.ScoutLocationsAsync(GetHintPolicy(createAsHint), locationIds);
            scoutTask.Wait();
            return scoutTask.Result.Values.ToArray();
        }

        private static HintCreationPolicy GetHintPolicy(bool createAsHint = false)
        {
            return createAsHint ? HintCreationPolicy.CreateAndAnnounceOnce : HintCreationPolicy.None;
        }

        private void SessionErrorReceived(Exception e, string message)
        {
            Logger.LogError(message);
            OnError();
            _lastConnectFailure = DateTime.Now;
            DisconnectAndCleanup();
        }

        private void SessionSocketClosed(string reason)
        {
            Logger.LogError($"Connection to Archipelago lost: {reason}");
            OnError();
            _lastConnectFailure = DateTime.Now;
            DisconnectAndCleanup();
        }

        protected virtual void OnError()
        {
        }

        public virtual void DisconnectAndCleanup()
        {
            if (!IsConnected)
            {
                return;
            }

            if (_session != null)
            {
                _session.Items.ItemReceived -= OnItemReceived;
                _session.MessageLog.OnMessageReceived -= OnMessageReceived;
                _session.Socket.PacketReceived -= OnPacketReceived;
                _session.Socket.ErrorReceived -= SessionErrorReceived;
                _session.Socket.SocketClosed -= SessionSocketClosed;
                _session.Socket.DisconnectAsync();
            }
            _session = null;
            IsConnected = false;
        }

        public void DisconnectTemporarily()
        {
            DisconnectAndCleanup();
            _allowRetries = false;
        }

        public void ReconnectAfterTemporaryDisconnect()
        {
            _allowRetries = true;
            MakeSureConnected(0);
        }

        public void DisconnectPermanently()
        {
            DisconnectAndCleanup();
            _connectionInfo = null;
        }

        private DateTime _lastConnectFailure;
        private const int THRESHOLD_TO_RETRY_CONNECTION_IN_SECONDS = 15;
        private bool _allowRetries = true;

        public bool MakeSureConnected(int threshold = THRESHOLD_TO_RETRY_CONNECTION_IN_SECONDS)
        {
            if (IsConnected)
            {
                return true;
            }

            if (_connectionInfo == null)
            {
                return false;
            }

            var now = DateTime.Now;
            var timeSinceLastFailure = now - _lastConnectFailure;
            if (timeSinceLastFailure.TotalSeconds < threshold)
            {
                return false;
            }

            if (!_allowRetries)
            {
                Logger.LogError("Reconnection attempt failed");
                _lastConnectFailure = DateTime.Now;
                return false;
            }

            TryConnect(_connectionInfo, out _);
            if (!IsConnected)
            {
                OnReconnectFailure();
                _lastConnectFailure = DateTime.Now;
                return false;
            }

            OnReconnectSuccess();
            return IsConnected;
        }

        protected virtual void OnReconnectSuccess()
        {
        }

        protected virtual void OnReconnectFailure()
        {
        }

        public void APUpdate()
        {
            MakeSureConnected(60);
        }

        private bool IsMultiworldVersionSupported()
        {
            var majorVersion = ModVersion.Split('.').First();
            var multiworldVersionParts = _slotData.MultiworldVersion.Split('.');
            if (multiworldVersionParts.Length < 3)
            {
                return false;
            }

            var multiworldMajor = multiworldVersionParts[0];
            var multiworldMinor = multiworldVersionParts[1];
            var multiworldFix = multiworldVersionParts[2];
            return majorVersion == multiworldMajor;
        }

        public IEnumerable<PlayerInfo> GetAllPlayers()
        {
            if (!MakeSureConnected())
            {
                return Enumerable.Empty<PlayerInfo>();
            }

            return _session.Players.AllPlayers;
        }

        public PlayerInfo GetCurrentPlayer()
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            return GetAllPlayers().FirstOrDefault(x => x.Slot == _session.ConnectionInfo.Slot);
        }

        public ArchipelagoSession GetSession()
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            return _session;
        }

        public void SendMoveLinkPacket(string slot, float timespan, float x, float y)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            EnableMoveLink();
            var data = new Dictionary<string, JToken>()
            {
                { "slot", slot }, // Unique identifier to be able to ignore your own packets. Can be just your slot number if you don't care about supporting slot coop
                { "timespan", timespan }, // Duration of this movement, in seconds. Helps scaling between games. Recommended 0.25 or 0.5
                { "x", x }, // Movement in number of "tiles". If your engine doesn't have tiles, scale it how you'd like. Your character should be about 1 tile wide
                { "y", y }, // Movement in number of "tiles". If your engine doesn't have tiles, scale it how you'd like. Your character should be about 1 tile wide
            };
            SendBouncePacket(new[] { MOVE_LINK_TAG }, data);
        }

        private void EnableMoveLink()
        {
            if (!MakeSureConnected())
            {
                return;
            }

            if (Array.IndexOf(_session.ConnectionInfo.Tags, MOVE_LINK_TAG) != -1)
            {
                return;
            }
            var newTags = _session.ConnectionInfo.Tags.Concat(new[] { MOVE_LINK_TAG }).ToArray();
            _session.ConnectionInfo.UpdateConnectionOptions(newTags);
        }

        public void SendBouncePacket(IEnumerable<string> tags, Dictionary<string, JToken> data)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            var bouncePacket = new BouncePacket();
            bouncePacket.Tags.AddRange(tags);
            bouncePacket.Data = data;
            _session.Socket.SendPacketAsync(bouncePacket).FireAndForget();
        }
    }
}