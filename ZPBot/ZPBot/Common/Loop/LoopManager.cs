using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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

        private Thread _tTeleport;
        private Thread _tLoop;
        private Thread _tRecord;

        private bool _townScript;
        private const int Delay = 200;
        private const int Timeout = 5000;
        private readonly object _lock;

        public LoopManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            _walkScript = new List<Walkscript>();
            _recordScript = new List<Walkscript>();

            _lock = new object();
        }

        private void Loop()
        {
            foreach (var cmd in _walkScript)
            {
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

                if (!Game.IsLooping)
                    return;
            }

            if (_townScript)
                StartLoop(false);
            else
                _globalManager.StartBot(true);
        }

        public void StartRecord()
        {
            _tRecord = new Thread(Record);
            _tRecord.Start();
        }

        private void Record()
        {
            var charPos = _globalManager.Player.GetIngamePosition();

            int checkPointXPos = charPos.XPos, 
                checkPointYPos = charPos.YPos;
            AddPosition(checkPointXPos, checkPointYPos);

            while (Game.RecordLoop)
            {
                double distance;
                do
                {
                    charPos = _globalManager.Player.GetIngamePosition();
                    distance = Math.Sqrt(Math.Pow((charPos.XPos - checkPointXPos), 2) + Math.Pow((charPos.YPos - checkPointYPos), 2));
                    Thread.Sleep(50);
                } while ((distance < 20  && Game.RecordLoop) || Game.IsTeleporting);

                checkPointXPos = charPos.XPos;
                checkPointYPos = charPos.YPos;
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

        public void SaveScript(string path)
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
            Game.IsLooping = true;
            _townScript = returnTown;

            if (returnTown)
            {
                Load("scripts\\hotan.txt");
                _tTeleport = new Thread(TeleportDelay);
                _tTeleport.Start();
            }
            else
            {
                Load(Config.WalkscriptLoop);
                _tLoop = new Thread(Loop);
                _tLoop.Start();
            }
        }

        private void TeleportDelay()
        {
            while (Game.IsTeleporting)
            {
                Thread.Sleep(Delay);
                if (!Game.IsLooping)
                    return;
            }

            if (_tLoop != null && _tLoop.IsAlive) _tLoop.Abort();
            _tLoop = new Thread(Loop);
            _tLoop.Start();
        }

        private void Walk(int xPos, int yPos)
        {
            var charPos = _globalManager.Player.GetIngamePosition();
            var distance = Math.Sqrt(Math.Pow((charPos.XPos - xPos), 2) + Math.Pow((charPos.YPos - yPos), 2));

            Game.IsWalking = false;
            _globalManager.PacketManager.MoveToCoords(xPos, yPos);

            if (Game.Clientless)
            {
                while (!Game.IsWalking && Game.IsLooping) Thread.Sleep(Delay);
                var estimatedTime = (distance / 10) * (100 / _globalManager.Player.Runspeed);
                Thread.Sleep((int)(estimatedTime * 1000));
            }
            else
            {
                double realDistance;

                do
                {
                    _globalManager.PacketManager.MoveToCoords(xPos, yPos);
                    var realCharPos = _globalManager.Player.GetIngamePosition();
                    realDistance = Math.Sqrt(Math.Pow((realCharPos.XPos - xPos), 2) + Math.Pow((realCharPos.YPos - yPos), 2));
                    Thread.Sleep(50);
                } while (realDistance > 10 && Game.IsLooping);
            }
        }

        private void Teleport(uint source, uint dest)
        {
            _globalManager.PacketManager.Teleport(source, dest);
            Game.IsTeleporting = true;

            while (Game.IsTeleporting && Game.IsLooping)
            {
                Thread.Sleep(Delay);
            }
        }

        private void Select(uint worldId)
        {
            Game.SelectedNpc = 0;
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
            } while (Game.SelectedNpc != worldId); 
        }

        private void HandShake(uint worldId, byte option)
        {
            Game.AllowBuy = false;
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
            } while (!Game.AllowBuy); 
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
                        var hpData = GetItemDataToBuy(srData, Config.HpLooptype);
                        if (Config.HpLoop)
                        {
                            BuyItems(hpData, worldId, Config.HpLooptype, Config.HpLoopcount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                            if (Config.Debug) Console.WriteLine(type + @": Bought HP");
                        } 

                        //Buy MP Potions
                        var mpData = GetItemDataToBuy(srData, Config.MpLooptype);
                        if (Config.MpLoop)
                        {
                            BuyItems(mpData, worldId, Config.MpLooptype, Config.MpLoopcount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                            if (Config.Debug) Console.WriteLine(type + @": Bought MP");
                        }

                        //Buy Universal Pills
                        var uniData = GetItemDataToBuy(srData, Config.UniLooptype);
                        if (Config.UniLoop)
                        {
                            BuyItems(uniData, worldId, Config.UniLooptype, Config.UniLoopcount);
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
                        var ammoData = GetItemDataToBuy(srData, Config.AmmoLooptype);
                        if (Config.AmmoLoop) BuyItems(ammoData, worldId, Config.AmmoLooptype, Config.AmmoLoopcount);
                        if (Config.Debug) Console.WriteLine(type + @": Bought Ammo");
                        break;
                    case ShopType.Accessory:
                        //Buy Drugs
                        var drugsData = GetItemDataToBuy(srData, Config.DrugsLooptype);
                        if (Config.DrugsLoop)
                        {
                            BuyItems(drugsData, worldId, Config.DrugsLooptype, Config.DrugsLoopcount);
                            _globalManager.InventoryManager.StackInventoryItems();
                            Thread.Sleep(Delay);
                        }

                        if (Config.Debug) Console.WriteLine(type + @": Bought Speed Drugs");

                        //Buy Return Scrolls
                        var scrollsData = GetItemDataToBuy(srData, Config.ScrollsLooptype);
                        if (Config.ScrollsLoop)
                        {
                            BuyItems(scrollsData, worldId, Config.ScrollsLooptype, Config.ScrollsLoopcount);
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
            Game.AllowBuy = false;
            _globalManager.PacketManager.BuyFromNpc(worldId, tab, slot, quantity);

            var timeout = new Stopwatch();
            timeout.Start();

            do
            {
                if (timeout.ElapsedMilliseconds > Timeout)
                {
                    _globalManager.PacketManager.BuyFromNpc(worldId, tab, slot, quantity);
                    timeout.Restart();
                }
                Thread.Sleep(Delay);
            } while (!Game.AllowBuy); 
        }

        private void Sell()
        {
            Game.AllowSell = false;

            byte slot = 0;
            while (slot != 0)
            {
                _globalManager.PacketManager.SellItem(slot);

                var timeout = new Stopwatch();
                timeout.Start();

                do
                {
                    if (timeout.ElapsedMilliseconds > Timeout)
                    {
                        _globalManager.PacketManager.SellItem(slot);
                        timeout.Restart();
                    }
                    Thread.Sleep(Delay);
                } while (!Game.AllowSell);

                //slot = _inventoryManager.GetSellItem();
            }
        }

        private void BuyItems(Silkroad.Shop data, uint worldId, uint itemId, ushort quantity)
        {
            if (itemId != 0)
            {
                var item = Silkroad.GetItemById(itemId);
                var quantityToBuy = quantity - _globalManager.InventoryManager.GetItemCount(itemId);
                if (Config.Debug) Console.WriteLine(@"Buying " + item.Code + @" (" + quantityToBuy + @")");

                while (quantityToBuy > 0)
                {
                    if (!Game.IsLooping)
                        return;

                    Game.AllowBuy = true;

                    if (quantityToBuy > item.MaxQuantity)
                    {
                        Buy(worldId, data.TabIndex, data.SlotIndex, item.MaxQuantity);
                        quantityToBuy -= item.MaxQuantity;
                    }
                    else
                    {
                        Buy(worldId, data.TabIndex, data.SlotIndex, (ushort)quantityToBuy);
                        quantityToBuy = 0;
                    }
                }
            }
        }

        private static Silkroad.Shop GetItemDataToBuy(List<Silkroad.Shop> shopdata, uint itemId)
        {
            return shopdata.Find(temp => temp.ItemId == itemId);
        }
    }
}
