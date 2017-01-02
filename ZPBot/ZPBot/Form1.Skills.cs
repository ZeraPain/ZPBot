using System;
using System.Windows.Forms;
using ZPBot.Annotations;
using ZPBot.Common;
using ZPBot.Common.Resources;
using ZPBot.Common.Skills;

namespace ZPBot
{
    internal partial class Form1
    {
        public void AddPlayerSkill(Skill skill) => Invoke((MethodInvoker)(() => listBox_skills.Items.Add(skill)));

        private void AddSkill(ESkillType skillType)
        {
            var index = listBox_skills.SelectedIndex;
            if (index < 0)
                return;

            try
            {
                var skill = (Skill)listBox_skills.Items[index];
                if (!_globalManager.SkillManager.Add(skill, skillType))
                    return;

                UpdateSkillCfg();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Form1.cs - AddSkill : " + ex.Message);
            }
        }

        private void RemoveSkill([NotNull] ListBox listbox, ESkillType skillType)
        {
            var index = listbox.SelectedIndex;
            if (index < 0)
                return;

            try
            {
                var skill = (Skill)listbox.Items[index];
                if (!_globalManager.SkillManager.Remove(skill, skillType))
                    return;

                UpdateSkillCfg();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Form1.cs - RemoveSkill : " + ex.Message);
            }
        }

        private void button_skills_attack_add_Click(object sender, EventArgs e) => AddSkill(ESkillType.Attack);
        private void button_skills_buff_add_Click(object sender, EventArgs e) => AddSkill(ESkillType.Buff);
        private void button_skills_imbue_add_Click(object sender, EventArgs e) => AddSkill(ESkillType.Imbue);
        private void button_skills_attack_remove_Click(object sender, EventArgs e) => RemoveSkill(listBox_skills_attack, ESkillType.Attack);
        private void button_skills_buff_remove_Click(object sender, EventArgs e) => RemoveSkill(listBox_skills_buff, ESkillType.Buff);
        private void button_skills_imbue_remove_Click(object sender, EventArgs e) => RemoveSkill(listBox_skills_imbue, ESkillType.Imbue);

        public void SkillSettings() => Invoke((MethodInvoker)delegate
        {
            if (_iniSet == null) return;

            uint skillId;
            Skill skill;

            for (var i = 0; i < _iniSet.Read<int>("AttackSkills", "Count"); i++)
            {
                skillId = _iniSet.Read<uint>("AttackSkills", i.ToString());
                skill = Silkroad.GetSkillById(skillId);
                _globalManager.SkillManager.Add(skill, ESkillType.Attack);
            }

            for (var i = 0; i < _iniSet.Read<int>("BuffSkills", "Count"); i++)
            {
                skillId = _iniSet.Read<uint>("BuffSkills", i.ToString());
                skill = Silkroad.GetSkillById(skillId);
                _globalManager.SkillManager.Add(skill, ESkillType.Buff);
            }

            skillId = _iniSet.Read<uint>("ImbueSkill", "Skill");
            skill = Silkroad.GetSkillById(skillId);
            _globalManager.SkillManager.Add(skill, ESkillType.Imbue);
        });

        private void UpdateSkillCfg()
        {
            _iniSet.RemoveSection("AttackSkills");
            _iniSet.Write("AttackSkills", "Count", listBox_skills_attack.Items.Count.ToString());

            for (var i = 0; i < listBox_skills_attack.Items.Count; i++)
            {
                try
                {
                    var skill = (Skill)listBox_skills_attack.Items[i];
                    _iniSet.Write("AttackSkills", i.ToString(), skill.Id.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"UpdateSkillCfg AttackSkills : " + ex.Message);
                }
            }

            _iniSet.RemoveSection("BuffSkills");
            _iniSet.Write("BuffSkills", "Count", listBox_skills_buff.Items.Count.ToString());

            for (var i = 0; i < listBox_skills_buff.Items.Count; i++)
            {
                try
                {
                    var skill = (Skill)listBox_skills_buff.Items[i];
                    _iniSet.Write("BuffSkills", i.ToString(), skill.Id.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"UpdateSkillCfg BuffSkills : " + ex.Message);
                }
            }

            _iniSet.RemoveSection("ImbueSkill");
            if (listBox_skills_imbue.Items.Count > 0)
            {
                try
                {
                    var skill = (Skill)listBox_skills_imbue.Items[0];
                    _iniSet.Write("ImbueSkill", "Skill", skill.Id.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"UpdateSkillCfg ImbueSkill : " + ex.Message);
                }
            }
        }
    }
}
