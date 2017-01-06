using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Windows.Forms;
using ZPBot.Common;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
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

        private GlobalManager _globalManager;
        private const string ConfigPath = "config.xml";
        private BindingList<string> _profilNames;
        private int _profilIndex;

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

            _profilNames = new BindingList<string>();
            _globalManager = new GlobalManager(this);

            SetBindings();

            if (!File.Exists(ConfigPath))
                CreateProfile();

            LoadProfiles();
            _globalManager.Load();
        }

        private void Form1_Unload(object sender, FormClosingEventArgs e) => _globalManager.Close();

        private void CreateProfile()
        {
            string input = null;
            var dialog = InputBox("Profile Editor", "Enter a new profil name:", ref input);

            if (dialog == DialogResult.OK && input != null)
            {
                _profilNames.Add(input);
                _profilIndex = _profilNames.Count - 1;
                SaveProfile();
            }
        }

        private void LoadProfiles()
        {
            var settingsFile = XElement.Load(ConfigPath);
            var zpbot = settingsFile.Element("ZPBot");
            if (zpbot == null) return;

            var profiles = zpbot.Element("Profiles");
            if (profiles != null)
            {
                foreach (var profil in profiles.Descendants("Profil"))
                {
                    _profilNames.Add(Parse<string>(profil.Attribute("Name")?.Value));
                }
            }

            comboBox_profile.SelectedIndex = Parse<int>(zpbot.Element("Profil")?.Value);
            SroPath = Parse<string>(zpbot.Element("SroPath")?.Value);
            LoadSettings();
        }

        private void SaveProfile()
        {
            object[] data = {new XElement("Profil", _profilIndex),
                    new XElement("Profiles", _profilNames.Select(x => new XElement("Profil", new XAttribute("Name", x)))),
                    new XElement("SroPath", SroPath)};

            if (!File.Exists(ConfigPath))
                new XElement("Config", new XElement("ZPBot", data)).Save(ConfigPath);

            var settingsFile = XElement.Load(ConfigPath);
            var zpbot = settingsFile.Element("ZPBot");

            zpbot?.ReplaceNodes(data);
            settingsFile.Save(ConfigPath);
        }

        private string GetProfilName()
        {
            if (_profilIndex >= _profilNames.Count || _profilIndex < 0)
                return null;

            return _profilNames[_profilIndex];
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            var form = new Form();
            var label = new Label();
            var textBox = new TextBox();
            var buttonOk = new Button();
            var buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = @"OK";
            buttonCancel.Text = @"Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] {label, textBox, buttonOk, buttonCancel});
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            var dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        public void AddAlchemyEvent(string text) => Invoke((MethodInvoker)delegate
        {
            if (richTextBox_alchemy.InvokeRequired)
                Invoke((MethodInvoker) (() => AddAlchemyEvent(text)));
            else
            {
                var dt = DateTime.Now;
                var date = $"{dt:HH:mm:ss}";

                richTextBox_alchemy.Text = date + @" " + text + Environment.NewLine + richTextBox_alchemy.Text;
            }
        });

        public void AddEvent(string text, string type, bool error = false) => Invoke((MethodInvoker)delegate
        {
            if (richTextBox_events.InvokeRequired)
                Invoke((MethodInvoker)(() => AddEvent(text, type, error)));
            else
            {
                var dt = DateTime.Now;
                var date = $"{dt:HH:mm:ss}";

                richTextBox_events.SelectionColor = error ? Color.Red : Color.Black;
                richTextBox_events.SelectedText = date + " [" + type + "] " + text + Environment.NewLine;
                richTextBox_events.ScrollToCaret();

                var windir = Environment.GetEnvironmentVariable("SystemRoot");
                var player = new SoundPlayer { SoundLocation = windir + "\\Media\\chimes.wav" };
                player.Play();
            }
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
            label_infophyatk.Text = player.MinPhydmg + @" - " + player.MaxPhydmg;
            label_infomagatk.Text = player.MinMagdmg + @" - " + player.MaxMagdmg;
            label_infophydef.Text = player.PhyDef.ToString();
            label_infomagdef.Text = player.MagDef.ToString();
            label_infohitrate.Text = player.HitRate.ToString();
            label_infoparry.Text = player.ParryRate.ToString();
            label_infozerk.Text = player.RemainHwanCount.ToString();
            label_infospeed.Text = ((int)player.Runspeed).ToString();
        });

        private void toolStripMenuItem_add_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvRow in dataGridView_party.SelectedRows)
            {
                var partyMember = (PartyMember)dgvRow.DataBoundItem;
                if (partyMember == null)
                    continue;

                _globalManager.PartyManager.ToggleInviteList(partyMember.Charname);
            }
        }

        public void FinishLoad(bool state) => Invoke((MethodInvoker)delegate
        {
            if (state)
            {
                AddEvent("Successfully loaded silkdata!", "System");
                LoadItemSettings();
                LoopSettings();
                button_launch.Enabled = true;
            }
            else
            {
                AddEvent(SroPath == "" ? "No silkroad directory is set!" : "Unable to load silkroad media.pk2!", "System", true);
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

        public void UpdateInventory([NotNull] Dictionary<byte, InventoryItem> inventoryList)
        {
            if (listView_inventory.InvokeRequired)
                Invoke((MethodInvoker) (() => UpdateInventory(inventoryList)));
            else
            {
                listView_inventory.Items.Clear();

                foreach (var kvp in inventoryList.OrderBy(k => k.Key).Where(k => k.Value.Slot > 12))
                {
                    var item = kvp.Value;
                    var listItem = new ListViewItem(item.Slot.ToString());
                    listItem.SubItems.Add(item.Name);
                    listItem.SubItems.Add(item.Plus.ToString());
                    listItem.SubItems.Add(item.Quantity.ToString());

                    listItem.ForeColor = item.ItemType1 != EItemType1.Equipable ? Color.Red : Color.Green;
                    listView_inventory.Items.Add(listItem);

                    if (item.Slot == Game.SlotFuseitem)
                        textBox_fuseitem.Text = item.Name + @" (+" + item.Plus + @")";
                }
            }
        }

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

        private void comboBox_profile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_profilIndex == comboBox_profile.SelectedIndex) return;
            _profilIndex = comboBox_profile.SelectedIndex;

            LoadSettings();
            if (_finishLoad)
            {
                LoadItemSettings();
                LoopSettings();
            }
            SaveProfile();
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages[5])
                label_pm.Visible = false;
        }

        private void LoopSettings()
        {
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

            var settingsFile = XElement.Load(ConfigPath);
            var loop = settingsFile.Element("Loop");
            if (loop != null)
            {
                _globalManager.LoopManager.HpLoopOption.BuyType = Silkroad.GetItemById(Parse<uint>(loop.Element("BuyHpType")?.Value));
                _globalManager.LoopManager.MpLoopOption.BuyType = Silkroad.GetItemById(Parse<uint>(loop.Element("BuyMpType")?.Value));
                _globalManager.LoopManager.UniLoopOption.BuyType = Silkroad.GetItemById(Parse<uint>(loop.Element("BuyUniType")?.Value));
                _globalManager.LoopManager.AmmoLoopOption.BuyType = Silkroad.GetItemById(Parse<uint>(loop.Element("BuyAmmoType")?.Value));
                _globalManager.LoopManager.SpeedLoopOption.BuyType = Silkroad.GetItemById(Parse<uint>(loop.Element("BuyDrugsType")?.Value));
                _globalManager.LoopManager.ReturnLoopOption.BuyType = Silkroad.GetItemById(Parse<uint>(loop.Element("BuyScrollsType")?.Value));
            }
        }

        private void LoadSettings()
        {
            var settingsFile = XElement.Load(ConfigPath).Element(GetProfilName());

            var settings = settingsFile?.Element("Settings");
            _globalManager.Autologin = Parse<bool>(settings?.Element("Autologin")?.Value);
            _globalManager.Userdata = Parse<bool>(settings?.Element("Userdata")?.Value);
            _globalManager.Clientless = Parse<bool>(settings?.Element("Clientless")?.Value);
            _globalManager.LoginId = Parse<string>(settings?.Element("LoginID")?.Value);
            _globalManager.LoginPw = Parse<string>(settings?.Element("LoginPW")?.Value);
            _globalManager.LoginChar = Parse<string>(settings?.Element("LoginChar")?.Value);

            var pickFilter = settingsFile?.Element("Pickfilter");
            _globalManager.ItemDropManager.PickupMyItems = Parse<bool>(pickFilter?.Element("Own")?.Value);

            var training = settingsFile?.Element("Training");
            _globalManager.MonsterManager.Range = Parse<int>(training?.Element("Range")?.Value);
            _globalManager.MonsterManager.UpdatePosition(new GamePosition(Parse<int>(training?.Element("Range_XPos")?.Value),
               Parse<int>(training?.Element("Range_YPos")?.Value)));
            _globalManager.InventoryManager.EnableHp = Parse<bool>(training?.Element("EnableHp")?.Value);
            _globalManager.InventoryManager.HpPercent = Parse<int>(training?.Element("HpPercent")?.Value);
            _globalManager.InventoryManager.EnableMp = Parse<bool>(training?.Element("EnableMp")?.Value);
            _globalManager.InventoryManager.MpPercent = Parse<int>(training?.Element("MpPercent")?.Value);
            _globalManager.InventoryManager.EnableUniversal = Parse<bool>(training?.Element("EnableUniversal")?.Value);
            _globalManager.ReturntownDied = Parse<bool>(training?.Element("ReturntownDied")?.Value);
            _globalManager.InventoryManager.ReturntownNoPotion = Parse<bool>(training?.Element("ReturntownNoPotion")?.Value);
            _globalManager.InventoryManager.ReturntownNoAmmo = Parse<bool>(training?.Element("ReturntownNoAmmo")?.Value);
            _globalManager.MonsterManager.UseZerk = Parse<bool>(training?.Element("UseZerk")?.Value);
            _globalManager.MonsterManager.UseZerkType = Parse<int>(training?.Element("UseZerkType")?.Value);
            _globalManager.InventoryManager.EnableSpeedDrug = Parse<bool>(training?.Element("EnableSpeedDrug")?.Value);

            var loop = settingsFile?.Element("Loop");
            _globalManager.LoopManager.HpLoopOption.Enabled = Parse<bool>(loop?.Element("BuyHP")?.Value);
            _globalManager.LoopManager.HpLoopOption.BuyAmount = Parse<ushort>(loop?.Element("BuyHpAmount")?.Value);
            _globalManager.LoopManager.MpLoopOption.Enabled = Parse<bool>(loop?.Element("BuyMP")?.Value);
            _globalManager.LoopManager.MpLoopOption.BuyAmount = Parse<ushort>(loop?.Element("BuyMpAmount")?.Value);
            _globalManager.LoopManager.UniLoopOption.Enabled = Parse<bool>(loop?.Element("BuyUni")?.Value);
            _globalManager.LoopManager.UniLoopOption.BuyAmount = Parse<ushort>(loop?.Element("BuyUniAmount")?.Value);
            _globalManager.LoopManager.AmmoLoopOption.Enabled = Parse<bool>(loop?.Element("BuyAmmo")?.Value);
            _globalManager.LoopManager.AmmoLoopOption.BuyAmount = Parse<ushort>(loop?.Element("BuyAmmoAmount")?.Value);
            _globalManager.LoopManager.SpeedLoopOption.Enabled = Parse<bool>(loop?.Element("BuyDrugs")?.Value);
            _globalManager.LoopManager.SpeedLoopOption.BuyAmount = Parse<ushort>(loop?.Element("BuyDrugsAmount")?.Value);
            _globalManager.LoopManager.ReturnLoopOption.Enabled = Parse<bool>(loop?.Element("BuyScrolls")?.Value);
            _globalManager.LoopManager.ReturnLoopOption.BuyAmount = Parse<ushort>(loop?.Element("BuyScrollsAmount")?.Value);
            _globalManager.LoopManager.WalkscriptPath = Parse<string>(loop?.Element("Walkscript")?.Value);

            var party = settingsFile?.Element("Party");
            _globalManager.PartyManager.AutoAccept = Parse<bool>(party?.Element("AutoAccept")?.Value);
            _globalManager.PartyManager.AutoInvite = Parse<bool>(party?.Element("AutoInvite")?.Value);
            _globalManager.PartyManager.AcceptInviteAll = Parse<bool>(party?.Element("AcceptInviteAll")?.Value);
            _globalManager.PartyManager.PartyType = Parse<int>(party?.Element("PartyType")?.Value);

            _globalManager.PartyManager.AcceptInviteList.Clear();
            var invites = party?.Element("Invites");
            if (invites != null)
            {
                foreach (var partyMember in invites.Descendants("Partymember"))
                {
                    _globalManager.PartyManager.AcceptInviteList.Add(Parse<string>(partyMember.Attribute("Name")?.Value));
                }
            }
        }

        [CanBeNull]
        private static T Parse<T>([CanBeNull] string value)
        {
            try
            {
                if (value == null) return default(T);

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Parse Error - " + ex.Message);
                return default(T);
            }
        }

        private void button_save_Click(object sender, EventArgs e) => SaveSettings();

        private void SaveSettings()
        {
            if (!File.Exists(ConfigPath))
                return;

            var sw = new Stopwatch();
            sw.Start();

            object[] settingData =
            {
                new XElement("Clientless", _globalManager.Clientless.ToString()),
                new XElement("Autologin", _globalManager.Autologin.ToString()),
                new XElement("Userdata", _globalManager.Userdata.ToString()),
                new XElement("LoginID", _globalManager.LoginId),
                new XElement("LoginPW", _globalManager.LoginPw),
                new XElement("LoginChar", _globalManager.LoginChar)
            };

            object[] pickData =
            {
                new XElement("Own", _globalManager.ItemDropManager.PickupMyItems.ToString())
            };

            object[] trainingData =
            {
                new XElement("Range", _globalManager.MonsterManager.Range.ToString()),
                new XElement("Range_XPos", _globalManager.MonsterManager.TrainingRange.XPos.ToString()),
                new XElement("Range_YPos", _globalManager.MonsterManager.TrainingRange.YPos.ToString()),
                new XElement("EnableHp", _globalManager.InventoryManager.EnableHp.ToString()),
                new XElement("HpPercent", _globalManager.InventoryManager.HpPercent.ToString()),
                new XElement("EnableMp", _globalManager.InventoryManager.EnableMp.ToString()),
                new XElement("MpPercent", _globalManager.InventoryManager.MpPercent.ToString()),
                new XElement("EnableUniversal", _globalManager.InventoryManager.EnableUniversal.ToString()),
                new XElement("ReturntownDied", _globalManager.ReturntownDied.ToString()),
                new XElement("ReturntownNoPotion", _globalManager.InventoryManager.ReturntownNoPotion.ToString()),
                new XElement("ReturntownNoAmmo", _globalManager.InventoryManager.ReturntownNoAmmo.ToString()),
                new XElement("UseZerk", _globalManager.MonsterManager.UseZerk.ToString()),
                new XElement("UseZerkType", _globalManager.MonsterManager.UseZerkType.ToString()),
                new XElement("EnableSpeedDrug", _globalManager.InventoryManager.EnableSpeedDrug.ToString())
            };

            object[] loopData =
            {
                new XElement("BuyHP", _globalManager.LoopManager.HpLoopOption.Enabled.ToString()),
                new XElement("BuyHpType", _globalManager.LoopManager.HpLoopOption.BuyType?.Id.ToString()),
                new XElement("BuyHpAmount", _globalManager.LoopManager.HpLoopOption.BuyAmount.ToString()),
                new XElement("BuyMP", _globalManager.LoopManager.MpLoopOption.Enabled.ToString()),
                new XElement("BuyMpType", _globalManager.LoopManager.MpLoopOption.BuyType?.Id.ToString()),
                new XElement("BuyMpAmount", _globalManager.LoopManager.MpLoopOption.BuyAmount.ToString()),
                new XElement("BuyUni", _globalManager.LoopManager.UniLoopOption.Enabled.ToString()),
                new XElement("BuyUniType", _globalManager.LoopManager.UniLoopOption.BuyType?.Id.ToString()),
                new XElement("BuyUniAmount", _globalManager.LoopManager.UniLoopOption.BuyAmount.ToString()),
                new XElement("BuyAmmo", _globalManager.LoopManager.AmmoLoopOption.Enabled.ToString()),
                new XElement("BuyAmmoType", _globalManager.LoopManager.AmmoLoopOption.BuyType?.Id.ToString()),
                new XElement("BuyAmmoAmount", _globalManager.LoopManager.AmmoLoopOption.BuyAmount.ToString()),
                new XElement("BuyDrugs", _globalManager.LoopManager.SpeedLoopOption.Enabled.ToString()),
                new XElement("BuyDrugsType", _globalManager.LoopManager.SpeedLoopOption.BuyType?.Id.ToString()),
                new XElement("BuyDrugsAmount", _globalManager.LoopManager.SpeedLoopOption.BuyAmount.ToString()),
                new XElement("BuyScrolls", _globalManager.LoopManager.ReturnLoopOption.Enabled.ToString()),
                new XElement("BuyScrollsType", _globalManager.LoopManager.ReturnLoopOption.BuyType?.Id.ToString()),
                new XElement("BuyScrollsAmount", _globalManager.LoopManager.ReturnLoopOption.BuyAmount.ToString()),
                new XElement("Walkscript", _globalManager.LoopManager.WalkscriptPath)
            };

            object[] partyData =
            {
                new XElement("AutoAccept", _globalManager.PartyManager.AutoAccept.ToString()),
                new XElement("AutoInvite", _globalManager.PartyManager.AutoInvite.ToString()),
                new XElement("AcceptInviteAll", _globalManager.PartyManager.AcceptInviteAll.ToString()),
                new XElement("PartyType", _globalManager.PartyManager.PartyType.ToString()),
                new XElement("Invites",
                    _globalManager.PartyManager.AcceptInviteList.Select(
                        x => new XElement("Partymember", new XAttribute("Name", x))))
            };

            var settingsFile = XElement.Load(ConfigPath);
            var mySection = settingsFile.Element(GetProfilName());

            if (mySection == null)
                settingsFile.Add(new XElement(GetProfilName(),
                    new XElement("Settings", settingData),
                    new XElement("Pickfilter", pickData),
                    new XElement("Training", trainingData),
                    new XElement("Loop", loopData),
                    new XElement("Party", partyData)));
            else
            {
                var setting = mySection.Element("Settings");
                if (setting == null)
                    mySection.Add(new XElement("Settings", settingData));
                else
                    setting.ReplaceNodes(settingData);

                var pickFilter = mySection.Element("Pickfilter");
                if (pickFilter == null)
                    mySection.Add(new XElement("Pickfilter", pickData));
                else
                    pickFilter.ReplaceNodes(pickData);

                var training = mySection.Element("Training");
                if (training == null)
                    mySection.Add(new XElement("Training", trainingData));
                else
                    training.ReplaceNodes(trainingData);

                var loop = mySection.Element("Loop");
                if (loop == null)
                    mySection.Add(new XElement("Loop", loopData));
                else
                    loop.ReplaceNodes(loopData);

                var party = mySection.Element("Party");
                if (party == null)
                    mySection.Add(new XElement("Party", partyData));
                else
                    party.ReplaceNodes(partyData);
            }

            settingsFile.Save(ConfigPath);

            sw.Stop();
            Console.WriteLine(@"Saving took " + sw.Elapsed);
        }

        
    }
}
