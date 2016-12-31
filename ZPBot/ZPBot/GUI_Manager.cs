using System;
using System.Windows.Forms;
using ZPBot.Common;
using System.Drawing;

namespace ZPBot
{
    internal partial class Form1
    {
        private void checkBox_Manager(object sender, EventArgs e)
        {
            //Main
            Config.Usehp = bool.Parse(checkBox_usehp.Checked.ToString());
            Config.Usemp = bool.Parse(checkBox_usemp.Checked.ToString());
            Config.Useuni = bool.Parse(checkBox_useuni.Checked.ToString());
            Config.Autologin = bool.Parse(checkBox_autologin.Checked.ToString());
            Config.LaunchClientless = bool.Parse(checkBox_launchclientless.Checked.ToString());

            //Pickup
            Config.PickupMyitems = bool.Parse(checkBox_pickfilter_myitems.Checked.ToString());

            //Loop
            Config.HpLoop = bool.Parse(checkBox_loophp.Checked.ToString());
            Config.MpLoop = bool.Parse(checkBox_loopmp.Checked.ToString());
            Config.UniLoop = bool.Parse(checkBox_loopuni.Checked.ToString());
            Config.AmmoLoop = bool.Parse(checkBox_loopammo.Checked.ToString());
            Config.DrugsLoop = bool.Parse(checkBox_loopdrugs.Checked.ToString());
            Config.ScrollsLoop = bool.Parse(checkBox_loopscrolls.Checked.ToString());

            Config.ReturntownDied = bool.Parse(checkBox_returntown_died.Checked.ToString());
            Config.ReturntownNoPotion = bool.Parse(checkBox_returntown_no_potion.Checked.ToString());
            Config.ReturntownNoAmmo = bool.Parse(checkBox_returntown_no_ammo.Checked.ToString());
            Config.UseZerk = bool.Parse(checkBox_usezerk.Checked.ToString());
            Config.UseSpeeddrug = bool.Parse(checkBox_usespeeddrug.Checked.ToString());

            if (_settingsLoad)
            {
                _iniSet.Write("Settings", "HP", Config.Usehp.ToString());
                _iniSet.Write("Settings", "MP", Config.Usemp.ToString());
                _iniSet.Write("Settings", "Uni", Config.Useuni.ToString());
                _iniSet.Write("Settings", "GMStatus", Config.Gmtag.ToString());
                _iniSet.Write("Settings", "Autologin", Config.Autologin.ToString());
                _iniSet.Write("Settings", "Clientless", Config.LaunchClientless.ToString());

                _iniSet.Write("Pickfilter", "Own", Config.PickupMyitems.ToString());

                _iniSet.Write("Loop", "BuyHP", Config.HpLoop.ToString());
                _iniSet.Write("Loop", "BuyMP", Config.MpLoop.ToString());
                _iniSet.Write("Loop", "BuyUni", Config.UniLoop.ToString());
                _iniSet.Write("Loop", "BuyAmmo", Config.AmmoLoop.ToString());
                _iniSet.Write("Loop", "BuyDrugs", Config.DrugsLoop.ToString());
                _iniSet.Write("Loop", "BuyScrolls", Config.ScrollsLoop.ToString());

                _iniSet.Write("Training", "Died", Config.ReturntownDied.ToString());
                _iniSet.Write("Training", "NoPotion", Config.ReturntownNoPotion.ToString());
                _iniSet.Write("Training", "NoAmmo", Config.ReturntownNoAmmo.ToString());
                _iniSet.Write("Training", "UseZerk", Config.UseZerk.ToString());
                _iniSet.Write("Training", "UseSpeedDrug", Config.UseSpeeddrug.ToString());
            }

            if (Config.LaunchClientless)
            {
                label_clientless.Text = @"active";
                label_clientless.ForeColor = Color.Green;
                button_clientless.Enabled = false;
                button_hide.Enabled = false;
            }
            else
            {
                label_clientless.Text = @"inactive";
                label_clientless.ForeColor = Color.Red;
                button_clientless.Enabled = _globalManager.ClientManager.is_running();
                button_hide.Enabled = true;
            }
        }

        private void textBox_Manager(object sender, EventArgs e)
        {
            Config.LoginId = textBox_loginid.Text;
            Config.LoginPw = textBox_loginpw.Text;
            Config.LoginChar = textBox_loginchar.Text;

            if (_settingsLoad)
            {
                _iniSet.Write("Settings", "LoginID", Config.LoginId);
                _iniSet.Write("Settings", "LoginPW", Config.LoginPw);
                _iniSet.Write("Settings", "LoginChar", Config.LoginChar);
            }
        }

