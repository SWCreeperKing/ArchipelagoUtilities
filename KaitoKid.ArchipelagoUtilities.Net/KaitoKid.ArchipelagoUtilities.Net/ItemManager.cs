using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace KaitoKid.ArchipelagoUtilities.Net
{
    public abstract class ItemManager
    {
        protected readonly ArchipelagoClient _archipelago;
        protected HashSet<ReceivedItem> _itemsAlreadyProcessed;

        protected ItemManager(ArchipelagoClient archipelago, IEnumerable<ReceivedItem> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemsAlreadyProcessed = new HashSet<ReceivedItem>(itemsAlreadyProcessed);
        }

        public void ReceiveAllNewItems(bool immediatelyIfPossible = true)
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();

            foreach (var receivedItem in allReceivedItems)
            {
                ReceiveNewItem(receivedItem, immediatelyIfPossible);
            }
        }

        protected virtual void ReceiveNewItem(ReceivedItem receivedItem, bool immediatelyIfPossible)
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
