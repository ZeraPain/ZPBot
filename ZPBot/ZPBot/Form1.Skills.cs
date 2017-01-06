using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
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
                _globalManager.SkillManager.Add(skill, skillType);
                SaveSkillSettings();
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
                _globalManager.SkillManager.Remove(skill, skillType);
                SaveSkillSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Form1.cs - RemoveSkill : " + ex.Message);
            }
        }

        public void LoadSkillSettings() => Invoke((MethodInvoker)delegate
        {
            _globalManager.SkillManager.ClearSkills();

            var settingsFile = XElement.Load(ConfigPath);
            var skills = settingsFile.Element(GetProfilName())?.Element("Skills");
            if (skills != null)
            {
                var attackskills = skills.Element("AttackList");
                if (attackskills != null)
                {
                    foreach (var partyMember in attackskills.Descendants("Skill"))
                    {
                        _globalManager.SkillManager.Add(Silkroad.GetSkillById(Parse<uint>(partyMember.Attribute("Id")?.Value)), ESkillType.Attack);
                    }
                }

                var buffskills = skills.Element("BuffList");
                if (buffskills != null)
                {
                    foreach (var partyMember in buffskills.Descendants("Skill"))
                    {
                        _globalManager.SkillManager.Add(Silkroad.GetSkillById(Parse<uint>(partyMember.Attribute("Id")?.Value)), ESkillType.Buff);
                    }
                }

                var imbueskills = skills.Element("ImbueList");
                if (imbueskills != null)
                {
                    foreach (var partyMember in imbueskills.Descendants("Skill"))
                    {
                        _globalManager.SkillManager.Add(Silkroad.GetSkillById(Parse<uint>(partyMember.Attribute("Id")?.Value)), ESkillType.Imbue);
                    }
                }
            }
        });

        private void SaveSkillSettings()
        {
            object[] data =
            {
                new XElement("AttackList",
                    _globalManager.SkillManager.AttackList.Select(
                        x => new XElement("Skill", new XAttribute("Id", x.Id.ToString())))),
                new XElement("BuffList",
                    _globalManager.SkillManager.BuffList.Select(
                        x => new XElement("Skill", new XAttribute("Id", x.Id.ToString())))),
                new XElement("ImbueList",
                    _globalManager.SkillManager.ImbueList.Select(
                        x => new XElement("Skill", new XAttribute("Id", x.Id.ToString()))))
            };

            var settingsFile = XElement.Load(ConfigPath);
            var skills = settingsFile.Element(GetProfilName())?.Element("Skills");
            if (skills != null)
            {
                skills.ReplaceNodes(data);
            }
            else
            {
                settingsFile.Element(GetProfilName())?.Add(new XElement("Skills", data));
            }

            settingsFile.Save(ConfigPath);
        }

        private void button_skills_attack_add_Click(object sender, EventArgs e) => AddSkill(ESkillType.Attack);
        private void button_skills_buff_add_Click(object sender, EventArgs e) => AddSkill(ESkillType.Buff);
        private void button_skills_imbue_add_Click(object sender, EventArgs e) => AddSkill(ESkillType.Imbue);
        private void button_skills_attack_remove_Click(object sender, EventArgs e) => RemoveSkill(listBox_skills_attack, ESkillType.Attack);
        private void button_skills_buff_remove_Click(object sender, EventArgs e) => RemoveSkill(listBox_skills_buff, ESkillType.Buff);
        private void button_skills_imbue_remove_Click(object sender, EventArgs e) => RemoveSkill(listBox_skills_imbue, ESkillType.Imbue);
    }
}
