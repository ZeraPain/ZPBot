using System;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;
using ZPBot.SilkroadSecurityApi;
using EPetType3 = ZPBot.Common.Items.EPetType3;

// ReSharper disable once CheckNamespace

namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private static InventoryItem ReadInvItem(ref Packet packet)
        {
            var slot = packet.ReadUInt8();
            var rentType = packet.ReadUInt32();

            switch (rentType)
            {
                case 1:
                    packet.ReadUInt16(); // CanDelete
                    packet.ReadUInt32(); // PeriodBeginTime
                    packet.ReadUInt32(); // PeriodEndTime
                    break;
                case 2:
                    packet.ReadUInt16(); // CanDelete
                    packet.ReadUInt16(); // CanRecharge
                    packet.ReadUInt32(); // MeterRateTime
                    break;
                case 3:
                    packet.ReadUInt16(); // CanDelete
                    packet.ReadUInt16(); // CanRecharge
                    packet.ReadUInt32(); // PeriodBeginTime
                    packet.ReadUInt32(); // PeriodEndTime
                    packet.ReadUInt32(); // PackingTime
                    break;
            }

            var itemId = packet.ReadUInt32();
            var item = Silkroad.GetItemById(itemId);

            switch (item.ItemType1)
            {
                case EItemType1.Equipable:
                    var optLevel = packet.ReadUInt8();
                    packet.ReadUInt64(); // Variance
                    packet.ReadUInt32(); // Durability
                    var magParamNum1 = packet.ReadUInt8();
                    for (var paramIndex = 0; paramIndex < magParamNum1; paramIndex++)
                    {
                        packet.ReadUInt32(); // Type
                        packet.ReadUInt32(); // Value
                    }

                    packet.ReadUInt8(); // bindingOptionType: 1 = Socket
                    var bindingOptionCount = packet.ReadUInt8();
                    for (var bindingOptionIndex = 0; bindingOptionIndex < bindingOptionCount; bindingOptionIndex++)
                    {
                        packet.ReadUInt8(); // Slot
                        packet.ReadUInt32(); // ID
                        packet.ReadUInt32(); // nParam1
                    }

                    packet.ReadUInt8(); // bindingOptionType: 2 = Advanced Elixir
                    bindingOptionCount = packet.ReadUInt8();
                    for (var bindingOptionIndex = 0; bindingOptionIndex < bindingOptionCount; bindingOptionIndex++)
                    {
                        packet.ReadUInt8(); // Slot
                        packet.ReadUInt32(); // ID
                        packet.ReadUInt32(); // OptValue
                    }

                    return new InventoryItem(item, slot, 1, optLevel);
                case EItemType1.SummonScroll:
                    switch (item.SummonScrollType2)
                    {
                        case ESummonScrollType2.Pet:
                            var statusFlag = packet.ReadUInt8(); // State
                            if (statusFlag == 2 || statusFlag == 3 || statusFlag == 4)
                            {
                                packet.ReadUInt32(); // RefObjId
                                packet.ReadAscii(); // Name

                                if (item.PetType3 == EPetType3.Grab)
                                {
                                    packet.ReadUInt32(); // SecondsToRentEndTime
                                }

                                packet.SkipBytes(1);
                            }
                            break;
                        case ESummonScrollType2.Skinchange:
                            packet.ReadUInt32(); // RefObjId
                            break;
                        case ESummonScrollType2.Cube:
                            packet.ReadUInt32(); // Quantity
                            break;
                    }

                    return new InventoryItem(item, slot, 1);
                case EItemType1.Consumable:
                    var stackCount = packet.ReadUInt16();
                    switch (item.ConsumableType2)
                    {
                        case EConsumableType2.Alchemy:
                            if (item.AlchemyType3 == EAlchemyType3.MagicStone ||
                                item.AlchemyType3 == EAlchemyType3.AttributeStone)
                            {
                                packet.ReadUInt8(); // AttributeAssimilationProbability
                            }
                            break;
                        case EConsumableType2.Card:
                            var magParamNum2 = packet.ReadUInt8();
                            for (var paramIndex = 0; paramIndex < magParamNum2; paramIndex++)
                            {
                                packet.ReadUInt32(); // Type
                                packet.ReadUInt32(); // Value
                            }
                            break;
                    }

                    return new InventoryItem(item, slot, stackCount);
            }

            return null;
        }


        private static InventoryItem ReadInventoryItem(ref Packet packet, byte success = 1)
        {
            try
            {
                var slot = packet.ReadUInt8();
                if (slot == 254)
                {
                    packet.ReadUInt32(); //goldAmount
                    return null;
                }

                switch (success)
                {
                    case 1:
                        packet.SkipBytes(4);
                        break;
                    case 0:
                        packet.SkipBytes(5);
                        break;
                }

                var itemId = packet.ReadUInt32();
                var item = Silkroad.GetItemById(itemId);
                if (item.ItemType1 == EItemType1.Equipable)
                {
                    var plus = packet.ReadUInt8();
                    packet.SkipBytes(12);

                    var blues = packet.ReadUInt8();
                    for (var k = 0; k < blues; k++)
                    {
                        packet.ReadUInt32(); //blue
                        packet.ReadUInt32(); //amount
                    }

                    packet.SkipBytes(1);
                    var sockets = packet.ReadUInt8();
                    for (var k = 0; k < sockets; k++)
                    {
                        packet.ReadUInt8(); //addSlot
                        packet.ReadUInt32(); //addItemId
                        packet.ReadUInt32(); //addAmount
                    }

                    packet.SkipBytes(1);
                    var additional = packet.ReadUInt8();
                    for (var k = 0; k < additional; k++)
                    {
                        packet.ReadUInt8(); //addSlot
                        packet.ReadUInt32(); //addItemId
                        packet.ReadUInt32(); //addAmount
                    }

                    return new InventoryItem(item, slot, 1, plus);
                }

                if (item.ItemType1 == EItemType1.SummonScroll && item.SummonScrollType2 == ESummonScrollType2.Pet)
                {
                    var statusFlag = packet.ReadUInt8();
                    if (statusFlag == 2 || statusFlag == 3 || statusFlag == 4)
                    {
                        packet.ReadUInt32(); //linkedCharId
                        packet.ReadAscii(); //name
                        packet.SkipBytes(1);

                        if (item.PetType3 == EPetType3.Grab)
                        {
                            packet.SkipBytes(3);
                            var test = packet.ReadUInt8();
                            packet.SkipBytes((uint) (test*14));
                        }
                    }

                    return new InventoryItem(item, slot, 1);
                }

                if (item.ItemType1 == EItemType1.Consumable)
                {
                    var itemAmount = packet.ReadUInt16();
                    if (item.ConsumableType2 == EConsumableType2.Alchemy && (item.AlchemyType3 == EAlchemyType3.MagicStone || item.AlchemyType3 == EAlchemyType3.AttributeStone))
                    {
                        packet.ReadUInt8(); //assimilation
                    }

                    return new InventoryItem(item, slot, itemAmount);
                }

                Console.WriteLine(@"ReadInventoryItem - Unknown Item");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"ReadInventoryItem - " + ex.Message);
                return null;
            }
        }

        private static EPosition GetPosition(ref Packet packet)
        {
            return new EPosition
            {
                XSection = packet.ReadUInt8(),
                YSection = packet.ReadUInt8(),
                XPosition = packet.ReadSingle(),
                ZPosition = packet.ReadSingle(),
                YPosition = packet.ReadSingle(),
                Angle = packet.ReadUInt16()
            };
        }
    }
}