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
            string comPort = "COM8"; // adjust to your system

            string devType = GetDeviceType(comPort);
            string serial = GetSerialNumber(comPort);
            string article = GetArticleNumber(comPort);
            float nomV = GetNominalVoltage(comPort);
            string manuf = GetManufacturer(comPort);
            string sw = GetSoftwareVersion(comPort);

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(comPort, devType, serial, article, nomV, manuf, sw));
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

        public static string GetSerialNumber(string comPort) => ReadString(comPort, 0x01);
        public static string GetDeviceType(string comPort) => ReadString(comPort, 0x00);
        public static string GetArticleNumber(string comPort) => ReadString(comPort, 0x06);
        public static string GetManufacturer(string comPort) => ReadString(comPort, 0x08);
        public static string GetSoftwareVersion(string comPort) => ReadString(comPort, 0x09);

        public static float GetNominalVoltage(string comPort) => ReadFloat(comPort, 0x02);

        public static void SwitchRemote(string comPort, bool on)
        {
            // Use the working telegram for enabling remote control
            if (on)
            {
                byte[] rcTelegram = { 0xF1, 0x00, 0x36, 0x10, 0x10, 0x01, 0x47 };
                SendAndReceive(comPort, rcTelegram, 100);
            }
            else
            {
                // Minimal telegram for disabling remote control: [SD, DN, OBJ, DATA, CS1, CS2]
                byte[] telegram = { 0xF1, 0x00, 0x36, 0x10, 0x00, 0x00, 0x00 };
                // Calculate checksum from first 4 bytes
                int sum = telegram[0] + telegram[1] + telegram[2] + telegram[3] + telegram[4];
                telegram[5] = (byte)((sum >> 8) & 0xFF); // High byte
                telegram[6] = (byte)(sum & 0xFF);        // Low byte
                SendAndReceive(comPort, telegram, 100);
            }
        }

        public static void SwitchOutput(string comPort, bool on)
        {
            SwitchRemote(comPort, true); // Ensure remote control is enabled

            byte[] telegram;
            if (on)
            {
                telegram = new byte[] { 0xF1, 0x00, 0x36, 0x01, 0x01, 0x00, 0x00 };
            }
            else
            {
                telegram = new byte[] { 0xF1, 0x00, 0x36, 0x01, 0x00, 0x00, 0x00 };
            }
            int sum = telegram[0] + telegram[1] + telegram[2] + telegram[3] + telegram[4];
            telegram[5] = (byte)((sum >> 8) & 0xFF); // High byte
            telegram[6] = (byte)(sum & 0xFF);        // Low byte
            SendAndReceive(comPort, telegram, 100);
        }

        public static void SetVoltage(string comPort, float volt)
        {
            // 1. Enable remote control
            SwitchRemote(comPort, true); // Ensure remote control is enabled

            // 2. Calculate percent value for voltage
            float nomV = GetNominalVoltage(comPort);
            int percentSetValue = (int)Math.Round((25600 * volt) / nomV);
            string hexValue = percentSetValue.ToString("X");
            string hexValue1 = "";
            string hexValue2 = "";
            if (hexValue.Length == 4)
            {
                hexValue1 = hexValue.Substring(0, 2);
                hexValue2 = hexValue.Substring(2, 2);
            }
            else if (hexValue.Length == 3)
            {
                hexValue1 = hexValue.Substring(0, 1);
                hexValue2 = hexValue.Substring(1);
            }
            else if (hexValue.Length == 2 || hexValue.Length == 1)
            {
                hexValue1 = "0";
                hexValue2 = hexValue;
            }

            byte[] telegram = { 0xF2, 0x00, 0x32, Convert.ToByte(hexValue1, 16), Convert.ToByte(hexValue2, 16), 0x00, 0x00 };
            int sum = 0;
            foreach (var b in telegram) sum += b;
            string hexSum = sum.ToString("X");
            string cs1 = "";
            string cs2 = "";
            if (hexSum.Length == 4)
            {
                cs1 = hexSum.Substring(0, 2);
                cs2 = hexSum.Substring(2, 2);
            }
            else if (hexSum.Length == 3)
            {
                cs1 = hexSum.Substring(0, 1);
                cs2 = hexSum.Substring(1);
            }
            else if (hexSum.Length == 2 || hexSum.Length == 1)
            {
                cs1 = "0";
                cs2 = hexSum;
            }
            if (cs1 != "")
            {
                telegram[telegram.Length - 2] = Convert.ToByte(cs1, 16);
                telegram[telegram.Length - 1] = Convert.ToByte(cs2, 16);
            }

            // 3. Send voltage telegram and check response
            var response = SendAndReceive(comPort, telegram, 500);
            Thread.Sleep(100);
            if (response.Count > 3 && response[3] == 0)
            {
                Console.WriteLine("New voltage was set");
            }
            else if (response.Count > 3)
            {
                Console.WriteLine($"Voltage not set, error: {response[3]}");
            }
            else
            {
                Console.WriteLine("No response or unexpected response when setting voltage.");
            }
        }
    }
}
