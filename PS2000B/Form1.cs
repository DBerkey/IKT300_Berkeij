using System;
using System.Windows.Forms;

namespace PS2000B
{
    public partial class Form1 : Form
    {
    private string comPort;
    private Label lblDevType = new Label();
    private Label lblSerial = new Label();
    private Label lblArticle = new Label();
    private Label lblNomV = new Label();
    private Label lblManufacturer = new Label();
    private Label lblSW = new Label();
    private Label lblVoltage = new Label();
    private Label lblSerialCheck = new Label();
    private TextBox txtSerialBack = new TextBox();

        public Form1(string comPort, string devType, string serial, string article,
                     float nomV, string manuf, string sw)
        {
            this.comPort = comPort;
            InitializeComponent();
            // Ensure labels are not null before setting text
            lblDevType.Text = devType ?? "-";
            lblSerial.Text = serial ?? "-";
            lblArticle.Text = article ?? "-";
            lblNomV.Text = $"{nomV:F1} V";
            lblManufacturer.Text = manuf ?? "-";
            lblSW.Text = sw ?? "-";
        }

        private void InitializeComponent()
        {
            this.Text = "PS2000B GUI";
            this.Size = new System.Drawing.Size(550, 500);

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            lblDevType = AddRow(panel, "Device Type:");
            lblSerial = AddRow(panel, "Serial:");
            lblArticle = AddRow(panel, "Article:");
            lblNomV = AddRow(panel, "Nominal Voltage:");
            lblManufacturer = AddRow(panel, "Manufacturer:");
            lblSW = AddRow(panel, "SW Version:");
            lblVoltage = AddRow(panel, "Current Voltage:");

            panel.Controls.Add(new Label { Text = "Serial on back:", AutoSize = true });
            txtSerialBack = new TextBox { Width = 150 };
            txtSerialBack.TextChanged += (s, e) => CheckSerialMatch();
            panel.Controls.Add(txtSerialBack);
            lblSerialCheck = new Label { Text = "Match: -", AutoSize = true };
            panel.Controls.Add(lblSerialCheck);
            panel.SetFlowBreak(lblSerialCheck, true);

            var btnShowV = new Button { Text = "Show Voltage", AutoSize = true };
            btnShowV.Click += (s, e) => lblVoltage.Text = $"{Program.GetVoltage(comPort):F2} V";
            var btnSetV = new Button { Text = "Set Voltage", AutoSize = true };
            btnSetV.Click += (s, e) =>
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter voltage (V):");
                if (float.TryParse(input, out float v)) Program.SetVoltage(comPort, v);
            };

            var btnOutOn = new Button { Text = "Output ON", AutoSize = true };
            btnOutOn.Click += (s, e) => Program.SwitchOutput(comPort, true);
            var btnOutOff = new Button { Text = "Output OFF", AutoSize = true };
            btnOutOff.Click += (s, e) => Program.SwitchOutput(comPort, false);
            var btnRemOn = new Button { Text = "Remote ON", AutoSize = true };
            btnRemOn.Click += (s, e) => Program.SwitchRemote(comPort, true);
            var btnRemOff = new Button { Text = "Remote OFF", AutoSize = true };
            btnRemOff.Click += (s, e) => Program.SwitchRemote(comPort, false);

            panel.Controls.AddRange(new Control[] { btnShowV, btnSetV, btnOutOn, btnOutOff, btnRemOn, btnRemOff });
            this.Controls.Add(panel);
        }

        private void CheckSerialMatch()
        {
            string entered = txtSerialBack.Text.Trim();
            string actual = lblSerial.Text.Trim();
            if (string.IsNullOrEmpty(entered)) { lblSerialCheck.Text = "Match: -"; lblSerialCheck.ForeColor = System.Drawing.Color.Gray; return; }
            bool match = string.Equals(entered, actual, StringComparison.OrdinalIgnoreCase);
            lblSerialCheck.Text = match ? "Match: YES" : "Match: NO";
            lblSerialCheck.ForeColor = match ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private Label AddRow(FlowLayoutPanel p, string caption)
        {
            var lbl = new Label { Text = caption, AutoSize = true };
            var val = new Label { Text = "-", AutoSize = true, Width = 200 };
            p.Controls.Add(lbl);
            p.Controls.Add(val);
            p.SetFlowBreak(val, true);
            return val;
        }
    }
}
