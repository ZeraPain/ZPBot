using System;
using System.Windows.Forms;
using ZPBot.Common;
using ZPBot.Common.Items;

namespace ZPBot
{
    internal partial class Form1
    {
        #region ComboBox
        private void SetLoopItem(ComboBox comboBox, ref uint globalVar, string iniText)
        {
            try
            {
                var item = (Item)comboBox.SelectedItem;
                if (item == null)
                    return;

                globalVar = item.Id;
                _iniSet.Write("Loop", iniText, item.Id.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Form1.cs SetLoopItem : " + ex.Message);
            }
        }

        private void comboBox_hploop_SelectedIndexChanged(object sender, EventArgs e) => SetLoopItem(comboBox_hploop, ref Config.HpLooptype, "BuyHPType");
        private void comboBox_mploop_SelectedIndexChanged(object sender, EventArgs e) => SetLoopItem(comboBox_mploop, ref Config.MpLooptype, "BuyMPType");
        private void comboBox_uniloop_SelectedIndexChanged(object sender, EventArgs e) => SetLoopItem(comboBox_uniloop, ref Config.UniLooptype, "BuyUniType");
        private void comboBox_ammoloop_SelectedIndexChanged(object sender, EventArgs e) => SetLoopItem(comboBox_ammoloop, ref Config.AmmoLooptype, "BuyAmmoType");
        private void comboBox_drugsloop_SelectedIndexChanged(object sender, EventArgs e) => SetLoopItem(comboBox_drugsloop, ref Config.DrugsLooptype, "BuyDrugsType");
        private void comboBox_scrollsloop_SelectedIndexChanged(object sender, EventArgs e) => SetLoopItem(comboBox_scrollsloop, ref Config.ScrollsLooptype, "BuyScrollsType");

        #endregion

        #region TextBox
        // ReSharper disable once RedundantAssignment
        private void SetLoopItemAmount(Control textBox, ref ushort globalVar, string iniText)
        {
            uint amount;
            amount = uint.TryParse(textBox.Text, out amount) ? amount : 0;

            if (amount > ushort.MaxValue)
            {
                amount = ushort.MaxValue;
                textBox.Text = amount.ToString();
            }

            globalVar = (ushort) amount;
            _iniSet.Write("Loop", iniText, globalVar.ToString());
        }

        private void textBox_loophpcount_TextChanged(object sender, EventArgs e) => SetLoopItemAmount(textBox_loophpcount, ref Config.HpLoopcount, "BuyHPAmount");
        private void textBox_loopmpcount_TextChanged(object sender, EventArgs e) => SetLoopItemAmount(textBox_loopmpcount, ref Config.MpLoopcount, "BuyMPAmount");
        private void textBox_loopunicount_TextChanged(object sender, EventArgs e) => SetLoopItemAmount(textBox_loopunicount, ref Config.UniLoopcount, "BuyUniAmount");
        private void textBox_loopammocount_TextChanged(object sender, EventArgs e) => SetLoopItemAmount(textBox_loopammocount, ref Config.AmmoLoopcount, "BuyAmmoAmount");
        private void textBox_loopdrugscount_TextChanged(object sender, EventArgs e) => SetLoopItemAmount(textBox_loopdrugscount, ref Config.DrugsLoopcount, "BuyDrugsAmount");
        private void textBox_loopscrollscount_TextChanged(object sender, EventArgs e) => SetLoopItemAmount(textBox_loopscrollscount, ref Config.ScrollsLoopcount, "BuyScrollsAmount");

        #endregion

        private void LoopSettings() => Invoke((MethodInvoker)delegate
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

            var hploop = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyHPType"));
            var mploop = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyMPType"));
            var uniloop = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyUniType"));
            var ammoloop = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyAmmoType"));
            var drugloop = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyDrugsType"));
            var scrollloop = Silkroad.GetItemById(_iniSet.Read<uint>("Loop", "BuyScrollsType"));

            if (hploop != null)
            {
                Config.HpLooptype = hploop.Id;
                comboBox_hploop.Text = hploop.Name;
            }

            if (mploop != null)
            {
                Config.MpLooptype = mploop.Id;
                comboBox_mploop.Text = mploop.Name;
            }

            if (uniloop != null)
            {
                Config.UniLooptype = uniloop.Id;
                comboBox_uniloop.Text = uniloop.Name;
            }

            if (ammoloop != null)
            {
                Config.AmmoLooptype = ammoloop.Id;
                comboBox_ammoloop.Text = ammoloop.Name;
            }

            if (drugloop != null)
            {
                Config.DrugsLooptype = drugloop.Id;
                comboBox_drugsloop.Text = drugloop.Name;
            }

            if (scrollloop != null)
            {
                Config.ScrollsLooptype = scrollloop.Id;
                comboBox_scrollsloop.Text = scrollloop.Name;
            }
        });
    }
}
