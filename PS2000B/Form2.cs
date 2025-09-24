using PSUBuisnesLogic;
using System;
using System.IO.Ports;
using System.Linq;
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

            // COM Port selection
            var cbCOMPort = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            panel.Controls.Add(new Label { Text = "COM Port:", AutoSize = true });
            
            // Get available COM ports
            string[] availablePorts = SerialPort.GetPortNames();
            if (availablePorts.Length > 0)
            {
                cbCOMPort.Items.AddRange(availablePorts.OrderBy(p => p).ToArray());
                cbCOMPort.SelectedIndex = 0; // Select first available port by default
            }
            else
            {
                cbCOMPort.Items.Add("No COM ports available");
                cbCOMPort.SelectedIndex = 0;
            }
            panel.Controls.Add(cbCOMPort);

            // Add refresh button for COM ports
            var btnRefresh = new Button { Text = "Refresh Ports", AutoSize = true };
            btnRefresh.Click += (s, e) =>
            {
                cbCOMPort.Items.Clear();
                string[] ports = SerialPort.GetPortNames();
                if (ports.Length > 0)
                {
                    cbCOMPort.Items.AddRange(ports.OrderBy(p => p).ToArray());
                    cbCOMPort.SelectedIndex = 0;
                }
                else
                {
                    cbCOMPort.Items.Add("No COM ports available");
                    cbCOMPort.SelectedIndex = 0;
                }
            };
            panel.Controls.Add(btnRefresh);

            var tbVersion = new TextBox { Width = 150, Text = "2000" }; 
            panel.Controls.Add(new Label { Text = "Version (2000 or -1 for dummy):", AutoSize = true });
            panel.Controls.Add(tbVersion);

            var btnSelect = new Button { Text = "Select", AutoSize = true };
            btnSelect.Click += (s, e) =>
            {
                if (cbCOMPort.SelectedItem != null && 
                    cbCOMPort.SelectedItem.ToString() != "No COM ports available" && 
                    !string.IsNullOrWhiteSpace(tbVersion.Text))
                {
                    string comPort = cbCOMPort.SelectedItem.ToString();
                    if (int.TryParse(tbVersion.Text.Trim(), out int version))
                    {
                        try
                        {
                            IPCUUtil psu = PowerSupplyFactory.Create(version);
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
                        catch (NotSupportedException ex)
                        {
                            MessageBox.Show(ex.Message, "Version Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid version number (2000 or -1 for dummy).");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a valid COM port and enter version.");
                }
            };
            panel.Controls.Add(btnSelect);
        }
    }
}
