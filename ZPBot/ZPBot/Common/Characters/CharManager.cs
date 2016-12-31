using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    internal class CharManager : ThreadManager
    {
        public BindingList<Character> PlayerList { get; protected set; }
        private readonly GlobalManager _globalManager;

        public CharManager(GlobalManager globalManager)
        {
            PlayerList = new BindingList<Character>();
            _globalManager = globalManager;
        }

        public void Add(Character character)
        {
            lock (PlayerList)
            {
                _globalManager.FMain.Invoke((MethodInvoker) (() => PlayerList.Add(character)));
            }
        }

        public void Remove(uint worldId)
        {
            lock (PlayerList)
            {
                foreach (var player in PlayerList.Where(player => player.WorldId == worldId))
                {
                    _globalManager.FMain.Invoke((MethodInvoker)(() => PlayerList.Remove(player)));
                    break;
                }
            }
        }

        protected override void MyThread()
        {
            while (BActive)
            {
                if (!Game.Clientless && _globalManager.Player.AccountId != 0)
                {
                    var position = new EPosition
                    {
                        XSection = _globalManager.ClientManager.ReadByte(0xEEF68C),
                        YSection = _globalManager.ClientManager.ReadByte(0xEEF68D),
                        XPosition = _globalManager.ClientManager.ReadSingle(0xEED1CC),
                        YPosition = _globalManager.ClientManager.ReadSingle(0xEED1D4)
                    };
                    _globalManager.Player.Position = position;
                    _globalManager.FMain.UpdateCharacter(_globalManager.Player);
                }

                Thread.Sleep(50);
            }
        }
    }
}
