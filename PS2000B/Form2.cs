using PSUBuisnesLogic;
using System;
using System.Windows.Forms;

namespace PS2000B
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Device";
            this.Size = new System.Drawing.Size(400, 300);

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            this.Controls.Add(panel);

            var tbCOMPort = new TextBox { Width = 150 };
            panel.Controls.Add(new Label { Text = "COM Port:", AutoSize = true });
            panel.Controls.Add(tbCOMPort);

            var btnSelect = new Button { Text = "Select", AutoSize = true };
            btnSelect.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(tbCOMPort.Text))
                {
                    string comPort = tbCOMPort.Text.Trim();
                    IPCUUtil psu = PowerSupplyFactory.Create();
                    try
                    {
                        var form1 = new Form1(
                            comPort,
                            psu.GetDeviceType(comPort),
                            psu.GetSerialNumber(comPort),
                            psu.GetItemNumber(comPort),
                            psu.GetNominalVoltage(comPort),
                            psu.GetManufacturer(comPort),
                            psu.GetSWVersion(comPort),
                            psu);
                        form1.ShowDialog();
                        this.Close();
                    }
                    catch (PSUBuisnesLogic.PS2000BPowerSupply.SerialPortNotFoundException ex)
                    {
                        MessageBox.Show(ex.Message, "Serial Port Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid COM port.");
                }
            };
            panel.Controls.Add(btnSelect);
        }
    }
}
