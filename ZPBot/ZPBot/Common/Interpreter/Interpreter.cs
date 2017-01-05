using System;
using ZPBot.Annotations;
using ZPBot.SilkroadSecurityApi;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private Packet _buffer;

        private string _loginid;
        public string LoginId
        {
            get { return _loginid; }
            set
            {
                _loginid = value;
                OnPropertyChanged(nameof(LoginId));
            }
        }

        private string _loginpw;
        public string LoginPw
        {
            get { return _loginpw; }
            set
            {
                _loginpw = value;
                OnPropertyChanged(nameof(LoginPw));
            }
        }

        private string _loginchar;
        public string LoginChar
        {
            get { return _loginchar; }
            set
            {
                _loginchar = value;
                OnPropertyChanged(nameof(LoginChar));
            }
        }

        private bool _autologin;
        public bool Autologin
        {
            get { return _autologin; }
            set
            {
                _autologin = value;
                OnPropertyChanged(nameof(Autologin));
            }
        }

        private bool _userdata;
        public bool Userdata
        {
            get { return _userdata; }
            set
            {
                _userdata = value;
                OnPropertyChanged(nameof(Userdata));
            }
        }

        public bool ClientToAgentServer(ref Packet packet)
        {
            switch (packet.Opcode)
            {
                case 0x7025:
                    OutgoingMessage(packet);
                    return false;
            }

            return true;
        }

        public bool AgentServerToClient(ref Packet packet)
        {
            try
            {
                if ((packet.Opcode == 0x3018) && (_buffer != null))
                {
                    _buffer.Lock();
                    MultipleSpawn(_buffer);
                }

                switch (packet.Opcode)
                {
                    case 0x2005:
                        if (Clientless && (Client.ServerLoginid != 0))
                        {
                            var newPacket = new Packet(0x6102, true);
                            newPacket.WriteUInt8(Client.ClientLocale);
                            newPacket.WriteAscii(LoginId);
                            newPacket.WriteAscii(LoginPw);
                            newPacket.WriteUInt16(Client.ServerId);
                            Silkroadproxy.Send(newPacket, Proxy.EPacketdestination.GatewayRemote);
                        }
                        break;
                    case 0x2322:
                        if (Autologin && (Client.ImageCode.Length > 0))
                        {
                            PacketManager.ImageCode();
                            return false;
                        }

                        break;
                    case 0x3011:
                        Died(packet);
                        break;
                    case 0x3013:
                        CharUpdate(packet);
                        break;
                    case 0x3015:
                        SingleSpawn(packet);
                        break;
                    case 0x3016:
                        SingleDeSpawn(packet);
                        break;
                    case 0x3017:
                        _buffer = new Packet(0x3019, false);
                        MultipleSpawnHelper(packet);
                        break;
                    case 0x3018:
                        ResetSpawnHelper();
                        break;
                    case 0x3019:
                        _buffer?.WriteUInt8Array(packet.GetBytes());
                        break;
                    case 0x3020:
                        if (Clientless)
                            PacketManager.EndTeleport();

                        break;
                    case 0x3026:
                        IncomingMessage(packet);
                        break;
                    case 0x3065:
                        PartyJoin(packet);
                        break;
                    case 0x3040:
                        UpdateInventoryAlchemy(packet);
                        break;
                    case 0x3049:
                        FullStorageUpdate(packet);
                        break;
                    case 0x3056:
                        ReceiveExp(packet);
                        break;
                    case 0x3057:
                        ChangeHpmp(packet);
                        break;
                    case 0x3080:
                        ReceivePlayerRequest(packet);
                        break;
                    case 0x303D:
                        UpdateStats(packet);
                        break;
                    case 0x304E:
                        UpdateZerk(packet);
                        if (LoopManager.BlockNpcAnswer)
                            return false;

                        break;
                    case 0x30BF:
                        ChangeState(packet);
                        break;
                    case 0x30C8:
                        SummonPet(packet);
                        break;
                    case 0x30C9:
                        UpdatePetStatus(packet);
                        break;
                    case 0x30D0:
                        UpdateSpeed(packet);
                        break;
                    case 0x30D2:
                        CureStatus(packet);
                        break;
                    case 0x3201:
                        DecreaseAmmo(packet);
                        break;
                    case 0x34B5:
                        LoopManager.IsTeleporting = true;
                        if (Clientless)
                            PacketManager.BeginTeleport();

                        break;
                    case 0x3809:
                        //Game.is_teleporting = false;
                        //packet = null;
                        break;
                    case 0x3864:
                        PartyUpdate(packet);
                        break;
                    case 0x7045:
                        Select(packet);
                        break;
                    case 0x705A:
                        Teleport(packet);
                        break;
                    case 0xB007:
                        CharListing(packet);
                        break;
                    case 0xB021:
                        Movement(packet);
                        break;
                    case 0xB025:
                        MessageCheck(packet);
                        break;
                    case 0xB034:
                        UpdateInventory(packet);
                        if (LoopManager.BlockNpcAnswer)
                            return false;

                        break;
                    case 0xB045:
                        SelectSuccess(packet);
                        break;
                    case 0xB046:
                        if (LoopManager.BlockNpcAnswer)
                        {
                            LoopManager.AllowBuy = true;
                            return false;
                        }

                        break;
                    case 0xB04C:
                        UpdateInventoryItemCount(packet);
                        break;
                    case 0xB070:
                        AttackResponse(packet);
                        break;
                    case 0xB072:
                        SkillUnRegister(packet);
                        break;
                    case 0xB074:
                        SkillCast(packet);
                        break;
                    case 0xB0A1:
                        UpdateSkill(packet);
                        break;
                    case 0xB0BD:
                        SkillRegister(packet);
                        break;
                    case 0xB150:
                        UpdateItemStats(packet);
                        break;
                    default:
                        return true;
                }

                return true;

            }
            catch (Exception ex)
            {
                PrintPacket("Interpreter", packet);
                Console.WriteLine(@"Interpreter failed " + ex.Message);
                return true;
            }
        }

        public void ServerStats([NotNull] Packet packet)
        {
            Player.AccountId = 0;

            var newEntry = packet.ReadUInt8();
            while (newEntry == 1)
            {
                packet.ReadUInt8(); //coreId
                packet.ReadAscii(); //coreName
                newEntry = packet.ReadUInt8();
            }

            newEntry = packet.ReadUInt8();
            while (newEntry == 1)
            {
                Client.ServerId = packet.ReadUInt16();
                packet.ReadAscii(); //name
                packet.ReadUInt16(); //current
                packet.ReadUInt16(); //maximum
                packet.ReadUInt8(); //status
                packet.SkipBytes(1);
                newEntry = packet.ReadUInt8();
            }
        }

        private static void PrintPacket(string sender, [NotNull] Packet packet)
        {
            var opcodeC = packet.Opcode.ToString("x").ToUpper();
            var data = packet.GetBytes();
            var sEncrypted = packet.Encrypted ? "[Encrypted]" : "";

            Console.WriteLine(@"[" + sender + @"] [" + opcodeC + @"] [" + data.Length + @" bytes] " + sEncrypted + Environment.NewLine + Utility.HexDump(data));
        }
    }
}
