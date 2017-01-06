using System;
using System.Collections.Generic;
using ZPBot.Annotations;
using ZPBot.Common.Characters;
using ZPBot.Common.Party;
using ZPBot.Common.Resources;
using ZPBot.SilkroadSecurityApi;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private void Died([NotNull] Packet packet)
        {
            var type = packet.ReadUInt8();
            if (type == 4)
            {
                FMain.AddEvent("You have been died!", "Character");
                Player.Dead = true;
            }
        }

        private void CharUpdate(Packet packet)
        {
            CharUpdate();
            Player.UsingJobFlag = false;
            Player.Dead = false;
            Player.CureCount = 0;

            //Main
            packet.SkipBytes(4); //SROTimeStamp
            Player.RefObjId = packet.ReadUInt32();
            Player.Scale = packet.ReadUInt8();
            Player.Curlevel = packet.ReadUInt8();
            Player.Maxlevel = packet.ReadUInt8();
            Player.ExpOffset = packet.ReadUInt64();
            Player.SExpOffset = packet.ReadUInt32();
            Player.RemainGold = packet.ReadUInt64();
            Player.RemainSkillPoint = packet.ReadUInt32();
            Player.RemainStatPoint = packet.ReadUInt16();
            Player.RemainHwanCount = packet.ReadUInt8();
            Player.GatheredExpPoint = packet.ReadUInt32();
            Player.Health = packet.ReadUInt32();
            Player.Mana = packet.ReadUInt32();
            Player.AutoInverstExp = packet.ReadUInt8();
            Player.DailyPk = packet.ReadUInt8();
            Player.TotalPk = packet.ReadUInt16();
            Player.PkPenaltyPoint = packet.ReadUInt32();
            Player.HwanLevel = packet.ReadUInt8();
            Player.FreePvp = packet.ReadUInt8();
            Player.InventorySize = packet.ReadUInt8();
            Player.InventoryItemCount = packet.ReadUInt8();

            //Items
            for (var i = 0; i < Player.InventoryItemCount; i++)
            {
                var invItem = ReadInvItem(ref packet);
                if (invItem != null) InventoryManager.AddOrUpdate(invItem);
            }

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Items");

            //Avatar
            packet.ReadUInt8(); //maxAvatar
            var avatarCount = packet.ReadUInt8();
            for (var i = 0; i < avatarCount; i++)
            {
                ReadInvItem(ref packet);
            }

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Avatars");

            //EMastery
            packet.SkipBytes(1);
            var newMastery = packet.ReadUInt8();
            while (newMastery == 1)
            {
                packet.ReadUInt32(); //masteryId
                packet.ReadUInt8(); //masteryLvl
                newMastery = packet.ReadUInt8();
            }

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Masteries");

            packet.SkipBytes(1);

            //Skills
            var newSkill = packet.ReadUInt8();
            while (newSkill == 1)
            {
                var skillId = packet.ReadUInt32();
                packet.ReadUInt8(); //skillLvl
                SkillManager.Add(Silkroad.GetSkillById(skillId), ESkillType.Common);
                newSkill = packet.ReadUInt8();
            }

            if (Player.AccountId == 0) FMain.LoadSkillSettings(); // Fire only once!

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Skills");

            //Quests (finished)
            var questAmount = packet.ReadUInt16();
            for (var i = 0; i < questAmount; i++)
            {
                packet.ReadUInt32(); //questId
            }

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Quests (finished)");

            //Quests (alive)
            var activeQuestCount = packet.ReadUInt8();
            for (var activeQuestIndex = 0; activeQuestIndex < activeQuestCount; activeQuestIndex++)
            {
                packet.ReadUInt32(); // RefQuestID
                packet.ReadUInt8(); // AchivementCount
                packet.ReadUInt8(); // RequiresAutoShareParty
                var questType = packet.ReadUInt8();

                if (questType == 28)
                    packet.ReadUInt32(); // remainingTime

                packet.ReadUInt8(); // Status

                if (questType != 8)
                {
                    var objectiveCount = packet.ReadUInt8();
                    for (var objectiveIndex = 0; objectiveIndex < objectiveCount; objectiveIndex++)
                    {
                        packet.ReadUInt8(); // ID
                        packet.ReadUInt8(); // Status: 0 = Done, 1 = On
                        packet.ReadAscii(); // Questname
                        var taskCount = packet.ReadUInt8();
                        for (var taskIndex = 0; taskIndex < taskCount; taskIndex++)
                            packet.ReadUInt32(); // Value
                    }
                }

                if (questType == 88)
                {
                    var refObjCount = packet.ReadUInt8();
                    for (var refObjIndex = 0; refObjIndex < refObjCount; refObjIndex++)
                    {
                        packet.ReadUInt32(); // NPC
                    }
                }
            }

            packet.SkipBytes(1);
            var collectionBookStartedThemeCount = packet.ReadUInt32();
            for (var i = 0; i < collectionBookStartedThemeCount; i++)
            {
                packet.ReadUInt32(); // Index
                packet.ReadUInt32(); // StartedDateTime   
                packet.ReadUInt32(); // Pages
            }

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Quests (available)");

            //Main
            Player.WorldId = packet.ReadUInt32();
            Player.InGamePosition = Game.PositionToGamePosition(GetPosition(ref packet));

            var hasDestination = packet.ReadUInt8();
            packet.ReadUInt8(); // movementType

            if (hasDestination == 1)
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
                packet.ReadUInt8(); //0 = Spinning, 1 = Sky-/Key-walking
                packet.ReadUInt16(); // new angle
            }

            packet.ReadUInt8(); // LifeState: 1 = Alive, 2 = Dead
            packet.SkipBytes(1);
            packet.ReadUInt8(); // MotionState: 0 = None, 2 = Walking, 3 = Running, 4 = Sitting
            packet.ReadUInt8(); // Status: 0 = None, 1 = Hwan, 2 = Untouchable, 3 = GameMasterInvincible, 5 = GameMasterInvisible, 5 = ?, 6 = Stealth, 7 = Invisible

            Player.Walkspeed = packet.ReadSingle();
            Player.Runspeed = packet.ReadSingle();
            packet.ReadSingle(); // HwanSpeed
            var buffCount = packet.ReadUInt8();
            for (var i = 0; i < buffCount; i++)
            {
                var skillId = packet.ReadUInt32();
                packet.ReadUInt32(); // Duration
                var skillById = Silkroad.GetSkillById(skillId);
                if (skillById?.GroupSkill == true)
                    packet.SkipBytes(1); // IsCreator
            }

            Player.Charname = packet.ReadAscii();

            packet.ReadAscii(); // JobName
            packet.ReadUInt8(); // JobType
            packet.ReadUInt8(); // JobLevel
            packet.ReadUInt32(); // JobExp
            packet.ReadUInt32(); // JobContribution
            packet.ReadUInt32(); // JobReward
            packet.ReadUInt8(); // PVPState  0 = White, 1 = Purple, 2 = Red
            var transportFlag = packet.ReadBoolean(); // TransportFlag
            packet.ReadUInt8(); // InCombat
            if (transportFlag)
                packet.ReadUInt32(); // unique Id

            packet.ReadUInt8(); // PVPFlag 0 = Red Side, 1 = Blue Side, 0xFF = None
            packet.ReadUInt64(); // GuideFlag

            Player.AccountId = packet.ReadUInt32();
            packet.ReadUInt8(); // GMFlag
            byte[] patch = {0x01};
            packet.Override(patch);

            packet.ReadUInt8(); //ActivationFlag ConfigType:0 --> (0 = Not activated, 7 = activated)

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Informations (2)");

            var quickbarAmount = packet.ReadUInt8();
            for (var i = 0; i < quickbarAmount; i++)
            {
                packet.ReadUInt8(); // SlotSeq
                packet.ReadUInt8(); // SlotContentType
                packet.ReadUInt32(); // SlotData
            }

            packet.ReadUInt16(); // AutoHPConfig
            packet.ReadUInt16(); // AutoMPConfig
            packet.ReadUInt16(); // AutoUniversalConfig
            packet.ReadUInt8(); // AutoPotionDelay 

            var blockedWhisperCount = packet.ReadUInt8();
            for (var i = 0; i < blockedWhisperCount; i++)
            {
                packet.ReadAscii(); // Blocked Name
            }

            packet.SkipBytes(5); // Junk

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Parsed Quickbar");

            Player.MaxHealth = Player.Health;
            Player.MaxMana = Player.Mana;
            FMain.UpdateCharacter(Player);
            InventoryManager.Start();
            ItemDropManager.Start();
            CharManager.Start();
            PartyManager.Start();

            if (Config.Debug) Console.WriteLine(@"CharUpdate - Completed");
        }

        private void ChangeHpmp([NotNull] Packet packet)
        {
            var worldId = packet.ReadUInt32();
            packet.ReadUInt16(); //damageType
            var flag = packet.ReadUInt8();

            switch (flag)
            {
                case 1:
                    if (worldId == Player.WorldId) Player.Health = packet.ReadUInt32();
                    break;
                case 2:
                    if (worldId == Player.WorldId) Player.Mana = packet.ReadUInt32();
                    break;
                case 3:
                    if (worldId == Player.WorldId)
                    {
                        Player.Health = packet.ReadUInt32();
                        Player.Mana = packet.ReadUInt32();
                    }
                    break;
                case 5:
                    var health = packet.ReadUInt32();
                    PetManager.UpdateHealth(worldId, health);
                    break;
            }
        }

        private void UpdateStats([NotNull] Packet packet)
        {
            Player.MinPhydmg = packet.ReadUInt32();
            Player.MaxPhydmg = packet.ReadUInt32();
            Player.MinMagdmg = packet.ReadUInt32();
            Player.MaxMagdmg = packet.ReadUInt32();
            Player.PhyDef = packet.ReadUInt16();
            Player.MagDef = packet.ReadUInt16();
            Player.HitRate = packet.ReadUInt16();
            Player.ParryRate = packet.ReadUInt16();
            Player.MaxHealth = packet.ReadUInt32();
            Player.MaxMana = packet.ReadUInt32();
            Player.Strength = packet.ReadUInt16();
            Player.Intelligence = packet.ReadUInt16();

            FMain.UpdateCharacter(Player);
        }

        private void SummonPet(Packet packet)
        {
            var worldId = packet.ReadUInt32();
            var charId = packet.ReadUInt32();
            var chardata = Silkroad.GetCharById(charId);
            var health = packet.ReadUInt32();
            packet.SkipBytes(4);
            if (chardata != null)
            {
                PetManager.Add(new Pet(chardata, worldId, health));

                if (chardata.PetType3 != EPetType3.Grab)
                    return;
            }

            packet.SkipBytes(4);
            packet.ReadAscii(); //pet name

            PetManager.ClearInventory(worldId);
            packet.ReadUInt8(); //maxSlot
            var curSlot = packet.ReadUInt8();
            for (var i = 0; i < curSlot; i++)
            {
                var petItem = ReadInvItem(ref packet);
                PetManager.AddItem(worldId, petItem);
            }
        }

        private void UpdatePetStatus([NotNull] Packet packet)
        {
            var worldId = packet.ReadUInt32();
            var status = packet.ReadUInt8();
            if (status == 1) PetManager.Remove(worldId);
        }

        private void UpdateSpeed([NotNull] Packet packet)
        {
            var worldId = packet.ReadUInt32();
            if (worldId != Player.WorldId) return;

            Player.Walkspeed = packet.ReadSingle();
            Player.Runspeed = packet.ReadSingle();
            FMain.UpdateCharacter(Player);
        }

        private void CureStatus([NotNull] Packet packet)
        {
            Player.CureCount = packet.ReadUInt8();
        }

        private void UpdateZerk([NotNull] Packet packet)
        {
            var type = packet.ReadUInt8();
            if (type == 4)
            {
                Player.RemainHwanCount = packet.ReadUInt8();
                packet.ReadUInt32(); //mobId

                FMain.UpdateCharacter(Player);
            }
        }

        private void Movement([NotNull] Packet packet)
        {
            var worldId = packet.ReadUInt32();

            var hasDestination = packet.ReadUInt8();
            if (hasDestination == 1)
            {
                var position = new EPosition
                {
                    XSection = packet.ReadUInt8(),
                    YSection = packet.ReadUInt8(),
                    XPosition = packet.ReadUInt16(),
                    ZPosition = packet.ReadUInt16(),
                    YPosition = packet.ReadUInt16()
                };

                if (worldId == Player.WorldId)
                {
                    if (Clientless)
                    {
                        Player.InGamePosition = Game.PositionToGamePosition(position);
                    }
                }
                else
                {
                    MonsterManager.UpdatePosition(worldId, position);
                    PetManager.UpdatePosition(worldId, position);
                }
            }
            else
            {
                packet.SkipBytes(1);
                packet.ReadUInt16(); //angle
            }

            if (worldId != Player.WorldId) return;

            LoopManager.IsWalking = true;

            var hasSource = packet.ReadUInt8();
            if (hasSource == 1)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new EPosition
                {
                    XSection = packet.ReadUInt8(),
                    YSection = packet.ReadUInt8(),
                    XPosition = packet.ReadUInt16(),
                    ZPosition = packet.ReadSingle(),
                    YPosition = packet.ReadUInt16()
                };
            }
        }

        private void Teleport([NotNull] Packet packet)
        {
            var source = packet.ReadUInt32();
            var type = packet.ReadUInt8();
            if (type == 3)
                packet.SkipBytes(1);
            else
            {
                var dest = packet.ReadUInt32();
                if (LoopManager.RecordLoop)
                    LoopManager?.AddTeleport(source, dest);
            }
        }

        private void ChangeState([NotNull] Packet packet)
        {
            var worldId = packet.ReadUInt32();
            var param1 = packet.ReadUInt8();
            var param2 = packet.ReadUInt8();

            switch (param1)
            {
                case 0:
                    //Set Life State? 2 = Dead
                    switch (param2)
                    {
                        case 2: //Dead
                            MonsterManager.SelectedMonster = 0;
                            MonsterManager.Remove(worldId);
                            break;
                        default:
                            PrintPacket("ChangeState 0", packet);
                            break;
                    }
                    break;
                case 1:
                    switch (param2)
                    {
                        case 0: //Stand up
                            break;
                        case 2: //Speed walk
                            break;
                        case 3: //Speed run
                            break;
                        case 4: //Sit down
                            break;
                        default:
                            PrintPacket("ChangeState 1", packet);
                            break;
                    }
                    break;
                case 4:
                    switch (param2)
                    {
                        case 0: //End spawn (5 sec)
                            break;
                        case 1: //Zerk
                            break;
                        case 2: //Start spawn
                            if (worldId == Player.WorldId)
                                LoopManager.IsTeleporting = false;
                            break;
                        case 7: //Invisible
                            break;
                        default:
                            PrintPacket("ChangeState 4", packet);
                            break;
                    }
                    break;
                case 8:
                    switch (param2)
                    {
                        case 0: //End combat
                            break;
                        case 1: //Start combat
                            break;
                        default:
                            PrintPacket("ChangeState 8", packet);
                            break;
                    }
                    break;
                case 11:
                    if (worldId == Player.WorldId)
                    {
                        switch (param2)
                        {
                            case 0: //End / Cancel teleport
                                LoopManager.StopLoop();
                                break;
                            case 1: //Start teleport
                                LoopManager.IsTeleporting = true;
                                break;
                            default:
                                PrintPacket("ChangeState 11", packet);
                                break;
                        }
                    }
                    break;
                default:
                    PrintPacket("ChangeState", packet);
                    break;
            }
        }

        private static void ReceiveExp([NotNull] Packet packet)
        {
            packet.ReadUInt32(); //worldId
        }

        private void ReceivePlayerRequest([NotNull] Packet packet)
        {
            var type = packet.ReadUInt8();
            switch (type)
            {
                case 1:
                    //exchange
                    break;
                case 2: // create party
                case 3: // join party
                    var worldId = packet.ReadUInt32(); // worldID
                    var partyType = packet.ReadUInt8();
                    PartyManager.PartyRequest(worldId, partyType);
                    break;
                case 4:
                    //ressurect
                    packet.ReadUInt32(); //player
                    PacketManager.AcceptPlayerRequest();
                    break;
            }
        }

        private void SelectSuccess([NotNull] Packet packet)
        {
            var success = packet.ReadUInt8();
            if (success == 1)
            {
                var worldId = packet.ReadUInt32();
                LoopManager.SelectedNpc = worldId;
            }
        }

        private static void Select([NotNull] Packet packet)
        {
            packet.ReadUInt32(); //worldId
        }

        private void AttackResponse([NotNull] Packet packet)
        {
            var success = packet.ReadUInt8();
            if (success == 2)
            {
                var param1 = packet.ReadUInt8();
                var param2 = packet.ReadUInt8();
                if (param1 == 16 && param2 == 48) //Cannot attack due to an obstacle
                {
                    MonsterManager.AttackBlacklist = MonsterManager.SelectedMonster;
                    MonsterManager.SelectedMonster = 0;
                }
            }
        }

        [NotNull]
        private static PartyMember ParsePartyMember(ref Packet packet)
        {
            var partyMember = new PartyMember();

            packet.SkipBytes(1); // 0xFF
            partyMember.AccountId = packet.ReadUInt32(); // accountId
            partyMember.Charname = packet.ReadAscii(); // charname
            partyMember.Model = packet.ReadUInt32(); // model
            partyMember.Level = packet.ReadUInt8(); // level
            partyMember.HpMpInfo = packet.ReadUInt8(); // hp mp info

            var xSection = packet.ReadUInt8();
            var ySection = packet.ReadUInt8();
            if (ySection < 128)
            {
                partyMember.InGamePosition = Game.PositionToGamePosition(new EPosition()
                {
                    XSection = xSection,
                    YSection = ySection,
                    XPosition = packet.ReadUInt16(),
                    ZPosition = packet.ReadUInt16(),
                    YPosition = packet.ReadUInt16()
                });
            }
            else
            {
                partyMember.InGamePosition = Game.PositionToGamePosition(new EPosition()
                {
                    XSection = xSection,
                    YSection = ySection,
                    XPosition = packet.ReadSingle(),
                    ZPosition = packet.ReadSingle(),
                    YPosition = packet.ReadSingle()
                });
            }

            packet.SkipBytes(4);
            partyMember.Guildname = packet.ReadAscii(); // guildname
            packet.SkipBytes(1);
            partyMember.SkillTree1 = packet.ReadUInt32(); // skill tree 1
            partyMember.SkillTree2 = packet.ReadUInt32(); // skill tree 2

            return partyMember;
        }

        private void PartyJoin([NotNull] Packet packet)
        {
            packet.SkipBytes(1); // 0xFF
            var partyId = packet.ReadUInt32();
            var masterId = packet.ReadUInt32(); // master worldID
            var partyType = packet.ReadUInt8(); // partyType
            var memberCount = packet.ReadUInt8();

            var partyMembers = new List<PartyMember>();
            for (var i = 0; i < memberCount; i++)
                partyMembers.Add(ParsePartyMember(ref packet));

            PartyManager.JoinParty(partyId, masterId, partyType, partyMembers);
        }

        private void PartyUpdate([NotNull] Packet packet)
        {
            var type = packet.ReadUInt8();

            uint accountId;
            switch (type)
            {
                case 1: // Dismiss
                    PartyManager.Dismiss();
                    break;
                case 2: // Join
                    var partyMember = ParsePartyMember(ref packet);
                    PartyManager.Join(partyMember);
                    break;
                case 3: // Leave / Kick
                    accountId = packet.ReadUInt32();
                    PartyManager.Leave(accountId);
                    break;
                case 6: // Update
                    accountId = packet.ReadUInt32();
                    var action = packet.ReadUInt8();
                    switch (action)
                    {
                        case 0x4: // HP MP
                            var hpMpInfo = packet.ReadUInt8();
                            PartyManager.Party?.UpdateHpMp(accountId, hpMpInfo);
                            break;
                        case 0x20: // Position
                            var inGamePosition = Game.PositionToGamePosition(new EPosition()
                            {
                                XSection = packet.ReadUInt8(),
                                YSection = packet.ReadUInt8(),
                                XPosition = packet.ReadUInt16(),
                                ZPosition = packet.ReadUInt16(),
                                YPosition = packet.ReadUInt16()
                            });
                            PartyManager.Party?.UpdatePosition(accountId, inGamePosition);
                            break;
                    }
                    break;
                case 9: // New Party Master
                    accountId = packet.ReadUInt32();
                    PartyManager.SetPartyMaster(accountId);
                    break;
            }
        }
    }
}
