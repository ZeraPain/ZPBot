using System.Collections.Generic;

using ZPBot.Common.Items;

namespace ZPBot.Common.Characters
{
    public class Character : Char
    {
        public uint RefObjId { get; set; }
        public byte Scale { get; set; }
        public byte Curlevel { get; set; }
        public byte Maxlevel { get; set; }
        public ulong ExpOffset { get; set; }
        public uint SExpOffset { get; set; }
        public ulong RemainGold { get; set; }
        public uint RemainSkillPoint { get; set; }
        public ushort RemainStatPoint { get; set; }
        public byte RemainHwanCount { get; set; }
        public uint GatheredExpPoint { get; set; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public byte AutoInverstExp { get; set; }
        public byte DailyPk { get; set; }
        public ushort TotalPk { get; set; }
        public uint PkPenaltyPoint { get; set; }
        public byte HwanLevel { get; set; }
        public byte FreePvp { get; set; }
        public byte InventorySize { get; set; } 
        public byte InventoryItemCount { get; set; }

        public string Charname { get; set; }
        public double Walkspeed { get; set; }
        public double Runspeed { get; set; }

        public uint MinPhydmg { get; set; }
        public uint MaxPhydmg { get; set; }
        public uint MinMagdmg { get; set; }
        public uint MaxMagdmg { get; set; }
        public ushort PhyDef { get; set; }
        public ushort MagDef { get; set; }
        public ushort HitRate { get; set; }
        public ushort ParryRate { get; set; }
        public uint MaxMana { get; set; }
        public ushort Strength { get; set; }
        public ushort Intelligence { get; set; }

        public uint AccountId { get; set; }
        public uint WorldId { get; set; }

        public byte CureCount { get; set; }
        public bool UsingJobFlag { get; set; }
        public bool Dead { get; set; }

        public List<InventoryItem> ItemList { get; protected set; }
        public EPosition Position { get; set; }

        public Character()
        {
            ItemList = new List<InventoryItem>();
            UsingJobFlag = false;
        }

        public Character(Char chardata) : base(chardata)
        {
            ItemList = new List<InventoryItem>();
            UsingJobFlag = false;
        }

        public EGamePosition GetIngamePosition() => new EGamePosition
        {
            XPos = (int) ((Position.XSection - 135)*192 + (Position.XPosition/10)),
            YPos = (int) ((Position.YSection - 92)*192 + (Position.YPosition/10))
        };

        public override string ToString() => Charname;
    }
}
