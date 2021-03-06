﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using ZPBot.Annotations;
using ZPBot.Common;

namespace ZPBot.SilkroadSecurityApi
{
    internal class Proxy
    {
        public enum EPacketdestination
        {
            GatewayRemote,
            GatewayLocal,
            AgentRemote,
            AgentLocal
        }

        private TcpListener _gwLocalServer;
        private TcpClient _gwLocalClient;
        private TcpClient _gwRemoteClient;
        private Security _gwLocalSecurity;
        private Security _gwRemoteSecurity;
        private NetworkStream _gwLocalStream;
        private NetworkStream _gwRemoteStream;
        private TransferBuffer _gwRemoteRecvBuffer;
        private TransferBuffer _gwLocalRecvBuffer;
        private List<Packet> _gwLocalBuffer;
        private List<Packet> _gwRemoteBuffer;

        private TcpListener _agLocalServer;
        private TcpClient _agLocalClient;
        private TcpClient _agRemoteClient;
        private Security _agLocalSecurity;
        private Security _agRemoteSecurity;
        private NetworkStream _agLocalStream;
        private NetworkStream _agRemoteStream;
        private TransferBuffer _agRemoteRecvBuffer;
        private TransferBuffer _agLocalRecvBuffer;
        private List<Packet> _agLocalBuffer;
        private List<Packet> _agRemoteBuffer;

        private string _xferRemoteIp;
        private int _xferRemotePort;
        public string XferLoginId { get; protected set; }
        public string XferLoginPw { get; protected set; }

        private readonly object _exitLock;
        private bool _shouldExit;
        private bool _gwShouldExit;
        private bool _agShouldExit;

        private readonly GlobalManager _globalManager;

        private readonly XmlWriter _xmlWriter;
        private int _xmlIndex;
        public bool LogPackets { get; set; }
        public bool AllowLoginRequest { get; set; }

        public Proxy(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            _exitLock = new object();

            TextWriter textWriter = new StreamWriter($"log_{DateTime.Now.Ticks}.xml");
            _xmlWriter = XmlWriter.Create(textWriter);
            _xmlWriter.WriteStartElement("packets");
        }

        private void WritePacket([NotNull] Packet packet, [NotNull] byte[] packetBytes, [NotNull] string direction)
        {
            lock (_xmlWriter)
            {
                _xmlWriter.WriteStartElement("packet");
                _xmlWriter.WriteAttributeString("Index", _xmlIndex.ToString());
                _xmlIndex++;
                _xmlWriter.WriteAttributeString("Time", DateTime.Now.Ticks.ToString());
                _xmlWriter.WriteAttributeString("Direction", direction);
                _xmlWriter.WriteAttributeString("Opcode", packet.Opcode.ToString());
                _xmlWriter.WriteAttributeString("Encrypted", packet.Encrypted.ToString());
                _xmlWriter.WriteAttributeString("Massive", packet.Massive.ToString());
                _xmlWriter.WriteAttributeString("Length", packetBytes.Length.ToString());
                _xmlWriter.WriteAttributeString("Payload", Convert.ToBase64String(packetBytes, 0, packetBytes.Length));
                _xmlWriter.WriteEndElement();
                _xmlWriter.Flush();
            }
        }

