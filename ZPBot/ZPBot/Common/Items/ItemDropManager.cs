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
        public List<uint> ItemFilter { get; protected set; }
        private readonly Dictionary<uint, ItemDrop> _itemList;
        public bool PickupMyItems { get; set; }
        public bool IsPicking { get; set; }

        public ItemDropManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            ItemFilter = new List<uint>();
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

        public bool ExistsPickup([NotNull] Item item) => ItemFilter.Contains(item.Id);

        public bool AddPickup(uint itemId)
        {
            if (itemId == 0)
                return false;

            if (ItemFilter.Contains(itemId))
                return false;

            ItemFilter.Add(itemId);
            return true;
        }

        public bool RemovePickup(uint itemId)
        {
            if (!ItemFilter.Contains(itemId))
                return false;

            ItemFilter.Remove(itemId);
            return true;
        }

        public void ClearPickup() => ItemFilter.Clear();

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

                    if ((item.Owner != 0) && (item.Owner != 0xFFFFFFFF) && (item.Owner != _globalManager.Player.AccountId) && (!_globalManager.PartyManager.IsPickableItem(item.Owner)))
                        continue;

                    if ((targetItem == null) || (Game.Distance(sourcePosition, item.GetIngamePosition()) < Game.Distance(sourcePosition, targetItem.GetIngamePosition())))
                        targetItem = item;
                }

                return targetItem;
            }
        }

        protected override void MyThread()
        {
            IsPicking = false;
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
                    IsPicking = false;
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
                    if (_globalManager.Botstate && !_globalManager.LoopManager.IsLooping)
                    {
                        IsPicking = true;
                        _globalManager.PacketManager.Pickup(item.WorldId);
                    }
                }
            }
        }
    }
}
