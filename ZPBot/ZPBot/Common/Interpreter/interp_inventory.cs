using System;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.SilkroadSecurityApi;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private void UpdateInventory(Packet packet)
        {
            try
            {
                var newItem = packet.ReadUInt8();
                var type = packet.ReadUInt8();

                if (newItem == 1)
                {
                    switch (type)
                    {
                        case 0: //Move Item in Inventory
                            {
                                var fromSlot = packet.ReadUInt8();
                                var toSlot = packet.ReadUInt8();
                                var amount = packet.ReadUInt16();
                                packet.SkipBytes(1);
                                InventoryManager.MoveItem(fromSlot, toSlot, amount);

                                Game.AllowStack = true;
                                if (LoopManager.BlockNpcAnswer) Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentLocal);
                            }
                            break;
                        case 1: //Move Item in Storage
                            {
                                var fromSlot = packet.ReadUInt8();
                                var toSlot = packet.ReadUInt8();
                                var amount = packet.ReadUInt16();
                                StorageManager.MoveItem(fromSlot, toSlot, amount);
                            }
                            break;
                        case 2: //Put Item to Storage
                            {
                                var inventorySlot = packet.ReadUInt8();
                                var invItem = InventoryManager.GetItembySlot(inventorySlot);
                                if (invItem != null)
                                {
                                    var storageItem = new InventoryItem(invItem) { Slot = packet.ReadUInt8() };
                                    StorageManager.Add(storageItem);
                                    InventoryManager.Remove(inventorySlot);
                                }
                            }
                            break;
                        case 3: //Get Item from Storage
                            {
                                var storageSlot = packet.ReadUInt8();
                                var storageItem = StorageManager.GetItembySlot(storageSlot);
                                if (storageItem != null)
                                {
                                    var invItem = new InventoryItem(storageItem) { Slot = packet.ReadUInt8() };
                                    InventoryManager.AddOrUpdate(invItem);
                                    StorageManager.Remove(storageSlot);
                                }
                            }
                            break;
                        case 6: //Pickup
                            {
                                PrintPacket("UpdateInventory", packet);
                                var invItem = ReadInventoryItem(ref packet);
                                InventoryManager.AddOrUpdate(invItem);
                            }
                            break;
                        case 7: //Drop item
                            {
                                var slot = packet.ReadUInt8();
                                InventoryManager.Remove(slot);
                            }
                            break;
                        case 8: //Receive From NPC
                            {
                                var tabIndex = packet.ReadUInt8();
                                var slotIndex = packet.ReadUInt8();
                                packet.ReadUInt8(); //receiveInventory
                                var slotInventory = packet.ReadUInt8();
                                var amount = packet.ReadUInt16();
                                packet.SkipBytes(4);

                                var item = Silkroad.GetItemFromShop(NpcManager.GetNpcid(LoopManager.SelectedNpc), tabIndex, slotIndex);
                                if (item != null)
                                {
                                    InventoryManager.AddOrUpdate(new InventoryItem(item, slotInventory, amount));
                                    if (LoopManager.BlockNpcAnswer)
                                    {
                                        PacketManager.FakePickup(item.Id, slotInventory, amount);
                                        LoopManager.AllowBuy = true;
                                    }
                                    //Console.WriteLine("Item From Shop - " + item.item_code + " Tab: " + tab_index + " Slot: " + slot_index);
                                }
                                else
                                {
                                    Console.WriteLine(@"Unknown Item From Shop - Tab: " + tabIndex + @" Slot: " + slotIndex);
                                }
                            }
                            break;
                        case 9: //Sell to NPC
                            {
                                var slot = packet.ReadUInt8();
                                packet.ReadUInt16(); //amount
                                packet.ReadUInt32(); //npcId
                                packet.ReadUInt8(); //rebuyId
                                InventoryManager.Remove(slot);

                                if (LoopManager.BlockNpcAnswer)
                                {
                                    Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentLocal);
                                    LoopManager.AllowSell = true;
                                }
                            }
                            break;
                        case 10:  //Drop Gold
                            {
                                packet.ReadUInt32(); //amount
                                packet.SkipBytes(4);
                            }
                            break;
                        case 15: //Item disappear
                            {
                                var slot = packet.ReadUInt8();
                                packet.ReadUInt8(); //param
                                InventoryManager.Remove(slot);
                            }
                            break;
                        case 14: //Item appears
                            {
                                var invItem = ReadInventoryItem(ref packet, 0);
                                InventoryManager.AddOrUpdate(invItem);
                            }
                            break;
                        case 16: //Move Item in Pet
                            {
                                var worldId = packet.ReadUInt32();
                                var fromSlot = packet.ReadUInt8();
                                var toSlot = packet.ReadUInt8();
                                var amount = packet.ReadUInt16();
                                PetManager.MoveItem(worldId, fromSlot, toSlot, amount);
                            }
                            break;
                        case 17: //Pet Pickup Item
                            {
                                var worldId = packet.ReadUInt32();
                                var petItem = ReadInventoryItem(ref packet);
                                if (petItem != null) PetManager.AddItem(worldId, petItem);
                            }
                            break;
                        case 26: //Get Item from Pet
                            {
                                var worldId = packet.ReadUInt32();
                                var petSlot = packet.ReadUInt8();
                                var petItem = PetManager.GetItembySlot(worldId, petSlot);
                                if (petItem != null)
                                {
                                    var invItem = new InventoryItem(petItem) { Slot = packet.ReadUInt8() };
                                    InventoryManager.AddOrUpdate(invItem);
                                    PetManager.RemoveItem(worldId, petSlot);
                                }
                            }
                            break;
                        case 27: //Put Item into Pet
                            {
                                var worldId = packet.ReadUInt32();
                                var invSlot = packet.ReadUInt8();
                                var invItem = InventoryManager.GetItembySlot(invSlot);
                                if (invItem != null)
                                {
                                    var petItem = new InventoryItem(invItem) { Slot = packet.ReadUInt8() };
                                    PetManager.AddItem(worldId, petItem);
                                    InventoryManager.Remove(invSlot);
                                }
                            }
                            break;
                        case 28: //Pet Pickup Gold
                            {
                                packet.ReadUInt32(); //Pet worldId
                                packet.ReadUInt8(); //slot
                                packet.ReadUInt32(); //amount
                            }
                            break;
                        default:
                            PrintPacket("UpdateInventory", packet);
                            break;
                    }
                }
                else
                {
                    packet.ReadUInt8(); //errorCode 3 = Cannot find target
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"UpdateInventory - " + ex.Message);
                PrintPacket("UpdateInventory", packet);
            }
        }

        private void UpdateInventoryAlchemy([NotNull] Packet packet)
        {
            var slot = packet.ReadUInt8();
            var type = packet.ReadUInt8();
            if (type == 8)
            {
                var quantity = packet.ReadUInt16();
                InventoryManager.UpdateQuantity(slot, quantity);
            }
        }

        private void UpdateInventoryItemCount([NotNull] Packet packet)
        {
            byte newItem = packet.ReadUInt8();
            if (newItem == 1)
            {
                var slot = packet.ReadUInt8();
                var quantity = packet.ReadUInt16();
                packet.ReadUInt8(); //flag1
                packet.ReadUInt8(); //flag2

                InventoryManager.UpdateQuantity(slot, quantity);
            }
        }

        private void UpdateItemStats(Packet packet)
        {
            var newItem = packet.ReadUInt8();
            var type = packet.ReadUInt8();
            if (newItem == 1 && type == 2)
            {
                var success = packet.ReadUInt8();

                var invItem = ReadInventoryItem(ref packet, success);
                if (invItem != null)
                {
                    InventoryManager.AddOrUpdate(invItem);
                    if (invItem.Slot == Game.SlotFuseitem)
                    {
                        switch (success)
                        {
                            case 1:
                                FMain.AddAlchemyEvent("Success to plus [" + invItem.Plus + "]");
                                break;
                            case 0:
                                FMain.AddAlchemyEvent("Failed to plus [" + invItem.Plus + "]");
                                break;
                        }
                        Game.AllowFuse = true;

                        if (invItem.Plus >= 1)
                        {
                            FMain.AddEvent("Reached the requested plus [" + invItem.Plus + "]", "Alchemy");
                            Game.AllowFuse = false;
                            Game.Fusing = false;
                        }
                    }
                }
            }
        }

        private void FullStorageUpdate(Packet packet)
        {
            StorageManager.Clear();

            packet.ReadUInt8(); //maxSlot
            var curSlot = packet.ReadUInt8();

            for (var i = 0; i < curSlot; i++)
            {
                var storageItem = ReadInventoryItem(ref packet);
                if (storageItem != null) StorageManager.Add(storageItem);
            }
        }
    }
}
