using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Characters
{
    internal class MonsterManager : ThreadManager, INotifyPropertyChanged
    {
        private int _range;
        public int Range
        {
            get { return _range; }
            set
            {
                if (value > 100) value = 100;
                if (value < 0) value = 0;
                _range = value;
            }
        }

        public GamePosition TrainingRange { get; protected set; }

        public bool UseZerk { get; set; }
        public int UseZerkType { get; set; }
        public uint SelectedMonster { get; set; }
        public uint AttackBlacklist { get; set; }

        private readonly GlobalManager _globalManager;
        private readonly Dictionary<uint, Monster> _monsterList;

        public MonsterManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            _monsterList = new Dictionary<uint, Monster>();
            TrainingRange = new GamePosition(0, 0);
        }

        public void UpdatePosition([NotNull] GamePosition position)
        {
            TrainingRange.XPos = position.XPos;
            TrainingRange.YPos = position.YPos;
            OnPropertyChanged(nameof(TrainingRange));
        }

        public void Add([NotNull] Monster monster)
        {
            lock (_monsterList)
            {
                if (!_monsterList.ContainsKey(monster.WorldId))
                    _monsterList.Add(monster.WorldId, monster);
            }
        }

        public void Remove(uint worldId)
        {
            lock (_monsterList)
            {
                if (_monsterList.ContainsKey(worldId))
                    _monsterList.Remove(worldId);
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
                    _monsterList[worldId].InGamePosition = Game.PositionToGamePosition(position);
            }
        }

        [CanBeNull]
        public Monster FindNextMonster(GamePosition charPos)
        {
            lock (_monsterList)
            {
                if (_monsterList.Count == 0)
                    return null;

                Monster targetMonster = null;

                foreach (var pair in _monsterList)
                {
                    var mob = pair.Value;

                    if ((Range > 0) && (Game.Distance(mob.InGamePosition, TrainingRange) > Range))
                        continue;

                    if (mob.WorldId == AttackBlacklist)
                    {
                        AttackBlacklist = 0;
                        continue;
                    }

                    if ((targetMonster == null) || (Game.Distance(mob.InGamePosition, _globalManager.Player.InGamePosition) < Game.Distance(targetMonster.InGamePosition, _globalManager.Player.InGamePosition)))
                        targetMonster = mob;

                    if ((mob.Type == EMonsterType.Unique) || (mob.Type == EMonsterType.SubUnique))
                        return mob;
                }

                return targetMonster;
            }
        }

        protected override void MyThread()
        {
            AttackBlacklist = 0;
            SelectedMonster = 0;

            while (BActive)
            {
                Thread.Sleep(200);

                if (_globalManager.Botstate && !_globalManager.LoopManager.IsLooping && (SelectedMonster == 0))
                {
                    var next = FindNextMonster(_globalManager.Player.InGamePosition);
                    if (next != null)
                    {
                        SelectedMonster = next.WorldId;
                        //c_PacketManager.SelectMonster(next.world_id);

                        if (UseZerk && (_globalManager.Player.RemainHwanCount == 5) && ((((next.Type == EMonsterType.Giant) || (next.Type == EMonsterType.GiantParty)) && (UseZerkType == 1)) || (UseZerkType == 0)))
                            _globalManager.PacketManager.UseZerk();
                    }
                    else
                    {
                        var random = new Random();
                        int randX, randY;
                        double rangeDistance;

                        do
                        {
                            randX = _globalManager.Player.InGamePosition.XPos + random.Next(-40, 40);
                            randY = _globalManager.Player.InGamePosition.YPos + random.Next(-40, 40);
                            rangeDistance = Game.Distance(TrainingRange, new GamePosition(randX, randY));
                        } while ((rangeDistance > Range) && (Range != 0));

                        _globalManager.PacketManager.MoveToCoords(randX, randY);
                        Thread.Sleep(2000);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var mainForm = _globalManager.FMain;
                if (mainForm == null) return; // No main form - no calls

                if (mainForm.InvokeRequired)
                    mainForm.Invoke(handler, this, new PropertyChangedEventArgs(propertyName));
                else
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
