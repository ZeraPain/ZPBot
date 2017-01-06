using System;
using System.Windows.Forms;
using ZPBot.Common;

namespace ZPBot
{
    internal partial class Form1
    {
        private void button_startbot_Click(object sender, EventArgs e) => _globalManager.StartBot(false);
        private void button_stopbot_Click(object sender, EventArgs e) => _globalManager.StopBot();
        private void button_hide_Click(object sender, EventArgs e) => button_hide.Text = _globalManager.ClientManager.Hide() ? @"Show Client" : @"Hide Client";

        private void button_clientless_Click(object sender, EventArgs e)
        {
            _globalManager.Clientless = true;
            _globalManager.ClientManager.Kill();
        }

        private void button_launch_Click(object sender, EventArgs e)
        {
            _globalManager.Silkroadproxy.Start();

            if (_globalManager.Clientless)
                return;

            button_launch.Enabled = false;
            _globalManager.ClientManager.Start(Client.LocalGatewayPort);
            button_clientless.Enabled = true;
            checkBox_launchclientless.Enabled = false;
        }

        private void button_searchclient_Click(object sender, EventArgs e)
        {
            var folder = new FolderBrowserDialog
            {
                Description = @"Please select your Silkroad directory"
            };

            if (folder.ShowDialog() == DialogResult.OK)
            {
                SroPath = folder.SelectedPath;
                SaveProfile();
                _globalManager.Load();
            }
        }

        private void button_monsters_setpos_Click(object sender, EventArgs e) => _globalManager.MonsterManager.UpdatePosition(_globalManager.Player.InGamePosition);

        private void button_searchloopscript_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog { Filter = @"Walkscript|*.txt|All files (*.*)|*.*" };

            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            _globalManager.LoopManager.WalkscriptPath = openFileDialog1.FileName;
        }

        private void button_looprecord_Click(object sender, EventArgs e)
        {
            if (_globalManager.LoopManager.RecordLoop)
            {
                var saveFileDialog1 = new SaveFileDialog { Filter = @"Walkscript|*.txt|All files (*.*)|*.*" };
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    _globalManager.LoopManager.StopRecord();
                    _globalManager.LoopManager.SaveScript(saveFileDialog1.FileName);
                    button_looprecord.Text = @"Start Record";
                }
            }
            else
            {
                button_looprecord.Text = @"Stop Record";
                _globalManager.LoopManager.StartRecord();
            }
        }

        private void button_newprofile_Click(object sender, EventArgs e)
        {
            CreateProfile();
        }
    }
}
