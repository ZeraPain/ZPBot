using System;
using System.IO;
using System.Text;

namespace ZPBot.SilkroadSecurityApi
{
    public class Packet
    {
        private PacketWriter _mWriter;
        private PacketReader _mReader;
        private bool _mLocked;
        byte[] _mReaderBytes;
        readonly object _mLock;

        public ushort Opcode { get; }
        public bool Encrypted { get; }
        public bool Massive { get; }

        public Packet(Packet rhs)
        {
            lock (rhs._mLock)
            {
                _mLock = new object();

                Opcode = rhs.Opcode;
                Encrypted = rhs.Encrypted;
                Massive = rhs.Massive;

                _mLocked = rhs._mLocked;
                if (!_mLocked)
                {
                    _mWriter = new PacketWriter();
                    _mReader = null;
                    _mReaderBytes = null;
                    _mWriter.Write(rhs._mWriter.GetBytes());
                }
                else
                {
                    _mWriter = null;
                    _mReaderBytes = rhs._mReaderBytes;
                    _mReader = new PacketReader(_mReaderBytes);
                }
            }
        }
        public Packet(ushort opcode)
        {
            _mLock = new object();
            Opcode = opcode;
            Encrypted = false;
            Massive = false;
            _mWriter = new PacketWriter();
            _mReader = null;
            _mReaderBytes = null;
        }
        public Packet(ushort opcode, bool encrypted)
        {
            _mLock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = false;
            _mWriter = new PacketWriter();
            _mReader = null;
            _mReaderBytes = null;
        }
        public Packet(ushort opcode, bool encrypted, bool massive)
        {
            if (encrypted && massive)
                throw new Exception("[Packet::Packet] Packets cannot both be massive and encrypted!");

            _mLock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = massive;
            _mWriter = new PacketWriter();
            _mReader = null;
            _mReaderBytes = null;
        }
        public Packet(ushort opcode, bool encrypted, bool massive, byte[] bytes)
        {
            if (encrypted && massive)
                throw new Exception("[Packet::Packet] Packets cannot both be massive and encrypted!");

            _mLock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = massive;
            _mWriter = new PacketWriter();
            _mWriter.Write(bytes);
            _mReader = null;
            _mReaderBytes = null;
        }
        public Packet(ushort opcode, bool encrypted, bool massive, byte[] bytes, int offset, int length)
        {
            if (encrypted && massive)
                throw new Exception("[Packet::Packet] Packets cannot both be massive and encrypted!");

            _mLock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = massive;
            _mWriter = new PacketWriter();
            _mWriter.Write(bytes, offset, length);
            _mReader = null;
            _mReaderBytes = null;
        }

        public byte[] GetBytes()
        {
            lock (_mLock)
            {
                return _mLocked ? _mReaderBytes : _mWriter.GetBytes();
            }
        }

        public void Lock()
        {
            lock (_mLock)
            {
                if (_mLocked) return;

                _mReaderBytes = _mWriter.GetBytes();
                _mReader = new PacketReader(_mReaderBytes);
                _mWriter.Close();
                _mWriter = null;
                _mLocked = true;
            }
        }

        public void SkipBytes(uint amount)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Skip from an unlocked Packet.");

