using System;
using System.Windows.Forms;
using ZPBot.Common.Items;
using ZPBot.Common.Loop;
using ZPBot.Common.Characters;
using ZPBot.Common.Resources;
using ZPBot.SilkroadSecurityApi;
using Char = ZPBot.Common.Characters.Char;
using EPetType3 = ZPBot.Common.Characters.EPetType3;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private void MultipleSpawn(Packet packet)
        {
            switch (_spawnType)
            {
                case 1: //Spawn
                    for (var i = 0; i < _spawnAmount; i++)
                    {
                        if (SingleSpawn(packet))
                            continue;

                        Console.WriteLine(@"Failed to parse Spawn Packet:");
                        PrintPacket("SingleSpawn", packet);
                        break;
                    }
                    break;
                case 2: //Despawn
                    for (var i = 0; i < _spawnAmount; i++)
                    {
                        SingleDeSpawn(packet);
                    }
                    break;
            }

            if (packet.RemainingRead() > 0)
                PrintPacket(@"MultipleSpawn", packet);
        }

        private void MultipleSpawnHelper(Packet packet)
        {
            _spawnType = packet.ReadUInt8();
            _spawnAmount = packet.ReadUInt8();
        }

        private void ResetSpawnHelper()
        {
            _spawnType = 0;
            _spawnAmount = 0;
        }

        private bool SingleSpawn(Packet packet)
        {
            var objectId = packet.ReadUInt32();
            if (objectId == 0xFFFFFFFF)
            {
                packet.SkipBytes(2);
                objectId = packet.ReadUInt32();

                var skill = Silkroad.GetSkillById(objectId);
                if (skill != null) return SingleSkillSpawn(packet);
            }

            var itemdata = Silkroad.GetItemById(objectId);
            if (itemdata != null) return SingleSpawnItem(itemdata, packet);

            var chardata = Silkroad.GetCharById(objectId);
            if (chardata != null) return SingleSpawnChar(chardata, packet);

            var teleportdata = Silkroad.GetTeleportById(objectId);
            if (teleportdata != null) return SingleSpawnTeleport(packet);

            Console.WriteLine(@"Unknown ID [" + objectId + @"] at Index: " + (packet.GetBytes().Length - packet.RemainingRead()), @"SingleSpawn");
            return false;
        }

        private void SingleDeSpawn(Packet packet)
        {
            if (packet.RemainingRead() == 0)
                return;

            var worldId = packet.ReadUInt32();
            if (worldId == Game.SelectedMonster)
                Game.SelectedMonster = 0;

            MonsterManager.Remove(worldId);
            ItemDropManager.Remove(worldId);
            NpcManager.Remove(worldId);
            CharManager.Remove(worldId);
        }

        private bool SingleSpawnChar(Char chardata, Packet packet)
        {
            try
            {
                var player = new Character(this, chardata);
                var mob = new Monster(chardata);
                var npc = new Npc(chardata);

                if (chardata.CharType1 == ECharType1.Player)
                {
                    //CHARACTER
                    player.Scale = packet.ReadUInt8();
                    player.HwanLevel = packet.ReadUInt8();
                    player.FreePvp = packet.ReadUInt8();
                    player.AutoInverstExp = packet.ReadUInt8();

                    //Inventory
                    player.InventorySize = packet.ReadUInt8();
                    player.InventoryItemCount = packet.ReadUInt8();
                    for (var i = 0; i < player.InventoryItemCount; i++)
                    {
                        var itemId = packet.ReadUInt32();
                        var item = Silkroad.GetItemById(itemId);
                        if (item.ItemType1 == EItemType1.Equipable)
                        {
                            var optLevel = packet.ReadUInt8();
                            player.ItemList.Add(new InventoryItem(item, 0, 1, optLevel));
                            if (item.JobSuitType3 == EJobSuitType3.Hunter ||
                                item.JobSuitType3 == EJobSuitType3.Trader ||
                                item.JobSuitType3 == EJobSuitType3.Thief)
                                player.UsingJobFlag = true;
                        }
                    }

                    //AvatarInventory
                    packet.ReadUInt8(); // AvatarMaxCount
                    var avatarCount = packet.ReadUInt8();
                    for (var i = 0; i < avatarCount; i++)
                    {
                        var itemId = packet.ReadUInt32();
                        var item = Silkroad.GetItemById(itemId);
                        if (item.ItemType1 == EItemType1.Equipable)
                        {
                            var optLevel = packet.ReadUInt8();
                            player.ItemList.Add(new InventoryItem(item, 0, 1, optLevel));
                        }
                    }

                    //Mask
                    var hasMask = packet.ReadBoolean();
                    if (hasMask)
                    {
                        var maskCharId = packet.ReadUInt32();
                        var maskChar = Silkroad.GetCharById(maskCharId);
                        if (maskChar.CharType1 == chardata.CharType1)
                        {
                            //Duplicate
                            packet.ReadUInt8(); // Scale
                            var maskItemCount = packet.ReadUInt8();
                            for (var i = 0; i < maskItemCount; i++)
                            {
                                packet.ReadUInt32(); // ItemId
                            }
                        }
                    }
                }
                else if (chardata.CharType1 == ECharType1.Npc && chardata.NpcType2 == ENpcType2.Tower)
                {
                    //NPC_FORTRESS_STRUCT
                    packet.ReadUInt32(); // Health
                    packet.ReadUInt32(); // RefEventStructID
                    packet.ReadUInt16(); // State
                }

                var worldId = packet.ReadUInt32();
                var position = GetPosition(ref packet);
                var hasDestination = packet.ReadBoolean();
                packet.ReadUInt8(); // IsWalking

                if (hasDestination)
                {
                    var regionId = packet.ReadUInt16();
                    if (regionId < short.MaxValue)
                    {
                        //World
                        packet.ReadUInt16(); //newXPosition
                        packet.ReadUInt16(); //newZPosition
                        packet.ReadUInt16(); //newYPosition
                    }
                    else
                    {
                        //Dungeon
                        packet.ReadSingle(); //newXPosition
                        packet.ReadSingle(); //newZPosition
                        packet.ReadSingle(); //newYPosition
                    }
                }
                else
                {
                    packet.ReadUInt8(); // Source: 0 = Spinning, 1 = Sky-/Key-walking
                    packet.ReadUInt16(); // New Angle
                }

                packet.ReadUInt8(); // LifeState: 1 = Alive, 2 = Dead
                packet.SkipBytes(1);
                packet.ReadUInt8(); // MotionState: 0 = None, 2 = Walking, 3 = Running, 4 = Sitting
                var status = packet.ReadUInt8();
                // Status: 0 = None, 1 = Hwan, 2 = Untouchable, 3 = GameMasterInvincible, 4 = GameMasterInvisible, 5 = ?, 6 = Stealth, 7 = Invisible

                packet.ReadSingle(); // WalkSpeed
                packet.ReadSingle(); // RunSpeed
                packet.ReadSingle(); // HwanSpeed

                if (chardata.PetType3 != EPetType3.Trade)
                {
                    var buffCount = packet.ReadUInt8();
                    for (var i = 0; i < buffCount; i++)
                    {
                        var skillId = packet.ReadUInt32();
                        packet.ReadUInt32(); // Duration
                        if (Silkroad.GetSkillById(skillId).GroupSkill)
                        {
                            packet.SkipBytes(1); // IsCreator
                        }
                    }
                }

                switch (chardata.CharType1)
                {
                    case ECharType1.Player:
                        //CHARACTER
                        player.Charname = packet.ReadAscii();
                        packet.ReadUInt8(); // JobType:  = None, 1 = Trader, 2 = Tief, 3 = Hunter
                        packet.ReadUInt8(); // JobLevel
                        packet.ReadUInt8(); // PVPState: 0 = White (Neutral), 1 = Purple (Assaulter), 2 = Red
                        var transportFlag = packet.ReadBoolean();
                        packet.ReadUInt8(); // InCombat
                        if (transportFlag)
                        {
                            packet.ReadUInt32(); // Transport WorldId
                        }

                        packet.ReadUInt8(); // ScrollMode: 0 = None, 1 = Return Scroll, 2 = Bandit Return Scroll
                        var interactMode = packet.ReadUInt8();
                        // InterActMode: 0 = None 2 = P2P, 4 = P2N_TALK, 6 = OPNMKT_DEAL
                        packet.SkipBytes(1);

                        packet.ReadAscii(); // Guildname
                        if (!player.UsingJobFlag)
                        {
                            packet.ReadUInt32(); // GuildId
                            packet.ReadAscii(); // Grantname
                            packet.ReadUInt32(); // Guild LastCrestRev
                            packet.ReadUInt32(); // UnionId
                            packet.ReadUInt32(); // Union LastCrestRev
                            packet.ReadBoolean(); // isFriendly
                            packet.ReadUInt8(); // SiegeAutority
                        }

                        if (interactMode == 4)
                        {
                            packet.ReadAscii(); // Stallname
                            packet.ReadUInt32(); // Stalldecoration
                        }

                        packet.ReadUInt8(); // EquipmentCooldown
                        packet.ReadUInt8(); // PKFlag

                        //Add Player
                        player.WorldId = worldId;
                        player.SetPosition(Game.PositionToGamePosition(position));
                        CharManager.Add(player);

                        if (status == 4 || status == 7)
                        {
                            SendMessage("Spotted an invisible player [" + player.Charname + "]", EMessageType.Notice);
                            PacketManager.SetState(worldId, 4, 0);
                        }
                        break;
                    case ECharType1.Npc:
                        //NPC
                        if (chardata.PetType3 != EPetType3.Trade)
                        {
                            var talkFlag = packet.ReadUInt8();
                            if (talkFlag == 2)
                            {
                                var talkOptionCount = packet.ReadUInt8();
                                packet.SkipBytes(talkOptionCount);
                            }
                        }
                            
                        switch (chardata.NpcType2)
                        {
                            case ENpcType2.Monster:
                                //NPC_MOB
                                mob.Type = (EMonsterType) packet.ReadUInt8(); // Rarity
                                /*if(obj.TypeID4 == 2 || obj.TypeID4 == 3)
                                {
                                    1   byte    Appearance  //Randomized by server.
                                }*/
                                mob.WorldId = worldId;
                                mob.Name = chardata.Name;
                                mob.SetPosition(position);
                                MonsterManager.Add(mob);

                                if (chardata.NpcType2 == ENpcType2.Monster && mob.Type == EMonsterType.Unique)
                                {
                                    SendMessage("[" + mob.Name + "] has appeared near you", EMessageType.Notice);
                                }
                                break;
                            case ENpcType2.Pet:
                                //NPC_COS
                                if (chardata.PetType3 != EPetType3.Transport)
                                {
                                    packet.ReadAscii(); //petName
                                    packet.ReadAscii(); //ownerName

                                    switch (chardata.PetType3)
                                    {
                                        case EPetType3.Trade:
                                        case EPetType3.Attack:
                                        case EPetType3.Grab:
                                        case EPetType3.Guild:
                                            packet.ReadUInt8(); // JobType

                                            if (chardata.PetType3 != EPetType3.Grab)
                                            {
                                                packet.ReadUInt8(); // MurderFlag
                                            }

                                            if (chardata.PetType3 == EPetType3.Guild)
                                            {
                                                packet.ReadUInt32(); // RefObjId
                                            }
                                            break;
                                    }

                                    packet.ReadUInt32(); //ownerWorldId
                                }

                                PetManager.UpdatePosition(worldId, position);
                                break;
                            case ENpcType2.Guard:
                                packet.ReadUInt32(); // GuildId
                                packet.ReadAscii(); // Guildname
                                break;
                        }

                        if (chardata.NpcType2 == ENpcType2.Shop)
                        {
                            //Add NPC
                            npc.WorldId = worldId;
                            npc.SetPosition(position);
                            NpcManager.Add(npc);
                        }
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"SingleSpawnChar: " + ex.Message);
                PrintPacket("SingleSpawnChar", packet);
                return false;
            }
        }

        private bool SingleSpawnItem(Item itemdata, Packet packet)
        {
            try
            {
                switch (itemdata.ItemType1)
                {
                    case EItemType1.Equipable:
                        packet.ReadUInt8(); // OptLevel
                        break;
                    case EItemType1.Consumable:
                        if (itemdata.ConsumableType2 == EConsumableType2.Currency &&
                            itemdata.CurrencyType3 == ECurrencyType3.Gold)
                        {
                            packet.ReadUInt32(); //amount
                        }
                        else if (itemdata.ConsumableType2 == EConsumableType2.Quest)
                        {
                            packet.ReadAscii(); //ownerName
                        }
                        break;
                }

                var worldId = packet.ReadUInt32();
                var position = GetPosition(ref packet);
                var hasOwner = packet.ReadBoolean();
                uint owner = 0;

                if (hasOwner)
                {
                    owner = packet.ReadUInt32();
                }

                packet.ReadUInt8(); // Rarity

                ItemDropManager.Add(new ItemDrop(itemdata, position, worldId, owner));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"SingleSpawnItem: " + ex.Message);
                PrintPacket("SingleSpawnItem", packet);
                return false;
            }
        }

        private bool SingleSpawnTeleport(Packet packet)
        {
            try
            {
                packet.ReadUInt32(); // WorldId
                GetPosition(ref packet);

                packet.SkipBytes(1);
                var param1 = packet.ReadUInt8();
                packet.SkipBytes(1);
                var param2 = packet.ReadUInt8();

                if (param2 == 1)
                {
                    //Regular
                    packet.SkipBytes(8);
                }
                else if (param2 == 6)
                {
                    //Dimension Hole
                    packet.ReadAscii(); // Ownername
                    packet.ReadUInt32(); // UniqueId
                }

                if (param1 == 1)
                {
                    //STORE_EVENTZONE_DEFAULT
                    packet.SkipBytes(5);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"SingleSpawnTeleport: " + ex.Message);
                PrintPacket("SingleSpawnTeleport", packet);
                return false;
            }
        }

        private static bool SingleSkillSpawn(Packet packet)
        {
            packet.ReadUInt32(); //worldId
            GetPosition(ref packet);

            return true;
        }
    }
}
