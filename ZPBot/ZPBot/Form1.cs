using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Forms;
using ZPBot.Common;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ZPBot.Annotations;
using ZPBot.Common.Items;
using ZPBot.Common.Characters;
using ZPBot.Common.Party;
using ZPBot.Common.Resources;

namespace ZPBot
{
    internal partial class Form1 : Form
    {
        internal static class NativeMethods
        {
            [DllImport("uxtheme.dll")]
            internal static extern int SetWindowTheme(IntPtr hwnd, string appname, string idlist);
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool AllocConsole();
        }

        private IniFile _iniSet;
        private IniFile _iniDef;
        private GlobalManager _globalManager;

        private bool _finishLoad;

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            NativeMethods.AllocConsole();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _finishLoad = false;

            NativeMethods.SetWindowTheme(progressBar_hpdisplay.Handle, "", "");
            NativeMethods.SetWindowTheme(progressBar_mpdisplay.Handle, "", "");
            progressBar_hpdisplay.ForeColor = Color.Red;
            progressBar_mpdisplay.ForeColor = Color.Blue;

            //Form Settings
            label_version.Text = Config.Version;
            AddEvent("Welcome to ZPBot (v." + Config.Version + ")!", "System");

            _globalManager = new GlobalManager(this);
            _globalManager.Load();

            SetBindings();

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\ZPBot\\default.zpb"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\ZPBot");

            _iniDef = new IniFile(Directory.GetCurrentDirectory() + "\\ZPBot\\default.zpb");
            Config.SroPath = _iniDef.Read<string>("Settings", "SRFolder", "Please select your Silkroad Folder");
            textBox_sropath.Text = Config.SroPath;

            var dir = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\ZPBot");
            foreach (var file in dir.GetFiles().Where(file => !comboBox_profile.Items.Contains(file.Name) && file.Name != "default.zpb"))
                comboBox_profile.Items.Add(file.Name);

