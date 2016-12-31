using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    internal class Character : Char, INotifyPropertyChanged
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

        private uint _health;
        public uint Health
        {
            get { return _health; }
            set
            {
                _health = value;
                OnPropertyChanged(nameof(Health));
            }
        }

        private uint _mana;
        public uint Mana
        {
            get { return _mana; }
            set
            {
                _mana = value;
                OnPropertyChanged(nameof(Mana));
            }
        }

        public byte AutoInverstExp { get; set; }
        public byte DailyPk { get; set; }
        public ushort TotalPk { get; set; }
        public uint PkPenaltyPoint { get; set; }
        public byte HwanLevel { get; set; }
        public byte FreePvp { get; set; }
        public byte InventorySize { get; set; } 
        public byte InventoryItemCount { get; set; }

        private string _charname;
        public string Charname
        {
            get { return _charname; }
            set
            {
                _charname = value;
                OnPropertyChanged(nameof(Charname));
            }
        }

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

        public GamePosition InGamePosition { get; protected set; }
        public void SetPosition(GamePosition position)
        {
            _globalManager.FMain.Invoke((MethodInvoker) delegate
            {
                InGamePosition = position;
                OnPropertyChanged(nameof(InGamePosition));
            });
        }

        private readonly GlobalManager _globalManager;

        public Character(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            ItemList = new List<InventoryItem>();
            InGamePosition = new GamePosition(0, 0);
            UsingJobFlag = false;
        }

        public Character(GlobalManager globalManager, Char chardata) : base(chardata)
        {
            _globalManager = globalManager;
            ItemList = new List<InventoryItem>();
            InGamePosition = new GamePosition(0, 0);
            UsingJobFlag = false;
        }

        public override string ToString() => Charname;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
