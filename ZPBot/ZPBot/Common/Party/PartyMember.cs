using ZPBot.Common.Resources;

namespace ZPBot.Common.Party
{
    internal class PartyMember
    {
        public uint WorldId { get; set; }
        public string Charname { get; set; }
        public uint Model { get; set; }
        public byte Level { get; set; }
        public byte HpMpInfo { get; set; }
        public GamePosition InGamePosition { get; set; }
        public string Guildname { get; set; }
        public uint SkillTree1 { get; set; }
        public uint SkillTree2 { get; set; }
    }
}
