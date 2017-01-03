using System.Collections.Generic;
using System.ComponentModel;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    internal class Player : Char, INotifyPropertyChanged
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
                if (value == _health) return;
                _health = value;
                UpdateHealthPercent();
            }
        }

        private uint _mana;
        public uint Mana
        {
            get { return _mana; }
            set
            {
                if (value == _mana) return;
                _mana = value;
                UpdateManaPercent();
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
                if (value == _charname) return;
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

        private uint _maxHealth;
        public new uint MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                if (value == _maxHealth) return;
                _maxHealth = value;
                UpdateHealthPercent();
            }
        }

        private uint _maxMana;
        public uint MaxMana
        {
            get { return _maxMana; }
            set
            {
                if (value == _maxMana) return;
                _maxMana = value;
                UpdateManaPercent();
            }
        }

        public ushort Strength { get; set; }
        public ushort Intelligence { get; set; }

        public uint AccountId { get; set; }
        public uint WorldId { get; set; }

        public byte CureCount { get; set; }
        public bool UsingJobFlag { get; set; }
        public bool Dead { get; set; }

        public int HealthPercent { get; protected set; }
        private void UpdateHealthPercent()
        {
            var healthPercent = (int)(Health / (float)MaxHealth * 100);
            if (healthPercent < 0) healthPercent = 0;
            if (healthPercent > 100) healthPercent = 100;
            if (healthPercent == HealthPercent) return;
            HealthPercent = healthPercent;
            OnPropertyChanged(nameof(HealthPercent));
        }

        public int ManaPercent { get; protected set; }
        private void UpdateManaPercent()
        {
            var manaPercent = (int)(Mana / (float)MaxMana * 100);
            if (manaPercent < 0) manaPercent = 0;
            if (manaPercent > 100) manaPercent = 100;
            if (manaPercent == ManaPercent) return;
            ManaPercent = manaPercent;
            OnPropertyChanged(nameof(ManaPercent));
        }


        private GamePosition _inGamePosition;
        public GamePosition InGamePosition
        {
            get { return _inGamePosition; }
            set
            {
                if (value == null) return;
                if (value.XPos == _inGamePosition.XPos && value.YPos == _inGamePosition.YPos) return;
                _inGamePosition = value;
                OnPropertyChanged(nameof(InGamePosition));
            }
        }

        private readonly GlobalManager _globalManager;
        public List<InventoryItem> ItemList { get; protected set; }

        public Player(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            ItemList = new List<InventoryItem>();
            _inGamePosition = new GamePosition(0, 0);
            Charname = "<no character>";
            UsingJobFlag = false;
        }

        public override string ToString() => Charname;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var mainForm = _globalManager.FMain;
                if (mainForm == null) return; // No main form - no calls

                if (mainForm.InvokeRequired)
                    mainForm.Invoke(handler, this, new PropertyChangedEventArgs(propertyName));
                else
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