                _mReader.BaseStream.Position += amount;
            }
        }

        public void Override(byte[] data)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Override an unlocked Packet.");

                var index = _mReader.BaseStream.Position - data.Length;
                Array.Copy(data, 0, _mReaderBytes, index, data.Length);
            }
        }

        public long SeekRead(long offset, SeekOrigin orgin)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot SeekRead on an unlocked Packet.");

                return _mReader.BaseStream.Seek(offset, orgin);
            }
        }

        public int RemainingRead()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot SeekRead on an unlocked Packet.");

                return (int)(_mReader.BaseStream.Length - _mReader.BaseStream.Position);
            }
        }

        public bool ReadBoolean()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadBoolean();
            }
        }

        public byte ReadUInt8()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadByte();
            }
        }
        public sbyte ReadInt8()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadSByte();
            }
        }
        public ushort ReadUInt16()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadUInt16();
            }
        }
        public short ReadInt16()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadInt16();
            }
        }
        public uint ReadUInt32()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadUInt32();
            }
        }
        public int ReadInt32()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadInt32();
            }
        }
        public ulong ReadUInt64()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadUInt64();
            }
        }
        public long ReadInt64()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadInt64();
            }
        }
        public float ReadSingle()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadSingle();
            }
        }
        public double ReadDouble()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                return _mReader.ReadDouble();
            }
        }
        public string ReadAscii()
        {
            return ReadAscii(1252);
        }
        public string ReadAscii(int codepage)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var length = _mReader.ReadUInt16();
                var bytes = _mReader.ReadBytes(length);

                return Encoding.GetEncoding(codepage).GetString(bytes);
            }
        }
        public string ReadUnicode()
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var length = _mReader.ReadUInt16();
                var bytes = _mReader.ReadBytes(length * 2);

                return Encoding.Unicode.GetString(bytes);
            }
        }

        public byte[] ReadUInt8Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new byte[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadByte();

                return values;
            }
        }
        public sbyte[] ReadInt8Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new sbyte[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadSByte();

                return values;
            }
        }
        public ushort[] ReadUInt16Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new ushort[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadUInt16();

                return values;
            }
        }
        public short[] ReadInt16Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new short[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadInt16();

                return values;
            }
        }
        public uint[] ReadUInt32Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new uint[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadUInt32();

                return values;
            }
        }
        public int[] ReadInt32Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new int[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadInt32();

                return values;
            }
        }
        public ulong[] ReadUInt64Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new ulong[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadUInt64();

                return values;
            }
        }
        public long[] ReadInt64Array(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new long[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadInt64();

                return values;
            }
        }
        public float[] ReadSingleArray(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new float[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadSingle();

                return values;
            }
        }
        public double[] ReadDoubleArray(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new double[count];
                for (var x = 0; x < count; ++x)
                    values[x] = _mReader.ReadDouble();

                return values;
            }
        }

        public string[] ReadAsciiArray(int codepage, int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new string[count];
                for (var x = 0; x < count; ++x)
                {
                    var length = _mReader.ReadUInt16();
                    var bytes = _mReader.ReadBytes(length);
                    values[x] = Encoding.UTF7.GetString(bytes);
                }

                return values;
            }
        }
        public string[] ReadUnicodeArray(int count)
        {
            lock (_mLock)
            {
                if (!_mLocked)
                    throw new Exception("Cannot Read from an unlocked Packet.");

                var values = new string[count];
                for (var x = 0; x < count; ++x)
                {
                    var length = _mReader.ReadUInt16();
                    var bytes = _mReader.ReadBytes(length * 2);
                    values[x] = Encoding.Unicode.GetString(bytes);
                }

                return values;
            }
        }

        public long SeekWrite(long offset, SeekOrigin orgin)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot SeekWrite on a locked Packet.");

                return _mWriter.BaseStream.Seek(offset, orgin);
            }
        }

        public void WriteUInt8(byte value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteInt8(sbyte value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteUInt16(ushort value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteInt16(short value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteUInt32(uint value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteInt32(int value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteUInt64(ulong value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteInt64(long value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteSingle(float value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteDouble(double value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(value);
            }
        }
        public void WriteAscii(string value)
        {
            WriteAscii(value, 1252);
        }
        public void WriteAscii(string value, int codePage)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                var codepageBytes = Encoding.GetEncoding(codePage).GetBytes(value);
                var utf7Value = Encoding.UTF7.GetString(codepageBytes);
                var bytes = Encoding.Default.GetBytes(utf7Value);

                _mWriter.Write((ushort)bytes.Length);
                _mWriter.Write(bytes);
            }
        }
        public void WriteUnicode(string value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                var bytes = Encoding.Unicode.GetBytes(value);

                _mWriter.Write((ushort)value.Length);
                _mWriter.Write(bytes);
            }
        }

        public void WriteUInt8(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write((byte)(Convert.ToUInt64(value) & 0xFF));
            }
        }
        public void WriteInt8(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write((sbyte)(Convert.ToInt64(value) & 0xFF));
            }
        }
        public void WriteUInt16(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write((ushort)(Convert.ToUInt64(value) & 0xFFFF));
            }
        }
        public void WriteInt16(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write((ushort)(Convert.ToInt64(value) & 0xFFFF));
            }
        }
        public void WriteUInt32(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write((uint)(Convert.ToUInt64(value) & 0xFFFFFFFF));
            }
        }
        public void WriteInt32(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write((int)(Convert.ToInt64(value) & 0xFFFFFFFF));
            }
        }
        public void WriteUInt64(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(Convert.ToUInt64(value));
            }
        }
        public void WriteInt64(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(Convert.ToInt64(value));
            }
        }
        public void WriteSingle(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(Convert.ToSingle(value));
            }
        }
        public void WriteDouble(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                _mWriter.Write(Convert.ToDouble(value));
            }
        }
        public void WriteAscii(object value)
        {
            WriteAscii(value, 1252);
        }
        public void WriteAscii(object value, int codePage)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                var codepageBytes = Encoding.GetEncoding(codePage).GetBytes(value.ToString());
                var utf7Value = Encoding.UTF7.GetString(codepageBytes);
                var bytes = Encoding.Default.GetBytes(utf7Value);

                _mWriter.Write((ushort)bytes.Length);
                _mWriter.Write(bytes);
            }
        }
        public void WriteUnicode(object value)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                var bytes = Encoding.Unicode.GetBytes(value.ToString());

                _mWriter.Write((ushort)value.ToString().Length);
                _mWriter.Write(bytes);
            }
        }

        public void WriteUInt8Array(byte[] values)
        {
            if (_mLocked)
                throw new Exception("Cannot Write to a locked Packet.");

            _mWriter.Write(values);
        }
        public void WriteUInt8Array(byte[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteUInt16Array(ushort[] values)
        {
            WriteUInt16Array(values, 0, values.Length);
        }
        public void WriteUInt16Array(ushort[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteInt16Array(short[] values)
        {
            WriteInt16Array(values, 0, values.Length);
        }
        public void WriteInt16Array(short[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteUInt32Array(uint[] values)
        {
            WriteUInt32Array(values, 0, values.Length);
        }
        public void WriteUInt32Array(uint[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteInt32Array(int[] values)
        {
            WriteInt32Array(values, 0, values.Length);
        }
        public void WriteInt32Array(int[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteUInt64Array(ulong[] values)
        {
            WriteUInt64Array(values, 0, values.Length);
        }
        public void WriteUInt64Array(ulong[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteInt64Array(long[] values)
        {
            WriteInt64Array(values, 0, values.Length);
        }
        public void WriteInt64Array(long[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteSingleArray(float[] values)
        {
            WriteSingleArray(values, 0, values.Length);
        }
        public void WriteSingleArray(float[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteDoubleArray(double[] values)
        {
            WriteDoubleArray(values, 0, values.Length);
        }
        public void WriteDoubleArray(double[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    _mWriter.Write(values[x]);
            }
        }
        public void WriteAsciiArray(string[] values, int codepage)
        {
            WriteAsciiArray(values, 0, values.Length, codepage);
        }
        public void WriteAsciiArray(string[] values, int index, int count, int codepage)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteAscii(values[x], codepage);
            }
        }
        public void WriteAsciiArray(string[] values)
        {
            WriteAsciiArray(values, 0, values.Length, 1252);
        }
        public void WriteAsciiArray(string[] values, int index, int count)
        {
            WriteAsciiArray(values, index, count, 1252);
        }
        public void WriteUnicodeArray(string[] values)
        {
            WriteUnicodeArray(values, 0, values.Length);
        }
        public void WriteUnicodeArray(string[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteUnicode(values[x]);
            }
        }

        public void WriteUInt8Array(object[] values)
        {
            WriteUInt8Array(values, 0, values.Length);
        }
        public void WriteUInt8Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteUInt8(values[x]);
            }
        }
        public void WriteInt8Array(object[] values)
        {
            WriteInt8Array(values, 0, values.Length);
        }
        public void WriteInt8Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteInt8(values[x]);
            }
        }
        public void WriteUInt16Array(object[] values)
        {
            WriteUInt16Array(values, 0, values.Length);
        }
        public void WriteUInt16Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteUInt16(values[x]);
            }
        }
        public void WriteInt16Array(object[] values)
        {
            WriteInt16Array(values, 0, values.Length);
        }
        public void WriteInt16Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteInt16(values[x]);
            }
        }
        public void WriteUInt32Array(object[] values)
        {
            WriteUInt32Array(values, 0, values.Length);
        }
        public void WriteUInt32Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteUInt32(values[x]);
            }
        }
        public void WriteInt32Array(object[] values)
        {
            WriteInt32Array(values, 0, values.Length);
        }
        public void WriteInt32Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteInt32(values[x]);
            }
        }
        public void WriteUInt64Array(object[] values)
        {
            WriteUInt64Array(values, 0, values.Length);
        }
        public void WriteUInt64Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteUInt64(values[x]);
            }
        }
        public void WriteInt64Array(object[] values)
        {
            WriteInt64Array(values, 0, values.Length);
        }
        public void WriteInt64Array(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteInt64(values[x]);
            }
        }
        public void WriteSingleArray(object[] values)
        {
            WriteSingleArray(values, 0, values.Length);
        }
        public void WriteSingleArray(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteSingle(values[x]);
            }
        }
        public void WriteDoubleArray(object[] values)
        {
            WriteDoubleArray(values, 0, values.Length);
        }
        public void WriteDoubleArray(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteDouble(values[x]);
            }
        }
        public void WriteAsciiArray(object[] values, int codepage)
        {
            WriteAsciiArray(values, 0, values.Length, codepage);
        }
        public void WriteAsciiArray(object[] values, int index, int count, int codepage)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteAscii(values[x].ToString(), codepage);
            }
        }
        public void WriteAsciiArray(object[] values)
        {
            WriteAsciiArray(values, 0, values.Length, 1252);
        }
        public void WriteAsciiArray(object[] values, int index, int count)
        {
            WriteAsciiArray(values, index, count, 1252);
        }
        public void WriteUnicodeArray(object[] values)
        {
            WriteUnicodeArray(values, 0, values.Length);
        }
        public void WriteUnicodeArray(object[] values, int index, int count)
        {
            lock (_mLock)
            {
                if (_mLocked)
                    throw new Exception("Cannot Write to a locked Packet.");

                for (var x = index; x < index + count; ++x)
                    WriteUnicode(values[x].ToString());
            }
        }
    }
}
