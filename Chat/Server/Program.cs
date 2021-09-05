using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    class Program
    {
        private static readonly List<string> _history = new List<string>();
        private const string EndOfData = "<EOF>";

        public static void StartListening(int port)
        {
            // Привязываем сокет ко всем интерфейсам на текущей машинe
            IPAddress ipAddress = IPAddress.Any;

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // CREATE
            Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                // BIND
                listener.Bind(localEndPoint);

                // LISTEN
                listener.Listen(10);

                while (true)
                {
                    // ACCEPT
                    Socket handler = listener.Accept();

                    byte[] buf = new byte[1024];
                    string data = null;

                    while (true)
                    {
                        // RECEIVE
                        int bytesRec = handler.Receive(buf);

                        data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                        if (data.IndexOf(EndOfData) > -1)
                        {
                            break;
                        }
                    }

                    data = data.Remove(data.Length - 5, 5);

                    _history.Add(data);
                    Console.WriteLine($"Message received: {data}");

                    // Отправляем текст обратно клиенту
                    var dataJsonString = JsonSerializer.Serialize(_history);
                    byte[] msg = Encoding.UTF8.GetBytes(dataJsonString);

                    // SEND
                    handler.Send(msg);

                    // RELEASE
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void Main(string[] args)
        {
            var port = Convert.ToInt32(args[0]);
            StartListening(port);
        }
    }
}