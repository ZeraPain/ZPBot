
using ZPBot.Common.Characters;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Loop
{
    public class Npc
    {
        public uint WorldId;
        public uint NpcId;

        public string CharCode;

        // ReSharper disable once NotAccessedField.Local
        private EPosition _position;

        public Npc(Char chardata)
        {
            NpcId = chardata.Id;
            CharCode = chardata.Code;
        }

        public void SetPosition(EPosition position)
        {
            _position = position;
        }
    }
}
