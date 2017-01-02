using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Skills
{
    internal class SkillManager : ThreadManager
    {
        private readonly GlobalManager _globalManager;

        public BindingList<Skill> SkillList { get; protected set; }
        public BindingList<Skill> AttackList { get; protected set; }
        public BindingList<Skill> BuffList { get; protected set; }

        public BindingList<Skill> ImbueList { get; protected set; }
        private Skill _speedDrug;
        private readonly object _lock;

        public SkillManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;

            SkillList = new BindingList<Skill>();
            AttackList = new BindingList<Skill>();
            BuffList = new BindingList<Skill>();
            ImbueList = new BindingList<Skill>();
            _speedDrug = null;

            _lock = new object();
        }

        private static bool Contains([NotNull] IEnumerable<Skill> list, Skill skill) => list.Any(skillItem => skillItem.Id == skill.Id);

        public bool Add(Skill skill, ESkillType skillType)
        {
            if (skill == null)
                return false;

            lock (_lock)
            {
                if (skillType != ESkillType.Common)
                {
                    skill = GetSkill(skill, SkillList);
                    if (skill == null)
                        return false;
                }

                switch (skillType)
                {
                    case ESkillType.Common:
                        if (Contains(SkillList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => SkillList.Add(skill)));
                        return true;
                    case ESkillType.Attack:
                        if (Contains(AttackList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => AttackList.Add(skill)));
                        return true;
                    case ESkillType.Buff:
                        if (Contains(BuffList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => BuffList.Add(skill)));
                        return true;
                    case ESkillType.Imbue:
                        if (Contains(ImbueList, skill) || (ImbueList.Count > 0)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => ImbueList.Add(skill)));
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool Remove(Skill skill, ESkillType skillType)
        {
            lock (_lock)
            {
                skill = GetSkill(skill, SkillList);
                if (skill == null)
                    return false;

                switch (skillType)
                {
                    case ESkillType.Common:
                        if (!Contains(SkillList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => SkillList.Remove(skill)));
                        return true;
                    case ESkillType.Attack:
                        if (!Contains(AttackList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => AttackList.Remove(skill)));
                        return true;
                    case ESkillType.Buff:
                        if (!Contains(BuffList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => BuffList.Remove(skill)));
                        return true;
                    case ESkillType.Imbue:
                        if (!Contains(ImbueList, skill)) return false;
                        _globalManager.FMain.Invoke((MethodInvoker)(() => ImbueList.Remove(skill)));
                        return true;
                    default:
                        return false;
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _globalManager.FMain.Invoke((MethodInvoker)(() => SkillList.Clear()));
            }
        }

        public void ResetBuffs()
        {
            lock (_lock)
            {
                foreach (var skill in BuffList)
                    skill.Active = false;
            }
        }

        [CanBeNull]
        private Skill GetSkill(Skill skill, [NotNull] IEnumerable<Skill> list)
        {
            lock (_lock)
            {
                return list.FirstOrDefault(temp => (temp.MasteryTopColumn == skill.MasteryTopColumn) && (temp.MasteryLowColumn == skill.MasteryLowColumn) && (temp.Row == skill.Row) && (temp.Column == skill.Column));
            }
        }

        public bool SpeedDrug_is_active() => _speedDrug != null;

        public void Update(Skill newSkill)
        {
            var skill = GetSkill(newSkill, SkillList);

            if (skill != null)
            {
                lock (_lock)
                {
                    SkillList[SkillList.IndexOf(skill)] = newSkill;

                    var attack = GetSkill(newSkill, AttackList);
                    if (attack != null) AttackList[AttackList.IndexOf(attack)] = newSkill;

                    var buff = GetSkill(newSkill, BuffList);
                    if (buff != null) BuffList[BuffList.IndexOf(buff)] = newSkill;
                }
            }
            else
            {
                Add(newSkill, ESkillType.Common);
            }
        }

        public void RegBuff([NotNull] Skill regSkill)
        {
            lock (_lock)
            {
                foreach (var skill in BuffList.Where(skill => skill.Id == regSkill.Id))
                {
                    skill.WorldId = regSkill.WorldId;
                    skill.Active = true;
                    break;
                }

                if (regSkill.SpeedDrugSkill)
                    _speedDrug = regSkill;
            }
        }

        public void UnRegBuff(uint worldId)
        {
            lock (_lock)
            {
                foreach (var skill in BuffList.Where(skill => skill.Active && (skill.WorldId == worldId)))
                {
                    skill.Active = false;
                    break;
                }

                if ((_speedDrug != null) && (_speedDrug.WorldId == worldId)) _speedDrug = null;
            }
        }

        protected override void MyThread()
        {
            while (BActive)
            {
                Thread.Sleep(200);

                if (!_globalManager.Botstate || Game.IsLooping || !Game.AllowCast || Game.IsPicking)
                    continue;

                var isBuffing = false;
                lock (_lock)
                {
                    foreach (var skill in BuffList.Where(skill => !skill.Active))
                    {
                        isBuffing = true;
                        _globalManager.PacketManager.CastSkill(skill.Id);
                        Thread.Sleep(1000);
                        break;
                    }
                }

                if (isBuffing)
                    continue;

                lock (_lock)
                {
                    foreach (var skill in AttackList.TakeWhile(skill => Game.SelectedMonster != 0))
                    {
                        _globalManager.PacketManager.CastSkill(skill.Id, Game.SelectedMonster);
                        Thread.Sleep(100);
                    }

                    if (ImbueList.Count > 0) _globalManager.PacketManager.CastSkill(ImbueList[0].Id);
                }

                if (Game.SelectedMonster == 0)
                    continue;

                Thread.Sleep(1000);
            }
        }
    }
}