            LoadProfile(_iniDef.Read<string>("Settings", "Profil"));
        }

        private void Form1_Unload(object sender, FormClosingEventArgs e) => _globalManager.Close();

        private void LoadProfile(string profil)
        {
            if (profil == "0")
                return;

            Config.IniPath = Directory.GetCurrentDirectory() + "\\ZPBot\\" + profil;
            _iniSet = new IniFile(Config.IniPath);

            if (!File.Exists(Config.IniPath))
            {
                _iniDef.Write("Settings", "Profil", "default.zpb");
                _iniDef.Write("Settings", "SRFolder", Config.SroPath);
            }
            else if (_iniDef.Read<string>("Settings", "Profil") != profil)
            {
                _iniDef.Write("Settings", "Profil", profil);
            }

            Settings();
            if (_finishLoad)
            {
                PickupSettings();
                LoopSettings();
            }
            comboBox_profile.Text = profil;
        }

        public void AddAlchemyEvent(string text) => Invoke((MethodInvoker)delegate
        {
            var dt = DateTime.Now;
            var date = $"{dt:HH:mm:ss}";

            richTextBox_alchemy.Text = date + @" " + text + Environment.NewLine + richTextBox_alchemy.Text;
        });

        public void AddEvent(string text, string type, bool error = false) => Invoke((MethodInvoker)delegate
        {
            var dt = DateTime.Now;
            var date = $"{dt:HH:mm:ss}";

            richTextBox_events.SelectionColor = error ? Color.Red : Color.Black;
            richTextBox_events.SelectedText = date + " [" + type + "] " + text + Environment.NewLine;
            richTextBox_events.ScrollToCaret();

            var windir = Environment.GetEnvironmentVariable("SystemRoot");
            var player = new SoundPlayer {SoundLocation = windir + "\\Media\\chimes.wav"};
            player.Play();
        });

        public void AddMessage(string text, EMessageType type) => Invoke((MethodInvoker)delegate
        {
            var dt = DateTime.Now;
            var date = $"{dt:HH:mm:ss}";

            switch (type)
            {
                case EMessageType.Private:
                    richTextBox_chat.SelectionColor = Color.FromArgb(0xFF, 0x9F, 0xFF, 0xFE);
                    richTextBox_chat_party.SelectionColor = Color.FromArgb(0xFF, 0x9F, 0xFF, 0xFE);
                    richTextBox_chat_party.SelectedText = date + " " + text + Environment.NewLine;
                    richTextBox_chat_guild.SelectionColor = Color.FromArgb(0xFF, 0x9F, 0xFF, 0xFE);
                    richTextBox_chat_guild.SelectedText = date + " " + text + Environment.NewLine;
                    richTextBox_chat_union.SelectionColor = Color.FromArgb(0xFF, 0x9F, 0xFF, 0xFE);
                    richTextBox_chat_union.SelectedText = date + " " + text + Environment.NewLine;
                    if (tabControl1.SelectedTab != tabControl1.TabPages[5]) label_pm.Visible = true;
                    break;
                case EMessageType.Notice:
                    richTextBox_chat.SelectionColor = Color.FromArgb(0xFF, 0xFF, 0xAE, 0xC3);
                    break;
                case EMessageType.Party:
                    richTextBox_chat.SelectionColor = Color.FromArgb(0xFF, 0x9A, 0xFF, 0xD0);
                    richTextBox_chat_party.SelectionColor = Color.FromArgb(0xFF, 0x9A, 0xFF, 0xD0);
                    richTextBox_chat_party.SelectedText = date + " " + text + Environment.NewLine;
                    richTextBox_chat_party.ScrollToCaret();
                    break;
                case EMessageType.Guild:
                    richTextBox_chat.SelectionColor = Color.FromArgb(0xFF, 0xFF, 0xB5, 0x41);
                    richTextBox_chat_guild.SelectionColor = Color.FromArgb(0xFF, 0xFF, 0xB5, 0x41);
                    richTextBox_chat_guild.SelectedText = date + " " + text + Environment.NewLine;
                    richTextBox_chat_guild.ScrollToCaret();
                    break;
                case EMessageType.Global:
                    richTextBox_chat.SelectionColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
                    break;
                case EMessageType.Union:
                    richTextBox_chat.SelectionColor = Color.FromArgb(0xFF, 0xC2, 0xF5, 0x73);
                    richTextBox_chat_union.SelectionColor = Color.FromArgb(0xFF, 0xC2, 0xF5, 0x73);
                    richTextBox_chat_union.SelectedText = date + " " + text + Environment.NewLine;
                    richTextBox_chat_union.ScrollToCaret();
                    break;
            }

            richTextBox_chat.SelectedText = date + " " + text + Environment.NewLine;
            richTextBox_chat.ScrollToCaret();
        });

        public void UpdateCharacter(Player player) => Invoke((MethodInvoker)delegate
        {
            var healthPercent = (int)(player.Health / (float)player.MaxHealth * 100);
            var manaPercent = (int)(player.Mana / (float)player.MaxMana * 100);
            if (healthPercent <= 100) progressBar_hpdisplay.Value = healthPercent;
            if (manaPercent <= 100) progressBar_mpdisplay.Value = manaPercent;

            label_infophyatk.Text = player.MinPhydmg + @" - " + player.MaxPhydmg;
            label_infomagatk.Text = player.MinMagdmg + @" - " + player.MaxMagdmg;
            label_infophydef.Text = player.PhyDef.ToString();
            label_infomagdef.Text = player.MagDef.ToString();
            label_infohitrate.Text = player.HitRate.ToString();
            label_infoparry.Text = player.ParryRate.ToString();
            label_infozerk.Text = player.RemainHwanCount.ToString();
            label_infospeed.Text = ((int)player.Runspeed).ToString();
        });

        public void UpdateParty(List<PartyMember> partyMembers) => Invoke((MethodInvoker)delegate
        {
            listView_party.Items.Clear();

            if (partyMembers == null)
                return;

            foreach (var player in partyMembers)
            {
                var listItem = new ListViewItem(player.AccountId.ToString());
                listItem.SubItems.Add(player.Charname);
                listItem.SubItems.Add(player.Guildname);
                listItem.SubItems.Add(player.Level.ToString());
                listItem.SubItems.Add(_globalManager.PartyManager.IsValidInvite(player.Charname) ? "X" : "");
                listView_party.Items.Add(listItem);
            }

            UpdatePartyCfg();
        });

        private void PartyInviteSettings()
        {
            if (_iniSet == null) return;

            for (var i = 0; i < _iniSet.Read<int>("PartyInvite", "Count"); i++)
            {
                try
                {
                    var player = _iniSet.Read<string>("PartyInvite", i.ToString());
                    _globalManager.PartyManager.AcceptInviteList.Add(player);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"PickupSettings - " + ex.Message);
                }
            }
        }

        private void UpdatePartyCfg()
        {
            _iniSet.RemoveSection("PartyInvite");

            var acceptInviteList = _globalManager.PartyManager.AcceptInviteList;
            if (acceptInviteList.Count == 0)
                return;

            _iniSet.Write("PartyInvite", "Count", acceptInviteList.Count.ToString());

            var index = 0;
            foreach (var player in acceptInviteList)
                _iniSet.Write("PartyInvite", index++.ToString(), player);
        }

        private void toolStripMenuItem_add_Click(object sender, EventArgs e)
        {
            listView_party.BeginUpdate();

            foreach (ListViewItem lvItem in listView_party.SelectedItems)
                lvItem.SubItems[4].Text = _globalManager.PartyManager.ToggleInviteList(lvItem.SubItems[1].Text) ? @"X" : "";

            listView_party.EndUpdate();
        }

        public void FinishLoad(bool state) => Invoke((MethodInvoker)delegate
        {
            if (state)
            {
                AddEvent("Successfully loaded silkdata!", "System");
                PickupSettings();
                LoopSettings();
                button_launch.Enabled = true;
            }
            else
            {
                AddEvent(Config.SroPath == "" ? "No silkroad directory is set!" : "Unable to load silkroad media.pk2!", "System", true);
            }

            _finishLoad = true;
        });

        private void textBox_chat_KeyPress(object sender, [NotNull] KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                // Then Enter key was pressed
                if (textBox_chat.Text == "") return;

                string message;
                switch (textBox_chat.Text.Substring(0, 1))
                {
                    case "$":
                        var split = textBox_chat.Text.Split(new[] { ' ' }, 2);
                        if (split.Length == 2 && split[1].Length > 0)
                        {
                            split[0] = split[0].Remove(0, 1);
                            _globalManager.SendMessage(split[1], EMessageType.Private, split[0]);
                            AddMessage(split[0] + "(TO): " + split[1], EMessageType.Private);

                            textBox_chat.Text = @"$" + split[0] + @" ";
                            textBox_chat.Select(textBox_chat.Text.Length, 0);
                        }
                        break;
                    case "#":
                        message = textBox_chat.Text.Remove(0, 1);
                        if (message.Length > 0)
                        {
                            _globalManager.SendMessage(message, EMessageType.Party);

                            textBox_chat.Text = @"#";
                            textBox_chat.Select(textBox_chat.Text.Length, 0);
                        }
                        break;
                    case "@":
                        message = textBox_chat.Text.Remove(0, 1);
                        if (message.Length > 0)
                        {
                            _globalManager.SendMessage(message, EMessageType.Guild);

                            textBox_chat.Text = @"@";
                            textBox_chat.Select(textBox_chat.Text.Length, 0);
                        }
                        break;
                }
            }
            else
            {
                if (textBox_chat.Text.StartsWith("$"))
                    textBox_chat.ForeColor = Color.FromArgb(0xFF, 0x9F, 0xFF, 0xFE);
                else if (textBox_chat.Text.StartsWith("#"))
                    textBox_chat.ForeColor = Color.FromArgb(0xFF, 0x9A, 0xFF, 0xD0);
                else if (textBox_chat.Text.StartsWith("@"))
                    textBox_chat.ForeColor = Color.FromArgb(0xFF, 0xFF, 0xB5, 0x41);
                else
                    textBox_chat.ForeColor = Color.White;
            }
        }

        public void UpdateInventory(List<InventoryItem> inventoryList) => Invoke((MethodInvoker) delegate
        {
            listView_inventory.Items.Clear();

            foreach (var item in inventoryList.Where(item => item.Slot > 12))
            {
                var listItem = new ListViewItem(item.Slot.ToString());
                listItem.SubItems.Add(item.Name);
                listItem.SubItems.Add(item.Plus.ToString());

                if (item.ItemType1 != EItemType1.Equipable)
                    listItem.ForeColor = Color.DarkGray;

                listView_inventory.Items.Add(listItem);

                if (item.Slot == Game.SlotFuseitem)
                    textBox_fuseitem.Text = item.Name + @" (+" + item.Plus + @")";
            }
        });

        private void listView_inventory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((listView_inventory.SelectedItems.Count == 1) && !Game.Fusing)
            {
                var index = listView_inventory.SelectedIndices[0];
                var slot = byte.Parse(listView_inventory.Items[index].SubItems[0].Text);
                button_startfuse.Enabled = _globalManager.InventoryManager.SetFuseItem(slot);
            }
        }

        private void listBox_players_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_players.SelectedIndex == -1) return;

            var player = (Character) listBox_players.SelectedItem;
            var playerItems = player?.ItemList;
            if (playerItems == null) return;

            listView_player.Items.Clear();
            listView_player.Items.Add(new ListViewItem("Headgear"));
            listView_player.Items.Add(new ListViewItem("Shoulders"));
            listView_player.Items.Add(new ListViewItem("Chest"));
            listView_player.Items.Add(new ListViewItem("Legs"));
            listView_player.Items.Add(new ListViewItem("Hands"));
            listView_player.Items.Add(new ListViewItem("Foot"));
            listView_player.Items.Add(new ListViewItem("Weapon"));
            listView_player.Items.Add(new ListViewItem("Shield"));
            listView_player.Items.Add(new ListViewItem("Avatar (Hat)"));
            listView_player.Items.Add(new ListViewItem("Avatar (Dress)"));
            listView_player.Items.Add(new ListViewItem("Avatar (Attach)"));
            listView_player.Items.Add(new ListViewItem("Avatar (Flag)"));
            listView_player.Items.Add(new ListViewItem("Avatar (Spirit)"));

            foreach (var invItem in playerItems)
            {
                var index = -1;
                switch (invItem.EquipableType2)
                {
                    case EEquipableType2.Weapon:
                        index = 7;
                        break;
                    case EEquipableType2.Shield:
                        index = 8;
                        break;
                    case EEquipableType2.CGarment:
                    case EEquipableType2.CProtector:
                    case EEquipableType2.CArmor:
                    case EEquipableType2.EGarment:
                    case EEquipableType2.EProtector:
                    case EEquipableType2.EArmor:
                        index = (int)invItem.ProtectorType3;
                        break;
                    case EEquipableType2.JobSuit:
                        break;
                    case EEquipableType2.Avatar:
                        index = 8 + (int)invItem.AvatarType3;
                        break;
                    case EEquipableType2.Spirit:
                        index = 13;
                        break;
                    default:
                        index = -1;
                        break;
                }

                if (index > -1) listView_player.Items[index - 1].SubItems.Add(invItem.Name + " [" + invItem.Level + "+" + invItem.Plus + "]");
            }
        }

        private void comboBox_profile_SelectedIndexChanged(object sender, EventArgs e) => LoadProfile(comboBox_profile.Text);

        private void tabControl1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages[5])
                label_pm.Visible = false;
        }

        private void LoopSettings()
        {
            //Items Settings
            if (_iniSet == null) return;

            var itemdata = Silkroad.GetLoopData();
            foreach (var item in itemdata)
            {
                switch (item.ConsumableType2)
                {
                    case EConsumableType2.Potion:
                        switch (item.PotionType3)
                        {
                            case EPotionType3.Health:
                                comboBox_hploop.Items.Add(item);
                                break;
                            case EPotionType3.Mana:
                                comboBox_mploop.Items.Add(item);
                                break;
                        }
                        break;
                    case EConsumableType2.Cure:
                        if (item.CureType3 == ECureType3.Univsersal)
                            comboBox_uniloop.Items.Add(item);
                        break;
                    case EConsumableType2.Scroll:
                        if (item.ScrollType3 == EScrollType3.Return)
                            comboBox_scrollsloop.Items.Add(item);
                        break;
                    case EConsumableType2.Ammo:
                        comboBox_ammoloop.Items.Add(item);
                        break;
                    case EConsumableType2.CharScroll:
                        if (item.ScrollType == EScrollType.Speed)
                            comboBox_drugsloop.Items.Add(item);
                        break;
                }
            }

            _globalManager.LoopManager.HpLoopOption.BuyType = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyHPType"));
            _globalManager.LoopManager.MpLoopOption.BuyType = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyMPType"));
            _globalManager.LoopManager.UniLoopOption.BuyType = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyUniType"));
            _globalManager.LoopManager.AmmoLoopOption.BuyType = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyAmmoType"));
            _globalManager.LoopManager.SpeedLoopOption.BuyType = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyDrugsType"));
            _globalManager.LoopManager.ReturnLoopOption.BuyType = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyScrollsType"));
        }

        private void Settings()
        {
            //Main
            _globalManager.Autologin = _iniSet.Read<bool>("Settings", "Autologin");
            _globalManager.Clientless = _iniSet.Read<bool>("Settings", "Clientless");
            _globalManager.LoginId = _iniSet.Read<string>("Settings", "LoginID", "");
            _globalManager.LoginPw = _iniSet.Read<string>("Settings", "LoginPW", "");
            _globalManager.LoginChar = _iniSet.Read<string>("Settings", "LoginChar", "");

            //Items
            _globalManager.ItemDropManager.PickupMyItems = _iniSet.Read<bool>("Pickfilter", "Own", "");

            //Training
            _globalManager.MonsterManager.Range = _iniSet.Read<int>("Training", "Range");
            _globalManager.MonsterManager.UpdatePosition(new GamePosition(_iniSet.Read<int>("Training", "Range_XPos"),
                _iniSet.Read<int>("Training", "Range_YPos")));

            _globalManager.InventoryManager.EnableHp = _iniSet.Read<bool>("Training", "EnableHp");
            _globalManager.InventoryManager.HpPercent = _iniSet.Read<int>("Training", "HpPercent");
            _globalManager.InventoryManager.EnableMp = _iniSet.Read<bool>("Training", "EnableMp");
            _globalManager.InventoryManager.MpPercent = _iniSet.Read<int>("Training", "MpPercent");
            _globalManager.InventoryManager.EnableUniversal = _iniSet.Read<bool>("Training", "EnableUniversal");

            _globalManager.ReturntownDied = _iniSet.Read<bool>("Training", "ReturntownDied");
            _globalManager.InventoryManager.ReturntownNoPotion = _iniSet.Read<bool>("Training", "ReturntownNoPotion");
            _globalManager.InventoryManager.ReturntownNoAmmo = _iniSet.Read<bool>("Training", "ReturntownNoAmmo");

            _globalManager.MonsterManager.UseZerk = _iniSet.Read<bool>("Training", "UseZerk");
            _globalManager.MonsterManager.UseZerkType = _iniSet.Read<int>("Training", "UseZerkType");
            _globalManager.InventoryManager.EnableSpeedDrug = _iniSet.Read<bool>("Training", "EnableSpeedDrug");

            //Loop
            _globalManager.LoopManager.HpLoopOption.Enabled = _iniSet.Read<bool>("Loop", "BuyHP");
            _globalManager.LoopManager.HpLoopOption.BuyAmount = _iniSet.Read<ushort>("Loop", "BuyHpAmount");
            _globalManager.LoopManager.MpLoopOption.Enabled = _iniSet.Read<bool>("Loop", "BuyMP");
            _globalManager.LoopManager.MpLoopOption.BuyAmount = _iniSet.Read<ushort>("Loop", "BuyMpAmount");
            _globalManager.LoopManager.UniLoopOption.Enabled = _iniSet.Read<bool>("Loop", "BuyUni");
            _globalManager.LoopManager.UniLoopOption.BuyAmount = _iniSet.Read<ushort>("Loop", "BuyUniAmount");
            _globalManager.LoopManager.AmmoLoopOption.Enabled = _iniSet.Read<bool>("Loop", "BuyAmmo");
            _globalManager.LoopManager.AmmoLoopOption.BuyAmount = _iniSet.Read<ushort>("Loop", "BuyAmmoAmount");
            _globalManager.LoopManager.SpeedLoopOption.Enabled = _iniSet.Read<bool>("Loop", "BuyDrugs");
            _globalManager.LoopManager.SpeedLoopOption.BuyAmount = _iniSet.Read<ushort>("Loop", "BuyDrugsAmount");
            _globalManager.LoopManager.ReturnLoopOption.Enabled = _iniSet.Read<bool>("Loop", "BuyScrolls");
            _globalManager.LoopManager.ReturnLoopOption.BuyAmount = _iniSet.Read<ushort>("Loop", "BuyScrollsAmount");

            _globalManager.LoopManager.WalkscriptPath = _iniSet.Read<string>("Loop", "Walkscript", "No Walkscript is set");

            //Party
            _globalManager.PartyManager.AutoAccept = _iniSet.Read<bool>("Party", "AutoAccept");
            _globalManager.PartyManager.AutoInvite = _iniSet.Read<bool>("Party", "AutoInvite");
            _globalManager.PartyManager.AcceptInviteAll = _iniSet.Read<bool>("Party", "AcceptInviteAll");
            _globalManager.PartyManager.PartyType = _iniSet.Read<int>("Party", "PartyType");
            PartyInviteSettings();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            //Main
            _iniSet.Write("Settings", "Clientless", _globalManager.Clientless.ToString());
            _iniSet.Write("Settings", "Autologin", _globalManager.Autologin.ToString());
            _iniSet.Write("Settings", "LoginID", _globalManager.LoginId);
            _iniSet.Write("Settings", "LoginPW", _globalManager.LoginPw);
            _iniSet.Write("Settings", "LoginChar", _globalManager.LoginChar);

            //Items
            _iniSet.Write("Pickfilter", "Own", _globalManager.ItemDropManager.PickupMyItems.ToString());

            //Training
            _iniSet.Write("Training", "Range", _globalManager.MonsterManager.Range.ToString());
            _iniSet.Write("Training", "Range_XPos", _globalManager.MonsterManager.TrainingRange.XPos.ToString());
            _iniSet.Write("Training", "Range_YPos", _globalManager.MonsterManager.TrainingRange.YPos.ToString());

            _iniSet.Write("Training", "EnableHp", _globalManager.InventoryManager.EnableHp.ToString());
            _iniSet.Write("Training", "HpPercent", _globalManager.InventoryManager.HpPercent.ToString());
            _iniSet.Write("Training", "EnableMp", _globalManager.InventoryManager.EnableMp.ToString());
            _iniSet.Write("Training", "MpPercent", _globalManager.InventoryManager.MpPercent.ToString());
            _iniSet.Write("Training", "EnableUniversal", _globalManager.InventoryManager.EnableUniversal.ToString());

            _iniSet.Write("Training", "ReturntownDied", _globalManager.ReturntownDied.ToString());
            _iniSet.Write("Training", "ReturntownNoPotion", _globalManager.InventoryManager.ReturntownNoPotion.ToString());
            _iniSet.Write("Training", "ReturntownNoAmmo", _globalManager.InventoryManager.ReturntownNoAmmo.ToString());

            _iniSet.Write("Training", "UseZerk", _globalManager.MonsterManager.UseZerk.ToString());
            _iniSet.Write("Training", "UseZerkType", _globalManager.MonsterManager.UseZerkType.ToString());
            _iniSet.Write("Training", "EnableSpeedDrug", _globalManager.InventoryManager.EnableSpeedDrug.ToString());

            //Loop
            _iniSet.Write("Loop", "BuyHP", _globalManager.LoopManager.HpLoopOption.Enabled.ToString());
            _iniSet.Write("Loop", "BuyHpType", _globalManager.LoopManager.HpLoopOption.BuyType?.Id.ToString());
            _iniSet.Write("Loop", "BuyHpAmount", _globalManager.LoopManager.HpLoopOption.BuyAmount.ToString());
            _iniSet.Write("Loop", "BuyMP", _globalManager.LoopManager.MpLoopOption.Enabled.ToString());
            _iniSet.Write("Loop", "BuyMpType", _globalManager.LoopManager.MpLoopOption.BuyType?.Id.ToString());
            _iniSet.Write("Loop", "BuyMpAmount", _globalManager.LoopManager.MpLoopOption.BuyAmount.ToString());
            _iniSet.Write("Loop", "BuyUni", _globalManager.LoopManager.UniLoopOption.Enabled.ToString());
            _iniSet.Write("Loop", "BuyUniType", _globalManager.LoopManager.UniLoopOption.BuyType?.Id.ToString());
            _iniSet.Write("Loop", "BuyUniAmount", _globalManager.LoopManager.UniLoopOption.BuyAmount.ToString());
            _iniSet.Write("Loop", "BuyAmmo", _globalManager.LoopManager.AmmoLoopOption.Enabled.ToString());
            _iniSet.Write("Loop", "BuyAmmoType", _globalManager.LoopManager.AmmoLoopOption.BuyType?.Id.ToString());
            _iniSet.Write("Loop", "BuyAmmoAmount", _globalManager.LoopManager.AmmoLoopOption.BuyAmount.ToString());
            _iniSet.Write("Loop", "BuyDrugs", _globalManager.LoopManager.SpeedLoopOption.Enabled.ToString());
            _iniSet.Write("Loop", "BuyDrugsType", _globalManager.LoopManager.SpeedLoopOption.BuyType?.Id.ToString());
            _iniSet.Write("Loop", "BuyDrugsAmount", _globalManager.LoopManager.SpeedLoopOption.BuyAmount.ToString());
            _iniSet.Write("Loop", "BuyScrolls", _globalManager.LoopManager.ReturnLoopOption.Enabled.ToString());
            _iniSet.Write("Loop", "BuyScrollsType", _globalManager.LoopManager.ReturnLoopOption.BuyType?.Id.ToString());
            _iniSet.Write("Loop", "BuyScrollsAmount", _globalManager.LoopManager.ReturnLoopOption.BuyAmount.ToString());

            _iniSet.Write("Loop", "Walkscript", _globalManager.LoopManager.WalkscriptPath);

            //Party
            _iniSet.Write("Party", "AutoAccept", _globalManager.PartyManager.AutoAccept.ToString());
            _iniSet.Write("Party", "AutoInvite", _globalManager.PartyManager.AutoInvite.ToString());
            _iniSet.Write("Party", "AcceptInviteAll", _globalManager.PartyManager.AcceptInviteAll.ToString());
            _iniSet.Write("Party", "PartyType", _globalManager.PartyManager.PartyType.ToString());
        }
    }
}
