using System.IO;
using ZPBot.Annotations;

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

        [NotNull]
        public byte[] GetBytes() => _mMs.ToArray();
    }
}