        private void comboBox_Manager(object sender, EventArgs e)
        {
            Config.UseZerktype = comboBox_usezerktype.SelectedIndex;
            if (comboBox_plustoreach.Text != "")
                Config.PlusToreach = byte.Parse(comboBox_plustoreach.Text);

            _iniSet.Write("Alchemy", "PlusToReach", Config.PlusToreach.ToString());
            _iniSet.Write("Training", "UseZerkType", Config.UseZerktype.ToString());
        }

        private void trackBar_Manager(object sender, EventArgs e)
        {
            if (sender == trackBar_monsters_range)
            {
                textBox_monsters_range.Text = trackBar_monsters_range.Value.ToString();
                Game.Range = int.Parse(textBox_monsters_range.Text);

                _iniSet.Write("Training", "Range", textBox_monsters_range.Text);
            }
            else if (sender == trackBar_usehp)
            {
                textBox_usehp.Text = trackBar_usehp.Value + @"%";
                Config.UsehpPercent = trackBar_usehp.Value;

                _iniSet.Write("Settings", "HPPercent", trackBar_usehp.Value.ToString());
            }
            else if (sender == trackBar_usemp)
            {
                textBox_usemp.Text = trackBar_usemp.Value + @"%";
                Config.UsempPercent = trackBar_usemp.Value;

                _iniSet.Write("Settings", "MPPercent", trackBar_usemp.Value.ToString());
            }
        }

