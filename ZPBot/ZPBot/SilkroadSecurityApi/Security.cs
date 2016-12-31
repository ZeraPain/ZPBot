using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ZPBot.SilkroadSecurityApi
{
    public class Security
    {
        #region SecurityFlags
        // Security flags container
        [StructLayout(LayoutKind.Explicit, Size = 8, CharSet = CharSet.Ansi)]
        private sealed class SecurityFlags
        {
            [FieldOffset(0)]
            public byte none;
            [FieldOffset(1)]
            public byte blowfish;
            [FieldOffset(2)]
            public byte security_bytes;
            [FieldOffset(3)]
            public byte handshake;
            [FieldOffset(4)]
            public byte handshake_response;
            [FieldOffset(5)]
            public byte _6;
            [FieldOffset(6)]
            public byte _7;
            [FieldOffset(7)]
            public byte _8;
        }

        // Returns a byte from a SecurityFlags object.
        private static byte FromSecurityFlags(SecurityFlags flags) => (byte)(flags.none | (flags.blowfish << 1) | (flags.security_bytes << 2) | (flags.handshake << 3) | (flags.handshake_response << 4) | (flags._6 << 5) | (flags._7 << 6) | (flags._8 << 7));

        // Returns a SecurityFlags object from a byte.
        private static SecurityFlags ToSecurityFlags(byte value)
        {
            var flags = new SecurityFlags {none = (byte) (value & 1)};
            value >>= 1;
            flags.blowfish = (byte)(value & 1);
            value >>= 1;
            flags.security_bytes = (byte)(value & 1);
            value >>= 1;
            flags.handshake = (byte)(value & 1);
            value >>= 1;
            flags.handshake_response = (byte)(value & 1);
            value >>= 1;
            flags._6 = (byte)(value & 1);
            value >>= 1;
            flags._7 = (byte)(value & 1);
            value >>= 1;
            flags._8 = (byte)(value & 1);
            return flags;
        }
        #endregion

        #region SecurityTable
        // Generates the crc bytes lookup table
        private static uint[] GenerateSecurityTable()
        {
            var securityTable = new uint[0x10000];
            byte[] baseSecurityTable =
            {
                0xB1, 0xD6, 0x8B, 0x96, 0x96, 0x30, 0x07, 0x77, 0x2C, 0x61, 0x0E, 0xEE, 0xBA, 0x51, 0x09, 0x99,
                0x19, 0xC4, 0x6D, 0x07, 0x8F, 0xF4, 0x6A, 0x70, 0x35, 0xA5, 0x63, 0xE9, 0xA3, 0x95, 0x64, 0x9E,
                0x32, 0x88, 0xDB, 0x0E, 0xA4, 0xB8, 0xDC, 0x79, 0x1E, 0xE9, 0xD5, 0xE0, 0x88, 0xD9, 0xD2, 0x97,
                0x2B, 0x4C, 0xB6, 0x09, 0xBD, 0x7C, 0xB1, 0x7E, 0x07, 0x2D, 0xB8, 0xE7, 0x91, 0x1D, 0xBF, 0x90,
                0x64, 0x10, 0xB7, 0x1D, 0xF2, 0x20, 0xB0, 0x6A, 0x48, 0x71, 0xB1, 0xF3, 0xDE, 0x41, 0xBE, 0x8C,
                0x7D, 0xD4, 0xDA, 0x1A, 0xEB, 0xE4, 0xDD, 0x6D, 0x51, 0xB5, 0xD4, 0xF4, 0xC7, 0x85, 0xD3, 0x83,
                0x56, 0x98, 0x6C, 0x13, 0xC0, 0xA8, 0x6B, 0x64, 0x7A, 0xF9, 0x62, 0xFD, 0xEC, 0xC9, 0x65, 0x8A,
                0x4F, 0x5C, 0x01, 0x14, 0xD9, 0x6C, 0x06, 0x63, 0x63, 0x3D, 0x0F, 0xFA, 0xF5, 0x0D, 0x08, 0x8D,
                0xC8, 0x20, 0x6E, 0x3B, 0x5E, 0x10, 0x69, 0x4C, 0xE4, 0x41, 0x60, 0xD5, 0x72, 0x71, 0x67, 0xA2,
                0xD1, 0xE4, 0x03, 0x3C, 0x47, 0xD4, 0x04, 0x4B, 0xFD, 0x85, 0x0D, 0xD2, 0x6B, 0xB5, 0x0A, 0xA5,
                0xFA, 0xA8, 0xB5, 0x35, 0x6C, 0x98, 0xB2, 0x42, 0xD6, 0xC9, 0xBB, 0xDB, 0x40, 0xF9, 0xBC, 0xAC,
                0xE3, 0x6C, 0xD8, 0x32, 0x75, 0x5C, 0xDF, 0x45, 0xCF, 0x0D, 0xD6, 0xDC, 0x59, 0x3D, 0xD1, 0xAB,
                0xAC, 0x30, 0xD9, 0x26, 0x3A, 0x00, 0xDE, 0x51, 0x80, 0x51, 0xD7, 0xC8, 0x16, 0x61, 0xD0, 0xBF,
                0xB5, 0xF4, 0xB4, 0x21, 0x23, 0xC4, 0xB3, 0x56, 0x99, 0x95, 0xBA, 0xCF, 0x0F, 0xA5, 0xB7, 0xB8,
                0x9E, 0xB8, 0x02, 0x28, 0x08, 0x88, 0x05, 0x5F, 0xB2, 0xD9, 0xEC, 0xC6, 0x24, 0xE9, 0x0B, 0xB1,
                0x87, 0x7C, 0x6F, 0x2F, 0x11, 0x4C, 0x68, 0x58, 0xAB, 0x1D, 0x61, 0xC1, 0x3D, 0x2D, 0x66, 0xB6,
                0x90, 0x41, 0xDC, 0x76, 0x06, 0x71, 0xDB, 0x01, 0xBC, 0x20, 0xD2, 0x98, 0x2A, 0x10, 0xD5, 0xEF,
                0x89, 0x85, 0xB1, 0x71, 0x1F, 0xB5, 0xB6, 0x06, 0xA5, 0xE4, 0xBF, 0x9F, 0x33, 0xD4, 0xB8, 0xE8,
                0xA2, 0xC9, 0x07, 0x78, 0x34, 0xF9, 0xA0, 0x0F, 0x8E, 0xA8, 0x09, 0x96, 0x18, 0x98, 0x0E, 0xE1,
                0xBB, 0x0D, 0x6A, 0x7F, 0x2D, 0x3D, 0x6D, 0x08, 0x97, 0x6C, 0x64, 0x91, 0x01, 0x5C, 0x63, 0xE6,
                0xF4, 0x51, 0x6B, 0x6B, 0x62, 0x61, 0x6C, 0x1C, 0xD8, 0x30, 0x65, 0x85, 0x4E, 0x00, 0x62, 0xF2,
                0xED, 0x95, 0x06, 0x6C, 0x7B, 0xA5, 0x01, 0x1B, 0xC1, 0xF4, 0x08, 0x82, 0x57, 0xC4, 0x0F, 0xF5,
                0xC6, 0xD9, 0xB0, 0x63, 0x50, 0xE9, 0xB7, 0x12, 0xEA, 0xB8, 0xBE, 0x8B, 0x7C, 0x88, 0xB9, 0xFC,
                0xDF, 0x1D, 0xDD, 0x62, 0x49, 0x2D, 0xDA, 0x15, 0xF3, 0x7C, 0xD3, 0x8C, 0x65, 0x4C, 0xD4, 0xFB,
                0x58, 0x61, 0xB2, 0x4D, 0xCE, 0x51, 0xB5, 0x3A, 0x74, 0x00, 0xBC, 0xA3, 0xE2, 0x30, 0xBB, 0xD4,
                0x41, 0xA5, 0xDF, 0x4A, 0xD7, 0x95, 0xD8, 0x3D, 0x6D, 0xC4, 0xD1, 0xA4, 0xFB, 0xF4, 0xD6, 0xD3,
                0x6A, 0xE9, 0x69, 0x43, 0xFC, 0xD9, 0x6E, 0x34, 0x46, 0x88, 0x67, 0xAD, 0xD0, 0xB8, 0x60, 0xDA,
                0x73, 0x2D, 0x04, 0x44, 0xE5, 0x1D, 0x03, 0x33, 0x5F, 0x4C, 0x0A, 0xAA, 0xC9, 0x7C, 0x0D, 0xDD,
                0x3C, 0x71, 0x05, 0x50, 0xAA, 0x41, 0x02, 0x27, 0x10, 0x10, 0x0B, 0xBE, 0x86, 0x20, 0x0C, 0xC9,
                0x25, 0xB5, 0x68, 0x57, 0xB3, 0x85, 0x6F, 0x20, 0x09, 0xD4, 0x66, 0xB9, 0x9F, 0xE4, 0x61, 0xCE,
                0x0E, 0xF9, 0xDE, 0x5E, 0x08, 0xC9, 0xD9, 0x29, 0x22, 0x98, 0xD0, 0xB0, 0xB4, 0xA8, 0x57, 0xC7,
                0x17, 0x3D, 0xB3, 0x59, 0x81, 0x0D, 0xB4, 0x3E, 0x3B, 0x5C, 0xBD, 0xB7, 0xAD, 0x6C, 0xBA, 0xC0,
                0x20, 0x83, 0xB8, 0xED, 0xB6, 0xB3, 0xBF, 0x9A, 0x0C, 0xE2, 0xB6, 0x03, 0x9A, 0xD2, 0xB1, 0x74,
                0x39, 0x47, 0xD5, 0xEA, 0xAF, 0x77, 0xD2, 0x9D, 0x15, 0x26, 0xDB, 0x04, 0x83, 0x16, 0xDC, 0x73,
                0x12, 0x0B, 0x63, 0xE3, 0x84, 0x3B, 0x64, 0x94, 0x3E, 0x6A, 0x6D, 0x0D, 0xA8, 0x5A, 0x6A, 0x7A,
                0x0B, 0xCF, 0x0E, 0xE4, 0x9D, 0xFF, 0x09, 0x93, 0x27, 0xAE, 0x00, 0x0A, 0xB1, 0x9E, 0x07, 0x7D,
                0x44, 0x93, 0x0F, 0xF0, 0xD2, 0xA2, 0x08, 0x87, 0x68, 0xF2, 0x01, 0x1E, 0xFE, 0xC2, 0x06, 0x69,
                0x5D, 0x57, 0x62, 0xF7, 0xCB, 0x67, 0x65, 0x80, 0x71, 0x36, 0x6C, 0x19, 0xE7, 0x06, 0x6B, 0x6E,
                0x76, 0x1B, 0xD4, 0xFE, 0xE0, 0x2B, 0xD3, 0x89, 0x5A, 0x7A, 0xDA, 0x10, 0xCC, 0x4A, 0xDD, 0x67,
                0x6F, 0xDF, 0xB9, 0xF9, 0xF9, 0xEF, 0xBE, 0x8E, 0x43, 0xBE, 0xB7, 0x17, 0xD5, 0x8E, 0xB0, 0x60,
                0xE8, 0xA3, 0xD6, 0xD6, 0x7E, 0x93, 0xD1, 0xA1, 0xC4, 0xC2, 0xD8, 0x38, 0x52, 0xF2, 0xDF, 0x4F,
                0xF1, 0x67, 0xBB, 0xD1, 0x67, 0x57, 0xBC, 0xA6, 0xDD, 0x06, 0xB5, 0x3F, 0x4B, 0x36, 0xB2, 0x48,
                0xDA, 0x2B, 0x0D, 0xD8, 0x4C, 0x1B, 0x0A, 0xAF, 0xF6, 0x4A, 0x03, 0x36, 0x60, 0x7A, 0x04, 0x41,
                0xC3, 0xEF, 0x60, 0xDF, 0x55, 0xDF, 0x67, 0xA8, 0xEF, 0x8E, 0x6E, 0x31, 0x79, 0x0E, 0x69, 0x46,
                0x8C, 0xB3, 0x51, 0xCB, 0x1A, 0x83, 0x63, 0xBC, 0xA0, 0xD2, 0x6F, 0x25, 0x36, 0xE2, 0x68, 0x52,
                0x95, 0x77, 0x0C, 0xCC, 0x03, 0x47, 0x0B, 0xBB, 0xB9, 0x14, 0x02, 0x22, 0x2F, 0x26, 0x05, 0x55,
                0xBE, 0x3B, 0xB6, 0xC5, 0x28, 0x0B, 0xBD, 0xB2, 0x92, 0x5A, 0xB4, 0x2B, 0x04, 0x6A, 0xB3, 0x5C,
                0xA7, 0xFF, 0xD7, 0xC2, 0x31, 0xCF, 0xD0, 0xB5, 0x8B, 0x9E, 0xD9, 0x2C, 0x1D, 0xAE, 0xDE, 0x5B,
                0xB0, 0x72, 0x64, 0x9B, 0x26, 0xF2, 0xE3, 0xEC, 0x9C, 0xA3, 0x6A, 0x75, 0x0A, 0x93, 0x6D, 0x02,
                0xA9, 0x06, 0x09, 0x9C, 0x3F, 0x36, 0x0E, 0xEB, 0x85, 0x68, 0x07, 0x72, 0x13, 0x07, 0x00, 0x05,
                0x82, 0x48, 0xBF, 0x95, 0x14, 0x7A, 0xB8, 0xE2, 0xAE, 0x2B, 0xB1, 0x7B, 0x38, 0x1B, 0xB6, 0x0C,
                0x9B, 0x8E, 0xD2, 0x92, 0x0D, 0xBE, 0xD5, 0xE5, 0xB7, 0xEF, 0xDC, 0x7C, 0x21, 0xDF, 0xDB, 0x0B,
                0x94, 0xD2, 0xD3, 0x86, 0x42, 0xE2, 0xD4, 0xF1, 0xF8, 0xB3, 0xDD, 0x68, 0x6E, 0x83, 0xDA, 0x1F,
                0xCD, 0x16, 0xBE, 0x81, 0x5B, 0x26, 0xB9, 0xF6, 0xE1, 0x77, 0xB0, 0x6F, 0x77, 0x47, 0xB7, 0x18,
                0xE0, 0x5A, 0x08, 0x88, 0x70, 0x6A, 0x0F, 0xF1, 0xCA, 0x3B, 0x06, 0x66, 0x5C, 0x0B, 0x01, 0x11,
                0xFF, 0x9E, 0x65, 0x8F, 0x69, 0xAE, 0x62, 0xF8, 0xD3, 0xFF, 0x6B, 0x61, 0x45, 0xCF, 0x6C, 0x16,
                0x78, 0xE2, 0x0A, 0xA0, 0xEE, 0xD2, 0x0D, 0xD7, 0x54, 0x83, 0x04, 0x4E, 0xC2, 0xB3, 0x03, 0x39,
                0x61, 0x26, 0x67, 0xA7, 0xF7, 0x16, 0x60, 0xD0, 0x4D, 0x47, 0x69, 0x49, 0xDB, 0x77, 0x6E, 0x3E,
                0x4A, 0x6A, 0xD1, 0xAE, 0xDC, 0x5A, 0xD6, 0xD9, 0x66, 0x0B, 0xDF, 0x40, 0xF0, 0x3B, 0xD8, 0x37,
                0x53, 0xAE, 0xBC, 0xA9, 0xC5, 0x9E, 0xBB, 0xDE, 0x7F, 0xCF, 0xB2, 0x47, 0xE9, 0xFF, 0xB5, 0x30,
                0x1C, 0xF9, 0xBD, 0xBD, 0x8A, 0xCD, 0xBA, 0xCA, 0x30, 0x9E, 0xB3, 0x53, 0xA6, 0xA3, 0xBC, 0x24,
                0x05, 0x3B, 0xD0, 0xBA, 0xA3, 0x06, 0xD7, 0xCD, 0xE9, 0x57, 0xDE, 0x54, 0xBF, 0x67, 0xD9, 0x23,
                0x2E, 0x72, 0x66, 0xB3, 0xB8, 0x4A, 0x61, 0xC4, 0x02, 0x1B, 0x38, 0x5D, 0x94, 0x2B, 0x6F, 0x2B,
                0x37, 0xBE, 0xCB, 0xB4, 0xA1, 0x8E, 0xCC, 0xC3, 0x1B, 0xDF, 0x0D, 0x5A, 0x8D, 0xED, 0x02, 0x2D,
            };

            using (var inMemoryStream = new MemoryStream(baseSecurityTable, false))
            {
                using (var reader = new BinaryReader(inMemoryStream))
                {
                    var index = 0;
                    for (var edi = 0; edi < 1024; edi += 4)
                    {
                        var edx = reader.ReadUInt32();
                        for (uint ecx = 0; ecx < 256; ++ecx)
                        {
                            var eax = ecx >> 1;
                            if ((ecx & 1) != 0)
                                eax ^= edx;

                            for (var bit = 0; bit < 7; ++bit)
                            {
                                if ((eax & 1) != 0)
                                {
                                    eax >>= 1;
                                    eax ^= edx;
                                }
                                else
                                {
                                    eax >>= 1;
                                }
                            }

                            securityTable[index++] = eax;
                        }
                    }
                }
            }

            return securityTable;
        }

        // Use one security table for all objects.
        private static readonly uint[] GlobalSecurityTable = GenerateSecurityTable();
        #endregion

        #region WIN32_Helper_Functions

        private static ulong MAKELONGLONG_(uint a, uint b)
        {
            ulong a1 = a;
            ulong b1 = b;
            return (b1 << 32) | a1;
        }

        private static ushort LOWORD_(uint a) => (ushort)(a & 0xFFFF);

        private static ushort HIWORD_(uint a) => (ushort)((a >> 16) & 0xFFFF);

        private static byte LOBYTE_(ushort a) => (byte)(a & 0xFF);

        private static byte HIBYTE_(ushort a) => (byte)((a >> 8) & 0xFF);

        #endregion

        #region Random

        private static readonly Random Random = new Random();

        private static ulong NextUInt64()
        {
            var buffer = new byte[sizeof(ulong)];
            Random.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        private static uint NextUInt32()
        {
            var buffer = new byte[sizeof(uint)];
            Random.NextBytes(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        private static ushort NextUInt16()
        {
            var buffer = new byte[sizeof(ushort)];
            Random.NextBytes(buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }

        private static byte NextUInt8() => (byte)(NextUInt16() & 0xFF);

        #endregion

        private uint _mValueX;
        private uint _mValueG;
        private uint _mValueP;
        private uint _mValueA;
        private uint _mValueB;
        private uint _mValueK;
        private uint _mSeedCount;
        private uint _mCrcSeed;
        private ulong _mInitialBlowfishKey;
        private ulong _mHandshakeBlowfishKey;
        private readonly byte[] _mCountByteSeeds;
        private ulong _mClientKey;
        private ulong _mChallengeKey;

        private bool _mClientSecurity;
        private byte _mSecurityFlag;
        private SecurityFlags _mSecurityFlags;
        private bool _mAcceptedHandshake;
        private bool _mStartedHandshake;
        private byte _mIdentityFlag;
        private string _mIdentityName;

        private List<Packet> _mIncomingPackets;
        private readonly List<Packet> _mOutgoingPackets;

        private readonly List<ushort> _mEncOpcodes;

        private readonly Blowfish _mBlowfish;

        private readonly TransferBuffer _mRecvBuffer;
        private TransferBuffer _mCurrentBuffer;

        private ushort _mMassiveCount;
        private Packet _mMassivePacket;

        private readonly object _mClassLock;

#region CoreSecurityFunction
        // This function's logic was written by jMerlin as part of the article "How to generate the security bytes for SRO"
        private uint GenerateValue(ref uint val)
        {
            for (var i = 0; i < 32; ++i)
            {
                val = (((((((((((val >> 2) ^ val) >> 2) ^ val) >> 1) ^ val) >> 1) ^ val) >> 1) ^ val) & 1) | ((((val & 1) << 31) | (val >> 1)) & 0xFFFFFFFE);
            }
            return val;
        }

        // Sets up the count bytes
        // This function's logic was written by jMerlin as part of the article "How to generate the security bytes for SRO"
        private void SetupCountByte(uint seed)
        {
            if (seed == 0) seed = 0x9ABFB3B6;
            var mut = seed;
            var mut1 = GenerateValue(ref mut);
            var mut2 = GenerateValue(ref mut);
            var mut3 = GenerateValue(ref mut);
            GenerateValue(ref mut);
            var byte1 = (byte)((mut & 0xFF) ^ (mut3 & 0xFF));
            var byte2 = (byte)((mut1 & 0xFF) ^ (mut2 & 0xFF));
            if (byte1 == 0) byte1 = 1;
            if (byte2 == 0) byte2 = 1;
            _mCountByteSeeds[0] = (byte)(byte1 ^ byte2);
            _mCountByteSeeds[1] = byte2;
            _mCountByteSeeds[2] = byte1;
        }

        // Helper function used in the handshake, X may be a or b, this clean version of the function is from jMerlin (Func_X_4)
        private uint G_pow_X_mod_P(uint p, uint x, uint g)
        {
            long result = 1;
            long mult = g;
            if (x == 0)
            {
                return 1;
            }
            while (x != 0)
            {
                if ((x & 1) > 0)
                {
                    result = (mult * result) % p;
                }
                x = x >> 1;
                mult = (mult * mult) % p;
            }
            return (uint)result;
        }

        // Helper function used in the handshake (Func_X_2)
        private void KeyTransformValue(ref ulong val, uint key, byte keyByte)
        {
            var stream = BitConverter.GetBytes(val);
            stream[0] ^= (byte)(stream[0] + LOBYTE_(LOWORD_(key)) + keyByte);
            stream[1] ^= (byte)(stream[1] + HIBYTE_(LOWORD_(key)) + keyByte);
            stream[2] ^= (byte)(stream[2] + LOBYTE_(HIWORD_(key)) + keyByte);
            stream[3] ^= (byte)(stream[3] + HIBYTE_(HIWORD_(key)) + keyByte);
            stream[4] ^= (byte)(stream[4] + LOBYTE_(LOWORD_(key)) + keyByte);
            stream[5] ^= (byte)(stream[5] + HIBYTE_(LOWORD_(key)) + keyByte);
            stream[6] ^= (byte)(stream[6] + LOBYTE_(HIWORD_(key)) + keyByte);
            stream[7] ^= (byte)(stream[7] + HIBYTE_(HIWORD_(key)) + keyByte);
            val = BitConverter.ToUInt64(stream, 0);
        }

        // Function called to generate a count byte
        // This function's logic was written by jMerlin as part of the article "How to generate the security bytes for SRO"
        private byte GenerateCountByte(bool update)
        {
            var result = (byte)(_mCountByteSeeds[2] * (~_mCountByteSeeds[0] + _mCountByteSeeds[1]));
            result = (byte)(result ^ (result >> 4));
            if (update)
            {
                _mCountByteSeeds[0] = result;
            }
            return result;
        }

        // Function called to generate the crc byte
        // This function's logic was written by jMerlin as part of the article "How to generate the security bytes for SRO"
        private byte GenerateCheckByte(byte[] stream, int offset, int length)
        {
            var checksum = 0xFFFFFFFF;
            var moddedseed = _mCrcSeed << 8;
            for (var x = offset; x < offset + length; ++x)
            {
                checksum = (checksum >> 8) ^ GlobalSecurityTable[moddedseed + ((stream[x] ^ checksum) & 0xFF)];
            }
            return (byte)(((checksum >> 24) & 0xFF) + ((checksum >> 8) & 0xFF) + ((checksum >> 16) & 0xFF) + (checksum & 0xFF));
        }

        private byte GenerateCheckByte(byte[] stream) => GenerateCheckByte(stream, 0, stream.Length);

        #endregion

#region ExtendedSecurityFunctions

        private void GenerateSecurity(SecurityFlags flags)
        {
            _mSecurityFlag = FromSecurityFlags(flags);
            _mSecurityFlags = flags;
            _mClientSecurity = true;

            var response = new Packet(0x5000);

            response.WriteUInt8(_mSecurityFlag);

            if (_mSecurityFlags.blowfish == 1)
            {
                _mInitialBlowfishKey = NextUInt64();

                _mBlowfish.Initialize(BitConverter.GetBytes(_mInitialBlowfishKey));

                response.WriteUInt64(_mInitialBlowfishKey);
            }
            if (_mSecurityFlags.security_bytes == 1)
            {
                _mSeedCount = NextUInt8();
                SetupCountByte(_mSeedCount);
                _mCrcSeed = NextUInt8();

                response.WriteUInt32(_mSeedCount);
                response.WriteUInt32(_mCrcSeed);
            }
            if (_mSecurityFlags.handshake == 1)
            {
                _mHandshakeBlowfishKey = NextUInt64();
                _mValueX = NextUInt32() & 0x7FFFFFFF;
                _mValueG = NextUInt32() & 0x7FFFFFFF;
                _mValueP = NextUInt32() & 0x7FFFFFFF;
                _mValueA = G_pow_X_mod_P(_mValueP, _mValueX, _mValueG);

                response.WriteUInt64(_mHandshakeBlowfishKey);
                response.WriteUInt32(_mValueG);
                response.WriteUInt32(_mValueP);
                response.WriteUInt32(_mValueA);
            }

            _mOutgoingPackets.Add(response);
        }

        private void Handshake(ushort packetOpcode, PacketReader packetData, bool packetEncrypted)
        {
            if (packetEncrypted)
                throw (new Exception("[SecurityAPI::Handshake] Received an illogical (encrypted) handshake packet."));

            if (_mClientSecurity)
            {
                // If this object does not need a handshake
                if (_mSecurityFlags.handshake == 0)
                {
                    // Client should only accept it then
                    switch (packetOpcode)
                    {
                        case 0x9000:
                            if (_mAcceptedHandshake)
                                throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (duplicate 0x9000)."));
                            _mAcceptedHandshake = true; // Otherwise, all good here
                            return;
                        case 0x5000:
                            throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (0x5000 with no handshake)."));
                        default:
                            throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (programmer error)."));
                    }
                }

                // Client accepts the handshake
                switch (packetOpcode)
                {
                    case 0x9000:
                        // Can't accept it before it's started!
                        if (!_mStartedHandshake)
                            throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (out of order 0x9000)."));
                        if (_mAcceptedHandshake) // Client error
                            throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (duplicate 0x9000)."));
                        // Otherwise, all good here
                        _mAcceptedHandshake = true;
                        return;
                    case 0x5000:
                        if (_mStartedHandshake) // Client error
                            throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (duplicate 0x5000)."));
                        _mStartedHandshake = true;
                        break;
                    default:
                        throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (programmer error)."));
                }

                _mValueB = packetData.ReadUInt32();
                _mClientKey = packetData.ReadUInt64();

                _mValueK = G_pow_X_mod_P(_mValueP, _mValueX, _mValueB);

                var keyArray = MAKELONGLONG_(_mValueA, _mValueB);
                KeyTransformValue(ref keyArray, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueK)) & 0x03));
                _mBlowfish.Initialize(BitConverter.GetBytes(keyArray));

                var tmpBytes = _mBlowfish.Decode(BitConverter.GetBytes(_mClientKey));
                _mClientKey = BitConverter.ToUInt64(tmpBytes, 0);

                keyArray = MAKELONGLONG_(_mValueB, _mValueA);
                KeyTransformValue(ref keyArray, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueB)) & 0x07));
                if (_mClientKey != keyArray)
                    throw (new Exception("[SecurityAPI::Handshake] Client signature error."));

                keyArray = MAKELONGLONG_(_mValueA, _mValueB);
                KeyTransformValue(ref keyArray, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueK)) & 0x03));
                _mBlowfish.Initialize(BitConverter.GetBytes(keyArray));

                _mChallengeKey = MAKELONGLONG_(_mValueA, _mValueB);
                KeyTransformValue(ref _mChallengeKey, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueA)) & 0x07));
                tmpBytes = _mBlowfish.Encode(BitConverter.GetBytes(_mChallengeKey));
                _mChallengeKey = BitConverter.ToUInt64(tmpBytes, 0);

                KeyTransformValue(ref _mHandshakeBlowfishKey, _mValueK, 0x3);
                _mBlowfish.Initialize(BitConverter.GetBytes(_mHandshakeBlowfishKey));

                var tmpFlags = new SecurityFlags {handshake_response = 1};
                var tmpFlag = FromSecurityFlags(tmpFlags);

                var response = new Packet(0x5000);
                response.WriteUInt8(tmpFlag);
                response.WriteUInt64(_mChallengeKey);
                _mOutgoingPackets.Add(response);
            }
            else
            {
                if (packetOpcode != 0x5000)
                    throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (programmer error)."));

                var flag = packetData.ReadByte();
                var flags = ToSecurityFlags(flag);

                if (_mSecurityFlag == 0)
                {
                    _mSecurityFlag = flag;
                    _mSecurityFlags = flags;
                }

                if (flags.blowfish == 1)
                {
                    _mInitialBlowfishKey = packetData.ReadUInt64();
                    _mBlowfish.Initialize(BitConverter.GetBytes(_mInitialBlowfishKey));
                }

                if (flags.security_bytes == 1)
                {
                    _mSeedCount = packetData.ReadUInt32();
                    _mCrcSeed = packetData.ReadUInt32();
                    SetupCountByte(_mSeedCount);
                }

                if (flags.handshake == 1)
                {
                    _mHandshakeBlowfishKey = packetData.ReadUInt64();
                    _mValueG = packetData.ReadUInt32();
                    _mValueP = packetData.ReadUInt32();
                    _mValueA = packetData.ReadUInt32();

                    _mValueX = NextUInt32() & 0x7FFFFFFF;

                    _mValueB = G_pow_X_mod_P(_mValueP, _mValueX, _mValueG);
                    _mValueK = G_pow_X_mod_P(_mValueP, _mValueX, _mValueA);

                    var keyArray = MAKELONGLONG_(_mValueA, _mValueB);
                    KeyTransformValue(ref keyArray, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueK)) & 0x03));
                    _mBlowfish.Initialize(BitConverter.GetBytes(keyArray));

                    _mClientKey = MAKELONGLONG_(_mValueB, _mValueA);
                    KeyTransformValue(ref _mClientKey, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueB)) & 0x07));
                    var tmpBytes = _mBlowfish.Encode(BitConverter.GetBytes(_mClientKey));
                    _mClientKey = BitConverter.ToUInt64(tmpBytes, 0);
                }

                if (flags.handshake_response == 1)
                {
                    _mChallengeKey = packetData.ReadUInt64();

                    var expectedChallengeKey = MAKELONGLONG_(_mValueA, _mValueB);
                    KeyTransformValue(ref expectedChallengeKey, _mValueK, (byte)(LOBYTE_(LOWORD_(_mValueA)) & 0x07));
                    var tmpBytes = _mBlowfish.Encode(BitConverter.GetBytes(expectedChallengeKey));
                    expectedChallengeKey = BitConverter.ToUInt64(tmpBytes, 0);

                    if (_mChallengeKey != expectedChallengeKey)
                        throw (new Exception("[SecurityAPI::Handshake] Server signature error."));

                    KeyTransformValue(ref _mHandshakeBlowfishKey, _mValueK, 0x3);
                    _mBlowfish.Initialize(BitConverter.GetBytes(_mHandshakeBlowfishKey));
                }

                // Generate the outgoing packet now
                if ((flags.handshake == 1) && (flags.handshake_response == 0))
                {
                    // Check to see if we already started a handshake
                    if (_mStartedHandshake || _mAcceptedHandshake)
                        throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (duplicate 0x5000)."));

                    // Handshake challenge
                    var response = new Packet(0x5000);
                    response.WriteUInt32(_mValueB);
                    response.WriteUInt64(_mClientKey);
                    _mOutgoingPackets.Insert(0, response);

                    // The handshake has started
                    _mStartedHandshake = true;
                }
                else
                {
                    // Check to see if we already accepted a handshake
                    if (_mAcceptedHandshake)
                        throw (new Exception("[SecurityAPI::Handshake] Received an illogical handshake packet (duplicate 0x5000)."));

                    // Handshake accepted
                    var response1 = new Packet(0x9000);

                    // Identify
                    var response2 = new Packet(0x2001, true, false);
                    response2.WriteAscii(_mIdentityName);
                    response2.WriteUInt8(_mIdentityFlag);

                    // Insert at the front, we want 0x9000 first, then 0x2001
                    _mOutgoingPackets.Insert(0, response2);
                    _mOutgoingPackets.Insert(0, response1);

                    // Mark the handshake as accepted now
                    _mStartedHandshake = true;
                    _mAcceptedHandshake = true;
                }
            }
        }

        private byte[] FormatPacket(ushort opcode, byte[] data, bool encrypted)
        {
            // Sanity check
            if (data.Length >= 0x8000)
                throw (new Exception("[SecurityAPI::FormatPacket] Payload is too large!"));

            var dataLength = (ushort)data.Length;

            // Add the packet header to the start of the data
            var writer = new PacketWriter();
            writer.Write(dataLength); // packet size
            writer.Write(opcode); // packet opcode
            writer.Write((ushort)0); // packet security bytes
            writer.Write(data);
            writer.Flush();

            // Determine if we need to mark the packet size as encrypted
            if (encrypted && ((_mSecurityFlags.blowfish == 1) || ((_mSecurityFlags.security_bytes == 1) && (_mSecurityFlags.blowfish == 0))))
            {
                var seekIndex = writer.BaseStream.Seek(0, SeekOrigin.Current);

                var packetSize = (ushort)(dataLength | 0x8000);

                writer.BaseStream.Seek(0, SeekOrigin.Begin);
                writer.Write(packetSize);
                writer.Flush();

                writer.BaseStream.Seek(seekIndex, SeekOrigin.Begin);
            }

            // Only need to stamp bytes if this is a clientless object
            if ((_mClientSecurity == false) && (_mSecurityFlags.security_bytes == 1))
            {
                var seekIndex = writer.BaseStream.Seek(0, SeekOrigin.Current);

                var sb1 = GenerateCountByte(true);
                writer.BaseStream.Seek(4, SeekOrigin.Begin);
                writer.Write(sb1);
                writer.Flush();

                var sb2 = GenerateCheckByte(writer.GetBytes());
                writer.BaseStream.Seek(5, SeekOrigin.Begin);
                writer.Write(sb2);
                writer.Flush();

                writer.BaseStream.Seek(seekIndex, SeekOrigin.Begin);
            }

            // If the packet should be physically encrypted, return an encrypted version of it
            if (encrypted && (_mSecurityFlags.blowfish == 1))
            {
                var rawData = writer.GetBytes();
                var encryptedData = _mBlowfish.Encode(rawData, 2, rawData.Length - 2);

                writer.BaseStream.Seek(2, SeekOrigin.Begin);

                writer.Write(encryptedData);
                writer.Flush();
            }
            else
            {
                // Determine if we need to unmark the packet size from being encrypted but not physically encrypted
                if (!encrypted || ((_mSecurityFlags.security_bytes != 1) || (_mSecurityFlags.blowfish != 0)))
                    return writer.GetBytes();

                var seekIndex = writer.BaseStream.Seek(0, SeekOrigin.Current);

                writer.BaseStream.Seek(0, SeekOrigin.Begin);
                writer.Write(dataLength);
                writer.Flush();

                writer.BaseStream.Seek(seekIndex, SeekOrigin.Begin);
            }

            // Return the final data
            return writer.GetBytes();
        }

        private bool HasPacketToSend()
        {
            // No packets, easy case
            if (_mOutgoingPackets.Count == 0)
                return false;

            // If we have packets and have accepted the handshake, we can send whenever,
            // so return true.
            if (_mAcceptedHandshake)
                return true;

            // Otherwise, check to see if we have pending handshake packets to send
            var packet = _mOutgoingPackets[0];
            return (packet.Opcode == 0x5000) || (packet.Opcode == 0x9000);

            // If we get here, we have out of order packets that cannot be sent yet.
        }

        private KeyValuePair<TransferBuffer, Packet> GetPacketToSend()
        {
            if (_mOutgoingPackets.Count == 0)
                throw (new Exception("[SecurityAPI::GetPacketToSend] No packets are avaliable to send."));

            var packet = _mOutgoingPackets[0];
            _mOutgoingPackets.RemoveAt(0);

            if (packet.Massive)
            {
                ushort parts = 0;

                var final = new PacketWriter();
                var finalData = new PacketWriter();

                var inputData = packet.GetBytes();

                var workspace = new TransferBuffer(4089, 0, inputData.Length);

                while (workspace.Size > 0)
                {
                    var partData = new PacketWriter();

                    var curSize = workspace.Size > 4089 ? 4089 : workspace.Size; // Max buffer size is 4kb for the client

                    partData.Write((byte)0); // Data flag

                    partData.Write(inputData, workspace.Offset, curSize);

                    workspace.Offset += curSize;
                    workspace.Size -= curSize; // Update the size

                    finalData.Write(FormatPacket(0x600D, partData.GetBytes(), false));

                    ++parts; // Track how many parts there are
                }

                // Write the final header packet to the front of the packet
                var finalHeader = new PacketWriter();
                finalHeader.Write((byte)1); // Header flag
                finalHeader.Write((short)parts);
                finalHeader.Write(packet.Opcode);
                final.Write(FormatPacket(0x600D, finalHeader.GetBytes(), false));

                // Finish the large packet of all the data
                final.Write(finalData.GetBytes());

                // Return the collated data
                var rawBytes = final.GetBytes();
                packet.Lock();
                return new KeyValuePair<TransferBuffer, Packet>(new TransferBuffer(rawBytes, 0, rawBytes.Length, true), packet);
            }
            else
            {
                var encrypted = packet.Encrypted;
                if (!_mClientSecurity)
                {
                    if (_mEncOpcodes.Contains(packet.Opcode))
                    {
                        encrypted = true;
                    }
                }
                var rawBytes = FormatPacket(packet.Opcode, packet.GetBytes(), encrypted);
                packet.Lock();
                return new KeyValuePair<TransferBuffer, Packet>(new TransferBuffer(rawBytes, 0, rawBytes.Length, true), packet);
            }
        }
