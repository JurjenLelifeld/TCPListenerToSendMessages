using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SendMessagesClient
{
    public class SendMessages : IDisposable
    {
        private TcpClient _tcpClient;

        private const int BufferCount = 100;

        // If you use this constructor you have to set up the connection manually
        public SendMessages()
        {
            //This constructor works only with IPv4 address types.
            _tcpClient = new TcpClient();
        }

        public SendMessages(IPAddress clientAddress, int port)
        {
            //This constructor works only with IPv4 address types.
            _tcpClient = new TcpClient();
            ConnectToRemoteClient(_tcpClient, clientAddress, port);
        }

        public void ConnectToRemoteClient(TcpClient tcpClient, IPAddress clientAddress, int port)
        {
            Console.WriteLine("Connecting.....");

            try
            {
                tcpClient.Connect(clientAddress, port);
                Console.WriteLine("Connected");
            }
            catch (SocketException e)
            {
                throw;
            }
            catch (ObjectDisposedException e)
            {
                throw;
            }
        }

        public byte[] TransmitAndGetResponse(byte[] bytesToSend, out int streamLength)
        {
            byte[] responseBytes;

            try
            {
                Console.WriteLine("Transmitting.....");

                var stream = _tcpClient.GetStream();
                stream.Write(bytesToSend, 0, bytesToSend.Length);

                Console.WriteLine("Waiting for response.....");

                responseBytes = new byte[BufferCount];
                streamLength = stream.Read(responseBytes, 0, BufferCount);
            }
            catch (ObjectDisposedException e)
            {
                throw;
            }
            catch (InvalidOperationException e)
            {
                throw;
            }            

            return responseBytes;
        }

        public byte[] GetMessageAndEncode()
        {
            Console.Write("Enter message: ");
            string inputString = Console.ReadLine();  

            return new ASCIIEncoding().GetBytes(inputString);
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }        
    }
}
