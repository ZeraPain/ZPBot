using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Items
{
    internal class ItemDropManager : ThreadManager
    {
        private readonly GlobalManager _globalManager;
        private readonly Dictionary<uint, Item> _itemFilter;
        private readonly Dictionary<uint, ItemDrop> _itemList;

        public ItemDropManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            _itemFilter = new Dictionary<uint, Item>();
            _itemList = new Dictionary<uint, ItemDrop>();
        }

        public void Add(ItemDrop item)
        {
            lock (_itemList)
            {
                if (!_itemList.ContainsKey(item.WorldId))
                {
                    _itemList.Add(item.WorldId, item);
                }
            }
        }

        public void Remove(uint worldId)
        {
            lock (_itemList)
            {
                if (_itemList.ContainsKey(worldId))
                {
                    _itemList.Remove(worldId);
                }
            }
        }

        public void Clear()
        {
            lock (_itemList)
            {
                _itemList.Clear();
            }
        }

        public bool ExistsPickup(Item item)
        {
            return _itemFilter.ContainsKey(item.Id);
        }

        public bool AddPickup(Item item)
        {
            if (_itemFilter.ContainsKey(item.Id))
                return false;

            _itemFilter.Add(item.Id, item);
            return true;
        }

        public bool RemovePickup(Item item)
        {
            if (!_itemFilter.ContainsKey(item.Id))
                return false;

            _itemFilter.Remove(item.Id);
            return true;
        }

        public Dictionary<uint, Item> GetPickupList()
        {
            return _itemFilter;
        }

        private double Distance(EGamePosition sourcePosition, EGamePosition itemPosition)
        {
            return Math.Sqrt(Math.Pow(sourcePosition.XPos - itemPosition.XPos, 2) + Math.Pow(sourcePosition.YPos - itemPosition.YPos, 2));
        }

        private ItemDrop GetNextItem(EGamePosition sourcePosition)
        {
            lock (_itemList)
            {
                if (_itemList.Count == 0)
                    return null;

                ItemDrop targetItem = null;
                 
                foreach (var pair in _itemList)
                {
                    var item = pair.Value;

                    if (!ExistsPickup(item))
                        continue;

                    if (Config.PickupMyitems && item.Owner != _globalManager.Player.AccountId)
                        continue;

                    if (item.Owner != 0 && item.Owner != 0xFFFFFFFF && item.Owner != _globalManager.Player.AccountId)
                        continue;

                    if (targetItem == null || (Distance(sourcePosition, item.GetIngamePosition()) < Distance(sourcePosition, targetItem.GetIngamePosition())))
                        targetItem = item;
                }

                return targetItem;
            }
        }

        protected override void MyThread()
        {
            Game.IsPicking = false;
            uint currenItemWorldId = 0;
            var sw = new Stopwatch();

            while (BActive)
            {
                Thread.Sleep(200);

                var sourcePosition = _globalManager.Player.GetIngamePosition();
                var pet = _globalManager.PetManager.GetGrabPet();
                if (pet != null)
                {
                    sourcePosition = pet.GetIngamePosition();
                }

                var item = GetNextItem(sourcePosition);
                if (item == null)
                {
                    Game.IsPicking = false;
                    continue;
                }

                if (currenItemWorldId != item.WorldId)
                {
                    sw.Restart();
                    currenItemWorldId = item.WorldId;
                }

                if (sw.ElapsedMilliseconds > 10000)
                {
                    currenItemWorldId = 0;
                    Remove(item.WorldId);
                }

                if (pet != null)
                {
                    _globalManager.PacketManager.Pickup(item.WorldId, pet.WorldId);
                }
                else
                {
                    if (Config.Botstate && !Game.IsLooping)
                    {
                        Game.IsPicking = true;
                        _globalManager.PacketManager.Pickup(item.WorldId);
                    }
                }
            }
        }
    }
}
