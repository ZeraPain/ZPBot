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
        private readonly List<InventoryItem> _itemList;
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

            _itemList = new List<InventoryItem>();
            _lock = new object();
        }

        public void Add(InventoryItem item)
        {
            lock (_lock)
            {
                _itemList.Add(item);
                _globalManager.FMain.UpdateInventory(_itemList);
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
                _globalManager.FMain.UpdateInventory(_itemList);
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

        public void Update(InventoryItem item)
        {
            var invItem = GetItembySlot(item.Slot);

            lock (_lock)
            {
                if (invItem == null) // New Item
                {
                    _itemList.Add(item);
                }
                else // Update Item
                {
                    _itemList[_itemList.IndexOf(invItem)].Quantity = item.Quantity;
                    _itemList[_itemList.IndexOf(invItem)].Plus = item.Plus;
                }
                _globalManager.FMain.UpdateInventory(_itemList);
            }
        }

        public void UpdateQuantity(byte slot, ushort quantity)
        {
            var invItem = GetItembySlot(slot);

            if (invItem == null) // New Item
            {
                Console.WriteLine(@"UpdateQuantity - Cannot find Item");
            }
            else // Update Item
            {
                lock (_lock)
                {
                    _itemList[_itemList.IndexOf(invItem)].Quantity = quantity;
                    _globalManager.FMain.UpdateInventory(_itemList);
                }
            }
        }

        public void MoveItem(byte fromSlot, byte toSlot, ushort quantity)
        {
            lock (_lock)
            {
                var invFrom = GetItembySlot(fromSlot);
                var invTo = GetItembySlot(toSlot);

                if (invTo == null)
                {
                    if (invFrom.Quantity == quantity || toSlot < 13 || fromSlot < 13) // Simple Move
                    {
                        invFrom.Slot = toSlot;
                    }
                    else // Split Item
                    {
                        invFrom.Quantity -= quantity;
                        _itemList.Add(new InventoryItem(invFrom, toSlot, quantity));
                    }
                }
                else if (invFrom.Id == invTo.Id) // Stack Items
                {
                    var item = Silkroad.GetItemById(invTo.Id);
                    if (invTo.Quantity + quantity > item.MaxQuantity) // Fill new slot with rest
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
                    var tmpSlot = invFrom.Slot;
                    invFrom.Slot = invTo.Slot;
                    invTo.Slot = tmpSlot;
                }
                _globalManager.FMain.UpdateInventory(_itemList);
            }
        }

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
                    foreach (var item in _itemList.Where(item => item.ReinforceType == elixirtype))
                    {
                        invItem = item;
                    }
                }
            }

            return invItem;
        }

        private InventoryItem GetLuckyPowder(byte degree)
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var item in _itemList.Where(item => item.ElixirType3 == EElixirType3.LuckyPowder && item.Degree == degree))
                {
                    invItem = item;
                }
            }

            return invItem;
        }

        private InventoryItem GetPotion(EPotionType3 potion)
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var item in _itemList.Where(item => item.PotionType3 == potion))
                {
                    invItem = item;
                }
            }

            return invItem;
        }

        private InventoryItem GetUnivsersalPill()
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var item in _itemList.Where(item => item.CureType3 == ECureType3.Univsersal))
                {
                    invItem = item;
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
                foreach (var item in _itemList.Where(item => item.ScrollType3 == EScrollType3.Return))
                {
                    invItem = item;
                }
            }

            return invItem;
        }

        private InventoryItem GetSpeedDrug()
        {
            InventoryItem invItem = null;

            lock (_lock)
            {
                foreach (var item in _itemList.Where(item => item.ScrollType == EScrollType.Speed))
                {
                    invItem = item;
                }
            }

            return invItem;
        }

        public ushort GetItemCount(uint itemId)
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(item => item.Slot > 12 && item.Id == itemId).Aggregate(quantity, (current, item) => (ushort) (current + item.Quantity));
            }

            return quantity;
        }

        private ushort GetAmmoCount()
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(item => item.Slot > 12 && item.ConsumableType2 == EConsumableType2.Ammo).Aggregate(quantity, (current, item) => (ushort)(current + item.Quantity));
            }

            return quantity;
        }

        private ushort GetPotionCount(EPotionType3 potion)
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(item => item.Slot > 12 && item.PotionType3 == potion).Aggregate(quantity, (current, item) => (ushort)(current + item.Quantity));
            }

            return quantity;
        }

        private ushort GetUniversalCount()
        {
            ushort quantity = 0;

            lock (_lock)
            {
                quantity = _itemList.Where(item => item.Slot > 12 && item.CureType3 == ECureType3.Univsersal).Aggregate(quantity, (current, item) => (ushort)(current + item.Quantity));
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
                } while (!Game.IsTeleporting);

                _globalManager.FMain.AddEvent("Returning to town " + reason, "Character");
                return true;
            }

            _globalManager.FMain.AddEvent("Cannot return to town because there are no return scrolls", "Character", true);
            return false;
        }

        public void StackInventoryItems()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var invItem in _itemList)
            {
                if (invItem.Slot <= 12 || invItem.Quantity >= invItem.MaxQuantity) continue;

                var item = invItem;
                foreach (var invItem2 in _itemList.Where(invItem2 => invItem2.Slot > item.Slot && item.Id == invItem2.Id && invItem2.Quantity < invItem2.MaxQuantity))
                {
                    _globalManager.PacketManager.StackItems(invItem2.Slot, invItem.Slot, (ushort) (invItem.MaxQuantity - invItem.Quantity));
                    Game.AllowStack = false;

                    while (!Game.AllowStack)
                    {
                        Thread.Sleep(500);
                        if (!Game.IsLooping)
                            return;
                    }

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
                    if (_globalManager.Botstate && ReturntownNoPotion && !Game.IsLooping && GetUniversalCount() < 5 && ReturnTown("No Universal Pills"))
                        _globalManager.StartLoop(true);
                }

                var healthPercent = _globalManager.Player.Health / (float)_globalManager.Player.MaxHealth * 100;
                if (EnableHp && healthPercent <= HpPercent) //HP Recovery
                {
                    var invItem = GetPotion(EPotionType3.Health);
                    if (invItem != null)
                    {
                        _globalManager.PacketManager.UseItem(invItem);
                        Thread.Sleep(500);
                    }
                    if (_globalManager.Botstate && ReturntownNoPotion && !Game.IsLooping && GetPotionCount(EPotionType3.Health) < 20 && ReturnTown("No Health Potion"))
                        _globalManager.StartLoop(true);
     
                }

                var manaPercent = _globalManager.Player.Mana / (float)_globalManager.Player.MaxMana * 100;
                if (EnableMp && manaPercent <= MpPercent) //MP Recovery
                {
                    var invItem = GetPotion(EPotionType3.Mana);
                    if (invItem != null)
                    {
                        _globalManager.PacketManager.UseItem(invItem);
                        Thread.Sleep(500);
                    }
                    if (_globalManager.Botstate && ReturntownNoPotion && !Game.IsLooping && GetPotionCount(EPotionType3.Mana) < 20 && ReturnTown("No Mana Potion"))
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
                if (_globalManager.Botstate && ReturntownNoAmmo && !Game.IsLooping && GetAmmoCount() == 0 && ReturnTown("Out of Ammo"))
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
