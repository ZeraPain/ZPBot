using System.ComponentModel;
using System.Threading;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Skills;
using ZPBot.Common.Loop;
using ZPBot.Common.Characters;
using ZPBot.Common.Party;
using ZPBot.Common.Resources;
using ZPBot.SilkroadSecurityApi;

namespace ZPBot.Common
{
    internal partial class GlobalManager : INotifyPropertyChanged
    {
        public Form1 FMain { get; protected set; }
        public Proxy Silkroadproxy { get; protected set; }
        public PacketManager PacketManager { get; protected set; }
        public ItemDropManager ItemDropManager { get; protected set; }
        public SkillManager SkillManager { get; protected set; }
        public ClientManager ClientManager { get; protected set; }
        public Player Player { get; protected set; }
        public CharManager CharManager { get; protected set; }
        public InventoryManager InventoryManager { get; protected set; }
        public StorageManager StorageManager { get; protected set; }
        public MonsterManager MonsterManager { get; protected set; }
        public NpcManager NpcManager { get; protected set; }
        public LoopManager LoopManager { get; protected set; }
        public PetManager PetManager { get; protected set; }
        public PartyManager PartyManager { get; protected set; }

        private bool _clientless;

        public bool Clientless
        {
            get { return _clientless; }
            set
            {
                _clientless = value;
                OnPropertyChanged(nameof(Clientless));
            }
        }

        public bool ReturntownDied { get; set; }
        public bool Botstate { get; set; }

        private byte _spawnType;
        private byte _spawnAmount;

        private Thread _tSrodata;
        private readonly Thread _tHelper;
        private bool _threadActive;

        public GlobalManager(Form1 fMain)
        {
            FMain = fMain;

            ClientManager = new ClientManager();
            Silkroadproxy = new Proxy(this);
            Player = new Player(this);

            PetManager = new PetManager();
            NpcManager = new NpcManager();
            StorageManager = new StorageManager();
            ItemDropManager = new ItemDropManager(this);
            SkillManager = new SkillManager(this);
            CharManager = new CharManager(this);
            PacketManager = new PacketManager(this);
            InventoryManager = new InventoryManager(this);
            MonsterManager = new MonsterManager(this);
            LoopManager = new LoopManager(this);
            PartyManager = new PartyManager(this);

            StopBot();

            _threadActive = true;
            _tHelper = new Thread(TimerThread);
            _tHelper.Start();
        }

        public void Close()
        {
            _threadActive = false;
            StopBot();
            _tSrodata?.Abort();
            _tHelper.Abort();
            CharManager.Stop();
            PartyManager.Stop();
            InventoryManager.Stop();
            ItemDropManager.Stop();
            Silkroadproxy.Stop();
            ClientManager.Kill();
        }

        public void StartLoop(bool returnTown)
        {
            StopBot();
            LoopManager.StartLoop(returnTown);
        }

        public void Load()
        {
            _tSrodata = new Thread(LoadSrData);
            _tSrodata.Start();
        }

        private void LoadSrData()
        {
            FMain.AddEvent("Loading silkroad data...", "System");
            FMain.FinishLoad(Silkroad.DumpObjects(Config.SroPath + "\\media.pk2"));
        }

        public void StartBot(bool forceArea)
        {
            if (Player.AccountId == 0) return;

            Botstate = true;

            var charPos = Player.InGamePosition;
            var distance = Game.Distance(charPos, MonsterManager.TrainingRange);
            if ((forceArea && (distance > MonsterManager.Range)) || (!forceArea && (distance < 200)))
            {
                MonsterManager.UpdatePosition(charPos);
                distance = Game.Distance(charPos, MonsterManager.TrainingRange);
            }

            if (MonsterManager.Range == 0 || distance <= MonsterManager.Range)
            {
                MonsterManager.Start();
                SkillManager.Start();
            }
            else
            {
                if (!InventoryManager.ReturnTown("Out of Trainingarea")) return;

                SendMessage("Out of Trainingarea - Returning to town", EMessageType.Notice);
                LoopManager.StartLoop(true);
            }
        }

        public void StopBot()
        {
            Botstate = false;

            MonsterManager.Stop();
            SkillManager.Stop();
            LoopManager.StopLoop();
        }

        public void SetFuseState(bool state)
        {
            Game.Fusing = state;
            if (state) InventoryManager.StartFuse();
        }

        public void SendMessage(string message, EMessageType type, string charname = "")
        {
            if (type != EMessageType.Notice)
                Game.ReturnChatcount = false;
            PacketManager.Message(message, type, charname);
        }

        public void CharUpdate()
        {
            ItemDropManager.Clear();
            InventoryManager.Clear();
            MonsterManager.Clear();
            SkillManager.Clear();
            SkillManager.ResetBuffs();
        }

        private void TimerThread()
        {
            while (_threadActive)
            {
                if (Player.WorldId != 0)
                {
                    if (ReturntownDied && (Player.Dead || Player.Health == 0))
                    {
                        PacketManager.ReturnTown();
                        if (Botstate) LoopManager.StartLoop(true);
                    }
                }
                else
                {
                    if (Autologin && Silkroadproxy.AllowLoginRequest)
                    {
                        if (Userdata && !string.IsNullOrEmpty(LoginId) && !string.IsNullOrEmpty(LoginPw))
                        {
                            Silkroadproxy.AllowLoginRequest = false;
                            PacketManager.SendLoginRequest(LoginId, LoginPw);
                        }

                        if (!Userdata && !string.IsNullOrEmpty(Silkroadproxy.XferLoginId) &&
                            !string.IsNullOrEmpty(Silkroadproxy.XferLoginPw))
                        {
                            Silkroadproxy.AllowLoginRequest = false;
                            PacketManager.SendLoginRequest(Silkroadproxy.XferLoginId, Silkroadproxy.XferLoginPw);
                        }
                    }
                }

                Thread.Sleep(2000);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
