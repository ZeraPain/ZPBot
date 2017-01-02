using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Items
{
    internal class ItemDropManager : ThreadManager
    {
        private readonly GlobalManager _globalManager;
        public Dictionary<uint, Item> ItemFilter { get; protected set; }
        private readonly Dictionary<uint, ItemDrop> _itemList;
        public bool PickupMyItems { get; set; }

        public ItemDropManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            ItemFilter = new Dictionary<uint, Item>();
            _itemList = new Dictionary<uint, ItemDrop>();
        }

        public void Add([NotNull] ItemDrop item)
        {
            lock (_itemList)
            {
                if (!_itemList.ContainsKey(item.WorldId))
                    _itemList.Add(item.WorldId, item);
            }
        }

        public void Remove(uint worldId)
        {
            lock (_itemList)
            {
                if (_itemList.ContainsKey(worldId))
                    _itemList.Remove(worldId);
            }
        }

        public void Clear()
        {
            lock (_itemList)
            {
                _itemList.Clear();
            }
        }

        public bool ExistsPickup([NotNull] Item item)
        {
            return ItemFilter.ContainsKey(item.Id);
        }

        public bool AddPickup([NotNull] Item item)
        {
            if (ItemFilter.ContainsKey(item.Id))
                return false;

            ItemFilter.Add(item.Id, item);
            return true;
        }

        public bool RemovePickup([NotNull] Item item)
        {
            if (!ItemFilter.ContainsKey(item.Id))
                return false;

            ItemFilter.Remove(item.Id);
            return true;
        }

        [CanBeNull]
        private ItemDrop GetNextItem(GamePosition sourcePosition)
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

                    if (PickupMyItems && (item.Owner != _globalManager.Player.AccountId))
                        continue;

                    if ((item.Owner != 0) && (item.Owner != 0xFFFFFFFF) && (item.Owner != _globalManager.Player.AccountId))
                        continue;

                    if ((targetItem == null) || (Game.Distance(sourcePosition, item.GetIngamePosition()) < Game.Distance(sourcePosition, targetItem.GetIngamePosition())))
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

                var sourcePosition = _globalManager.Player.InGamePosition;
                var pet = _globalManager.PetManager.GetGrabPet();
                if (pet != null)
                    sourcePosition = pet.GetIngamePosition();

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
                    if (_globalManager.Botstate && !Game.IsLooping)
                    {
                        Game.IsPicking = true;
                        _globalManager.PacketManager.Pickup(item.WorldId);
                    }
                }
            }
        }
    }
}
