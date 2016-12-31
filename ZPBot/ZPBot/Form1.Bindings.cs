using System.Drawing;
using System.Windows.Forms;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot
{
    internal partial class Form1
    {
        private void SetBindings()
        {
            //Main
            SetPlayerStatsBindings();
            SetMainBindings();

            //Items
            checkBox_pickfilter_myitems.DataBindings.Add("Checked", _globalManager.ItemDropManager, "PickupMyItems", false, DataSourceUpdateMode.OnPropertyChanged);

            //Training
            SetTrainingBindings();

            //Skills
            listBox_skills.DataSource = _globalManager.SkillManager.SkillList;
            listBox_skills_attack.DataSource = _globalManager.SkillManager.AttackList;
            listBox_skills_buff.DataSource = _globalManager.SkillManager.BuffList;
            listBox_skills_imbue.DataSource = _globalManager.SkillManager.ImbueList;

            //Loop
            SetLoopBindings();

            //Spy
            listBox_players.DataSource = _globalManager.CharManager.PlayerList;

            checkBox_partyautoaccept.DataBindings.Add("Checked", _globalManager.PartyManager, "AutoAccept", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_partytype1.DataBindings.Add("Checked", _globalManager.PartyManager, "AcceptType1", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_partytype2.DataBindings.Add("Checked", _globalManager.PartyManager, "AcceptType2", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_partytype3.DataBindings.Add("Checked", _globalManager.PartyManager, "AcceptType3", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_partytype4.DataBindings.Add("Checked", _globalManager.PartyManager, "AcceptType4", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void SetPlayerStatsBindings()
        {
            label_charname.DataBindings.Add("Text", _globalManager.Player, "Charname", false, DataSourceUpdateMode.OnPropertyChanged);

            var binding = new Binding("Text", _globalManager.Player, "InGamePosition", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += PositionConverter;
            label_position.DataBindings.Add(binding);
        }

        private void SetMainBindings()
        {
            checkBox_autologin.DataBindings.Add("Checked", _globalManager, "Autologin", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_launchclientless.DataBindings.Add("Checked", _globalManager, "Clientless", false, DataSourceUpdateMode.OnPropertyChanged);

            var binding = new Binding("ForeColor", _globalManager, "Botstate", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += BoolToColorConverter;
            label_botstate.DataBindings.Add(binding);

            binding = new Binding("Text", _globalManager, "Botstate", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += BoolToTextConverter;
            label_botstate.DataBindings.Add(binding);

            binding = new Binding("Enabled", _globalManager, "Clientless", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += BoolInverter;
            button_hide.DataBindings.Add(binding);

            binding = new Binding("ForeColor", _globalManager, "Clientless", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += BoolToColorConverter;
            label_clientless.DataBindings.Add(binding);

            binding = new Binding("Text", _globalManager, "Clientless", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += BoolToTextConverter;
            label_clientless.DataBindings.Add(binding);

            textBox_loginid.DataBindings.Add("Text", _globalManager, "LoginId", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loginpw.DataBindings.Add("Text", _globalManager, "LoginPw", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loginchar.DataBindings.Add("Text", _globalManager, "LoginChar", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_logpackets.DataBindings.Add("Checked", _globalManager.Silkroadproxy, "LogPackets", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void SetTrainingBindings()
        {
            trackBar_monsters_range.DataBindings.Add("Value", _globalManager.MonsterManager, "Range", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_monsters_range.DataBindings.Add("Text", _globalManager.MonsterManager, "Range", false, DataSourceUpdateMode.OnPropertyChanged);
            var binding = new Binding("Text", _globalManager.MonsterManager, "TrainingRange", false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += PositionConverter;
            label_monster_rangepos.DataBindings.Add(binding);

            checkBox_usehp.DataBindings.Add("Checked", _globalManager.InventoryManager, "EnableHp", false, DataSourceUpdateMode.OnPropertyChanged);
            trackBar_usehp.DataBindings.Add("Value", _globalManager.InventoryManager, "HpPercent", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_usehp.DataBindings.Add("Text", _globalManager.InventoryManager, "HpPercent", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_usemp.DataBindings.Add("Checked", _globalManager.InventoryManager, "EnableMp", false, DataSourceUpdateMode.OnPropertyChanged);
            trackBar_usemp.DataBindings.Add("Value", _globalManager.InventoryManager, "MpPercent", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_usemp.DataBindings.Add("Text", _globalManager.InventoryManager, "MpPercent", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_useuni.DataBindings.Add("Checked", _globalManager.InventoryManager, "EnableUniversal", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_returntown_died.DataBindings.Add("Checked", _globalManager, "ReturntownDied", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_returntown_no_potion.DataBindings.Add("Checked", _globalManager.InventoryManager, "ReturntownNoPotion", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_returntown_no_ammo.DataBindings.Add("Checked", _globalManager.InventoryManager, "ReturntownNoAmmo", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_usezerk.DataBindings.Add("Checked", _globalManager.MonsterManager, "UseZerk", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_usezerktype.DataBindings.Add("SelectedIndex", _globalManager.MonsterManager, "UseZerkType", false, DataSourceUpdateMode.OnPropertyChanged);
            checkBox_usespeeddrug.DataBindings.Add("Checked", _globalManager.InventoryManager, "EnableSpeedDrug", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void SetLoopBindings()
        {
            checkBox_loophp.DataBindings.Add("Checked", _globalManager.LoopManager.HpLoopOption, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_hploop.DataBindings.Add("SelectedItem", _globalManager.LoopManager.HpLoopOption, "BuyType", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loophpcount.DataBindings.Add("Text", _globalManager.LoopManager.HpLoopOption, "BuyAmount", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_loopmp.DataBindings.Add("Checked", _globalManager.LoopManager.MpLoopOption, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_mploop.DataBindings.Add("SelectedItem", _globalManager.LoopManager.MpLoopOption, "BuyType", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loopmpcount.DataBindings.Add("Text", _globalManager.LoopManager.MpLoopOption, "BuyAmount", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_loopuni.DataBindings.Add("Checked", _globalManager.LoopManager.UniLoopOption, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_uniloop.DataBindings.Add("SelectedItem", _globalManager.LoopManager.UniLoopOption, "BuyType", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loopunicount.DataBindings.Add("Text", _globalManager.LoopManager.UniLoopOption, "BuyAmount", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_loopammo.DataBindings.Add("Checked", _globalManager.LoopManager.AmmoLoopOption, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_ammoloop.DataBindings.Add("SelectedItem", _globalManager.LoopManager.AmmoLoopOption, "BuyType", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loopammocount.DataBindings.Add("Text", _globalManager.LoopManager.AmmoLoopOption, "BuyAmount", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_loopdrugs.DataBindings.Add("Checked", _globalManager.LoopManager.SpeedLoopOption, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_drugsloop.DataBindings.Add("SelectedItem", _globalManager.LoopManager.SpeedLoopOption, "BuyType", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loopdrugscount.DataBindings.Add("Text", _globalManager.LoopManager.SpeedLoopOption, "BuyAmount", false, DataSourceUpdateMode.OnPropertyChanged);

            checkBox_loopscrolls.DataBindings.Add("Checked", _globalManager.LoopManager.ReturnLoopOption, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
            comboBox_scrollsloop.DataBindings.Add("SelectedItem", _globalManager.LoopManager.ReturnLoopOption, "BuyType", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox_loopscrollscount.DataBindings.Add("Text", _globalManager.LoopManager.ReturnLoopOption, "BuyAmount", false, DataSourceUpdateMode.OnPropertyChanged);

            textBox_loopscript.DataBindings.Add("Text", _globalManager.LoopManager, "WalkscriptPath", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private static void BoolInverter(object sender, [NotNull] ConvertEventArgs e)
        {
            var valid = (bool)e.Value;
            e.Value = !valid;
        }

        private static void BoolToColorConverter(object sender, [NotNull] ConvertEventArgs e)
        {
            var valid = (bool)e.Value;
            e.Value = valid ? Color.Green : Color.Red;
        }

        private static void BoolToTextConverter(object sender, [NotNull] ConvertEventArgs e)
        {
            var valid = (bool)e.Value;
            e.Value = valid ? "active" : "inactive";
        }

        private static void PositionConverter(object sender, [NotNull] ConvertEventArgs e)
        {
            var position = (GamePosition)e.Value;
            e.Value = "(X: " + position.XPos + " Y: " + position.YPos + ")";
        }
    }
}
