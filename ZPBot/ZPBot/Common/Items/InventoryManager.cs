using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Items
{
    internal class InventoryManager : ThreadManager
    {
        private readonly Dictionary<byte, InventoryItem> _itemList;
        private readonly GlobalManager _globalManager;
        private readonly object _lock;
        private Thread _fuseThread;
        public bool EnableHp { get; set; }
        public int HpPercent { get; set; }
        public bool EnableMp { get; set; }
        public int MpPercent { get; set; }
        public bool EnableUniversal { get; set; }
        public bool ReturntownNoAmmo { get; set; }
        public bool ReturntownNoPotion { get; set; }
        public bool EnableSpeedDrug { get; set; }

        public InventoryManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            _itemList = new Dictionary<byte, InventoryItem>();
            _lock = new object();
        }

        public void AddOrUpdate([CanBeNull] InventoryItem item)
        {
            lock (_lock)
            {
                if (item == null)
                    return;

                if (_itemList.ContainsKey(item.Slot))
                {
                    _itemList[item.Slot].Quantity = item.Quantity;
                    _itemList[item.Slot].Plus = item.Plus;
                }
                else
                {
                    _itemList.Add(item.Slot, item);
                }

                _globalManager.FMain.UpdateInventory(_itemList);
            }
        }

        public void Remove(byte slot)
        {
            lock (_lock)
            {
                if (_itemList.ContainsKey(slot))
                {
                    _itemList.Remove(slot);
                    _globalManager.FMain.UpdateInventory(_itemList);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _itemList.Clear();
                _globalManager.FMain.UpdateInventory(_itemList);
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

        public void UpdateQuantity(byte slot, ushort quantity)
        {
            lock (_lock)
            {
                if (_itemList.ContainsKey(slot))
                {
                    if (quantity == 0)
                        _itemList.Remove(slot);
                    else
                        _itemList[slot].Quantity = quantity;

                    _globalManager.FMain.UpdateInventory(_itemList);
                }
                else
                {
                    Console.WriteLine(@"UpdateQuantity - Cannot find Item");
                }
            }
        }

        public void MoveItem(byte fromSlot, byte toSlot, ushort quantity)
        {
            lock (_lock)
            {
                var exchange = new Action(delegate
                {
                    var itemFrom = new InventoryItem(_itemList[fromSlot]) { Slot = toSlot };
                    var itemTo = new InventoryItem(_itemList[toSlot]) { Slot = fromSlot };

                    _itemList.Remove(fromSlot);
                    _itemList.Remove(toSlot);

                    _itemList.Add(toSlot, itemFrom);
                    _itemList.Add(fromSlot, itemTo);
                });

                var move = new Action<ushort> (delegate (ushort amount)
                {
                    var item = new InventoryItem(_itemList[fromSlot])
                    {
                        Slot = toSlot,
                        Quantity = amount
                    };

                    _itemList[fromSlot].Quantity -= amount;
                    if (_itemList[fromSlot].Quantity == 0) _itemList.Remove(fromSlot);
                    _itemList.Add(toSlot, item);
                });

                if (_itemList.ContainsKey(fromSlot)) // move to / from equipped
                {
                    if (toSlot < 13 || fromSlot < 13)
                    {
                        if (quantity == 0) quantity = _itemList[fromSlot].Quantity;

                        if (_itemList.ContainsKey(toSlot)) // Exchange
                            exchange();
                        else // move
                            move(quantity);
                    }
                    else if (!_itemList.ContainsKey(toSlot)) // simple move or split
                    {
                        move(quantity);
                    }
                    else // exchange or stack
                    {
                        if (_itemList[fromSlot].Id == _itemList[toSlot].Id) // Stack
                        {
                            if (_itemList[toSlot].Quantity + quantity > _itemList[toSlot].MaxQuantity) // exchange
                            {
                                exchange();
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
                            exchange();
                        }
                    }

                    _globalManager.FMain.UpdateInventory(_itemList);
                }
                else
                {
                    Console.WriteLine(@"MoveItem - Cannot find Item");
                }
            }
        }

        [CanBeNull]
        private InventoryItem GetElixir(EEquipableType2 equipable)
        {
            InventoryItem invItem = null;
            var elixirtype = EReinforceType.None;

            switch (equipable)
            {
                case EEquipableType2.Weapon:
                    elixirtype = EReinforceType.Weapon;
                    break;
                case EEquipableType2.Shield:
                    elixirtype = EReinforceType.Shield;
                    break;
                case EEquipableType2.CGarment:
                case EEquipableType2.CProtector:
                case EEquipableType2.CArmor:
                case EEquipableType2.EGarment:
                case EEquipableType2.EProtector:
                case EEquipableType2.EArmor:
                    elixirtype = EReinforceType.Protector;
                    break;
                case EEquipableType2.CAccessory:
                case EEquipableType2.EAccessory:
                    elixirtype = EReinforceType.Accessory;
                    break;

            }

            if (elixirtype != EReinforceType.None)
            {
                lock (_lock)
                {
                    foreach (var item in _itemList.Where(item => item.Value.ReinforceType == elixirtype))
                    {
                        invItem = item.Value;
                    }
                }
            }

            return invItem;
        }

        [CanBeNull]
        private InventoryItem GetLuckyPowder(byte degree)
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var kvp in _itemList.Where(k => k.Value.ElixirType3 == EElixirType3.LuckyPowder && k.Value.Degree == degree))
                {
                    invItem = kvp.Value;
                }
            }

            return invItem;
        }

        [CanBeNull]
        private InventoryItem GetPotion(EPotionType3 potion)
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var kvp in _itemList.Where(k => k.Value.PotionType3 == potion))
                {
                    invItem = kvp.Value;
                }
            }

            return invItem;
        }

        [CanBeNull]
        private InventoryItem GetUnivsersalPill()
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var kvp in _itemList.Where(k => k.Value.CureType3 == ECureType3.Univsersal))
                {
                    invItem = kvp.Value;
                }
            }

            return invItem;
        }

        [CanBeNull]
        private InventoryItem GetReturnScroll()
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var kvp in _itemList.Where(k => k.Value.ScrollType3 == EScrollType3.Return))
                {
                    invItem = kvp.Value;
                }
            }

            return invItem;
        }

        [CanBeNull]
        private InventoryItem GetSpeedDrug()
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var kvp in _itemList.Where(k => k.Value.ScrollType == EScrollType.Speed))
                {
                    invItem = kvp.Value;
                }
            }

            return invItem;
        }

        public ushort GetItemCount(uint itemId)
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(k => k.Value.Slot > 12 && k.Value.Id == itemId).Aggregate(quantity, (current, k) => (ushort) (current + k.Value.Quantity));
            }

            return quantity;
        }

        private ushort GetAmmoCount()
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(item => item.Value.Slot > 12 && item.Value.ConsumableType2 == EConsumableType2.Ammo).Aggregate(quantity, (current, item) => (ushort)(current + item.Value.Quantity));
            }

            return quantity;
        }

        private ushort GetPotionCount(EPotionType3 potion)
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(k => k.Value.Slot > 12 && k.Value.PotionType3 == potion).Aggregate(quantity, (current, k) => (ushort)(current + k.Value.Quantity));
            }

            return quantity;
        }

        private ushort GetUniversalCount()
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(k => k.Value.Slot > 12 && k.Value.CureType3 == ECureType3.Univsersal).Aggregate(quantity, (current, k) => (ushort)(current + k.Value.Quantity));
            }

            return quantity;
        }

        public bool ReturnTown(string reason)
        {
            var invItem = GetReturnScroll();
            if (invItem != null)
            {
                do
                {
                    _globalManager.PacketManager.UseItem(invItem); //Return Scroll
                    Thread.Sleep(1000);
                } while (!_globalManager.LoopManager.IsTeleporting);

                _globalManager.FMain.AddEvent("Returning to town " + reason, "Character");
                return true;
            }

            _globalManager.FMain.AddEvent("Cannot return to town because there are no return scrolls", "Character", true);
            return false;
        }

        private bool Stack()
        {
            lock (_lock)
            {
                foreach (var kvp in _itemList)
                {
                    var invItem = kvp.Value;
                    if (invItem.Slot <= 12 || invItem.Quantity >= invItem.MaxQuantity) continue;

                    var item = invItem;
                    foreach (var kvp2 in _itemList.Where(k => k.Value.Slot > item.Slot && item.Id == k.Value.Id && k.Value.Quantity < k.Value.MaxQuantity))
                    {
                        var invItem2 = kvp2.Value;
                        _globalManager.PacketManager.StackItems(invItem2.Slot, invItem.Slot, (ushort)(invItem.MaxQuantity - invItem.Quantity));
                        Game.AllowStack = false;

                        return true;
                    }
                }

                return false;
            }
        }

        public void StackInventoryItems()
        {
            var stackable = true;

            while (stackable)
            {
                stackable = Stack();
                while (stackable && !Game.AllowStack)
                {
                    Thread.Sleep(500);
                    if (!_globalManager.LoopManager.IsLooping)
                        return;
                }
            }
        }

        protected override void MyThread()
        {
            while (BActive)
            {
                Thread.Sleep(200);

                if (EnableUniversal && _globalManager.Player.CureCount > 0) //Cure Recovery
                {
                    var invItem = GetUnivsersalPill();
                    if (invItem != null)
                    {
                        _globalManager.PacketManager.UseItem(invItem);
                        Thread.Sleep(500);
                    }
                    if (_globalManager.Botstate && ReturntownNoPotion && !_globalManager.LoopManager.IsLooping && GetUniversalCount() < 5 && ReturnTown("No Universal Pills"))
                        _globalManager.StartLoop(true);
                }

                if (EnableHp && _globalManager.Player.HealthPercent <= HpPercent) //HP Recovery
                {
                    var invItem = GetPotion(EPotionType3.Health);
                    if (invItem != null)
                    {
                        _globalManager.PacketManager.UseItem(invItem);
                        Thread.Sleep(500);
                    }
                    if (_globalManager.Botstate && ReturntownNoPotion && !_globalManager.LoopManager.IsLooping && GetPotionCount(EPotionType3.Health) < 20 && ReturnTown("No Health Potion"))
                        _globalManager.StartLoop(true);

                }

                if (EnableMp && _globalManager.Player.ManaPercent <= MpPercent) //MP Recovery
                {
                    var invItem = GetPotion(EPotionType3.Mana);
                    if (invItem != null)
                    {
                        _globalManager.PacketManager.UseItem(invItem);
                        Thread.Sleep(500);
                    }
                    if (_globalManager.Botstate && ReturntownNoPotion && !_globalManager.LoopManager.IsLooping && GetPotionCount(EPotionType3.Mana) < 20 && ReturnTown("No Mana Potion"))
                        _globalManager.StartLoop(true);
                }

                var pet = _globalManager.PetManager.GetHealPet();
                if (pet != null)
                {
                    var invItem = GetPotion(EPotionType3.RecoveryKit);
                    if (invItem != null)
                        _globalManager.PacketManager.UseItem(invItem, pet.WorldId);
                }

                //Check for Ammo
                if (_globalManager.Botstate && ReturntownNoAmmo && !_globalManager.LoopManager.IsLooping && GetAmmoCount() == 0 && ReturnTown("Out of Ammo"))
                    _globalManager.StartLoop(true);

                //Use Speed Drug
                if (EnableSpeedDrug && !_globalManager.SkillManager.SpeedDrug_is_active())
                {
                    var invItem = GetSpeedDrug();
                    if (invItem != null)
                        _globalManager.PacketManager.UseItem(invItem);
                }
            }
        }

        public bool SetFuseItem(byte slot)
        {
            Game.SlotFuseitem = slot;
            return GetItembySlot(slot) != null;
        }

        public void StartFuse()
        {
            Game.Fusing = true;
            Game.AllowFuse = true;

            if (_fuseThread != null && _fuseThread.IsAlive) _fuseThread.Abort();
            _fuseThread = new Thread(FuseThread);
            _fuseThread.Start();
        }

        private void FuseThread()
        {
            while (Game.Fusing)
            {
                if (Game.AllowFuse)
                {
                    Thread.Sleep(500);
                    var fuseItem = GetItembySlot(Game.SlotFuseitem);
                    if (fuseItem != null)
                    {
                        var elixir = GetElixir(fuseItem.EquipableType2);
                        var powder = GetLuckyPowder(fuseItem.Degree);

                        if (elixir == null || powder == null)
                        {
                            _globalManager.FMain.AddAlchemyEvent(elixir == null
                                ? "Missing Elixir!"
                                : "Missing LuckyPowder!");

                            Game.Fusing = false;
                            break;
                        }

                        if (fuseItem.Plus < 1)
                        {
                            _globalManager.PacketManager.FuseItem(fuseItem.Slot, elixir.Slot, powder.Slot);
                            Game.AllowFuse = false;
                        }
                        else
                        {
                            Game.Fusing = false;
                            Game.AllowFuse = false;
                            _globalManager.FMain.AddAlchemyEvent("Fusing finished!");
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}
