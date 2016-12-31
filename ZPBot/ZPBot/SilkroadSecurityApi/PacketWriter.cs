using System.IO;

namespace ZPBot.SilkroadSecurityApi
{
    internal class PacketWriter : BinaryWriter
    {
        readonly MemoryStream _mMs;

        public PacketWriter()
        {
            _mMs = new MemoryStream();
            OutStream = _mMs;
        }

        public byte[] GetBytes() => _mMs.ToArray();
    }
}
