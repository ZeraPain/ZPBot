using System.ComponentModel;
using ZPBot.Annotations;
using ZPBot.Common.Items;

namespace ZPBot.Common.Resources
{
    public class GamePosition
    {
        public int XPos { get; set; }
        public int YPos { get; set; }

        public GamePosition(int xpos, int ypos)
        {
            XPos = xpos;
            YPos = ypos;
        }
    }

    public class LoopOption : INotifyPropertyChanged
    {
        private bool _enabled;
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        private Item _buyType;
        public Item BuyType
        {
            get { return _buyType; }
            set
            {
                _buyType = value;
                OnPropertyChanged(nameof(BuyType));
            }
        }

        private ushort _buyAmount;
        public ushort BuyAmount
        {
            get { return _buyAmount; }
            set
            {
                _buyAmount = value;
                OnPropertyChanged(nameof(BuyAmount));
            }
        }

        public LoopOption()
        {
            _buyType = new Item();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public struct EPosition
    {
        public byte XSection;
        public byte YSection;
        public float XPosition;
        public float ZPosition;
        public float YPosition;
        public ushort Angle;
    }

    public enum EBotState
    {
        None,
        Active,
        Looping
    }

    public enum ECharState
    {
        None,
        Picking,
        Walking,
        Teleporting,
        Fusing
    }

    public enum EObjectType
    {
        Char, 
        Skill, 
        Item, 
        Text, 
        Teleport
    }

    public enum ERace
    {
        Chinese = 0,
        European = 1,
        All = 3
    }

    public enum EGender
    {
        Female = 0,
        Male = 1,
        None = 2
    }

    public enum EMessageType
    {
        Notice, 
        Private,
        Party,
        Guild,
        Global,
        Union
    }

    public enum EMonsterType
    {
        General = 0,
        Champion = 1,
        Unique = 3,
        Giant = 4,
        Elite = 6,
        SubUnique = 8,
        GeneralParty = 16,
        ChampionParty = 17,
        GiantParty = 20
    }

    public enum ESkillType
    {
        Common,
        Attack,
        Buff,
        Imbue
    }
}
