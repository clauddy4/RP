using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client
{
    class Program
    {
        private const string EndOfData = "<EOF>";
        public static void StartClient(Parameters parameters)
        {
            try
            {
                IPAddress ipAddress = parameters.Address != "localhost" ? IPAddress.Parse(parameters.Address) : IPAddress.Loopback;

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, parameters.Port);

                // CREATE
                Socket sender = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                try
                {
                    // CONNECT
                    sender.Connect(remoteEP);

                    byte[] msg = Encoding.UTF8.GetBytes(parameters.Message + EndOfData);

                    // SEND
                    int bytesSent = sender.Send(msg);

                    // RECEIVE
                    byte[] buf = new byte[1024];

                    var history = new List<string>();
                    string data = null;

                    while (true)
                    {
                        int bytesRec = sender.Receive(buf);
                        data += Encoding.UTF8.GetString(buf, 0, bytesRec); 

                        try
                        {
                            history = JsonSerializer.Deserialize<List<string>>(data);
                            break;
                        }
                        catch { }
                    }

                    foreach (var item in history)
                    {
                        Console.WriteLine(item);
                    }

                    // RELEASE
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static Parameters ParseParameters(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Invalid argument count. Usage: <address> <port> <message>");
            }
            var parameters = new Parameters(args[0], Convert.ToInt32(args[1]), args[2]);
            return parameters;
        }

        static void Main(string[] args)
        {
            var parameters = ParseParameters(args);
            StartClient(parameters);
        }
    }
}