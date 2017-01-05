using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using ZPBot.Annotations;
using ZPBot.Common;
using ZPBot.Common.Items;
using ZPBot.Common.Resources;

namespace ZPBot
{
    internal partial class Form1
    {
        private bool FilterCheck(Item item, [NotNull] string pattern)
        {
            var typeFilter = comboBox_itemtype.SelectedIndex;
            var degreeFilter = comboBox_itemdegree.SelectedIndex;
            var rarityFilter = comboBox_itemrarity.SelectedIndex;
            var sortFilter = comboBox_itemsort.SelectedIndex;
            var raceFilter = comboBox_itemrace.SelectedIndex;
            var genderFilter = comboBox_itemgender.SelectedIndex;

            if (pattern.Length > 0 && !item.Name.ToLower().Contains(pattern))
                return false;

            switch (typeFilter)
            {
                case 1:
                    if (item.CurrencyType3 != ECurrencyType3.Gold) return false;
                    break;
                case 2:
                    if (item.ItemType1 != EItemType1.Equipable) return false;
                    break;
                case 3:
                    if (item.ItemType1 != EItemType1.Consumable) return false;
                    break;
                case 4:
                    if (item.ConsumableType2 != EConsumableType2.Quest) return false;
                    break;
                case 5:
                    if (!_globalManager.ItemDropManager.ExistsPickup(item)) return false;
                    break;
            }

            if (degreeFilter > 0 && degreeFilter != item.Degree)
                return false;

            switch (rarityFilter)
            {
                case 1:
                    if (item.RareType != ERareType.Star) return false;
                    break;
                case 2:
                    if (item.RareType != ERareType.Moon) return false;
                    break;
                case 3:
                    if (item.RareType != ERareType.Sun) return false;
                    break;

            }

            switch (sortFilter)
            {
                case 1:
                    if (item.EquipableType2 != EEquipableType2.CArmor) return false;
                    break;
                case 2:
                    if (item.EquipableType2 != EEquipableType2.CProtector) return false;
                    break;
                case 3:
                    if (item.EquipableType2 != EEquipableType2.CGarment) return false;
                    break;
                case 4:
                    if (item.EquipableType2 != EEquipableType2.EArmor) return false;
                    break;
                case 5:
                    if (item.EquipableType2 != EEquipableType2.EProtector) return false;
                    break;
                case 6:
                    if (item.EquipableType2 != EEquipableType2.EGarment) return false;
                    break;
            }

            switch (raceFilter)
            {
                case 1:
                    if (item.Race != ERace.Chinese) return false;
                    break;
                case 2:
                    if (item.Race != ERace.European) return false;
                    break;
            }

            switch (genderFilter)
            {
                case 1:
                    if (item.Gender != EGender.None) return false;
                    break;
                case 2:
                    if (item.Gender != EGender.Male) return false;
                    break;
                case 3:
                    if (item.Gender != EGender.Female) return false;
                    break;
            }

            return true;
        }

        private void button_resetitemfilter_Click(object sender, EventArgs e)
        {
            textBox_searchitem.Clear();
            comboBox_itemtype.SelectedIndex = 0;
            comboBox_itemdegree.SelectedIndex = 0;
            comboBox_itemrarity.SelectedIndex = 0;
            comboBox_itemsort.SelectedIndex = 0;
            comboBox_itemrace.SelectedIndex = 0;
            comboBox_itemgender.SelectedIndex = 0;
        }

        private void button_searchitem_Click(object sender, EventArgs e)
        {
            if (!_finishLoad)
                return;

            var sw = new Stopwatch();
            sw.Start();

            var itemPattern = textBox_searchitem.Text;

            var items = new List<Item>();
            foreach (var item in Silkroad.RItemdata)
            {
                if (!FilterCheck(item.Value, itemPattern))
                    continue;

                var addItem = new Item(item.Value) {Additional = _globalManager.ItemDropManager.ExistsPickup(item.Value) ? "Pick" : ""};
                items.Add(addItem);
            }

            dataGridView_items.AutoGenerateColumns = false;
            dataGridView_items.DataSource = items;

            sw.Stop();
            Console.WriteLine(@"We found " + items.Count + @" Items in " + sw.Elapsed);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvRow in dataGridView_items.SelectedRows)
            {
                var item = (Item) dgvRow.DataBoundItem;
                if (item == null)
                    continue;

                if (_globalManager.ItemDropManager.AddPickup(item.Id)) item.Additional = "Pick";
            }

            UpdateItemCfg();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvRow in dataGridView_items.SelectedRows)
            {
                var item = (Item)dgvRow.DataBoundItem;
                if (item == null)
                    continue;

                if (_globalManager.ItemDropManager.RemovePickup(item.Id)) item.Additional = "";
            }

            UpdateItemCfg();
        }

        private void PickupSettings() => Invoke((MethodInvoker)delegate
        {
            if (_iniSet == null) return;

            for (var i = 0; i < _iniSet.Read<int>("Items", "Count"); i++)
            {
                try
                {
                    var itemId = _iniSet.Read<uint>("Items", i.ToString());
                    _globalManager.ItemDropManager.AddPickup(itemId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"PickupSettings - " + ex.Message);
                }
            }
        });

        private void UpdateItemCfg()
        {
            _iniSet.RemoveSection("Items");

            var itemList = _globalManager.ItemDropManager.ItemFilter;
            if (itemList.Count == 0)
                return;

            _iniSet.Write("Items", "Count", itemList.Count.ToString());

            for (var i = 0; i < itemList.Count; i++)
                _iniSet.Write("Items", i.ToString(), itemList[i].ToString());
        }
    }
}
