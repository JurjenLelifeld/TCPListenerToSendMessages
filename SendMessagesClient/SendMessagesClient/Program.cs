using System;
using System.Net;
using System.Net.Sockets;

namespace SendMessagesClient
{
    class Program
    {
        private static string _hostName;
        private static int _port;

        static void Main(string[] args)
        {
            GetUserInput();
            if (IsUserInputValid())
                return;

            var clientAddress = GetClientIPAddress();
            if (clientAddress == null)
                return;

            try
            {
                using (var messageClient = new SendMessages(clientAddress, _port))
                {
                    var bytesToSend = messageClient.GetMessageAndEncode();

                    int streamLength;
                    var responseBytes = messageClient.TransmitAndGetResponse(bytesToSend, out streamLength);

                    if (responseBytes == null)
                        throw new ArgumentNullException("responseBytes", "Receiving error, no bytes received");

                    for (int i = 0; i < streamLength; i++)
                        Console.Write(Convert.ToChar(responseBytes[i]));

                    Console.WriteLine("Message sent, exiting...");
                }            
            }
            catch (SocketException e)
            {
                Console.WriteLine("Error with code {0} occured within the network. {0}", e.ErrorCode, e.StackTrace);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("Object has already been disposed. {0}", e.StackTrace);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCP Client is not connected (anymore) to the remote host. {0}", e.StackTrace);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("TCP Client was not able to receive from local host, '{0}' is empty. {1}", e.ParamName, e.Message);
            }
        }

        private static void GetUserInput()
        {
            Console.WriteLine("Enter hostname: ");
            _hostName = Console.ReadLine();

            Console.WriteLine("Enter port number: ");
            int.TryParse(Console.ReadLine(), out _port);
        }

        private static bool IsUserInputValid()
        {
            return string.IsNullOrWhiteSpace(_hostName) || _port <= IPEndPoint.MinPort || _port >= IPEndPoint.MaxPort;
        }

        private static IPAddress GetClientIPAddress()
        {
            IPAddress clientAddress = null;

            try
            {
                #if DEBUG
                    IPAddress[] foundIPs = Dns.GetHostAddresses(Dns.GetHostName());
                #else
                    IPAddress[] foundIPs = Dns.GetHostAddresses(HOSTNAME);
                #endif
                
                foreach (var currentAddress in foundIPs)
                {
                    if (currentAddress.AddressFamily == AddressFamily.InterNetwork)
                        clientAddress = currentAddress;
                }

                if (clientAddress != null)
                    Console.WriteLine("GetHostAddresses({0}) returns: {1}", _hostName, clientAddress.ToString());
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("The length of the hostname is greater than 255 characters. Name: {0}. {1}", _hostName, e.StackTrace);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Error with code {0} occured within the network. {0}", e.ErrorCode, e.StackTrace);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Hostname is an invalid name. Name: {0}. {1}", _hostName, e.StackTrace);
            }
            return clientAddress;
        }
    }
}