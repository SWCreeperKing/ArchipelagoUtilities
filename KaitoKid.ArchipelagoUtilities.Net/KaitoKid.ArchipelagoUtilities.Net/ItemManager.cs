using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace KaitoKid.ArchipelagoUtilities.Net
{
    public abstract class ItemManager
    {
        private ArchipelagoClient _archipelago;
        private IItemParser _itemParser;
        private HashSet<ReceivedItem> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, IItemParser itemParser, IEnumerable<ReceivedItem> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemParser = itemParser;
            _itemsAlreadyProcessed = new HashSet<ReceivedItem>(itemsAlreadyProcessed);
        }

        public void ReceiveAllNewItems(bool immediatelyIfPossible)
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();

            foreach (var receivedItem in allReceivedItems)
            {
                ReceiveNewItem(receivedItem, immediatelyIfPossible);
            }
        }

        private void ReceiveNewItem(ReceivedItem receivedItem, bool immediatelyIfPossible)
        {
            if (_itemsAlreadyProcessed.Contains(receivedItem))
            {
                return;
            }

            ProcessItem(receivedItem, immediatelyIfPossible);
            _itemsAlreadyProcessed.Add(receivedItem);
        }

        protected abstract void ProcessItem(ReceivedItem receivedItem, bool immediatelyIfPossible);

        public List<ReceivedItem> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed.ToList();
        }
    }
}
