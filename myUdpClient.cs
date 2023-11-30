
using System;
using System.Diagnostics;
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

        public myUdpClient()
        {
        }

        public void start(int port)
        {
            receivingUdpClient = new UdpClient(port);
            RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);
            server_on = true;
            System.Threading.Thread x1 = new System.Threading.Thread(start1);
            x1.Start();
        }
        public void stopsvr()
        {
            server_on = false;
            receivingUdpClient.Close();
        }

        private void start1()
        {
            do
            {
                try
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.Default.GetString(receiveBytes);
                    if (server_on == false)
                        break;
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