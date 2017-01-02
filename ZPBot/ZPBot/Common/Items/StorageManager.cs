using System;
using System.Collections.Generic;
using ZPBot.Annotations;

namespace ZPBot.Common.Items
{
    public class StorageManager
    {
        private readonly Dictionary<byte, InventoryItem> _itemList;
        private readonly object _lock;

        public StorageManager()
        {
            _itemList = new Dictionary<byte, InventoryItem>();
            _lock = new object();
        }

        public void Add([NotNull] InventoryItem item)
        {
            lock (_lock)
            {
                _itemList.Add(item.Slot, item);
            }
        }

        public void Remove(byte slot)
        {
            lock (_lock)
            {
                if (_itemList.ContainsKey(slot))
                {
                    _itemList.Remove(slot);
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
            lock (_lock)
            {
                return _itemList.ContainsKey(slot) ? _itemList[slot] : null;
            }
        }

        public void MoveItem(byte fromSlot, byte toSlot, ushort quantity)
        {
            lock (_lock)
            {
                if (_itemList.ContainsKey(fromSlot))
                {
                    if (!_itemList.ContainsKey(toSlot) || toSlot < 13 || fromSlot < 13) // simple move
                    {
                        var item = new InventoryItem(_itemList[fromSlot])
                        {
                            Slot = toSlot,
                            Quantity = quantity
                        };

                        _itemList[fromSlot].Quantity -= quantity;
                        if (_itemList[fromSlot].Quantity == 0) _itemList.Remove(fromSlot);
                        _itemList.Add(toSlot, item);
                    }
                    else // exchange or stack
                    {
                        if (_itemList[fromSlot].Id == _itemList[toSlot].Id) // Stack
                        {
                            if (_itemList[toSlot].Quantity + quantity > _itemList[toSlot].MaxQuantity) // exchange
                            {
                                var itemFrom = new InventoryItem(_itemList[fromSlot]) { Slot = toSlot };
                                var itemTo = new InventoryItem(_itemList[toSlot]) { Slot = fromSlot };

                                _itemList.Remove(fromSlot);
                                _itemList.Remove(toSlot);

                                _itemList.Add(toSlot, itemFrom);
                                _itemList.Add(fromSlot, itemTo);
                            }
                            else // stack all from fromSlot to toSlot
                            {
                                _itemList[toSlot].Quantity += quantity;
                                _itemList[fromSlot].Quantity -= quantity;
                                if (_itemList[fromSlot].Quantity == 0) _itemList.Remove(fromSlot);
                            }
                        }
                        else // exchange
                        {
                            var itemFrom = new InventoryItem(_itemList[fromSlot]) { Slot = toSlot };
                            var itemTo = new InventoryItem(_itemList[toSlot]) { Slot = fromSlot };

                            _itemList.Remove(fromSlot);
                            _itemList.Remove(toSlot);

                            _itemList.Add(toSlot, itemFrom);
                            _itemList.Add(fromSlot, itemTo);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(@"MoveItem - Cannot find Item");
                }
            }
        }
    }
}
