using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>PLC D0 报警位状态列表（只读）。</summary>
    public sealed class PlcAlarmPanelForm : Form
    {
        private readonly ListView _list = new ListView();

        public PlcAlarmPanelForm()
        {
            Text = "PLC 报警状态";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 420);
            Font = new Font("Microsoft YaHei UI", 10F);
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(8),
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            _list.Dock = DockStyle.Fill;
            _list.View = View.Details;
            _list.FullRowSelect = true;
            _list.GridLines = true;
            _list.Columns.Add("位地址", 90);
            _list.Columns.Add("说明", 280);
            _list.Columns.Add("状态", 80);

            var btnClose = new Button { Text = "关闭", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 100 };
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            bottom.Controls.Add(btnClose);

            panel.Controls.Add(_list, 0, 0);
            panel.Controls.Add(bottom, 0, 1);
            Controls.Add(panel);
            AcceptButton = btnClose;
        }

        public void Bind(ushort alarmWord, int pcMaterialBit, int pcPositionLimitBit = 12, int pcVisionRecognizeFailBit = 13)
        {
            _list.Items.Clear();
            foreach (var bit in PlcAlarmDefinitions.PlcToPcAlarms)
            {
                bool on = (alarmWord & (1 << bit.BitIndex)) != 0;
                var item = new ListViewItem($"D0.{bit.BitIndex}");
                item.SubItems.Add(bit.Name);
                item.SubItems.Add(on ? "报警" : "正常");
                item.ForeColor = on ? Color.FromArgb(197, 48, 48) : Color.FromArgb(45, 55, 72);
                if (on) item.Font = new Font(item.Font, FontStyle.Bold);
                _list.Items.Add(item);
            }
            bool pcOn = (alarmWord & (1 << pcMaterialBit)) != 0;
            var pcItem = new ListViewItem($"D0.{pcMaterialBit}");
            pcItem.SubItems.Add(PlcAlarmDefinitions.PcForeignObjectAlarmBitName);
            pcItem.SubItems.Add(pcOn ? "1(异物)" : "0(正常)");
            pcItem.ForeColor = pcOn ? Color.DarkGreen : Color.DimGray;
            _list.Items.Add(pcItem);
            bool limitOn = (alarmWord & (1 << pcPositionLimitBit)) != 0;
            var limitItem = new ListViewItem($"D0.{pcPositionLimitBit}");
            limitItem.SubItems.Add(PlcAlarmDefinitions.PcPositionLimitAlarmBitName);
            limitItem.SubItems.Add(limitOn ? "1(超限)" : "0(正常)");
            limitItem.ForeColor = limitOn ? Color.FromArgb(197, 48, 48) : Color.DimGray;
            if (limitOn) limitItem.Font = new Font(limitItem.Font, FontStyle.Bold);
            _list.Items.Add(limitItem);
            bool visionFailOn = (alarmWord & (1 << pcVisionRecognizeFailBit)) != 0;
            var visionFailItem = new ListViewItem($"D0.{pcVisionRecognizeFailBit}");
            visionFailItem.SubItems.Add(PlcAlarmDefinitions.PcVisionRecognizeFailAlarmBitName);
            visionFailItem.SubItems.Add(visionFailOn ? "1(识别失败)" : "0(正常)");
            visionFailItem.ForeColor = visionFailOn ? Color.FromArgb(197, 48, 48) : Color.DimGray;
            if (visionFailOn) visionFailItem.Font = new Font(visionFailItem.Font, FontStyle.Bold);
            _list.Items.Add(visionFailItem);
        }
    }
}
