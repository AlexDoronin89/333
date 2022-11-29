using ConnectionLibrary.Entity;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConnectionLibrary.Tools
{
    public class ConnectionTools
    {
        public static TcpListener GetListener()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Parse(ConstantData.ConnectionData.Ip),
                    ConstantData.ConnectionData.Port);
                listener.Start(2);

                return listener;
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR: " + exception.Message);
                return null;
            }
        }

        public static TcpClient Connect()
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(ConstantData.ConnectionData.Ip), ConstantData.ConnectionData.Port);

                return client;
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR: " + exception.Message);
                return null;
            }
        }

        public static Responce GetResponce(TcpClient client)
        {
            int count = 0;
            byte[] data = new byte[1024];
           
            while (count == 0)
            {
                BinaryReader stream = new BinaryReader(client.GetStream());
                count = stream.Read(data, 0, data.Length);
            }

            string a = Encoding.UTF8.GetString(data, 0, count);
            Responce responce = ConverterData.DeserializeResponce(a);
            return responce;
        }

        public static Request GetRequest(TcpClient client)
        {
            int count = 0;
            byte[] data = new byte[1024];

            while (count == 0)
            {
                NetworkStream stream = client.GetStream();
                count = stream.Read(data, 0, data.Length);
            }

            return ConverterData.DeserializeRequest(Encoding.UTF8.GetString(data, 0, count));
        }

        public static void SendRequest(TcpClient client, Request request)
        {
            Thread.Sleep(100);
            StreamWriter stream = new StreamWriter(client.GetStream());
            stream.Write(ConverterData.SerializeRequest(request));
            stream.Flush();
        }

        public static void SendResponce(TcpClient client, string value)
        {
            Responce responce = new Responce()
            {
                Result = string.IsNullOrWhiteSpace(value) ? ConstantData.ResponceResults.Error : ConstantData.ResponceResults.Ok,
                Value = value
            };
            
            Thread.Sleep(100);

            StreamWriter stream = new StreamWriter(client.GetStream());
            stream.Write(ConverterData.SerializeResponce(responce));
            stream.Flush();
        }
    }
}
