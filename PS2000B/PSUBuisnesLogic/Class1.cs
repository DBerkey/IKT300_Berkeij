
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace PSUBuisnesLogic
{
    public interface IPCUUtil
    {
        string GetDeviceType(string comPort);
        string GetSerialNumber(string comPort);
        string GetItemNumber(string comPort);
        string GetManufacturer(string comPort);
        string GetSWVersion(string comPort);
        float GetNominalVoltage(string comPort);
        double GetVoltage(string comPort);
        void SetVoltage(string comPort, float volt);
        void SwitchOutput(string comPort, bool on);
        void SwitchRemote(string comPort, bool on);
    }

    public class PS2000BPowerSupply : IPCUUtil
    {
        public class SerialPortNotFoundException : Exception
        {
            public SerialPortNotFoundException(string message) : base(message) { }
        }

        private List<byte> SendAndReceive(string comPort, byte[] telegram, int waitMs = 200)
        {
            List<byte> response = new();
            try
            {
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
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw new SerialPortNotFoundException($"Serial port error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new SerialPortNotFoundException($"Unexpected error: {ex.Message}");
            }
            return response;
        }

        private string ReadString(string comPort, byte obj)
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

        private float ReadFloat(string comPort, byte obj)
        {
            byte[] telegram = { 0x74, 0x00, obj, 0x00, 0x00 };
            telegram[4] = (byte)((telegram[0] + telegram[1] + telegram[2] + telegram[3]) & 0xFF);
            var response = SendAndReceive(comPort, telegram);
            if (response.Count < 7) return float.NaN;
            byte[] arr = { response[6], response[5], response[4], response[3] };
            return BitConverter.ToSingle(arr, 0);
        }

        public void SwitchRemote(string comPort, bool on)
        {
            if (on)
            {
                byte[] rcTelegram = { 0xF1, 0x00, 0x36, 0x10, 0x10, 0x01, 0x47 };
                SendAndReceive(comPort, rcTelegram, 100);
            }
            else
            {
                byte[] telegram = { 0xF1, 0x00, 0x36, 0x10, 0x00, 0x00, 0x00 };
                int sum = telegram[0] + telegram[1] + telegram[2] + telegram[3] + telegram[4];
                telegram[5] = (byte)((sum >> 8) & 0xFF);
                telegram[6] = (byte)(sum & 0xFF);
                SendAndReceive(comPort, telegram, 100);
            }
        }

        public double GetVoltage(string comPort)
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

        public string GetSerialNumber(string comPort) => ReadString(comPort, 0x01);
        public string GetDeviceType(string comPort) => ReadString(comPort, 0x00);
        public string GetItemNumber(string comPort) => ReadString(comPort, 0x06);
        public string GetManufacturer(string comPort) => ReadString(comPort, 0x08);
        public string GetSWVersion(string comPort) => ReadString(comPort, 0x09);
        public float GetNominalVoltage(string comPort) => ReadFloat(comPort, 0x02);

        public void SwitchOutput(string comPort, bool on)
        {
            SwitchRemote(comPort, true);
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
            telegram[5] = (byte)((sum >> 8) & 0xFF);
            telegram[6] = (byte)(sum & 0xFF);
            SendAndReceive(comPort, telegram, 100);
        }

        public void SetVoltage(string comPort, float volt)
        {
            SwitchRemote(comPort, true);
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

    public static class PowerSupplyFactory
    {
        public static IPCUUtil Create()
        {
            return new PS2000BPowerSupply();
        }
    }
}
