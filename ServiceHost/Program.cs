using System;
using System.ServiceModel;
using NSpeexTestServer;

namespace ServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var serviceHost = new System.ServiceModel.ServiceHost(new SpeexStreamer()))
            {
                serviceHost.AddServiceEndpoint(typeof(ISpeexStreamer), new NetTcpBinding(), "net.tcp://localhost:8001/SpeexStreamer");
                serviceHost.Open();
                Console.WriteLine("Service open...");
                Console.ReadLine();
            }
        }
    }
}
