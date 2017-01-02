
using ZPBot.Annotations;

namespace ZPBot.Common.Characters
{
    public class Char
    {
        public uint Id;
        public uint MaxHealth;
        public string Code;
        public string Name;

        public ECharType1 CharType1 = ECharType1.None;
        public ENpcType2 NpcType2 = ENpcType2.None;
        public EPetType3 PetType3 = EPetType3.None;

        public Char()
        {

        }

        public Char([NotNull] Char chardata)
        {
            Id = chardata.Id;
            MaxHealth = chardata.MaxHealth;
            Code = chardata.Code;
            Name = chardata.Name;

            CharType1 = chardata.CharType1;
            NpcType2 = chardata.NpcType2;
            PetType3 = chardata.PetType3;
        }
    }

    public enum ECharType1
    {
        None = 0,
        Player = 1,
        Npc = 2
    }

    public enum ENpcType2
    {
        None = 0,
        Monster = 1,
        Shop = 2,
        Pet = 3,
        Guard = 4,
        Tower = 5
    }

    public enum EPetType3
    {
        None = 0,
        Transport = 1,
        Trade = 2,
        Attack = 3,
        Grab = 4,
        Guild = 5,
        Quest = 6,
        MoonShadow = 7,
        FlameMaster = 8
    }
}
