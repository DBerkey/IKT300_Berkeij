namespace PS2000B;

public partial class Form1 : Form
{
    private Label VoltLabel;
    private Label SerialNumberLabel;
    private Label DeviceTypeLabel;
    private Label ArticleNumberLabel;

    public Form1(double voltage, string serialNumber, string deviceType, string articleNumber)
    {
        InitializeComponent();
        VoltLabel = new Label();
        VoltLabel.AutoSize = true;
        VoltLabel.Location = new System.Drawing.Point(30, 30);
        VoltLabel.Name = "VoltLabel";
        VoltLabel.Size = new System.Drawing.Size(200, 23);
        VoltLabel.TabIndex = 0;
        VoltLabel.Text = $"Voltage: {voltage}";
        this.Controls.Add(VoltLabel);

        SerialNumberLabel = new Label();
        SerialNumberLabel.AutoSize = true;
        SerialNumberLabel.Location = new System.Drawing.Point(30, 60);
        SerialNumberLabel.Name = "SerialNumberLabel";
        SerialNumberLabel.Size = new System.Drawing.Size(200, 23);
        SerialNumberLabel.TabIndex = 1;
        SerialNumberLabel.Text = $"Serial Number: {serialNumber}";
        this.Controls.Add(SerialNumberLabel);

        DeviceTypeLabel = new Label();
        DeviceTypeLabel.AutoSize = true;
        DeviceTypeLabel.Location = new System.Drawing.Point(30, 90);
        DeviceTypeLabel.Name = "DeviceTypeLabel";
        DeviceTypeLabel.Size = new System.Drawing.Size(200, 23);
        DeviceTypeLabel.TabIndex = 2;
        DeviceTypeLabel.Text = $"Device Type: {deviceType}";
        this.Controls.Add(DeviceTypeLabel);

        WattLabel = new Label();
        WattLabel.AutoSize = true;
        WattLabel.Location = new System.Drawing.Point(30, 120);
        WattLabel.Name = "WattLabel";
        WattLabel.Size = new System.Drawing.Size(200, 23);
        WattLabel.TabIndex = 5;
        WattLabel.Text = $"Power: {watt} W";
        this.Controls.Add(WattLabel);
    }
}
