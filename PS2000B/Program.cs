using PSUBuisnesLogic;
using System;
using System.Windows.Forms;

namespace PS2000B
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form2());
        }

    }
}
