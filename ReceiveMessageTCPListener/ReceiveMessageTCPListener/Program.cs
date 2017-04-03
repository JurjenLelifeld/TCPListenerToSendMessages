using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ReceiveMessageTCPListener
{
    static class Program
    {
        private static readonly string AppName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
        private const string RegistryLocation = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const int PORT = 53535;
        private const string MessageBoxCaption = "Important!";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CheckAndSetStartup();

            IPAddress ipAddress = GetHostMachineIPAddress();
            if (ipAddress == null)
                return;

            ReceiveMessages(ipAddress);
        }

        private static void ReceiveMessages(IPAddress ipAddress)
        {
            TcpListener listener = null; 

            try
            {
                listener = new TcpListener(ipAddress, PORT);
                listener.Start();

                while (true)
                {
                    Socket client = listener.AcceptSocket();
                    client.ReceiveTimeout = 15000;
                    StartThreadToReceiveIncomingMessage(client);
                }
            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine("IP address is null! \n {0}", e.StackTrace);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.WriteLine("Port is not between the min and max port number! \n {0}", e.StackTrace);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        private static void StartThreadToReceiveIncomingMessage(Socket client)
        {
            var childSocketThread = new Thread(() =>
            {
                try
                {
                    byte[] data = new byte[100];
                    int buffersize = client.Receive(data);

                    string result = "";
                    for (int i = 0; i < buffersize; i++)
                        result += Convert.ToChar(data[i]);

                    MessageBox.Show(result, MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                catch (InvalidCastException e)
                {
                    Debug.WriteLine("Can't convert byte to char. {0}", e.StackTrace);
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("Error with code {0} occured within the network. {0}", e.ErrorCode, e.StackTrace);
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine("Object has already been disposed. {0}", e.StackTrace);
                }
                finally
                {
                    client.Close();
                } 
            });
            childSocketThread.Start();
        }

        private static IPAddress GetHostMachineIPAddress()
        {
            IPAddress[] foundIPs = Dns.GetHostAddresses(Dns.GetHostName());

            IPAddress ipAddress = null;
            foreach (var currentAddress in foundIPs)
            {
                if (currentAddress.AddressFamily == AddressFamily.InterNetwork)
                    ipAddress = currentAddress;
            }

            return ipAddress;
        }

        private static void CheckAndSetStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryLocation, true);

            if (key != null && IsSoftwareInstalled(key))
                key.SetValue(AppName, Application.ExecutablePath.ToString());
        }

        private static bool IsSoftwareInstalled(RegistryKey key)
        {
            return string.IsNullOrEmpty(key.GetValue(AppName) as string);
        }
    }
}