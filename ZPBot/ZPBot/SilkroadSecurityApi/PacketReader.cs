using System.IO;
using ZPBot.Annotations;

namespace ZPBot.SilkroadSecurityApi
{
    internal class PacketReader : BinaryReader
    {
        byte[] _mInput;

        public PacketReader([NotNull] byte[] input)
            : base(new MemoryStream(input, false))
        {
            _mInput = input;
        }

        public PacketReader([NotNull] byte[] input, int index, int count)
            : base(new MemoryStream(input, index, count, false))
        {
            _mInput = input;
        }
    }
}
