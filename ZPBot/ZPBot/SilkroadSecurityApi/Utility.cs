﻿using System;
using System.Text;
using ZPBot.Annotations;

namespace ZPBot.SilkroadSecurityApi
{
    public class Utility
    {
        public static string HexDump([NotNull] byte[] buffer) => HexDump(buffer, 0, buffer.Length);

        [NotNull]
        public static string HexDump(byte[] buffer, int offset, int count)
        {
            const int bytesPerLine = 16;
            var output = new StringBuilder();
            var asciiOutput = new StringBuilder();
            var length = count;
            if (length % bytesPerLine != 0)
            {
                length += bytesPerLine - length % bytesPerLine;
            }
            for (var x = 0; x <= length; ++x)
            {
                if (x % bytesPerLine == 0)
                {
                    if (x > 0)
                    {
                        output.AppendFormat("  {0}{1}", asciiOutput, Environment.NewLine);
                        asciiOutput.Clear();
                    }
                    if (x != length)
                        output.AppendFormat("{0:d10}   ", x);
                }
                if (x < count)
                {
                    output.AppendFormat("{0:X2} ", buffer[offset + x]);
                    var ch = (char)buffer[offset + x];
                    if (!char.IsControl(ch))
                        asciiOutput.AppendFormat("{0}", ch);
                    else
                        asciiOutput.Append(".");
                }
                else
                {
                    output.Append("   ");
                    asciiOutput.Append(".");
                }
            }

            return output.ToString();
        }
    }
}
