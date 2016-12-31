
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    public class Monster : Char
    {
        public uint WorldId;
        public EMonsterType Type;

        private EPosition _position;

        public Monster(Char chardata) : base(chardata)
        {
        }

        public void SetPosition(EPosition position)
        {
            _position = position;
        }

        [NotNull]
        public GamePosition GetIngamePosition() => new GamePosition((int)((_position.XSection - 135) * 192 + _position.XPosition / 10),
            (int)((_position.YSection - 92) * 192 + _position.YPosition / 10));
    }
}
