
namespace ZPBot.Common.Items
{
    public class InventoryItem : Item
    {
        public byte Slot;
        public uint RentType;


        public ushort Quantity;
        public byte Plus;

        public InventoryItem(Item item, byte slot, ushort quantity, byte plus = 0) : base(item)
        {
            Slot = slot;
            Quantity = quantity;
            Plus = plus;
        }

        public InventoryItem(InventoryItem item) : base(item)
        {
            Slot = item.Slot;
            Quantity = item.Quantity;
            Plus = item.Plus;
        }
    }
}
