using System.ComponentModel;
using System.Runtime.CompilerServices;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Party
{
    internal class PartyMember : INotifyPropertyChanged
    {
        public uint AccountId { get; set; }
        public string Charname { get; set; }
        public uint Model { get; set; }
        public byte Level { get; set; }
        public byte HpMpInfo { get; set; }
        public GamePosition InGamePosition { get; set; }
        public string Guildname { get; set; }
        public uint SkillTree1 { get; set; }
        public uint SkillTree2 { get; set; }

        private string _additional;
        public string Additional
        {
            get { return _additional; }
            set
            {
                if (_additional == value) return;
                _additional = value;
                OnPropertyChanged(nameof(Additional));

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
