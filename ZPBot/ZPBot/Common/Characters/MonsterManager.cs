using System;
using System.Collections.Generic;
using System.Threading;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    internal class MonsterManager : ThreadManager
    {
        private readonly GlobalManager _globalManager;
        private readonly Dictionary<uint, Monster> _monsterList;

        public MonsterManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            _monsterList = new Dictionary<uint, Monster>();
        }

        public void Add(Monster monster)
        {
            lock (_monsterList)
            {
                if (!_monsterList.ContainsKey(monster.WorldId))
                {
                    _monsterList.Add(monster.WorldId, monster);
                }
            }
        }

        public void Remove(uint worldId)
        {
            lock (_monsterList)
            {
                if (_monsterList.ContainsKey(worldId))
                {
                    _monsterList.Remove(worldId);
                }
            }
        }

        public void Clear()
        {
            lock (_monsterList)
            {
                _monsterList.Clear();
            }
        }

        public void UpdatePosition(uint worldId, EPosition position)
        {
            lock (_monsterList)
            {
                if (_monsterList.ContainsKey(worldId))
                {
                    _monsterList[worldId].SetPosition(position);
                }
            }
        }

        private static double GetDistance(EGamePosition sourcePosition, EGamePosition destPosition)
        {
            return Math.Sqrt(Math.Pow(sourcePosition.XPos - destPosition.XPos, 2) + Math.Pow(sourcePosition.YPos - destPosition.YPos, 2));
        }

        public Monster FindNextMonster(EGamePosition charPos)
        {
            lock (_monsterList)
            {
                if (_monsterList.Count == 0)
                    return null;

                Monster targetMonster = null;

                foreach (var pair in _monsterList)
                {
                    var mob = pair.Value;

                    if (Game.Range > 0 && (GetDistance(mob.GetIngamePosition(), Game.GetRangePosition()) > Game.Range))
                        continue;

                    if (mob.WorldId == Game.AttackBlacklist)
                    {
                        Game.AttackBlacklist = 0;
                        continue;
                    }

                    if (targetMonster == null || (GetDistance(mob.GetIngamePosition(), _globalManager.Player.GetIngamePosition()) < GetDistance(targetMonster.GetIngamePosition(), _globalManager.Player.GetIngamePosition())))
                        targetMonster = mob;

                    if (mob.Type == EMonsterType.Unique || mob.Type == EMonsterType.SubUnique)
                        return mob;
                }

                return targetMonster;
            }
        }

        protected override void MyThread()
        {
            while (BActive)
            {
                Thread.Sleep(200);

                if (Config.Botstate && !Game.IsLooping && Game.SelectedMonster == 0)
                {
                    var next = FindNextMonster(_globalManager.Player.GetIngamePosition());
                    if (next != null)
                    {
                        Game.SelectedMonster = next.WorldId;
                        //c_PacketManager.SelectMonster(next.world_id);

                        if (Config.UseZerk && _globalManager.Player.RemainHwanCount == 5 && (((next.Type == EMonsterType.Giant || next.Type == EMonsterType.GiantParty) && Config.UseZerktype == 1) || Config.UseZerktype == 0))
                            _globalManager.PacketManager.UseZerk();
                    }
                    else
                    {
                        var charPos = _globalManager.Player.GetIngamePosition();
                        var random = new Random();
                        int randX, randY;
                        double rangeDistance;

                        do
                        {
                            randX = charPos.XPos + random.Next(-40, 40);
                            randY = charPos.YPos + random.Next(-40, 40);
                            rangeDistance = Math.Sqrt(Math.Pow((randX - Game.RangeXpos), 2) + Math.Pow((randY - Game.RangeYpos), 2));
                        } while (rangeDistance > Game.Range && Game.Range != 0);

                        _globalManager.PacketManager.MoveToCoords(randX, randY);
                        Thread.Sleep(2000);
                    }
                }
            }
        }
    }
}
