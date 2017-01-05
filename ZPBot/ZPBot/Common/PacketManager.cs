using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;
using ZPBot.SilkroadSecurityApi;

namespace ZPBot.Common
{
    internal class PacketManager
    {
        private readonly GlobalManager _globalManager;

        public PacketManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;
        }

        public void SendLoginRequest(string loginId, string loginPw)
        {
            var packet = new Packet(0x6102, true);
            packet.WriteUInt8(Client.ClientLocale);
            packet.WriteAscii(loginId);
            packet.WriteAscii(loginPw);
            packet.WriteUInt16(Client.ServerId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.GatewayRemote);
        }

        public void RequestStats()
        {
            var packet = new Packet(0x6101, true);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.GatewayRemote);
        }

        public void ImageCode()
        {
            var packet = new Packet(0x6323, false);
            packet.WriteAscii(Client.ImageCode);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void Message(string message, EMessageType type, string charname = "")
        {
            switch (type)
            {
                case EMessageType.Private:
                {
                    var packet = new Packet(0x7025, false);
                    Game.ChatCount[2, 1] += 1;
                    if (Game.ChatCount[2, 1] > 250)
                        Game.ChatCount[2, 1] = 0;

                    packet.WriteUInt8(2);
                    packet.WriteUInt8(Game.ChatCount[2, 1]);
                    packet.WriteAscii(charname);
                    packet.WriteAscii(message);
                    _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
                }
                    break;
                case EMessageType.Party:
                {
                    var packet = new Packet(0x7025, false);
                    Game.ChatCount[4, 1] += 1;
                    if (Game.ChatCount[4, 1] > 250)
                        Game.ChatCount[4, 1] = 0;

                    packet.WriteUInt8(4);
                    packet.WriteUInt8(Game.ChatCount[4, 1]);
                    packet.WriteAscii(message);
                    _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
                }
                    break;
                case EMessageType.Guild:
                {
                    var packet = new Packet(0x7025, false);
                    Game.ChatCount[5, 1] += 1;
                    if (Game.ChatCount[5, 1] > 250)
                        Game.ChatCount[5, 1] = 0;

                    packet.WriteUInt8(5);
                    packet.WriteUInt8(Game.ChatCount[5, 1]);
                    packet.WriteAscii(message);
                    _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
                }
                    break;
                case EMessageType.Notice:
                {
                    var packet = new Packet(0x3026, false);
                    packet.WriteUInt8(7);
                    packet.WriteAscii(message);
                    _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentLocal);
                }
                    break;
            }
        }

        public void BeginTeleport()
        {
            var packet = new Packet(0x34B6, false);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void EndTeleport()
        {
            var packet = new Packet(0x3012, false);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);

            packet = new Packet(0x750E, false);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void AcceptPlayerRequest()
        {
            var packet = new Packet(0x3080, false);
            packet.WriteUInt8(1);
            packet.WriteUInt8(1);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void DenyPlayerRequest()
        {
            var packet = new Packet(0x3080, false);
            packet.WriteUInt8(1);
            packet.WriteUInt8(0);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void Pickup(uint worldId, uint petId)
        {
            var packet = new Packet(0x70C5, false);
            packet.WriteUInt32(petId);
            packet.WriteUInt8(8);
            packet.WriteUInt32(worldId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void Pickup(uint worldId)
        {
            var packet = new Packet(0x7074, false);
            packet.WriteUInt8(1);
            packet.WriteUInt8(2);
            packet.WriteUInt8(1);
            packet.WriteUInt32(worldId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void CastSkill(uint skillId, uint worldId = 0)
        {
            var packet = new Packet(0x7074, false);
            packet.WriteUInt8(1);
            packet.WriteUInt8(4);
            packet.WriteUInt32(skillId);
            if (worldId == 0)
                packet.WriteUInt8(0);
            else
            {
                packet.WriteUInt8(1);
                packet.WriteUInt32(worldId);
            }
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void SelectMonster(uint worldId)
        {
            var packet = new Packet(0x7045, false);
            packet.WriteUInt32(worldId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void MoveToCoords(double xPosGame, double yPosGame)
        {
            var xPos = ((xPosGame % 192 + 192) % 192) * 10;
            var yPos = ((yPosGame % 192 + 192) % 192) * 10;
            var xSec = (byte)((xPosGame - xPos / 10) / 192 + 135);
            var ySec = (byte)((yPosGame - yPos / 10) / 192 + 92);

            var packet = new Packet(0x7021, false);
            packet.WriteUInt8(1);
            packet.WriteUInt8(xSec);
            packet.WriteUInt8(ySec);
            packet.WriteUInt16(xPos);
            packet.WriteUInt16(240);
            packet.WriteUInt16(yPos);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void Teleport(uint source, uint dest)
        {
            var packet = new Packet(0x705A, false);
            packet.WriteUInt32(source);
            packet.WriteUInt8(2);
            packet.WriteUInt32(dest);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void SetState(uint worldId, byte param1, byte param2)
        {
            var packet = new Packet(0x30BF, false);
            packet.WriteUInt32(worldId);
            packet.WriteUInt8(param1);
            packet.WriteUInt8(param2);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentLocal);
        }

        public void TalkToNpc(uint npcId, byte option)
        {
            _globalManager.LoopManager.BlockNpcAnswer = true;

            var packet = new Packet(0x704B, false);
            packet.WriteUInt32(npcId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);

            packet = new Packet(0x7046, false);
            packet.WriteUInt32(npcId);
            packet.WriteUInt8(option);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void CancelNpcTalk(uint npcId)
        {
            _globalManager.LoopManager.BlockNpcAnswer = false;

            var packet = new Packet(0x704B, false);
            packet.WriteUInt32(npcId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void BuyFromNpc(uint npcId, byte tabId, byte slotId, ushort amount)
        {
            var packet = new Packet(0x7034, false);
            packet.WriteUInt8(8);
            packet.WriteUInt8(tabId);
            packet.WriteUInt8(slotId);
            packet.WriteUInt16(amount);
            packet.WriteUInt32(npcId);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void FakePickup(uint itemId, byte slotInventory, ushort amount)
        {
            var packet = new Packet(0xB034, false);
            packet.WriteUInt8(1);
            packet.WriteUInt8(6);
            packet.WriteUInt8(slotInventory);
            packet.WriteUInt32(0);
            packet.WriteUInt32(itemId);
            packet.WriteUInt16(amount);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentLocal);
        }

        public void SellItem(uint npcid, byte slot)
        {
            var packet = new Packet(0x7034, false);
            packet.WriteUInt8(9);
            packet.WriteUInt8(slot);
            packet.WriteUInt16(1);
            packet.WriteUInt32(npcid);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void UseItem([NotNull] InventoryItem invItem, uint worldId = 0)
        {
            var packet = new Packet(0x704C, true);
            packet.WriteUInt8(invItem.Slot);

            if (invItem.CureType3 == ECureType3.Univsersal)
            {
                packet.WriteUInt8(0x6C);
                packet.WriteUInt8(0x31);
            }
            else
            {
                packet.WriteUInt8(0xEC);

                switch (invItem.PotionType3)
                {
                    case EPotionType3.Health:
                        packet.WriteUInt8(8);
                        break;
                    case EPotionType3.Mana:
                        packet.WriteUInt8(16);
                        break;
                    case EPotionType3.RecoveryKit:
                        packet.WriteUInt8(32);
                        packet.WriteUInt32(worldId);
                        break;
                }

                if (invItem.ScrollType3 == EScrollType3.Return)
                    packet.WriteUInt8(9);

                if (invItem.ScrollType == EScrollType.Speed)
                    packet.WriteUInt8(14);
            }

            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void StackItems(byte fromSlot, byte toSlot, ushort amount)
        {
            var packet = new Packet(0x7034, false);
            packet.WriteUInt8(0);
            packet.WriteUInt8(fromSlot);
            packet.WriteUInt8(toSlot);
            packet.WriteUInt16(amount);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void ReturnTown()
        {
            var packet = new Packet(0x3053, false);
            packet.WriteUInt8(1);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void RepairAll(uint worldId)
        {
            var packet = new Packet(0x703E, false);
            packet.WriteUInt32(worldId);
            packet.WriteUInt8(2);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void FuseItem(byte slot, byte elixir, byte powder)
        {
            var packet = new Packet(0x7150, false);
            packet.WriteUInt8(2);
            packet.WriteUInt8(3);
            packet.WriteUInt8(3);
            packet.WriteUInt8(slot);
            packet.WriteUInt8(elixir);
            packet.WriteUInt8(powder);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void UseZerk()
        {
            var packet = new Packet(0x70A7, false);
            packet.WriteUInt8(1);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }

        public void SendPartyInvite(uint worldId, byte partyType)
        {
            var packet = new Packet(0x7060, false);
            packet.WriteUInt32(worldId);
            packet.WriteUInt8(partyType);
            _globalManager.Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
        }
    }
}
