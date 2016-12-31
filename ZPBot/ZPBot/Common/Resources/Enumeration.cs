
 // ReSharper disable once CheckNamespace
namespace ZPBot
{
    public struct EGamePosition
    {
        public int XPos;
        public int YPos;
    }

    public struct EPosition
    {
        public byte XSection;
        public byte YSection;
        public float XPosition;
        public float ZPosition;
        public float YPosition;
        public ushort Angle;
    }

    public enum EBotState
    {
        None,
        Active,
        Looping
    }

    public enum ECharState
    {
        None,
        Picking,
        Walking,
        Teleporting,
        Fusing
    }

    public enum EObjectType
    {
        Char, 
        Skill, 
        Item, 
        Text, 
        Teleport
    }

    public enum ERace
    {
        Chinese = 0,
        European = 1,
        All = 3
    }

    public enum EGender
    {
        Female = 0,
        Male = 1,
        None = 2
    }

    public enum EMessageType
    {
        Notice, 
        Private,
        Party,
        Guild,
        Global,
        Union
    }

    public enum EMonsterType
    {
        General = 0,
        Champion = 1,
        Unique = 3,
        Giant = 4,
        Elite = 6,
        SubUnique = 8,
        GeneralParty = 16,
        ChampionParty = 17,
        GiantParty = 20
    }

    public enum ESkillType
    {
        Common,
        Attack,
        Buff,
        Imbue
    }
}
