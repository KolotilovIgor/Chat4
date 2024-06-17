using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Chat4;

namespace Chat4
{
    public class Programm
    {
        public static event Action<string> MessageReceived;

        static void Main(string[] args)
        {
            MessageReceived += ConfirmMessageReceived;
            var cts = new CancellationTokenSource();
            var serverThread = new Thread(() => Server(cts.Token));
            serverThread.Start();

            Console.WriteLine("Нажмите любую клавишу для остановки сервера...");
            Console.ReadKey();
            cts.Cancel();
            serverThread.Join();
        }

        public static void ConfirmMessageReceived(string message)
        {
            Console.WriteLine($"Подтверждение: сообщение '{message}' получено.");
        }

        public static void Server(CancellationToken ct)
        {
            using (UdpClient udpClient = new UdpClient(12345))
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Console.WriteLine("Ожидание сообщений...");

                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        if (udpClient.Available > 0)
                        {
                            byte[] buffer = udpClient.Receive(ref iPEndPoint);
                            var messageText = Encoding.UTF8.GetString(buffer);
                            MessageReceived?.Invoke(messageText);
                            Message originalMessage = Message.DeserializeFromJson(messageText);
                            Message clonedMessage = originalMessage.Clone();
                            clonedMessage.Print();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    udpClient.Close();
                    Console.WriteLine("Сервер остановлен по запросу.");
                }
            }
        }
    }
}
