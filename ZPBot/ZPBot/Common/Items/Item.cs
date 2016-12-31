
namespace ZPBot.Common.Items
{
    public class Item
    {
        public uint Id;
        public string Code;
        public string Name;
        public byte Level;
        public byte Degree;
        public bool IsRare;
        public ushort MaxQuantity;

        public ERace Race;
        public EGender Gender;
        public EItemType1 ItemType1 = EItemType1.None;

        public EEquipableType2 EquipableType2 = EEquipableType2.None;
        public ESummonScrollType2 SummonScrollType2 = ESummonScrollType2.None;
        public EConsumableType2 ConsumableType2 = EConsumableType2.None;

        public ECurrencyType3 CurrencyType3 = ECurrencyType3.None;
        public EWeaponType3 WeaponType3 = EWeaponType3.None;
        public EShieldType3 ShieldType3 = EShieldType3.None;
        public EProtectorType3 ProtectorType3 = EProtectorType3.None;
        public EAccessoryType3 AccessoryType3 = EAccessoryType3.None;
        public EJobSuitType3 JobSuitType3 = EJobSuitType3.None;
        public EAvatarType3 AvatarType3 = EAvatarType3.None;
        public EPetType3 PetType3 = EPetType3.None;
        public EElixirType3 ElixirType3 = EElixirType3.None;
        public EPotionType3 PotionType3 = EPotionType3.None;
        public ECureType3 CureType3 = ECureType3.None;
        public EScrollType3 ScrollType3 = EScrollType3.None;
        public EAlchemyType3 AlchemyType3 = EAlchemyType3.None;
        public EReinforceType ReinforceType = EReinforceType.None;
        public EScrollType ScrollType = EScrollType.None;
        public ERareType RareType = ERareType.None;

        public Item()
        {
            
        }

        public Item(Item item)
        {
            Id = item.Id;
            Code = item.Code;
            Name = item.Name;
            Level = item.Level;
            Degree = item.Degree;
            IsRare = item.IsRare;
            MaxQuantity = item.MaxQuantity;

            Race = item.Race;
            Gender = item.Gender;
            ItemType1 = item.ItemType1;

            EquipableType2 = item.EquipableType2;
            SummonScrollType2 = item.SummonScrollType2;
            ConsumableType2 = item.ConsumableType2;

            CurrencyType3 = item.CurrencyType3;
            WeaponType3 = item.WeaponType3;
            ShieldType3 = item.ShieldType3;
            ProtectorType3 = item.ProtectorType3;
            AccessoryType3 = item.AccessoryType3;
            JobSuitType3 = item.JobSuitType3;
            AvatarType3 = item.AvatarType3;
            PetType3 = item.PetType3;
            ElixirType3 = item.ElixirType3;
            PotionType3 = item.PotionType3;
            CureType3 = item.CureType3;
            ScrollType3 = item.ScrollType3;
            AlchemyType3 = item.AlchemyType3;
            ReinforceType = item.ReinforceType;
            ScrollType = item.ScrollType;
            RareType = item.RareType;
        }

        public override string ToString() => Name ?? "Error";
    }

    public enum EItemType1
    {
        None = 0,
        Equipable = 1,
        SummonScroll = 2,
        Consumable = 3
    }

    public enum EEquipableType2
    {
        None = 0,
        CGarment = 1,
        CProtector = 2,
        CArmor = 3,
        Shield = 4,
        CAccessory = 5,
        Weapon = 6,
        JobSuit = 7,
        EGarment = 9,
        EProtector = 10,
        EArmor = 11,
        EAccessory = 12,
        Avatar = 13,
        Spirit = 14
    }

    public enum ESummonScrollType2
    {
        None = 0,
        Pet = 1,
        Skinchange = 2,
        Cube = 3
    }

    public enum EConsumableType2
    {
        None = 0,
        Potion = 1,
        Cure = 2,
        Scroll = 3,
        Ammo = 4,
        Currency = 5,
        Firework = 6,
        Campfire = 7,
        TradeGood = 8,
        Quest = 9,
        Elixir = 10,
        Alchemy = 11,
        Guild = 12,
        CharScroll = 13,
        Card = 14,
        MonsterScroll = 15,
        PetScroll = 16
    }

    public enum ECurrencyType3
    {
        None = -1,
        Gold = 0,
        Coin = 1
    }

    public enum EWeaponType3
    {
        None = 0,
        CSword = 2,
        CBlade = 3,
        CSpear = 4,
        CGlavie = 5,
        CBow = 6,
        ESword = 7,
        EThSword = 8,
        EAxe = 9,
        EDarkStaff = 10,
        EThStaff = 11,
        ECrossbow = 12,
        EDagger = 13,
        EHarp = 14,
        EStaff = 15,
        Fortress = 16
    }

    public enum EShieldType3
    {
        None = 0,
        CShield = 1,
        EShield = 2
    }

    public enum EProtectorType3
    {
        None = 0,
        Head = 1,
        Shoulder = 2,
        Chest = 3,
        Legs = 4,
        Hands = 5,
        Foot = 6
    }

    public enum EAccessoryType3
    {
        None = 0,
        Earring = 1,
        Necklace = 2,
        Ring = 3
    }

    public enum EJobSuitType3
    {
        None = 0,
        Trader = 1,
        Thief = 2,
        Hunter = 3,
        Pvp = 5,
        TraderSpecial = 6,
        HunterSpecial = 7
    }

    public enum EAvatarType3
    {
        None = 0,
        Hat = 1,
        Dress = 2,
        Attach = 3,
        Additional = 4
    }

    public enum EPetType3
    {
        None = 0,
        Attack = 1,
        Grab = 2
    }

    public enum EElixirType3
    {
        None = 0,
        Reinforce = 1,
        LuckyPowder = 2,
        Advanced = 4
    }

    public enum EPotionType3
    {
        None = 0,
        Health = 1,
        Mana = 2,
        Vigor = 3,
        RecoveryKit = 4,
        GrassOfLife = 6,
        Quest = 8,
        Hsp = 9,
        Repair = 10
    }

    public enum EReinforceType
    {
        None = 0,
        Weapon = 100663296,
        Shield = 67108864,
        Protector = 16909056,
        Accessory = 83886080
    }

    public enum ECureType3
    {
        None = 0,
        Purification = 1,
        Univsersal = 6,
        PetUniversal = 7,
        MallUniversal = 8
    }

    public enum EScrollType3
    {
        None = 0,
        Return = 1,
        Transport = 2,
        Reverse = 3,
        StallDecoration = 4,
        Global = 5,
        Fortress = 6,
        Guard = 7,
        Construction = 8,
        FortressFlag = 9,
        Experience = 10,
        FortressUnique = 11,
        SkillPoint = 12,
        Enhancement = 14,
        Spirit = 17
    }

    public enum EScrollType
    {
        None = 0,
        Speed = 1
    }

    public enum EAlchemyType3
    {
        None = 0,
        MagicStone = 1,
        AttributeStone = 2,
        Tablet = 3,
        Material = 4,
        Element = 5,
        Rondo1 = 6,
        MagicStone0 = 7,
        Socket = 8,
        Rondo2 = 9,
        MagicStoneSocket = 10,
        Spirit1 = 14,
        Spirit2 = 15
    }

    public enum ERareType
    {
        Sun = 0,
        Star = 1,
        Moon = 2,
        None = 3
    }
}
