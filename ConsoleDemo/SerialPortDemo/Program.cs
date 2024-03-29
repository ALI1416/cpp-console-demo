using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialPortDemo
{

    /// <summary>
    /// 串口测试
    /// </summary>
    public class Program
    {

        /// <summary>
        /// 串口配置
        /// </summary>
        private static readonly SerialPort serialPort = new SerialPort
        {
            PortName = "COM3",
            BaudRate = 4800,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
        };

        /// <summary>
        /// 串口初始化
        /// </summary>
        private static void SerialPortInit()
        {
            // 接收消息处理
            serialPort.DataReceived += Receive;
            // 建立连接(失败10秒后重连)
            while (true)
            {
                if (!serialPort.IsOpen)
                {
                    // 连接串口
                    try
                    {
                        serialPort.Open();
                        Console.WriteLine("建立连接成功！");
                    }
                    catch
                    {
                        Console.WriteLine("建立连接失败！等待重连...");
                    }
                }
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// 接收消息处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">SerialDataReceivedEventArgs</param>
        private static void Receive(object sender, SerialDataReceivedEventArgs e)
        {
            // 读取串口数据
            int length = serialPort.BytesToRead;
            if (length > 0)
            {
                byte[] buffer = new byte[length];
                serialPort.Read(buffer, 0, length);
                Console.WriteLine("收到消息：" + Encoding.UTF8.GetString(buffer) + " ，Hex：" + Bytes2Hex(buffer));
            }
        }

        /// <summary>
        /// byte[]转Hex字符串
        /// </summary>
        /// <param name="bytes">byte[]</param>
        /// <returns>Hex字符串</returns>
        private static string Bytes2Hex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        private static void Send(string message)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    // 发送消息
                    serialPort.Write(data, 0, data.Length);
                    Console.WriteLine("发送消息：" + message + " ，Hex：" + Bytes2Hex(data));
                }
                catch
                {
                    Console.WriteLine("发送消息失败！");
                }
            }
            else
            {
                Console.WriteLine("连接未建立，发送失败！");
            }
        }

        public static void Main(string[] args)
        {
            // 串口初始化
            new Thread(t =>
            {
                SerialPortInit();
            })
            {
                IsBackground = true
            }.Start();
            // 发送消息
            while (true)
            {
                Send(Console.ReadLine());
            }
        }

    }
}
