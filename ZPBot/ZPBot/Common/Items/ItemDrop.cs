
namespace ZPBot.Common.Items
{
    public class ItemDrop : Item
    {
        public uint WorldId;
        public uint Owner;
        public EPosition Position;
        public byte PickCounter;

        public ItemDrop(Item item, EPosition position, uint worldId, uint owner) : base(item)
        {
            Position = position;
            WorldId = worldId;
            Owner = owner;
            PickCounter = 0;
        }

        public EGamePosition GetIngamePosition()
        {
            return new EGamePosition
            {
                XPos = (int) ((Position.XSection - 135) * 192 + (Position.XPosition / 10)),
                YPos = (int) ((Position.YSection - 92) * 192 + (Position.YPosition / 10))
            };
        }
    }
}
