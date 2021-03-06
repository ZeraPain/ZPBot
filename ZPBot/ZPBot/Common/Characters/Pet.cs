﻿using System.Collections.Generic;
using System.Linq;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    public class Pet : Char
    {
        public uint WorldId;
        public uint CurHealth;
        private EPosition _position;

        private readonly List<InventoryItem> _itemList;
        private readonly object _lock;


        public Pet([NotNull] Char chardata, uint worldId, uint curHealth)
        {
            Id = chardata.Id;
            MaxHealth = chardata.MaxHealth;
            PetType3 = chardata.PetType3;

            WorldId = worldId;
            CurHealth = curHealth;

            _itemList = new List<InventoryItem>();
            _lock = new object();
        }

        public void SetPosition(EPosition position)
        {
            _position = position;
        }

        [NotNull]
        public GamePosition GetIngamePosition() => new GamePosition((int)((_position.XSection - 135) * 192 + _position.XPosition / 10),
            (int)((_position.YSection - 92) * 192 + _position.YPosition / 10));

        public void ClearInventory()
        {
            lock (_lock)
            {
                _itemList.Clear();
            }
        }

        public void AddItem(InventoryItem item)
        {
            lock (_lock)
            {
                _itemList.Add(item);
            }
        }

        public void RemoveItem(byte slot)
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