        private void button_Manager(object sender, EventArgs e)
        {
            if (sender == button_searchclient)
            {
                SearchSrFolder();
            }
            else if (sender == button_launch)
            {
                Game.Clientless = Config.LaunchClientless;
                _globalManager.Silkroadproxy.Start();

                if (Game.Clientless)
                    return;

                button_launch.Enabled = false;
                _globalManager.ClientManager.Start(Client.LocalGatewayPort);
                button_clientless.Enabled = true;
            }
            else if (sender == button_startbot)
            {
                if (_globalManager.StartBot(false))
                {
                    label_botstate.Text = @"active";
                    label_botstate.ForeColor = Color.Green;
                }
            }
            else if (sender == button_stopbot)
            {
                _globalManager.StopBot();
                label_botstate.Text = @"inactive";
                label_botstate.ForeColor = Color.Red;
            }
            else if (sender == button_monsters_setpos)
            {
                var charPos = _globalManager.Player.GetIngamePosition();
                UpdateTrainingArea(charPos);
            }
            else if (sender == button_looprecord)
            {
                if (Game.RecordLoop)
                {
                    var saveFileDialog1 = new SaveFileDialog {Filter = @"Walkscript|*.txt|All files (*.*)|*.*"};
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        _globalManager.LoopManager.SaveScript(saveFileDialog1.FileName);
                        Game.RecordLoop = false;
                        button_looprecord.Text = @"Start Record";
                    }
                }
                else
                {
                    Game.RecordLoop = true;
                    button_looprecord.Text = @"Stop Record";
                    _globalManager.LoopManager.StartRecord();
                }
            }
            else if (sender == button_searchloopscript)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog {Filter = @"Walkscript|*.txt|All files (*.*)|*.*"};
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Config.WalkscriptLoop = openFileDialog1.FileName;
                    textBox_loopscript.Text = Config.WalkscriptLoop;
                    _iniSet.Write("Loop", "Walkscript", Config.WalkscriptLoop);
                }
            }
            else if (sender == button_hide)
            {
                button_hide.Text = _globalManager.ClientManager.Hide() ? @"Show Client" : @"Hide Client";
            }
            else if (sender == button_clientless)
            {
                label_clientless.Text = @"active";
                label_clientless.ForeColor = Color.Green;
                Game.Clientless = true;
                button_clientless.Enabled = false;
                button_hide.Enabled = false;
                _globalManager.ClientManager.Kill();
            }
            else if (sender == button_startfuse)
                _globalManager.SetFuseState(true);
            else if (sender == button_stopfuse)
                _globalManager.SetFuseState(false);
        }

        private void SearchSrFolder()
        {
            FolderBrowserDialog folder = new FolderBrowserDialog
            {
                Description = @"Please select your Silkroad directory"
            };
            if (folder.ShowDialog() == DialogResult.OK)
            {
                Config.SroPath = folder.SelectedPath;
                textBox_sropath.Text = Config.SroPath;

                _iniDef.Write("Settings", "SRFolder", textBox_sropath.Text);

                _globalManager.Load();
            }
        }

        private void Settings()
        {
            //Main
            checkBox_usehp.Checked = _iniSet.Read<bool>("Settings", "HP");
            checkBox_usemp.Checked = _iniSet.Read<bool>("Settings", "MP");
            checkBox_useuni.Checked = _iniSet.Read<bool>("Settings", "Uni");

            Config.UsehpPercent = _iniSet.Read<int>("Settings", "HPPercent");
            textBox_usehp.Text = Config.UsehpPercent + @"%";
            trackBar_usehp.Value = Config.UsehpPercent;

            Config.UsempPercent = _iniSet.Read<int>("Settings", "MPPercent");
            textBox_usemp.Text = Config.UsempPercent + @"%";
            trackBar_usemp.Value = Config.UsempPercent;

            checkBox_autologin.Checked = _iniSet.Read<bool>("Settings", "AutoLogin");
            checkBox_launchclientless.Checked = _iniSet.Read<bool>("Settings", "Clientless");
            textBox_loginid.Text = _iniSet.Read<string>("Settings", "LoginID", "");
            textBox_loginpw.Text = _iniSet.Read<string>("Settings", "LoginPW", "");
            textBox_loginchar.Text = _iniSet.Read<string>("Settings", "LoginChar", "");

            //Pickfilter Settings
            checkBox_pickfilter_myitems.Checked = _iniSet.Read<bool>("Pickfilter", "Own");

            //Monster Settings
            Game.Range = _iniSet.Read<int>("Training", "Range");
            if (Game.Range > trackBar_monsters_range.Maximum)
                Game.Range = trackBar_monsters_range.Maximum;
            else if (Game.Range < trackBar_monsters_range.Minimum)
                Game.Range = trackBar_monsters_range.Minimum;

            trackBar_monsters_range.Value = Game.Range;
            textBox_monsters_range.Text = Game.Range.ToString();


            Game.RangeXpos = _iniSet.Read<int>("Training", "Range_XPos");
            Game.RangeYpos = _iniSet.Read<int>("Training", "Range_YPos");

            label_monster_rangepos.Text = @"(X: " + Game.RangeXpos + @" Y: " + Game.RangeYpos + @")";

            //Loop Settings
            Config.WalkscriptLoop = _iniSet.Read<string>("Loop", "Walkscript", "No Walkscript is set");
            textBox_loopscript.Text = Config.WalkscriptLoop;
            checkBox_loophp.Checked = _iniSet.Read<bool>("Loop", "BuyHP");
            textBox_loophpcount.Text = _iniSet.Read<string>("Loop", "BuyHPAmount");
            checkBox_loopmp.Checked = _iniSet.Read<bool>("Loop", "BuyMP");
            textBox_loopmpcount.Text = _iniSet.Read<string>("Loop", "BuyMPAmount");
            checkBox_loopuni.Checked = _iniSet.Read<bool>("Loop", "BuyUni");
            textBox_loopunicount.Text = _iniSet.Read<string>("Loop", "BuyUniAmount");
            checkBox_loopammo.Checked = _iniSet.Read<bool>("Loop", "BuyAmmo");
            textBox_loopammocount.Text = _iniSet.Read<string>("Loop", "BuyAmmoAmount");
            checkBox_loopdrugs.Checked = _iniSet.Read<bool>("Loop", "BuyDrugs");
            textBox_loopdrugscount.Text = _iniSet.Read<string>("Loop", "BuyDrugsAmount");
            checkBox_loopscrolls.Checked = _iniSet.Read<bool>("Loop", "BuyScrolls");
            textBox_loopscrollscount.Text = _iniSet.Read<string>("Loop", "BuyScrollsAmount");

            checkBox_returntown_died.Checked = _iniSet.Read<bool>("Training", "Died");
            checkBox_returntown_no_potion.Checked = _iniSet.Read<bool>("Training", "NoPotion");
            checkBox_returntown_no_ammo.Checked = _iniSet.Read<bool>("Training", "NoAmmo");
            checkBox_usezerk.Checked = _iniSet.Read<bool>("Training", "UseZerk");
            comboBox_usezerktype.SelectedIndex = _iniSet.Read<int>("Training", "UseZerkType");
            checkBox_usespeeddrug.Checked = _iniSet.Read<bool>("Training", "UseSpeedDrug");
        }
    }
}