#endregion

        // Default constructor
        public Security()
        {
            _mValueX = 0;
            _mValueG = 0;
            _mValueP = 0;
            _mValueA = 0;
            _mValueB = 0;
            _mValueK = 0;
            _mSeedCount = 0;
            _mCrcSeed = 0;
            _mInitialBlowfishKey = 0;
            _mHandshakeBlowfishKey = 0;
            _mCountByteSeeds = new byte[3];
            _mCountByteSeeds[0] = 0;
            _mCountByteSeeds[1] = 0;
            _mCountByteSeeds[2] = 0;
            _mClientKey = 0;
            _mChallengeKey = 0;

            _mClientSecurity = false;
            _mSecurityFlag = 0;
            _mSecurityFlags = new SecurityFlags();
            _mAcceptedHandshake = false;
            _mStartedHandshake = false;
            _mIdentityFlag = 0;
            _mIdentityName = "SR_Client";

            _mOutgoingPackets = new List<Packet>();
            _mIncomingPackets = new List<Packet>();

            _mEncOpcodes = new List<ushort> {0x2001, 0x6100, 0x6101, 0x6102, 0x6103, 0x6107};

            _mBlowfish = new Blowfish();

            _mRecvBuffer = new TransferBuffer(8192); // must be at minimal 2 bytes!
            _mCurrentBuffer = null;

            _mMassiveCount = 0;
            _mMassivePacket = null;

            _mClassLock = new object();
        }

        // Changes the 0x2001 identify packet data that will be sent out by
        // this security object.
        public void ChangeIdentity(string name, byte flag)
        {
            lock (_mClassLock)
            {
                _mIdentityName = name;
                _mIdentityFlag = flag;
            }
        }

        // Generates the security settings. This should only be called if the security object
        // is being used to process an incoming connection's data (server).
        public void GenerateSecurity(bool blowfish, bool securityBytes, bool handshake)
        {
            lock (_mClassLock)
            {
                var flags = new SecurityFlags();
                if (blowfish)
                {
                    flags.none = 0;
                    flags.blowfish = 1;
                }
                if (securityBytes)
                {
                    flags.none = 0;
                    flags.security_bytes = 1;
                }
                if (handshake)
                {
                    flags.none = 0;
                    flags.handshake = 1;
                }
                if (!blowfish && !securityBytes && !handshake)
                {
                    flags.none = 1;
                }
                GenerateSecurity(flags);
            }
        }

        // Adds an encrypted opcode to the API. Any opcodes registered here will automatically
        // be encrypted as needed. Users should add the current Item Use opcode if they will be
        // mixing security modes.
        public void AddEncryptedOpcode(ushort opcode)
        {
            lock (_mClassLock)
            {
                if (_mEncOpcodes.Contains(opcode) == false)
                {
                    _mEncOpcodes.Add(opcode);
                }
            }
        }

        // Queues a packet for processing to be sent. The data is simply formatted and processed during
        // the next call to TransferOutgoing.
        public void Send(Packet packet)
        {
            if ((packet.Opcode == 0x5000) || (packet.Opcode == 0x9000))
            {
                throw (new Exception("[SecurityAPI::Send] Handshake packets cannot be sent through this function."));
            }
            lock (_mClassLock)
            {
                _mOutgoingPackets.Add(packet);
            }
        }

        // Transfers raw incoming data into the security object. Call TransferIncoming to
        // obtain a list of ready to process packets.
        public void Recv(byte[] buffer, int offset, int length) => Recv(new TransferBuffer(buffer, offset, length, true));

        // Transfers raw incoming data into the security object. Call TransferIncoming to
        // obtain a list of ready to process packets.
        public void Recv(TransferBuffer rawBuffer)
        {
            var incomingBuffersTmp = new List<TransferBuffer>();
            lock (_mClassLock)
            {
                var length = rawBuffer.Size - rawBuffer.Offset;
                var index = 0;
                while (length > 0)
                {
                    var maxLength = length;
                    var calcLength = _mRecvBuffer.Buffer.Length - _mRecvBuffer.Size;

                    if (maxLength > calcLength)
                    {
                        maxLength = calcLength;
                    }
                    length -= maxLength;

                    Buffer.BlockCopy(rawBuffer.Buffer, rawBuffer.Offset + index, _mRecvBuffer.Buffer, _mRecvBuffer.Size, maxLength);

                    _mRecvBuffer.Size += maxLength;
                    index += maxLength;

                    // Loop while we have data to process
                    while (_mRecvBuffer.Size > 0)
                    {
                        // If we do not have a current packet object, try to allocate one.
                        if (_mCurrentBuffer == null)
                        {
                            // We need at least two bytes to allocate a packet.
                            if (_mRecvBuffer.Size < 2)
                            {
                                break;
                            }

                            // Calculate the packet size.
                            var packetSize = (_mRecvBuffer.Buffer[1] << 8) | _mRecvBuffer.Buffer[0];

                            // Check to see if this packet is encrypted.
                            if ((packetSize & 0x8000) > 0)
                            {
                                // If so, calculate the total payload size.
                                packetSize &= 0x7FFF; // Mask off the encryption.
                                if (_mSecurityFlags.blowfish == 1)
                                {
                                    packetSize = 2 + _mBlowfish.GetOutputLength(packetSize + 4);
                                }
                                else
                                {
                                    packetSize += 6;
                                }
                            }
                            else
                            {
                                // The packet is unencrypted. The final size is simply
                                // header size + payload size.
                                packetSize += 6;
                            }

                            // Allocate the final buffer the packet will be written to
                            _mCurrentBuffer = new TransferBuffer(packetSize, 0, packetSize);
                        }

                        // Calculate how many bytes are left to receive in the packet.
                        var maxCopyCount = _mCurrentBuffer.Size - _mCurrentBuffer.Offset;

                        // If we need more bytes than we currently have, update the size.
                        if (maxCopyCount > _mRecvBuffer.Size)
                        {
                            maxCopyCount = _mRecvBuffer.Size;
                        }

                        // Copy the buffer data to the packet buffer
                        Buffer.BlockCopy(_mRecvBuffer.Buffer, 0, _mCurrentBuffer.Buffer, _mCurrentBuffer.Offset, maxCopyCount);

                        // Update how many bytes we now have
                        _mCurrentBuffer.Offset += maxCopyCount;
                        _mRecvBuffer.Size -= maxCopyCount;

                        // If there is data remaining in the buffer, copy it over the data
                        // we just removed (sliding buffer).
                        if (_mRecvBuffer.Size > 0)
                        {
                            Buffer.BlockCopy(_mRecvBuffer.Buffer, maxCopyCount, _mRecvBuffer.Buffer, 0, _mRecvBuffer.Size);
                        }

                        // Check to see if the current packet is now complete.
                        if (_mCurrentBuffer.Size == _mCurrentBuffer.Offset)
                        {
                            // If so, dispatch it to the manager class for processing by the system.
                            _mCurrentBuffer.Offset = 0;
                            incomingBuffersTmp.Add(_mCurrentBuffer);

                            // Set the current packet to null so we can process the next packet
                            // in the stream.
                            _mCurrentBuffer = null;
                        }
                        else
                        {
                            // Otherwise, we are done with this loop, since we need more
                            // data for the current packet.
                            break;
                        }
                    }
                }

                if (incomingBuffersTmp.Count > 0)
                {
                    foreach (var buffer in incomingBuffersTmp)
                    {
                        var packetEncrypted = false;

                        var packetSize = (buffer.Buffer[1] << 8) | buffer.Buffer[0];
                        if ((packetSize & 0x8000) > 0)
                        {
                            if (_mSecurityFlags.blowfish == 1)
                            {
                                packetSize &= 0x7FFF;
                                packetEncrypted = true;
                            }
                            else
                            {
                                packetSize &= 0x7FFF;
                            }
                        }

                        if (packetEncrypted)
                        {
                            var decrypted = _mBlowfish.Decode(buffer.Buffer, 2, buffer.Size - 2);
                            var newBuffer = new byte[6 + packetSize];
                            Buffer.BlockCopy(BitConverter.GetBytes((ushort)packetSize), 0, newBuffer, 0, 2);
                            Buffer.BlockCopy(decrypted, 0, newBuffer, 2, 4 + packetSize);
                            buffer.Buffer = null;
                            buffer.Buffer = newBuffer;
                        }

                        var packetData = new PacketReader(buffer.Buffer);
                        packetSize = packetData.ReadUInt16();
                        var packetOpcode = packetData.ReadUInt16();
                        var packetSecurityCount = packetData.ReadByte();
                        var packetSecurityCrc = packetData.ReadByte();

                        // Client object whose bytes the server might need to verify
                        if (_mClientSecurity)
                        {
                            if (_mSecurityFlags.security_bytes == 1)
                            {
                                var expectedCount = GenerateCountByte(true);
                                if (packetSecurityCount != expectedCount)
                                {
                                    throw (new Exception("[SecurityAPI::Recv] Count byte mismatch."));
                                }

                                if (packetEncrypted || ((_mSecurityFlags.security_bytes == 1) && (_mSecurityFlags.blowfish == 0)))
                                {
                                    if (packetEncrypted || _mEncOpcodes.Contains(packetOpcode))
                                    {
                                        packetSize |= 0x8000;
                                        Buffer.BlockCopy(BitConverter.GetBytes((ushort)packetSize), 0, buffer.Buffer, 0, 2);
                                    }
                                }

                                buffer.Buffer[5] = 0;

                                var expectedCrc = GenerateCheckByte(buffer.Buffer);
                                if (packetSecurityCrc != expectedCrc)
                                {
                                    throw (new Exception("[SecurityAPI::Recv] CRC byte mismatch."));
                                }

                                buffer.Buffer[4] = 0;

                                if (packetEncrypted || ((_mSecurityFlags.security_bytes == 1) && (_mSecurityFlags.blowfish == 0)))
                                {
                                    if (packetEncrypted || _mEncOpcodes.Contains(packetOpcode))
                                    {
                                        packetSize &= 0x7FFF;
                                        Buffer.BlockCopy(BitConverter.GetBytes((ushort)packetSize), 0, buffer.Buffer, 0, 2);
                                    }
                                }
                            }
                        }

                        if ((packetOpcode == 0x5000) || (packetOpcode == 0x9000)) // New logic processing!
                        {
                            Handshake(packetOpcode, packetData, packetEncrypted);

                            // Pass the handshake packets to the user so they can at least see them.
                            // They do not need to actually do anything with them. This was added to
                            // help debugging and make output logs complete.

                            var packet = new Packet(packetOpcode, packetEncrypted, false, buffer.Buffer, 6, packetSize);
                            packet.Lock();
                            _mIncomingPackets.Add(packet);
                        }
                        else
                        {
                            if (_mClientSecurity)
                            {
                                // Make sure the client accepted the security system first
                                if (!_mAcceptedHandshake)
                                {
                                    throw (new Exception("[SecurityAPI::Recv] The client has not accepted the handshake."));
                                }
                            }
                            if (packetOpcode == 0x600D) // Auto process massive messages for the user
                            {
                                var mode = packetData.ReadByte();
                                if (mode == 1)
                                {
                                    _mMassiveCount = packetData.ReadUInt16();
                                    var containedPacketOpcode = packetData.ReadUInt16();
                                    _mMassivePacket = new Packet(containedPacketOpcode, packetEncrypted, true);
                                }
                                else
                                {
                                    if (_mMassivePacket == null)
                                    {
                                        throw (new Exception("[SecurityAPI::Recv] A malformed 0x600D packet was received."));
                                    }
                                    _mMassivePacket.WriteUInt8Array(packetData.ReadBytes(packetSize - 1));
                                    _mMassiveCount--;
                                    if (_mMassiveCount == 0)
                                    {
                                        _mMassivePacket.Lock();
                                        _mIncomingPackets.Add(_mMassivePacket);
                                        _mMassivePacket = null;
                                    }
                                }
                            }
                            else
                            {
                                var packet = new Packet(packetOpcode, packetEncrypted, false, buffer.Buffer, 6, packetSize);
                                packet.Lock();
                                _mIncomingPackets.Add(packet);
                            }
                        }
                    }
                }
            }
        }

        // Returns a list of buffers that is ready to be sent. These buffers must be sent in order.
        // If no buffers are available for sending, null is returned.
        public List<KeyValuePair<TransferBuffer, Packet>> TransferOutgoing()
        {
            List<KeyValuePair<TransferBuffer, Packet>> buffers = null;
            lock (_mClassLock)
            {
                if (HasPacketToSend())
                {
                    buffers = new List<KeyValuePair<TransferBuffer, Packet>>();
                    while (HasPacketToSend())
                    {
                        buffers.Add(GetPacketToSend());
                    }
                }
            }
            return buffers;
        }

        // Returns a list of all packets that are ready for processing. If no packets are available,
        // null is returned.
        public List<Packet> TransferIncoming()
        {
            List<Packet> packets = null;
            lock (_mClassLock)
            {
                if (_mIncomingPackets.Count > 0)
                {
                    packets = _mIncomingPackets;
                    _mIncomingPackets = new List<Packet>();
                }
            }
            return packets;
        }
    }
}
