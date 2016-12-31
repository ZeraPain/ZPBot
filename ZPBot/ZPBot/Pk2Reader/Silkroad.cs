using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZPBot.Common.Characters;
using ZPBot.Pk2Reader;
using ZPBot.Common.Skills;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;
using Char = ZPBot.Common.Characters.Char;
using EPetType3 = ZPBot.Common.Characters.EPetType3;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    public class Silkroad
    {
        public class Teleport
        {
            public uint Id;
            public string Code;
            public string Name;
        }

        public struct Shop
        {
            public uint NpcId;
            public uint ItemId;
            public byte TabIndex;
            public byte SlotIndex;
        }

        public struct TempShop
        {
            public string Code1;
            public string Code2;
            public byte Slot;
        }

        private static Reader _pk2Reader;
        private const string Level = "\\server_dep\\silkroad\\textdata\\";

        public static Dictionary<string, string> RTextdata { get; protected set; } = new Dictionary<string, string>();
        public static Dictionary<uint, Item> RItemdata { get; protected set; } = new Dictionary<uint, Item>();
        public static Dictionary<uint, Char> RChardata { get; protected set; } = new Dictionary<uint, Char>();
        public static Dictionary<uint, Skill> RSkilldata { get; protected set; } = new Dictionary<uint, Skill>();
        public static Dictionary<uint, Teleport> RTeleportdata { get; protected set; } = new Dictionary<uint, Teleport>();

        private static readonly List<Shop> RShopdata = new List<Shop>();
        private static readonly List<TempShop> Refshopgoods = new List<TempShop>();
        private static readonly List<TempShop> Refscrapofpackageitem = new List<TempShop>();

        public static bool DumpObjects(string mediaPath)
        {
            RChardata.Clear();
            RItemdata.Clear();
            RSkilldata.Clear();
            RTextdata.Clear();
            RTeleportdata.Clear();
            RShopdata.Clear();
            Refshopgoods.Clear();
            Refscrapofpackageitem.Clear();

            _pk2Reader = new Reader(mediaPath);
            if (_pk2Reader.IsLoaded())
            {
                DumpClientVersion("\\SV.T");
                DumpDivisionInfo("\\DIVISIONINFO.TXT");
                DumpGatePort("\\GATEPORT.TXT");
                DumpMultipleFiles(Level + "textdataname.txt", EObjectType.Text);
                DumpMultipleFiles(Level + "characterdata.txt", EObjectType.Char);
                DumpMultipleFiles(Level + "itemdata.txt", EObjectType.Item);
                DumpMultipleFiles(Level + "skilldataenc.txt", EObjectType.Skill);
                DumpTeleportFiles(Level + "teleportbuilding.txt");

                DumpShopFiles();

                return true;
            }

            return false;
        }

        public static void DumpClientVersion(string path)
        {
            var rFile = _pk2Reader.GetFile(path);

            var bReader = new BinaryReader(new MemoryStream(rFile));
            var lenght = (int)bReader.ReadUInt32();
            var encryptedVersion = bReader.ReadBytes(lenght);

            var blowfishKey = System.Text.Encoding.ASCII.GetBytes("SILKROADVERSION");
            var bf = new SilkroadSecurityApi.Blowfish();
            bf.Initialize(blowfishKey, 0, lenght);

            var decryptedVersion = System.Text.Encoding.ASCII.GetString(bf.Decode(encryptedVersion));

            Client.ClientVersion = ushort.Parse(decryptedVersion);
        }

        public static void DumpDivisionInfo(string path)
        {
            var rFile = _pk2Reader.GetFile(path);
            var bReader = new BinaryReader(new MemoryStream(rFile));

            Client.ClientLocale = bReader.ReadByte();
            bReader.ReadByte(); //newDiv
            var divLen = bReader.ReadInt32();
            bReader.ReadChars(divLen); //div
            bReader.ReadByte(); //unknown
            bReader.ReadByte(); //newIp
            var ipLen = bReader.ReadInt32();
            var ip = bReader.ReadChars(ipLen);

            Client.ServerIp = new string(ip);
        }

        public static void DumpGatePort(string path)
        {
            var rFile = _pk2Reader.GetFile(path);
            var mTxtReader = new StreamReader(new MemoryStream(rFile));
            Client.ServerPort = Convert.ToUInt16(mTxtReader.ReadLine());
        }

        public static void DumpMultipleFiles(string path, EObjectType type)
        {
            var rFile = _pk2Reader.GetFile(path);
            var mTxtReader = new StreamReader(new MemoryStream(rFile));

            while (true)
            {
                var mFile = mTxtReader.ReadLine();
                if (mFile == null)
                    break;

                switch (type)
                {
                    case EObjectType.Char:
                        DumpCharFiles(Level + mFile);
                        break;
                    case EObjectType.Item:
                        DumpItemFiles(Level + mFile);
                        break;
                    case EObjectType.Skill:
                        DumpSkillFiles(Level + mFile);
                        break;
                    case EObjectType.Text:
                        DumpNameFiles(Level + mFile);
                        break;
                }
            }
        }

        public static void DumpCharFiles(string path)
        {
            var sFile = _pk2Reader.GetFile(path);
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var line in lines)
            {
                var tmpString = line.Split('\t');
                if (tmpString.Length <= 59 || Convert.ToUInt32(tmpString[0]) != 1)
                    continue;

                var param1 = IntParse(tmpString[10]);
                var param2 = IntParse(tmpString[11]);
                var param3 = IntParse(tmpString[12]);

                var tmp = new Char
                {
                    Id = Convert.ToUInt32(tmpString[1]),
                    Code = tmpString[2],
                    Name = GetName(tmpString[5]),
                    CharType1 = (ECharType1) param1,
                    MaxHealth = UintParse(tmpString[59])
                };

                switch (tmp.CharType1)
                {
                    case ECharType1.Player:
                        break;
                    case ECharType1.Npc:
                        tmp.NpcType2 = (ENpcType2) param2;

                        switch (tmp.NpcType2)
                        {
                            case ENpcType2.Monster:
                                break;
                            case ENpcType2.Shop:
                                break;
                            case ENpcType2.Pet:
                                tmp.PetType3 = (EPetType3) param3;
                                if (!Enum.IsDefined(typeof(EPetType3), param3))
                                    Console.WriteLine(@"Undefinded EPetType3: " + param3 + @" [" + tmp.Code + @"]");
                                break;
                            case ENpcType2.Guard:
                                break;
                            case ENpcType2.Tower:
                                break;
                            default:
                                Console.WriteLine(@"Undefinded ENpcType2: " + param2 + @" [" + tmp.Id + @"]");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine(@"Undefinded ECharType1: " + param1 + @" [" + tmp.Id + @"]");
                        break;
                }

                RChardata.Add(tmp.Id, tmp);
            }
        }

        private static void DumpItemFiles(string path)
        {
            var sFile = _pk2Reader.GetFile(path);
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var line in lines)
            {
                var tmpString = line.Split('\t');
                if (tmpString.Length <= 118 || Convert.ToUInt32(tmpString[0]) != 1)
                    continue;

                var param1 = IntParse(tmpString[10]);
                var param2 = IntParse(tmpString[11]);
                var param3 = IntParse(tmpString[12]);

                var tmp = new Item
                {
                    Id = UintParse(tmpString[1]),
                    Code = tmpString[2],
                    Name = GetName(tmpString[5]),
                    ItemType1 = (EItemType1) param1,
                    Race = (ERace) ByteParse(tmpString[14]),
                    IsRare = ByteParse(tmpString[15]) == 2,
                    Level = ByteParse(tmpString[33]),
                    MaxQuantity = UshortParse(tmpString[57]),
                    Gender = (EGender) ByteParse(tmpString[58]),
                    Degree = ByteParse(tmpString[61])
                };

                switch (tmp.ItemType1)
                {
                    case EItemType1.Equipable:
                        tmp.RareType = tmp.IsRare ? (ERareType) (tmp.Degree % 3) : ERareType.None;
                        tmp.Degree = (byte)((tmp.Degree + 2) / 3);
                        tmp.EquipableType2 = (EEquipableType2) param2;

                        switch (tmp.RareType)
                        {
                            case ERareType.Sun:
                                tmp.Name += " (Sun)";
                                break;
                            case ERareType.Star:
                                tmp.Name += " (Star)";
                                break;
                            case ERareType.Moon:
                                tmp.Name += " (Moon)";
                                break;
                        }

                        switch (tmp.EquipableType2)
                        {
                            case EEquipableType2.CGarment:
                            case EEquipableType2.CProtector:
                            case EEquipableType2.CArmor:
                            case EEquipableType2.EGarment:
                            case EEquipableType2.EProtector:
                            case EEquipableType2.EArmor:
                                tmp.ProtectorType3 = (EProtectorType3) param3;
                                if (!Enum.IsDefined(typeof(EProtectorType3), param3))
                                    Console.WriteLine(@"Undefinded EProtectorType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EEquipableType2.Shield:
                                tmp.ShieldType3 = (EShieldType3) param3;
                                if (!Enum.IsDefined(typeof(EShieldType3), param3))
                                    Console.WriteLine(@"Undefinded EShieldType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EEquipableType2.CAccessory:
                            case EEquipableType2.EAccessory:
                                tmp.AccessoryType3 = (EAccessoryType3) param3;
                                if (!Enum.IsDefined(typeof(EAccessoryType3), param3))
                                    Console.WriteLine(@"Undefinded EAccessoryType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EEquipableType2.Weapon:
                                tmp.WeaponType3 = (EWeaponType3) param3;
                                if (!Enum.IsDefined(typeof(EWeaponType3), param3))
                                    Console.WriteLine(@"Undefinded EWeaponType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EEquipableType2.JobSuit:
                                tmp.JobSuitType3 = (EJobSuitType3) param3;
                                if (!Enum.IsDefined(typeof(EJobSuitType3), param3))
                                    Console.WriteLine(@"Undefinded EJobSuitType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EEquipableType2.Avatar:
                                tmp.AvatarType3 = (EAvatarType3) param3;
                                if (!Enum.IsDefined(typeof(EAvatarType3), param3))
                                    Console.WriteLine(@"Undefinded EAvatarType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EEquipableType2.Spirit:
                                break;
                            default:
                                Console.WriteLine(@"Undefinded EEquipableType2: " + param2 + @" [" + tmp.Id + @"]");
                                break;
                        }
                        break;
                    case EItemType1.SummonScroll:
                        tmp.SummonScrollType2 = (ESummonScrollType2) param2;

                        switch (tmp.SummonScrollType2)
                        {
                            case ESummonScrollType2.Pet:
                                tmp.PetType3 = (Items.EPetType3) param3;
                                if (!Enum.IsDefined(typeof(Items.EPetType3), param3))
                                    Console.WriteLine(@"Undefinded EPetType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case ESummonScrollType2.Skinchange:
                                break;
                            case ESummonScrollType2.Cube:
                                break;
                            default:
                                Console.WriteLine(@"Undefinded ESummonScrollType2: " + param2 + @" [" + tmp.Id + @"]");
                                break;
                        }
                        break;
                    case EItemType1.Consumable:
                        tmp.ConsumableType2 = (EConsumableType2) param2;

                        switch (tmp.ConsumableType2)
                        {
                            case EConsumableType2.None:
                                break;
                            case EConsumableType2.Potion:
                                tmp.PotionType3 = (EPotionType3) param3;
                                if (!Enum.IsDefined(typeof(EPotionType3), param3))
                                    Console.WriteLine(@"Undefinded EPotionType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EConsumableType2.Cure:
                                tmp.CureType3 = (ECureType3) param3;
                                if (!Enum.IsDefined(typeof(ECureType3), param3))
                                    Console.WriteLine(@"Undefinded ECureType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EConsumableType2.Scroll:
                                tmp.ScrollType3 = (EScrollType3) param3;
                                if (!Enum.IsDefined(typeof(EScrollType3), param3))
                                    Console.WriteLine(@"Undefinded EScrollType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EConsumableType2.Ammo:
                                break;
                            case EConsumableType2.Currency:
                                tmp.CurrencyType3 = (ECurrencyType3) param3;
                                if (!Enum.IsDefined(typeof(ECurrencyType3), param3))
                                    Console.WriteLine(@"Undefinded ECurrencyType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EConsumableType2.Firework:
                                break;
                            case EConsumableType2.Campfire:
                                break;
                            case EConsumableType2.TradeGood:
                                break;
                            case EConsumableType2.Quest:
                                break;
                            case EConsumableType2.Elixir:
                                tmp.ElixirType3 = (EElixirType3) param3;
                                if (tmp.ElixirType3 == EElixirType3.Reinforce)
                                    tmp.ReinforceType = (EReinforceType) IntParse(tmpString[118]);

                                if (!Enum.IsDefined(typeof(EElixirType3), param3))
                                    Console.WriteLine(@"Undefinded EElixirType3: " + param3 + @" [" + tmp.Id + @"]");
                                break;
                            case EConsumableType2.Alchemy:
                                tmp.AlchemyType3 = (EAlchemyType3) param3;
                                if (!Enum.IsDefined(typeof(EAlchemyType3), param3))
                                    Console.WriteLine(@"Undefinded EAlchemyType3: " + param3 + @" [" + tmp.Code + @"]");
                                break;
                            case EConsumableType2.Guild:
                                break;
                            case EConsumableType2.CharScroll:
                                if (tmp.Code.StartsWith("ITEM_ETC_ARCHEMY_POTION_SPEED"))
                                    tmp.ScrollType = EScrollType.Speed;
                                break;
                            case EConsumableType2.Card:
                                break;
                            case EConsumableType2.MonsterScroll:
                                break;
                            case EConsumableType2.PetScroll:
                                break;
                            default:
                                Console.WriteLine(@"Undefinded EConsumableType2: " + param2 + @" [" + tmp.Id + @"]");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine(@"Undefinded EItemType1: " + param1 + @" [" + tmp.Id + @"]");
                        break;
                }

                if (RItemdata.ContainsKey(tmp.Id))
                    RItemdata[tmp.Id] = tmp;
                else
                    RItemdata.Add(tmp.Id, tmp);
            }
        }

        private static void DumpSkillFiles(string path)
        {
            var sFile = SRODecrypt.Decrypt(_pk2Reader.GetFile(path));
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var line in lines)
            {
                var tmpString = line.Split('\t');
                if (tmpString.Length <= 72 || Convert.ToUInt32(tmpString[0]) != 1)
                    continue;

                var tmp = new Skill
                {
                    Id = UintParse(tmpString[1]),
                    Code = tmpString[3],
                    MasteryTopColumn = ByteParse(tmpString[57]),
                    MasteryLowColumn = ByteParse(tmpString[58]),
                    Row = ByteParse(tmpString[59]),
                    Column = ByteParse(tmpString[60]),
                    Name = GetName(tmpString[62]),
                    GroupSkill = IntParse(tmpString[69]) == 1701213281,
                    SpeedDrugSkill = IntParse(tmpString[72]) == 1752396901
                };

                RSkilldata.Add(tmp.Id, tmp);
            }
        }

        private static void DumpNameFiles(string path)
        {
            var sFile = _pk2Reader.GetFile(path);
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var line in lines)
            {
                var tmpString = line.Split('\t');
                if (tmpString.Length <= Client.Language)
                    continue;

                if (tmpString[1].Length == 0 || tmpString[Client.Language].Length == 0 || RTextdata.ContainsKey(tmpString[1]))
                    continue;

                RTextdata.Add(tmpString[1], tmpString[Client.Language]);
            }
        }

        private static void DumpTeleportFiles(string path)
        {
            var sFile = _pk2Reader.GetFile(path);
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var line in lines)
            {
                if (line == "") continue;

                var tmpString = line.Split('\t');
                if (tmpString.Length <= 12) continue;

                var tmp = new Teleport
                {
                    Id = Convert.ToUInt32(tmpString[1]),
                    Code = tmpString[2],
                    Name = tmpString[5]
                };

                RTeleportdata.Add(tmp.Id, tmp);
            }
        }

        private static void DumpShopFiles()
        {
            CreateTempShop("refshopgoods.txt", Refshopgoods);
            CreateTempShop("refscrapofpackageitem.txt", Refscrapofpackageitem);

            var sFile = _pk2Reader.GetFile(Level + "refshopgroup.txt");
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var line in lines)
            {
                if (line == "") continue;

                var tmpString = line.Split('\t');
                if (tmpString.Length <= 4 || ByteParse(tmpString[0]) != 1)
                    continue;

                var npc = GetCharDataByCode(tmpString[4]);
                if (npc != null && npc.Id > 0)
                    GroupToStore(tmpString[3], npc.Id);
            }
        }

        private static void CreateTempShop(string path, List<TempShop> temp)
        {
            var sFile = _pk2Reader.GetFile(Level + path);
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            temp.AddRange(from line in lines
                where line != ""
                select line.Split('\t')
                into tmpString
                where tmpString.Length > 4 && ByteParse(tmpString[0]) == 1
                select new TempShop
                {
                    Code1 = tmpString[2], Code2 = tmpString[3], Slot = ByteParse(tmpString[4])
                });
        }

        private static void GroupToStore(string group, uint npcId)
        {
            var sFile = _pk2Reader.GetFile(Level + "refmappingshopgroup.txt");
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var tmpString in lines.Where(line => line != "").Select(line => line.Split('\t')).Where(tmpString => tmpString.Length > 3 && ByteParse(tmpString[0]) == 1 && @group.StartsWith(tmpString[2])))
            {
                StoreToStoreGroup(tmpString[3], npcId);
            }
        }

        private static void StoreToStoreGroup(string store, uint npcId)
        {
            byte tabIndex = 0;

            var sFile = _pk2Reader.GetFile(Level + "refmappingshopwithtab.txt");
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var tmpString in lines.Where(line => line != "").Select(line => line.Split('\t')).Where(tmpString => ByteParse(tmpString[0]) == 1 && store.StartsWith(tmpString[2])))
            {
                StoreGroupToTab(tmpString[3], npcId, tabIndex);
                tabIndex += 3;
            }
        }

        private static void StoreGroupToTab(string storegroup, uint npcId, byte tabIndex)
        {
            var sFile = _pk2Reader.GetFile(Level + "refshoptab.txt");
            var sTxtReader = new StreamReader(new MemoryStream(sFile));

            var file = sTxtReader.ReadToEnd();
            var lines = file.Split('\n');

            foreach (var tmpString in lines.Where(line => line != "").Select(line => line.Split('\t')).Where(tmpString => ByteParse(tmpString[0]) == 1 && storegroup.StartsWith(tmpString[4])))
            {
                GetPackageByTab(tmpString[3], npcId, tabIndex);
                tabIndex++;
            }
        }

        private static void GetPackageByTab(string tab, uint npcId, byte tabIndex)
        {
            foreach (var tmp in from shopGood in Refshopgoods
                where shopGood.Code1 == tab
                let itemCode = GetItemByPackage(shopGood.Code2)
                where itemCode != null
                select new Shop
                {
                    NpcId = npcId, TabIndex = tabIndex, SlotIndex = shopGood.Slot, ItemId = GetItemByCode(itemCode).Id
                })
            {
                RShopdata.Add(tmp);
            }
        }

        private static string GetItemByPackage(string package) => Refscrapofpackageitem.Find(temp => temp.Code1 == package).Code2;
        private static string GetName(string itemCode) => RTextdata.ContainsKey(itemCode) ? RTextdata[itemCode] : "Error";

        public static Item GetItemFromShop(uint npcId, byte tabIndex, byte slotIndex)
        {
            var itemId = RShopdata.Find(temp => temp.NpcId == npcId && temp.TabIndex == tabIndex && temp.SlotIndex == slotIndex).ItemId;

            return RItemdata.ContainsKey(itemId) ? RItemdata[itemId] : null;
        }

        public static List<Item> GetLoopData()
        {
            var loopdata = new List<Item>();

            foreach (var item in RShopdata.Select(shop => GetItemById(shop.ItemId)).Where(item => item.PotionType3 == EPotionType3.Health || item.PotionType3 == EPotionType3.Mana || item.CureType3 == ECureType3.Univsersal || item.ConsumableType2 == EConsumableType2.Ammo || item.ScrollType == EScrollType.Speed || item.ScrollType3 == EScrollType3.Return).Where(item => loopdata.Find(delegate(Item temp) { return temp.Id == item.Id; }) == null))
            {
                loopdata.Add(item);
            }

            return loopdata;
        }

        public static Char GetCharById(uint searchValue) => RChardata.ContainsKey(searchValue) ? RChardata[searchValue] : null;
        private static Char GetCharDataByCode(string searchValue) => (from search in RChardata where search.Value.Code == searchValue select search.Value).FirstOrDefault();
        public static Item GetItemById(uint searchValue) => RItemdata.ContainsKey(searchValue) ? RItemdata[searchValue] : null;
        private static Item GetItemByCode(string searchValue) => (from search in RItemdata where search.Value.Code == searchValue select search.Value).FirstOrDefault();
        public static Teleport GetTeleportById(uint searchValue) => RTeleportdata.ContainsKey(searchValue) ? RTeleportdata[searchValue] : null;
        public static Skill GetSkillById(uint searchValue) => RSkilldata.ContainsKey(searchValue) ? RSkilldata[searchValue] : null;
        public static List<Shop> GetShopData(uint npcId) => RShopdata.Where(shopData => shopData.NpcId == npcId).ToList();

        private static int IntParse(string input)
        {
            int number;
            return int.TryParse(input, out number) ? number : 0;
        }

        private static uint UintParse(string input)
        {
            uint number;
            return uint.TryParse(input, out number) ? number : 0;
        }

        private static ushort UshortParse(string input)
        {
            ushort number;
            return ushort.TryParse(input, out number) ? number : (ushort) 0;
        }

        private static byte ByteParse(string input)
        {
            byte number;
            return byte.TryParse(input, out number) ? number : (byte) 0;
        }
    }
}
