using System.Collections.Generic;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    internal class Character : Char
    {
        public uint RefObjId { get; set; }
        public byte Scale { get; set; }
        public byte AutoInverstExp { get; set; }
        public byte HwanLevel { get; set; }
        public byte FreePvp { get; set; }
        public byte InventorySize { get; set; } 
        public byte InventoryItemCount { get; set; }
        public string Charname { get; set; }
        public uint AccountId { get; set; }
        public uint WorldId { get; set; }
        public bool UsingJobFlag { get; set; }
        public List<InventoryItem> ItemList { get; protected set; }
        public GamePosition InGamePosition { get; set; }

        public Character([NotNull] Char chardata) : base(chardata)
        {
            ItemList = new List<InventoryItem>();
            InGamePosition = new GamePosition(0, 0);
            UsingJobFlag = false;
        }

        public override string ToString() => Charname;
    }
}
