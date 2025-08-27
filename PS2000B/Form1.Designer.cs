namespace PS2000B;
/// <summary>
/// Create a graphical user interface for PS2000 that does the following:
///- at startup it checks the device type and writes this in Gui*.
///- at start-up, it checks the serial number and writes this in Gui*. Also check that the serial number matches the actual serial number on the back of the PS2000.
///- at start-up, checks the nominal voltage (volts) and writes this in Gui*.
///- At start-up, check the rated power (watts) and enter this in Gui*.
///- at start-up, check the article number and enter this in Gui*.
///- at start-up checks manufacturer and writes this in Gui*
///- at start-up, check software version and write this in Gui*
///*These must remain permanently in the Gui until the program is closed.
///
///Pressing a button shows the current voltage (V)
///Pressing a button shows the current power (W)
///Pressing a button sets the desired voltage (V)
///Pressing a button sets the desired power (W)
/// </summary>
partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Text = "Form1";
        
    }

    #endregion
}