        private void GatewayRemoteThread()
        {
            try
            {
                while (true)
                {
                    lock (_exitLock)
                    {
                        if (_gwShouldExit)
                        {
                            _gwRemoteStream.Close();
                            break;
                        }
                    }

                    if (_gwRemoteStream.DataAvailable)
                    {
                        _gwRemoteRecvBuffer.Offset = 0;
                        _gwRemoteRecvBuffer.Size = _gwRemoteStream.Read(_gwRemoteRecvBuffer.Buffer, 0, _gwRemoteRecvBuffer.Buffer.Length);
                        _gwRemoteSecurity.Recv(_gwRemoteRecvBuffer);
                    }

                    var gwRemoteRecvPackets = _gwRemoteSecurity.TransferIncoming();
                    if (gwRemoteRecvPackets != null)
                    {
                        foreach (var packet in gwRemoteRecvPackets)
                        {
                            //Console.WriteLine(@"[Gateway] [S->P][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine);

                            switch (packet.Opcode)
                            {
                                // Do not pass through these packets.
                                case 0x5000:
                                case 0x9000:
                                    continue;
                                case 0xA100:
                                    if (_globalManager.Clientless)
                                    {
                                        Send(new Packet(0x6106, true), EPacketdestination.GatewayRemote);
                                        Send(new Packet(0x6101, true), EPacketdestination.GatewayRemote);
                                    }
                                    break;
                                case 0xA101:
                                    _globalManager.ServerStats(packet);
                                    if (_globalManager.Userdata) AllowLoginRequest = true;
                                    break;
                                case 0xA102:
                                    var result = packet.ReadUInt8();
                                    switch (result)
                                    {
                                        case 1:
                                            AllowLoginRequest = false;
                                            _globalManager.ClientManager.SetLocalConnection(Client.LocalAgentPort);
                                            Client.ServerLoginid = packet.ReadUInt32();
                                            var ip = packet.ReadAscii();
                                            var port = packet.ReadUInt16();

                                            _xferRemoteIp = ip;
                                            _xferRemotePort = port;

                                            var newPacket = new Packet(0xA102, true);
                                            newPacket.WriteUInt8(result);
                                            newPacket.WriteUInt32(Client.ServerLoginid);
                                            newPacket.WriteAscii("127.0.0.1");
                                            newPacket.WriteUInt16(Client.LocalAgentPort);

                                            _gwLocalSecurity.Send(newPacket);
                                            continue;
                                        case 2:
                                            AllowLoginRequest = true;
                                            break;
                                    }

                                    break;
                            }

                            _gwLocalSecurity.Send(packet);
                        }
                    }

                    lock (_gwLocalBuffer)
                    {
                        foreach (var packet in _gwLocalBuffer)
                            _gwLocalSecurity.Send(packet);

                        _gwLocalBuffer.Clear();
                    }

                    var gwRemoteSendBuffers = _gwRemoteSecurity.TransferOutgoing();
                    if (gwRemoteSendBuffers != null)
                    {
                        foreach (var kvp in gwRemoteSendBuffers)
                        {
                            var packet = kvp.Value;
                            var buffer = kvp.Key;
                            var packetBytes = packet.GetBytes();

                            if (LogPackets)
                            {
                                Console.WriteLine(@"[Gateway P->S][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packetBytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packetBytes), Environment.NewLine);
                                WritePacket(packet, packet.GetBytes(), "[Gateway P->S]");
                            }

                            _gwRemoteStream.Write(buffer.Buffer, 0, buffer.Size);
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[GatewayRemoteThread] Exception: {0}", ex);
            }
        }

        private void GatewayLocalThread()
        {
            try
            {
                var timeout = new Stopwatch();
                timeout.Start();

                while (true)
                {
                    lock (_exitLock)
                    {
                        if (_gwShouldExit)
                        {
                            _gwLocalStream.Close();
                            break;
                        }
                    }

                    if (_gwLocalStream.DataAvailable)
                    {
                        _gwLocalRecvBuffer.Offset = 0;
                        _gwLocalRecvBuffer.Size = _gwLocalStream.Read(_gwLocalRecvBuffer.Buffer, 0, _gwLocalRecvBuffer.Buffer.Length);
                        _gwLocalSecurity.Recv(_gwLocalRecvBuffer);
                    }

                    var gwLocalRecvPackets = _gwLocalSecurity.TransferIncoming();
                    if (gwLocalRecvPackets != null)
                    {
                        foreach (var packet in gwLocalRecvPackets)
                        {
                            //Console.WriteLine(@"[Gateway] [C->P][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine);

                            // Do not pass through these packets.
                            switch (packet.Opcode)
                            {
                                case 0x5000:
                                case 0x9000:
                                case 0x2001:
                                    continue;
                                case 0x6102:
                                    if (!_globalManager.Userdata)
                                    {
                                        packet.ReadUInt8(); // ClientLocale
                                        XferLoginId = packet.ReadAscii();
                                        XferLoginPw = packet.ReadAscii();
                                        packet.ReadUInt16(); // ServerID
                                        AllowLoginRequest = true;
                                    }
                                    break;
                            }

                            _gwRemoteSecurity.Send(packet);
                        }

                        timeout.Restart();
                    }

                    if (timeout.ElapsedMilliseconds > 4000)
                    {
                        _gwRemoteSecurity.Send(new Packet(0x2002, false));
                        timeout.Restart();
                    }

                    lock (_gwRemoteBuffer)
                    {
                        foreach (var packet in _gwRemoteBuffer)
                            _gwRemoteSecurity.Send(packet);

                        _gwRemoteBuffer.Clear();
                    }

                    var gwLocalSendBuffers = _gwLocalSecurity.TransferOutgoing();
                    if (gwLocalSendBuffers != null)
                    {
                        foreach (var kvp in gwLocalSendBuffers)
                        {
                            var packet = kvp.Value;
                            var buffer = kvp.Key;
                            var packetBytes = packet.GetBytes();

                            if (LogPackets)
                            {
                                Console.WriteLine(@"[Gateway P->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packetBytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packetBytes), Environment.NewLine);
                                WritePacket(packet, packet.GetBytes(), "[Gateway P->C]");
                            }

                            _gwLocalStream.Write(buffer.Buffer, 0, buffer.Size);
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[GatewayLocalThread] Exception: {0}", ex);
            }
        }

        private void GatewayThread()
        {
            try
            {
                while (true)
                {
                    lock (_exitLock)
                    {
                        if (_shouldExit)
                            break;
                    }

                    // ReSharper disable once InconsistentlySynchronizedField
                    _gwLocalBuffer = new List<Packet>();
                    // ReSharper disable once InconsistentlySynchronizedField
                    _gwRemoteBuffer = new List<Packet>();

                    _gwLocalSecurity = new Security();
                    _gwLocalSecurity.GenerateSecurity(true, true, true);

                    _gwRemoteSecurity = new Security();

                    _gwRemoteRecvBuffer = new TransferBuffer(4096, 0, 0);
                    _gwLocalRecvBuffer = new TransferBuffer(4096, 0, 0);

                    _gwLocalServer = new TcpListener(IPAddress.Parse(Client.LocalIp), Client.LocalGatewayPort);
                    _gwLocalServer.Start();

                    Console.WriteLine(@"[GatewayThread] Waiting for a connection... ");

                    _gwLocalClient = _gwLocalServer.AcceptTcpClient();
                    _gwRemoteClient = new TcpClient();

                    Console.WriteLine(@"[GatewayThread] A connection has been made!");

                    _gwLocalServer.Stop();

                    Console.WriteLine(@"[GatewayThread] Connecting to Server... ");

                    _gwRemoteClient.Connect(Client.ServerIp, Client.ServerPort);
                    _gwShouldExit = false;
                    _agShouldExit = true;

                    Console.WriteLine(@"[GatewayThread] The connection has been made!");

                    _gwLocalStream = _gwLocalClient.GetStream();
                    _gwRemoteStream = _gwRemoteClient.GetStream();

                    var remoteThread = new Thread(GatewayRemoteThread);
                    remoteThread.Start();

                    var localThread = new Thread(GatewayLocalThread);
                    localThread.Start();

                    remoteThread.Join();
                    localThread.Join();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[GatewayThread] Exception: {0}", ex);
            }
        }

        private void AgentRemoteThread()
        {
            ushort lastOpCode = 0;

            try
            {
                while (true)
                {
                    lock (_exitLock)
                    {
                        if (_agShouldExit)
                        {
                            _agRemoteStream.Close();
                            break;
                        }
                    }

                    if (_agRemoteStream.DataAvailable)
                    {
                        _agRemoteRecvBuffer.Offset = 0;
                        _agRemoteRecvBuffer.Size = _agRemoteStream.Read(_agRemoteRecvBuffer.Buffer, 0, _agRemoteRecvBuffer.Buffer.Length);
                        _agRemoteSecurity.Recv(_agRemoteRecvBuffer);
                    }

                    var agRemoteRecvPackets = _agRemoteSecurity.TransferIncoming();
                    if (agRemoteRecvPackets != null)
                    {
                        foreach (var packet in agRemoteRecvPackets)
                        {
                            //Console.WriteLine(@"[Agent] [S->P][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine);
                            lastOpCode = packet.Opcode;
                            // Do not pass through these packets.
                            switch (packet.Opcode)
                            {
                                case 0x5000:
                                case 0x9000:
                                    continue;
                                case 0xA103:
                                    _agLocalSecurity.Send(packet);
                                    _globalManager.ClientManager.SetLocalConnection(Client.LocalGatewayPort);

                                    Packet newPacket;
                                    var result = packet.ReadUInt8();
                                    switch (result)
                                    {
                                        case 1:
                                            if (_globalManager.Clientless)
                                            {
                                                newPacket = new Packet(0x7007, false);
                                                newPacket.WriteUInt8(2);
                                                Send(newPacket, EPacketdestination.AgentRemote);
                                            }
                                            break;
                                        case 2:
                                            newPacket = new Packet(0xA103, true);
                                            newPacket.WriteUInt8(1);
                                            _agLocalSecurity.Send(newPacket);
                                            break;
                                    }

                                    continue;
                            }

                            var packetToSend = packet;
                            if (_globalManager.AgentServerToClient(ref packetToSend))
                                _agLocalSecurity.Send(packetToSend);
                        }
                    }

                    lock (_agLocalBuffer)
                    {
                        foreach (var packet in _agLocalBuffer)
                            _agLocalSecurity.Send(packet);

                        _agLocalBuffer.Clear();
                    }

                    var agRemoteSendBuffers = _agRemoteSecurity.TransferOutgoing();
                    if (agRemoteSendBuffers != null)
                    {
                        foreach (var kvp in agRemoteSendBuffers)
                        {
                            var packet = kvp.Value;
                            var buffer = kvp.Key;
                            var packetBytes = packet.GetBytes();

                            if (LogPackets)
                            {
                                Console.WriteLine(@"[Agent P->S][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packetBytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packetBytes), Environment.NewLine);
                                WritePacket(packet, packet.GetBytes(), "[Agent P->S]");
                            }

                            _agRemoteStream.Write(buffer.Buffer, 0, buffer.Size);
                        }

                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[AgentRemoteThread] Exception: {0}", ex);

                if (lastOpCode == 0x2005)
                {
                    var newPacket = new Packet(0x6005, false, true);
                    newPacket.WriteUInt8(3);
                    newPacket.WriteUInt8(0);
                    newPacket.WriteUInt8(2);
                    newPacket.WriteUInt8(0);
                    newPacket.WriteUInt8(2);
                    _agLocalSecurity.Send(newPacket);
                    lastOpCode = 0x6005;
                }

                if (lastOpCode == 0x6005)
                {
                    _globalManager.ClientManager.SetLocalConnection(Client.LocalGatewayPort);
                    var newPacket = new Packet(0xA103, true);
                    newPacket.WriteUInt8(1);
                    _agLocalSecurity.Send(newPacket);
                }
            }
        }

        private void AgentLocalThread()
        {
            try
            {
                var timeout = new Stopwatch();
                timeout.Start();

                while (true)
                {
                    lock (_exitLock)
                    {
                        if (_agShouldExit)
                        {
                            _agLocalStream.Close();
                            break;
                        }
                    }

                    if (_agLocalStream.DataAvailable)
                    {
                        _agLocalRecvBuffer.Offset = 0;
                        _agLocalRecvBuffer.Size = _agLocalStream.Read(_agLocalRecvBuffer.Buffer, 0, _agLocalRecvBuffer.Buffer.Length);
                        _agLocalSecurity.Recv(_agLocalRecvBuffer);
                    }

                    var agLocalRecvPackets = _agLocalSecurity.TransferIncoming();
                    if (agLocalRecvPackets != null)
                    {
                        foreach (var packet in agLocalRecvPackets)
                        {
                            //Console.WriteLine(@"[Agent] [C->P][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine);

                            // Do not pass through these packets.
                            switch (packet.Opcode)
                            {
                                case 0x5000:
                                case 0x9000:
                                case 0x2001:
                                    continue;
                                case 0x6103:
                                    if (_globalManager.Autologin && _globalManager.Userdata)
                                    {
                                        var newPacket = new Packet(0x6103, true);
                                        newPacket.WriteUInt32(Client.ServerLoginid);
                                        newPacket.WriteAscii(_globalManager.LoginId);
                                        newPacket.WriteAscii(_globalManager.LoginPw);
                                        newPacket.WriteUInt16(Client.ClientLocale);
                                        newPacket.WriteUInt32(0);
                                        newPacket.WriteUInt8(0);
                                        _agRemoteSecurity.Send(newPacket);

                                        continue;
                                    }

                                    break;
                            }

                            var packetToSend = packet;
                            if (_globalManager.ClientToAgentServer(ref packetToSend))
                                _agRemoteSecurity.Send(packetToSend);
                        }

                        timeout.Restart();
                    }

                    if (timeout.ElapsedMilliseconds > 4000)
                    {
                        _gwRemoteSecurity.Send(new Packet(0x2002, false));
                        timeout.Restart();
                    }

                    lock (_agRemoteBuffer)
                    {
                        foreach (var packet in _agRemoteBuffer)
                            _agRemoteSecurity.Send(packet);

                        _agRemoteBuffer.Clear();
                    }

                    var agLocalSendBuffers = _agLocalSecurity.TransferOutgoing();
                    if (agLocalSendBuffers != null)
                    {
                        foreach (var kvp in agLocalSendBuffers)
                        {
                            var packet = kvp.Value;
                            var buffer = kvp.Key;
                            var packetBytes = packet.GetBytes();

                            if (LogPackets)
                            {
                                Console.WriteLine(@"[Agent P->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packetBytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packetBytes), Environment.NewLine);
                                WritePacket(packet, packet.GetBytes(), "[Agent P->C]");
                            }

                            _agLocalStream.Write(buffer.Buffer, 0, buffer.Size);
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[AgentLocalThread] Exception: {0}", ex);
            }
        }

        private void AgentThread()
        {
            try
            {
                while (true)
                {
                    lock (_exitLock)
                    {
                        if (_shouldExit)
                            break;
                    }

                    // ReSharper disable once InconsistentlySynchronizedField
                    _agLocalBuffer = new List<Packet>();
                    // ReSharper disable once InconsistentlySynchronizedField
                    _agRemoteBuffer = new List<Packet>();

                    _agLocalSecurity = new Security();
                    _agLocalSecurity.GenerateSecurity(true, true, true);

                    _agRemoteSecurity = new Security();

                    _agRemoteRecvBuffer = new TransferBuffer(4096, 0, 0);
                    _agLocalRecvBuffer = new TransferBuffer(4096, 0, 0);

                    _agLocalServer = new TcpListener(IPAddress.Parse("127.0.0.1"), Client.LocalAgentPort);
                    _agLocalServer.Start();

                    Console.WriteLine(@"[AgentThread] Waiting for a connection... ");

                    _agLocalClient = _agLocalServer.AcceptTcpClient();
                    _agRemoteClient = new TcpClient();

                    Console.WriteLine(@"[AgentThread] A connection has been made!");

                    _agLocalServer.Stop();

                    Console.WriteLine(@"[AgentThread] Connecting to {0}:{1}", _xferRemoteIp, _xferRemotePort);

                    _agRemoteClient.Connect(_xferRemoteIp, _xferRemotePort);
                    _agShouldExit = false;
                    _gwShouldExit = true;

                    Console.WriteLine(@"[AgentThread] The connection has been made!");

                    _agLocalStream = _agLocalClient.GetStream();
                    _agRemoteStream = _agRemoteClient.GetStream();

                    var remoteThread = new Thread(AgentRemoteThread);
                    remoteThread.Start();

                    var localThread = new Thread(AgentLocalThread);
                    localThread.Start();

                    remoteThread.Join();
                    localThread.Join();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[AgentThread] Exception: {0}", ex);
            }
        }

        public void Send(Packet packet, EPacketdestination destination)
        {
            switch (destination)
            {
                case EPacketdestination.GatewayRemote:
                    lock (_gwRemoteBuffer)
                    {
                        _gwRemoteBuffer.Add(packet);
                    }
                    break;
                case EPacketdestination.GatewayLocal:
                    lock (_gwLocalBuffer)
                    {
                        _gwLocalBuffer.Add(packet);
                    }
                    break;
                case EPacketdestination.AgentRemote:
                    lock (_agRemoteBuffer)
                    {
                        _agRemoteBuffer.Add(packet);
                    }
                    break;
                case EPacketdestination.AgentLocal:
                    lock (_agLocalBuffer)
                    {
                        _agLocalBuffer.Add(packet);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(destination), destination, null);
            }
        }

        public void Stop()
        {
            lock (_exitLock)
            {
                _shouldExit = true;
                _gwShouldExit = true;
                _agShouldExit = true;
                _agLocalServer?.Stop();
                _gwLocalServer?.Stop();
            }
        }

        public void Start()
        {
            try
            {
                GetOpenPorts(20000);

                var gw = new Thread(GatewayThread);
                gw.Start();

                var ag = new Thread(AgentThread);
                ag.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"[Main] Exception: {0}", ex);
            }
        }

        private static void GetOpenPorts(ushort startPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();
            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();

            for (var port = startPort; port < startPort + 100; port++)
            {
                if (usedPorts.Contains(port))
                    continue;

                if (Client.LocalGatewayPort == 0)
                {
                    Client.LocalGatewayPort = port;
                    continue;
                }

                if (Client.LocalAgentPort == 0)
                {
                    Client.LocalAgentPort = port;
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}