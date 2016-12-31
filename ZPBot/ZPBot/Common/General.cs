
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
        public static string IniPath;

        public static bool Usehp = false;
        public static bool Usemp = false;
        public static bool Useuni = false;
        public static bool Gmtag = false;
        public static bool Autologin = false;
        public static bool LaunchClientless = false;

        public static string LoginId = "";
        public static string LoginPw = "";
        public static string LoginChar = "";

        public static bool PickupMyitems = false;
        public static bool Botstate = false;

        public static string WalkscriptLoop = null;
        public static string SroPath = null;

        public static bool HpLoop = false;
        public static bool MpLoop = false;
        public static bool UniLoop = false;
        public static bool AmmoLoop = false;
        public static bool DrugsLoop = false;
        public static bool ScrollsLoop = false;
        public static ushort HpLoopcount = 0;
        public static ushort MpLoopcount = 0;
        public static ushort UniLoopcount = 0;
        public static ushort AmmoLoopcount = 0;
        public static ushort DrugsLoopcount = 0;
        public static ushort ScrollsLoopcount = 0;
        public static uint HpLooptype = 0;
        public static uint MpLooptype = 0;
        public static uint UniLooptype = 0;
        public static uint AmmoLooptype = 0;
        public static uint DrugsLooptype = 0;
        public static uint ScrollsLooptype = 0;

        public static bool ReturntownDied = false;
        public static bool ReturntownNoPotion = false;
        public static bool ReturntownNoAmmo = false;

        public static int UsehpPercent = 0;
        public static int UsempPercent = 0;
        public static int UsePethpPercent = 70;

        public static bool UseZerk = false;
        public static int UseZerktype = 0;
        public static bool UseSpeeddrug = false;

        public static byte PlusToreach = 0;
        public static bool LogPackets = false;
    }

    internal class Game
    {
        public static int Range;
        public static int RangeXpos, RangeYpos;
        public static bool Clientless = false;

        public static uint SelectedNpc = 0;
        public static uint SelectedMonster = 0;
        public static uint AttackBlacklist = 0;
        public static bool Blocknpcanswer = false;
        public static bool RecordLoop = false;

        public static bool AllowCast = false;
        public static bool AllowBuy = false;
        public static bool AllowSell = false;
        public static bool AllowStack = false;
        public static bool AllowFuse = false;

        public static bool IsPicking = false;
        public static bool IsWalking = false;
        public static bool IsTeleporting = false;
        public static bool IsLooping = false;

        public static bool Fusing = false;
        public static byte SlotFuseitem = 0;

        public static byte[,] ChatCount = new byte[6, 2];
        public static bool ReturnChatcount = true;

        public static EGamePosition GetRangePosition()
        {
            return new EGamePosition
            {
                XPos = RangeXpos,
                YPos = RangeYpos
            };
        }
    }
}
