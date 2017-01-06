using System;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common
{
    internal class Client
    {
        public static ushort ClientVersion { get; set; }
        public static uint ClientLocale { get; set; } = 22;
        public static ushort ServerId { get; set; }
        public static string LocalIp { get; } = "127.0.0.1";
        public static ushort LocalGatewayPort { get; set; }
        public static ushort LocalAgentPort { get; set; }
        public static uint ServerLoginid { get; set; }
        public static string ServerIp { get; set; }
        public static int ServerPort { get; set; }
        public static string ImageCode { get; }= "";
        public static int Language { get; } = 9;
    }

    internal class Config
    {
        public static string Version = "1.1.2";
        public static bool Debug = false;
    }

    internal class Game
    {
        public static double Distance([NotNull] GamePosition sourcePosition, [NotNull] GamePosition itemPosition) => Math.Sqrt(Math.Pow(sourcePosition.XPos - itemPosition.XPos, 2) + Math.Pow(sourcePosition.YPos - itemPosition.YPos, 2));

        [NotNull]
        public static GamePosition PositionToGamePosition(EPosition position) => new GamePosition((int)((position.XSection - 135) * 192 + position.XPosition / 10),
            (int)((position.YSection - 92) * 192 + position.YPosition / 10));

        public static bool AllowStack = false;
        public static bool AllowFuse = false;

        public static bool Fusing = false;
        public static byte SlotFuseitem = 0;

        public static byte[,] ChatCount = new byte[6, 2];
        public static bool ReturnChatcount = true;
    }
}
