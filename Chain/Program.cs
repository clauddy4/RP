using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chain
{
    class Program
    {
        private static Parameters ParseParameters(string[] args)
        {
            if (args.Length == 4 && args[3] == "true")
            {
                return new Parameters(int.Parse(args[0]), args[1], int.Parse(args[2]), true);
            }
            else if (args.Length == 3)
            {
                return new Parameters(int.Parse(args[0]), args[1], int.Parse(args[2]), false);
            }
            else
            {
                throw new ArgumentException("Invalid argument count. Usage: <listening-port> <next-host> <next-port> [true]");
            }
        }

        private static void StartProcess(Parameters parameters)
        {
            IPAddress ipAddress = parameters.NextHost != "localhost" ? IPAddress.Parse(parameters.NextHost) : IPAddress.Loopback;
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, parameters.NextPort);

            var sender = InitializeSender(ipAddress);
            var listener = InitializeListener(parameters.ListeningPort);

            var numberX = int.Parse(Console.ReadLine());

            ConnectSender(sender, remoteEP);

            var handler = listener.Accept();

            if (parameters.IsInitiator)
            {
                SendNumber(numberX, sender);

                numberX = ReceiveNumber(handler);

                Console.WriteLine(numberX);

                SendNumber(numberX, sender);
            }
            else
            {
                var numberY = ReceiveNumber(handler);

                var maxNumber = Math.Max(numberX, numberY);
                SendNumber(maxNumber, sender);

                numberY = ReceiveNumber(handler);

                Console.WriteLine(numberY);

                SendNumber(numberY, sender);
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private static void SendNumber(int number, Socket sender)
        {
            sender.Send(BitConverter.GetBytes(number));
        }

        private static int ReceiveNumber(Socket handler)
        {
            byte[] buf = new byte[sizeof(int)];
            handler.Receive(buf);
            return BitConverter.ToInt32(buf);
        }

        private static Socket InitializeListener(int port)
        {
            IPAddress ipAddress = IPAddress.Any;

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // CREATE
            Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);

            return listener;
        }

        private static Socket InitializeSender(IPAddress ipAddress)
        {
            // CREATE
            Socket sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            return sender;
        }

        private static void ConnectSender(Socket sender, IPEndPoint endPoint)
        {
            while (true)
            {
                try
                {
                    sender.Connect(endPoint);
                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void Main(string[] args)
        {
            var parameters = ParseParameters(args);
            StartProcess(parameters);
            Console.ReadKey();
        }
    }
}
