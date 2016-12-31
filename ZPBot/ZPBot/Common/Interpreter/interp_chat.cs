using ZPBot.SilkroadSecurityApi;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private void OutgoingMessage(Packet packet)
        {
            var messagetype = packet.ReadUInt8();
            var count = packet.ReadUInt8();

            switch (messagetype)
            {
                case 2:
                    {
                        var charname = packet.ReadAscii();
                        var message = packet.ReadAscii();
                        FMain.AddMessage(charname + "(TO): " + message, EMessageType.Private);
                        Game.ChatCount[messagetype, 0] = count;
                    }
                    break;
                case 4:
                    {
                        packet.ReadAscii(); //message
                        Game.ChatCount[messagetype, 0] = count;
                    }
                    break;
                case 5:
                    {
                        packet.ReadAscii(); //message
                        Game.ChatCount[messagetype, 0] = count;
                    }
                    break;
            }

            if (messagetype == 7)
            {
                var message = packet.ReadAscii();
                var newPacket = new Packet(0x3026, false);
                newPacket.WriteUInt8(7);
                newPacket.WriteAscii(message);
                Silkroadproxy.Send(newPacket, Proxy.EPacketdestination.AgentLocal);
            }
            else
            {
                Silkroadproxy.Send(packet, Proxy.EPacketdestination.AgentRemote);
            }
        }

        private static void MessageCheck(Packet packet)
        {
            var success = packet.ReadUInt8();
            if (success != 1)
                return;

            var messagetype = packet.ReadUInt8();
            var count = packet.ReadUInt8();

            switch (messagetype)
            {
                case 2:
                case 4:
                case 5:
                    Game.ChatCount[messagetype, 1] = count;

                    if (Game.ReturnChatcount)
                    {
                        byte[] patch = { Game.ChatCount[messagetype, 0] };
                        packet.Override(patch);
                    }

                    Game.ReturnChatcount = true;
                    break;
            }
        }

        private void IncomingMessage(Packet packet)
        {
            var messagetype = packet.ReadUInt8();

            switch (messagetype)
            {
                case 2:
                    {
                        var charname = packet.ReadAscii();
                        var message = packet.ReadAscii();

                        FMain.AddEvent(charname + ": " + message, "PM");
                        FMain.AddMessage(charname + "(FROM): " + message, EMessageType.Private);
                    }
                    break;
                case 3:
                    {
                        var charname = packet.ReadAscii();
                        var message = packet.ReadAscii();

                        FMain.AddEvent(charname + ": " + message, "GM");
                        FMain.AddMessage(charname + ": " + message, EMessageType.Notice);
                    }
                    break;
                case 4:
                    {
                        var charname = packet.ReadAscii();
                        var message = packet.ReadAscii();

                        FMain.AddMessage(charname + "(Party): " + message, EMessageType.Party);
                    }
                    break;
                case 5:
                    {
                        var charname = packet.ReadAscii();
                        var message = packet.ReadAscii();

                        FMain.AddMessage(charname + "(Guild): " + message, EMessageType.Guild);
                    }
                    break;
                case 6:
                    {
                        var charname = packet.ReadAscii();
                        var message = packet.ReadAscii();
                        FMain.AddMessage(charname + ": " + message, EMessageType.Global);
                    }
                    break;
                case 7:
                    {
                        var message = packet.ReadAscii();
                        FMain.AddMessage("(Notice): " + message, EMessageType.Notice);
                    }
                    break;
            }
        }
    }
}
