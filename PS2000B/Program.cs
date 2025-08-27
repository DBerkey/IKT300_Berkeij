namespace PS2000B;

using System;
using System.Collections;
using System.IO.Ports;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        string comPort = "COM7"; // Replace with your COM port
        double voltage = getVoltage(comPort);
        string serialNumber = getSerialNumber(comPort);
        string deviceType = getDeviceType(comPort);
        string articleNumber = getArticleNumber(comPort);

        // console log of values
        System.Windows.Forms.MessageBox.Show($"Voltage: {voltage}");
        System.Windows.Forms.MessageBox.Show($"Serial Number: {serialNumber}");
        System.Windows.Forms.MessageBox.Show($"Device Type: {deviceType}");
        System.Windows.Forms.MessageBox.Show($"Article Number: {articleNumber}");

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1(voltage, serialNumber, deviceType, articleNumber));
        static double getVoltage(string comPort)
        {
            double volt = 0;
            int percentVolt = 0;

            // get voltage

            //SD = MessageType + CastType + Direction + Length
            int SDHex = (int)0x40 + (int)0x20 + 0x10 + 5; //6-1 ref spec 3.1.1
            byte SD = Convert.ToByte(SDHex.ToString(), 10);

            //SD, DN, OBJ, DATA, CS
            byte[] byteWithOutCheckSum = { SD, (int)0x00, (int)0x47, 0x0, 0x0 }; // quert status

            int sum = 0;
            int arrayLength = byteWithOutCheckSum.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                sum += byteWithOutCheckSum[i];
            }

            string hexSum = sum.ToString("X");
            string cs1 = "";
            string cs2 = "";
            if (hexSum.Length == 4)
            {
                cs1 = hexSum.Substring(0, hexSum.Length / 2);
                cs2 = hexSum.Substring(hexSum.Length / 2);
            }
            else if (hexSum.Length == 3)
            {
                cs1 = hexSum.Substring(0, 1);
                cs2 = hexSum.Substring(1);
            }
            else if ((hexSum.Length is 2) || (hexSum.Length is 1))
            {
                cs1 = "0";
                cs2 = hexSum;
            }

            if (cs1 != "")
            {


                byteWithOutCheckSum[arrayLength - 2] = Convert.ToByte(cs1, 16);
                byteWithOutCheckSum[arrayLength - 1] = Convert.ToByte(cs2, 16);
            }

            // now the byte array is ready to be sent

            List<byte> responseTelegram;
            using (SerialPort port = new SerialPort(comPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(byteWithOutCheckSum, 0, byteWithOutCheckSum.Length);
                Thread.Sleep(500);

                responseTelegram = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        Console.WriteLine(t);
                        responseTelegram.Add(t);
                    }
                }
                port.Close();
                Thread.Sleep(500);
            }

            if (responseTelegram == null)
            {
                Console.WriteLine("No telegram was read");
            }
            else
            {

                string percentVoltString = responseTelegram[5].ToString("X") + responseTelegram[6].ToString("X");
                percentVolt = Convert.ToInt32(percentVoltString, 16);


            }
            float nominalVoltage = 0;

            // get nominal voltage
            List<byte> response;
            byte[] bytesToSend = { 0x74, 0x00, 0x02, 0x00, 0x76 };

            using (SerialPort port = new SerialPort("COM7", 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                port.Write(bytesToSend, 0, bytesToSend.Length);
                Thread.Sleep(50);
                response = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        response.Add(t);
                    }
                }
                port.Close();
                Thread.Sleep(500);
            }
            if (response == null)
            {
                Console.WriteLine("No telegram was read");
            }
            else
            {
                byte[] byteArray = { response[6], response[5], response[4], response[3] };
                nominalVoltage = BitConverter.ToSingle(byteArray, 0);
                volt = (double)percentVolt * nominalVoltage / 25600;
                Console.WriteLine(string.Format("Voltage:{0}", volt));
            }

            return volt;
        }

        static string getSerialNumber(string comPort)
        {
            // reading serial number
            List<byte> Serialresponse;
            // Remember the dataframe setup, SD, DN,   OBJ, DATA checksum1, checksum2
            // OBJ = 0x01 = 1
            string serialNumberString = "";
            byte[] serialBytesToSend = { 0x7F, 0x00, 0x01, 0x00, 0x80 };
            using (SerialPort port = new SerialPort("COM7", 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(serialBytesToSend, 0, serialBytesToSend.Length);
                Thread.Sleep(500);

                Serialresponse = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        //Console.WriteLine(t);
                        Serialresponse.Add(t);
                    }
                }
                port.Close();
                Thread.Sleep(500);

                string binary = Convert.ToString(Serialresponse[0], 2);
                string payloadLengtBinaryString = binary.Substring(4);
                int payloadLength = Convert.ToInt32(payloadLengtBinaryString, 2);

                if (Serialresponse[2] == 1) // means that I got a response on obj, which is refers to the object list.
                {
                    for (var i = 0; i < payloadLength; i++)
                    {
                        serialNumberString += Convert.ToChar(Serialresponse[3 + i]);
                    }
                }

                Console.WriteLine(string.Format("serialNumberString:{0}", serialNumberString));

            }

            return serialNumberString;
        }

        static string getDeviceType(string comPort)
        {
            // reading device type
            List<byte> deviceTypeResponse;
            string deviceTypeString = "";
            byte[] deviceTypeBytesToSend = { 0x7F, 0x00, 0x00, 0x00, 0x7F };
            using (SerialPort port = new SerialPort("COM7", 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(deviceTypeBytesToSend, 0, deviceTypeBytesToSend.Length);
                Thread.Sleep(500);

                deviceTypeResponse = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        deviceTypeResponse.Add(t);
                    }
                }
                port.Close();
                Thread.Sleep(500);

                string binary = Convert.ToString(deviceTypeResponse[0], 2);
                string payloadLengtBinaryString = binary.Substring(4);
                int payloadLength = Convert.ToInt32(payloadLengtBinaryString, 2);

                if (deviceTypeResponse[2] == 2) // means that I got a response on obj, which is refers to the object list.
                {
                    for (var i = 0; i < payloadLength; i++)
                    {
                        deviceTypeString += Convert.ToChar(deviceTypeResponse[3 + i]);
                    }
                }

                Console.WriteLine(string.Format("deviceTypeString:{0}", deviceTypeString));
            }
            return deviceTypeString;
        }

        static string getArticleNumber(string comPort)
        {
            byte SD = 0x7F;
            byte DN = 0x00; // Output 1
            byte OBJ = 0x06; // Device article no
            byte CS = (byte)((SD + DN + OBJ + 0x00) & 0xFF);
            byte[] telegram = { SD, DN, OBJ, 0x00, CS };

            string articleNumberString = "";

            using (SerialPort port = new SerialPort(comPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(telegram, 0, telegram.Length);
                Thread.Sleep(500);

                // Read the response
                List<byte> response = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        response.Add(t);
                    }
                }
                port.Close();
                Thread.Sleep(500);

                // Process the response
                if (response.Count > 0)
                {
                    for (int i = 0; i < response.Count; i++)
                    {
                        articleNumberString += Convert.ToChar(response[i]);
                    }
                }
            }

            return articleNumberString;
        }
    }
}