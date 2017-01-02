
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    public class Monster : Char
    {
        public uint WorldId;
        public EMonsterType Type;
        public GamePosition InGamePosition { get; set; }

        public Monster([NotNull] Char chardata) : base(chardata)
        {
        }
    }
}
