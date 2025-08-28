using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PS2000B
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            string comPort = "COM7"; // adjust to your system

            string devType = GetDeviceType(comPort);
            string serial = GetSerialNumber(comPort);
            string article = GetArticleNumber(comPort);
            float nomV = GetNominalVoltage(comPort);
            float nomP = GetNominalPower(comPort);
            string manuf = GetManufacturer(comPort);
            string sw = GetSoftwareVersion(comPort);

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(comPort, devType, serial, article, nomV, nomP, manuf, sw));
        }

        // ===== Helper functions for binary protocol =====

        private static List<byte> SendAndReceive(string comPort, byte[] telegram, int waitMs = 200)
        {
            List<byte> response = new();
            using (SerialPort port = new SerialPort(comPort, 115200, Parity.None, 8, StopBits.One))
            {
                Thread.Sleep(100);
                port.Open();
                port.Write(telegram, 0, telegram.Length);
                Thread.Sleep(waitMs);
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    response.AddRange(message);
                }
                port.Close();
            }
            return response;
        }

        private static string ReadString(string comPort, byte obj)
        {
            byte[] telegram = { 0x7F, 0x00, obj, 0x00, 0x00 };
            telegram[4] = (byte)((telegram[0] + telegram[1] + telegram[2] + telegram[3]) & 0xFF);
            var response = SendAndReceive(comPort, telegram);
            if (response.Count < 4) return "";
            int len = response[0] & 0x0F;
            StringBuilder sb = new();
            for (int i = 0; i < len; i++)
            {
                if (3 + i < response.Count && response[3 + i] != 0)
                    sb.Append((char)response[3 + i]);
            }
            return sb.ToString();
        }

        private static float ReadFloat(string comPort, byte obj)
        {
            byte[] telegram = { 0x74, 0x00, obj, 0x00, 0x00 };
            telegram[4] = (byte)((telegram[0] + telegram[1] + telegram[2] + telegram[3]) & 0xFF);
            var response = SendAndReceive(comPort, telegram);
            if (response.Count < 7) return float.NaN;
            byte[] arr = { response[6], response[5], response[4], response[3] };
            return BitConverter.ToSingle(arr, 0);
        }

        // === Public wrappers ===
        public static double GetVoltage(string comPort)
        {
            byte[] telegram = { 0x74, 0x00, 0x47, 0x00, 0x00 };
            telegram[4] = (byte)((telegram[0] + telegram[1] + telegram[2] + telegram[3]) & 0xFF);
            var response = SendAndReceive(comPort, telegram);
            if (response.Count < 7) return 0;
            string hex = response[5].ToString("X2") + response[6].ToString("X2");
            int percent = Convert.ToInt32(hex, 16);
            float nomV = GetNominalVoltage(comPort);
            return percent * nomV / 25600.0;
        }

        public static double GetCurrent(string comPort)
        {
            // TODO: implement parsing of actual current (OBJ 71 Word2).
            return 0; 
        }

        public static double GetPower(string comPort)
        {
            double v = GetVoltage(comPort);
            double i = GetCurrent(comPort);
            return v * i;
        }

        public static string GetSerialNumber(string comPort) => ReadString(comPort, 0x01);
        public static string GetDeviceType(string comPort) => ReadString(comPort, 0x00);
        public static string GetArticleNumber(string comPort) => ReadString(comPort, 0x06);
        public static string GetManufacturer(string comPort) => ReadString(comPort, 0x08);
        public static string GetSoftwareVersion(string comPort) => ReadString(comPort, 0x09);

        public static float GetNominalVoltage(string comPort) => ReadFloat(comPort, 0x02);
        public static float GetNominalPower(string comPort) => ReadFloat(comPort, 0x04);

        public static void SwitchRemote(string comPort, bool on)
        {
            byte[] telegram = { 0xF1, 0x00, 0x36, (byte)(on ? 0x10 : 0x00), 0x00, 0x00, 0x00 };
            int sum = 0; foreach (var b in telegram) sum += b;
            telegram[^2] = (byte)((sum >> 8) & 0xFF);
            telegram[^1] = (byte)(sum & 0xFF);
            SendAndReceive(comPort, telegram);
        }

        public static void SwitchOutput(string comPort, bool on)
        {
            byte[] telegram = { 0xF1, 0x00, 0x36, (byte)(on ? 0x01 : 0x00), 0x00, 0x00, 0x00 };
            int sum = 0; foreach (var b in telegram) sum += b;
            telegram[^2] = (byte)((sum >> 8) & 0xFF);
            telegram[^1] = (byte)(sum & 0xFF);
            SendAndReceive(comPort, telegram);
        }

        public static void SetVoltage(string comPort, float volt)
        {
            float nomV = GetNominalVoltage(comPort);
            int percent = (int)Math.Round((25600 * volt) / nomV);
            string hex = percent.ToString("X4");
            byte hi = Convert.ToByte(hex.Substring(0, 2), 16);
            byte lo = Convert.ToByte(hex.Substring(2, 2), 16);
            byte[] telegram = { 0xF2, 0x00, 0x32, hi, lo, 0x00, 0x00 };
            int sum = 0; foreach (var b in telegram) sum += b;
            telegram[^2] = (byte)((sum >> 8) & 0xFF);
            telegram[^1] = (byte)(sum & 0xFF);
            SendAndReceive(comPort, telegram);
        }

        public static void SetPower(string comPort, float p)
        {
            double i = GetCurrent(comPort);
            if (i <= 0) return;
            float vTarget = (float)(p / i);
            SetVoltage(comPort, vTarget);
        }
    }
}
