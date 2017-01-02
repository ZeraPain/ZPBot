using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Loop
{
    internal class LoopManager
    {
        private enum ShopType
        {
            Potion,
            Blacksmith,
            Accessory
        }

        private struct Walkscript
        {
            public string Type;
            public int Param1;
            public int Param2;
        }

        private readonly GlobalManager _globalManager;
        private readonly List<Walkscript> _walkScript;
        private readonly List<Walkscript> _recordScript;

        public string WalkscriptPath { get; set; }

        private Thread _tTeleport;
        private Thread _tLoop;
        private Thread _tRecord;

        private bool _townScript;
        private const int Delay = 200;
        private const int Timeout = 5000;
        private readonly object _lock;
        public bool IsTeleporting { get; set; }
        public bool IsLooping { get; protected set; }
        public bool IsWalking { get; set; }
        public bool AllowBuy { get; set; }
        public bool AllowSell { get; set; }
        public bool BlockNpcAnswer { get; set; }
        public uint SelectedNpc { get; set; }
        public bool RecordLoop { get; protected set; }

        public LoopOption HpLoopOption { get; set; }
        public LoopOption MpLoopOption { get; set; }
        public LoopOption UniLoopOption { get; set; }
        public LoopOption AmmoLoopOption { get; set; }
        public LoopOption SpeedLoopOption { get; set; }
        public LoopOption ReturnLoopOption { get; set; }

        public LoopManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            _walkScript = new List<Walkscript>();
            _recordScript = new List<Walkscript>();

            _lock = new object();

            HpLoopOption = new LoopOption();
            MpLoopOption = new LoopOption();
            UniLoopOption = new LoopOption();
            AmmoLoopOption = new LoopOption();
            SpeedLoopOption = new LoopOption();
            ReturnLoopOption = new LoopOption();
        }

        private void Loop()
        {
            foreach (var cmd in _walkScript)
            {
                if (!IsLooping)
                    return;

                switch (cmd.Type)
                {
                    case "walk":
                        Walk(cmd.Param1, cmd.Param2);
                        break;
                    case "tele":
                        Teleport((uint)cmd.Param1, (uint)cmd.Param2);
                        break;
                    case "potion":
                        Shop((uint)cmd.Param1, (byte)cmd.Param2, ShopType.Potion);
                        break;
                    case "blacksmith":
                        Shop((uint)cmd.Param1, (byte)cmd.Param2, ShopType.Blacksmith);
                        break;
                    case "accessory":
                        Shop((uint)cmd.Param1, (byte)cmd.Param2, ShopType.Accessory);
                        break;
                    default:
                        Console.WriteLine(@"Unknown Command in Walkscript");
                        break;

                }

                Thread.Sleep(Delay);
            }

            if (_townScript)
                StartLoop(false);
            else
                _globalManager.StartBot(true);
        }

        public void StartRecord()
        {
            RecordLoop = true;

            _tRecord = new Thread(Record);
            _tRecord.Start();
        }

        public void StopRecord()
        {
            RecordLoop = false;
            _tRecord?.Abort();
        }

        private void Record()
        {
            int checkPointXPos = _globalManager.Player.InGamePosition.XPos,
                checkPointYPos = _globalManager.Player.InGamePosition.YPos;
            AddPosition(checkPointXPos, checkPointYPos);

            while (RecordLoop)
            {
                double distance;
                do
                {
                    distance = Game.Distance(_globalManager.Player.InGamePosition,
                        new GamePosition(checkPointXPos, checkPointYPos));
                    Thread.Sleep(50);
                } while (((distance < 20)  && RecordLoop) || IsTeleporting);

                checkPointXPos = _globalManager.Player.InGamePosition.XPos;
                checkPointYPos = _globalManager.Player.InGamePosition.YPos;
                AddPosition(checkPointXPos, checkPointYPos);
            }
        }

        private void AddPosition(int xPos, int yPos)
        {
            lock (_lock)
            {
                var tmp = new Walkscript
                {
                    Type = "walk",
                    Param1 = xPos,
                    Param2 = yPos
                };
                _recordScript.Add(tmp);
            }
        }

        public void AddTeleport(uint source, uint dest)
        {
            lock (_lock)
            {
                var tmp = new Walkscript
                {
                    Type = "tele",
                    Param1 = (int) source,
                    Param2 = (int) dest
                };
                _recordScript.Add(tmp);
            }
        }

        public void SaveScript([NotNull] string path)
        {
            TextWriter tw = new StreamWriter(path);

            if (_recordScript != null)
            {
                lock (_lock)
                {
                    foreach (var cmd in _recordScript)
                    {
                        tw.WriteLine(cmd.Type + ";" + cmd.Param1 + ";" + cmd.Param2);
                    }
                }

                tw.Close();
                lock (_lock)
                {
                    _recordScript.Clear();
                }
            }
        }

        private void Load(string path)
        {
            if (path != "" && File.Exists(path))
            {
                _walkScript.Clear();

                var lines = File.ReadAllLines(path);
                foreach (var tmp in from t in lines select t.Split(';') into tmpString where tmpString.Length == 3 select new Walkscript
                {
                    Type = tmpString[0],
                    Param1 = int.Parse(tmpString[1]),
                    Param2 = int.Parse(tmpString[2])
                })
                {
                    _walkScript.Add(tmp);
                }
            }
            else
            {
                Console.WriteLine(@"Load - No Walkscript is set");
            }
        }

        public void StartLoop(bool returnTown)
        {
            IsLooping = true;
            _townScript = returnTown;

            if (returnTown)
            {
                Load("scripts\\hotan.txt");
                _tTeleport = new Thread(TeleportDelay);
                _tTeleport.Start();
            }
            else
            {
                Load(WalkscriptPath);
                _tLoop = new Thread(Loop);
                _tLoop.Start();
            }
        }

        public void StopLoop()
        {
            IsLooping = false;
            IsTeleporting = false;
            _tTeleport?.Abort();
            _tLoop?.Abort();
        }

        private void TeleportDelay()
        {
            while (IsTeleporting)
            {
                Thread.Sleep(Delay);
                if (!IsLooping)
                    return;
            }

            if (_tLoop != null && _tLoop.IsAlive) _tLoop.Abort();
            _tLoop = new Thread(Loop);
            _tLoop.Start();
        }

        private void Walk(int xPos, int yPos)
        {
            var distance = Game.Distance(_globalManager.Player.InGamePosition, new GamePosition(xPos, yPos));

            IsWalking = false;
            _globalManager.PacketManager.MoveToCoords(xPos, yPos);

            if (_globalManager.Clientless)
            {
                while (!IsWalking && IsLooping) Thread.Sleep(Delay);

                var estimatedTime = (distance / 10) * (100 / _globalManager.Player.Runspeed);
                Thread.Sleep((int)(estimatedTime * 1000));
            }
            else
            {
                double realDistance;

                do
                {
                    _globalManager.PacketManager.MoveToCoords(xPos, yPos);
                    realDistance = Game.Distance(_globalManager.Player.InGamePosition, new GamePosition(xPos, yPos));
                    Thread.Sleep(50);
                } while ((realDistance > 10) && IsLooping);
            }
        }

        private void Teleport(uint source, uint dest)
        {
            _globalManager.PacketManager.Teleport(source, dest);
            IsTeleporting = true;

            while (IsTeleporting && IsLooping)
                Thread.Sleep(Delay);
        }

        private void Select(uint worldId)
        {
            SelectedNpc = 0;
            _globalManager.PacketManager.SelectMonster(worldId);

            var timeout = new Stopwatch();
            timeout.Start();

            do
            {
                if (timeout.ElapsedMilliseconds > Timeout)
                {
                    _globalManager.PacketManager.SelectMonster(worldId);
                    timeout.Restart();
                }
                Thread.Sleep(Delay);
            } while (SelectedNpc != worldId);
        }

        private void HandShake(uint worldId, byte option)
        {
            AllowBuy = false;
            _globalManager.PacketManager.TalkToNpc(worldId, option);

            var timeout = new Stopwatch();
            timeout.Start();

            do
            {
                if (timeout.ElapsedMilliseconds > Timeout)
                {
                    _globalManager.PacketManager.TalkToNpc(worldId, option);
                    timeout.Restart();
                }
                Thread.Sleep(Delay);
            } while (!AllowBuy);
        }

        private void Shop(uint npcId, byte option, ShopType type)
        {
            var worldId = _globalManager.NpcManager.GetWorldId(npcId);
            if (worldId != 0)
            {
                //Select NPC
                Select(worldId);
                if (Config.Debug) Console.WriteLine(type + @": Selected");

                //Begin NPC Handshake
                HandShake(worldId, option);
                if (Config.Debug) Console.WriteLine(type + @": Allowed to buy");

                var srData = Silkroad.GetShopData(npcId);
                switch (type)
                {
                    case ShopType.Potion:
                        //Buy HP Potions
                        var hpData = GetItemDataToBuy(srData, HpLoopOption.BuyType.Id);
                        if (HpLoopOption.Enabled && (hpData.ItemId > 0))
                        {
                            BuyItems(hpData, worldId, HpLoopOption.BuyAmount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                            if (Config.Debug) Console.WriteLine(type + @": Bought HP");
                        }

                        //Buy MP Potions
                        var mpData = GetItemDataToBuy(srData, MpLoopOption.BuyType.Id);
                        if (MpLoopOption.Enabled && (mpData.ItemId > 0))
                        {
                            BuyItems(mpData, worldId, MpLoopOption.BuyAmount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                            if (Config.Debug) Console.WriteLine(type + @": Bought MP");
                        }

                        //Buy Universal Pills
                        var uniData = GetItemDataToBuy(srData, UniLoopOption.BuyType.Id);
                        if (UniLoopOption.Enabled && (uniData.ItemId > 0))
                        {
                            BuyItems(uniData, worldId, UniLoopOption.BuyAmount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                            if (Config.Debug) Console.WriteLine(type + @": Bought Universal Pills");
                        }
                        break;
                    case ShopType.Blacksmith:
                        //Sell Items
                        Sell();

                        //Repair...
                        _globalManager.PacketManager.RepairAll(worldId);
                        Thread.Sleep(1000);

                        //Buy Arrows / Bolts
                        var ammoData = GetItemDataToBuy(srData, AmmoLoopOption.BuyType.Id);
                        if (AmmoLoopOption.Enabled && (ammoData.ItemId > 0)) BuyItems(ammoData, worldId, AmmoLoopOption.BuyAmount);
                        if (Config.Debug) Console.WriteLine(type + @": Bought Ammo");
                        break;
                    case ShopType.Accessory:
                        //Buy Drugs
                        var drugsData = GetItemDataToBuy(srData, SpeedLoopOption.BuyType.Id);
                        if (SpeedLoopOption.Enabled && (drugsData.ItemId > 0))
                        {
                            BuyItems(drugsData, worldId, SpeedLoopOption.BuyAmount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                        }

                        if (Config.Debug) Console.WriteLine(type + @": Bought Speed Drugs");

                        //Buy Return Scrolls
                        var scrollsData = GetItemDataToBuy(srData, ReturnLoopOption.BuyType.Id);
                        if (ReturnLoopOption.Enabled && (scrollsData.ItemId > 0))
                        {
                            BuyItems(scrollsData, worldId, ReturnLoopOption.BuyAmount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                        }

                        if (Config.Debug) Console.WriteLine(type + @": Bought Return Scrolls");
                        break;
                }

                //Close NPC
                _globalManager.PacketManager.CancelNpcTalk(worldId);
            }
            else
            {
                if (Config.Debug) Console.WriteLine(@"Cannot find NPC: " + npcId);
            }
        }

        private void Buy(uint worldId, byte tab, byte slot, ushort quantity)
        {
            AllowBuy = false;
            BlockNpcAnswer = true;
            _globalManager.PacketManager.BuyFromNpc(worldId, tab, slot, quantity);

            var timeout = new Stopwatch();
            timeout.Start();

            do
            {
                if (timeout.ElapsedMilliseconds > Timeout)
                {
                    BlockNpcAnswer = true;
                    _globalManager.PacketManager.BuyFromNpc(worldId, tab, slot, quantity);
                    timeout.Restart();
                }
                Thread.Sleep(Delay);
            } while (!AllowBuy);
        }

        private void Sell()
        {
            AllowSell = false;

            byte slot = 0;
            while (slot != 0)
            {
                _globalManager.PacketManager.SellItem(SelectedNpc, slot);

                var timeout = new Stopwatch();
                timeout.Start();

                do
                {
                    if (timeout.ElapsedMilliseconds > Timeout)
                    {
                        _globalManager.PacketManager.SellItem(SelectedNpc, slot);
                        timeout.Restart();
                    }
                    Thread.Sleep(Delay);
                } while (!AllowSell);

                //slot = _inventoryManager.GetSellItem();
            }
        }

        private void BuyItems(Silkroad.Shop data, uint worldId, ushort quantity)
        {
            if (data.ItemId == 0)
                return;

            var item = Silkroad.GetItemById(data.ItemId);
            var quantityToBuy = quantity - _globalManager.InventoryManager.GetItemCount(data.ItemId);
            if (Config.Debug)
                if (item != null) Console.WriteLine(@"Buying " + item.Code + @" (" + quantityToBuy + @")");

            while (quantityToBuy > 0)
            {
                if (!IsLooping)
                    return;

                AllowBuy = true;

                if (item != null && quantityToBuy > item.MaxQuantity)
                {
                    Buy(worldId, data.TabIndex, data.SlotIndex, item.MaxQuantity);
                    quantityToBuy -= item.MaxQuantity;
                }
                else
                {
                    Buy(worldId, data.TabIndex, data.SlotIndex, (ushort) quantityToBuy);
                    quantityToBuy = 0;
                }
            }
        }

        private static Silkroad.Shop GetItemDataToBuy([NotNull] List<Silkroad.Shop> shopdata, uint itemId) => shopdata.Find(temp => temp.ItemId == itemId);
    }
}
