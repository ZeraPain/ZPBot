using System.Collections.Generic;
using System.Linq;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    public class PetManager
    {
        private readonly List<Pet> _petList = new List<Pet>();

        public void Add(Pet pet)
        {
            lock (_petList)
            {
                _petList.Add(pet);
            }
        }

        public void Remove(uint worldId)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    _petList.Remove(pet);
                    break;
                }
            }
        }

        public Pet GetGrabPet()
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.PetType3 == EPetType3.Grab))
                {
                    return pet;
                }
            }

            return null;
        }

        public Pet GetHealPet()
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => (pet.CurHealth / (float)pet.MaxHealth * 100) <= 70))
                {
                    return pet;
                }
            }

            return null;
        }

        public void UpdateHealth(uint worldId, uint health)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    pet.CurHealth = health;
                }
            }
        }

        public void UpdatePosition(uint worldId, EPosition position)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    pet.SetPosition(position);
                    break;
                }
            }
        }

        public void ClearInventory(uint worldId)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    pet.ClearInventory();
                }
            }
        }

        public void AddItem(uint worldId, InventoryItem item)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    pet.AddItem(item);
                }
            }
        }

        public void RemoveItem(uint worldId, byte slot)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    pet.RemoveItem(slot);
                }
            }
        }

        public InventoryItem GetItembySlot(uint worldId, byte slot)
        {
            InventoryItem item = null;

            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    item = pet.GetItembySlot(slot);
                }
            }

            return item;
        }

        public void MoveItem(uint worldId, byte fromSlot, byte toSlot, ushort quantity)
        {
            lock (_petList)
            {
                foreach (var pet in _petList.Where(pet => pet.WorldId == worldId))
                {
                    pet.MoveItem(fromSlot, toSlot, quantity);
                }
            }
        }

    }
}
