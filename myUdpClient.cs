
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace myCTI2
{

    public class myUdpClient
    {
        public delegate void UDPEventHandler(object sender, myEventArgs e);
        public event UDPEventHandler UDPreceive;

        private UdpClient receivingUdpClient;
        private IPEndPoint RemoteIpEndPoint;
        private bool server_on = false;

        // nur Pakete von localhost und vom myCTI Server annehmen
        private volatile string serverHost = "";
        private volatile IPAddress[] serverIPs = new IPAddress[0];
        private DateTime lastResolve = DateTime.MinValue;

        public myUdpClient()
        {
        }

        public void start(int port)
        {
            receivingUdpClient = new UdpClient(port);
            RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);
            server_on = true;
            System.Threading.Thread x1 = new System.Threading.Thread(start1);
            x1.IsBackground = true;
            x1.Start();
        }
        public void stopsvr()
        {
            server_on = false;
            receivingUdpClient.Close();
        }

        public void setServer(string server)
        {
            string host;
            int port;
            TOOL.SplitServer(server, 0, out host, out port);
            serverHost = host;
            resolveServer();
        }

        private void resolveServer()
        {
            lastResolve = DateTime.Now;
            try
            {
                serverIPs = Dns.GetHostAddresses(serverHost);
            }
            catch (Exception e)
            {
                serverIPs = new IPAddress[0];
                TOOL.errorx("UDP resolveServer " + serverHost + " error: " + e.Message);
            }
        }

        private bool isAllowed(IPAddress ip)
        {
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();
            if (IPAddress.IsLoopback(ip))
                return true;
            if (serverIPs.Contains(ip))
                return true;
            // DNS kann sich geändert haben, höchstens einmal pro Minute neu auflösen
            if ((DateTime.Now - lastResolve).TotalSeconds >= 60)
            {
                resolveServer();
                return serverIPs.Contains(ip);
            }
            return false;
        }

        private void start1()
        {
            do
            {
                try
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    if (server_on == false)
                        break;
                    if (!isAllowed(RemoteIpEndPoint.Address))
                    {
                        TOOL.errorx("UDP Paket von nicht erlaubtem Absender " + RemoteIpEndPoint.Address.ToString() + " verworfen (Server=" + serverHost + ")");
                        continue;
                    }
                    string returnData = Encoding.Default.GetString(receiveBytes);
                    RaiseSampleEvent(returnData);
                }
                catch (Exception e)
                {
                    if (server_on == false)
                        break;
                    Debug.Print("UDP_CLIENT sstart1 " + e.ToString());
                }
            }
            while (true);
        }
        protected virtual void RaiseSampleEvent(string returnData)
        {
            UDPreceive?.Invoke(this, new myEventArgs(returnData));
        }

    }
}
