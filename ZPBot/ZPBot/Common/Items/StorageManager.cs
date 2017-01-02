using System.Collections.Generic;
using System.Linq;
using ZPBot.Annotations;

namespace ZPBot.Common.Items
{
    public class StorageManager
    {
        private readonly List<InventoryItem> _itemList;
        private readonly object _lock;

        public StorageManager()
        {
            _itemList = new List<InventoryItem>();
            _lock = new object();
        }

        public void Add(InventoryItem item)
        {
            lock (_lock)
            {
                _itemList.Add(item);
            }
        }

        public void Remove(byte slot)
        {
            lock (_lock)
            {
                foreach (var item in _itemList.Where(item => item.Slot == slot))
                {
                    _itemList.Remove(item);
                    break;
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _itemList.Clear();
            }
        }

        [CanBeNull]
        public InventoryItem GetItembySlot(byte slot)
        {
            InventoryItem item = null;

            lock (_lock)
            {
                foreach (var invItem in _itemList.Where(invItem => invItem.Slot == slot))
                {
                    item = invItem;
                }
            }

            return item;
        }

        public void MoveItem(byte fromSlot, byte toSlot, ushort quantity)
        {
            lock (_lock)
            {
                var invFrom = GetItembySlot(fromSlot);
                var invTo = GetItembySlot(toSlot);

                if (invTo == null)
                {
                    if (invFrom != null && invFrom.Quantity == quantity) // Simple Move
                    {
                        invFrom.Slot = toSlot;
                    }
                    else // Split Item
                    {
                        if (invFrom != null)
                        {
                            invFrom.Quantity -= quantity;
                            _itemList.Add(new InventoryItem(invFrom, toSlot, quantity));
                        }
                    }
                }
                else if (invFrom != null && invFrom.Id == invTo.Id) // Stack Items
                {
                    var item = Silkroad.GetItemById(invTo.Id);
                    if (item != null && invTo.Quantity + quantity > item.MaxQuantity) // Fill new slot with rest
                    {
                        var rest = (ushort)(invTo.Quantity + quantity - item.MaxQuantity);

                        invFrom.Quantity = rest;
                        invTo.Quantity = item.MaxQuantity;
                    }
                    else // Fill new slot
                    {
                        invTo.Quantity += quantity;
                        _itemList.Remove(invFrom);
                    }
                }
                else // Switch Items
                {
                    if (invFrom != null)
                    {
                        var tmpSlot = invFrom.Slot;
                        invFrom.Slot = invTo.Slot;
                        invTo.Slot = tmpSlot;
                    }
                }
            }
        }
    }
}
