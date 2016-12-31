using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Forms;
using ZPBot.Common;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ZPBot.Common.Items;
using ZPBot.Common.Characters;
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
        private bool _settingsLoad;

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            NativeMethods.AllocConsole();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _finishLoad = false;
            _settingsLoad = false;

            NativeMethods.SetWindowTheme(progressBar_hpdisplay.Handle, "", "");
            NativeMethods.SetWindowTheme(progressBar_mpdisplay.Handle, "", "");
            progressBar_hpdisplay.ForeColor = Color.Red;
            progressBar_mpdisplay.ForeColor = Color.Blue;

            //Form Settings
            label_botstate.Text = @"inactive";
            label_botstate.ForeColor = Color.Red;
            label_version.Text = Config.Version;
            label_clientless.Text = @"inactive";
            label_clientless.ForeColor = Color.Red;

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

        private void SetBindings()
        {
            listBox_players.DataSource = _globalManager.CharManager.PlayerList;
            listBox_skills.DataSource = _globalManager.SkillManager.SkillList;
            listBox_skills_attack.DataSource = _globalManager.SkillManager.AttackList;
            listBox_skills_buff.DataSource = _globalManager.SkillManager.BuffList;
            listBox_skills_imbue.DataSource = _globalManager.SkillManager.ImbueList;
        }

        private void LoadProfile(string profil)
        {
            if (profil == "0")
                return;

            _settingsLoad = false;
            Config.IniPath = Directory.GetCurrentDirectory() + "\\ZPBot\\" + profil;
            _iniSet = new IniFile(Config.IniPath);

            if (!File.Exists(Config.IniPath))
            {
                _iniDef.Write("Settings", "Profil", "default.zpb");
                _iniDef.Write("Settings", "SRFolder", Config.SroPath);
            }
            else
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
            _settingsLoad = true;
        }

        public void AddAlchemyEvent(string text)
        {
            Invoke((MethodInvoker)delegate
            {
                var dt = DateTime.Now;
                var date = $"{dt:HH:mm:ss}";

                richTextBox_alchemy.Text = date + @" " + text + Environment.NewLine + richTextBox_alchemy.Text;
            });
        }

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


        public void UpdateCharacterPos(Character player) => Invoke((MethodInvoker)delegate
        {
            var charPos = player.GetIngamePosition();
            label_position.Text = @"(X:" + charPos.XPos + @" Y:" + charPos.YPos + @")";
        });

        public void UpdateCharacter(Character player) => Invoke((MethodInvoker)delegate
        {
            var healthPercent = (int)(player.Health / (float)player.MaxHealth * 100);
            var manaPercent = (int)(player.Mana / (float)player.MaxMana * 100);
            if (healthPercent <= 100) progressBar_hpdisplay.Value = healthPercent;
            if (manaPercent <= 100) progressBar_mpdisplay.Value = manaPercent;

            label_charname.Text = player.Charname;
            label_infophyatk.Text = player.MinPhydmg + @" - " + player.MaxPhydmg;
            label_infomagatk.Text = player.MinMagdmg + @" - " + player.MaxMagdmg;
            label_infophydef.Text = player.PhyDef.ToString();
            label_infomagdef.Text = player.MagDef.ToString();
            label_infohitrate.Text = player.HitRate.ToString();
            label_infoparry.Text = player.ParryRate.ToString();
            label_infozerk.Text = player.RemainHwanCount.ToString();
            label_infospeed.Text = ((int)player.Runspeed).ToString();
            UpdateCharacterPos(player);
        });

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

        private void textBox_chat_KeyPress(object sender, KeyPressEventArgs e)
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

        public void UpdateTrainingArea(EGamePosition charPos) => Invoke((MethodInvoker) delegate
        {
            Game.RangeXpos = charPos.XPos;
            Game.RangeYpos = charPos.YPos;
            label_monster_rangepos.Text = @"(X: " + Game.RangeXpos + @" Y: " + Game.RangeYpos + @")";

            _iniSet.Write("Training", "Range_XPos", Game.RangeXpos.ToString());
            _iniSet.Write("Training", "Range_YPos", Game.RangeYpos.ToString());
        });

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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Config.LogPackets = checkBox1.Checked;
            if (checkBox1.Checked) Console.Clear();
        }
    }
}
