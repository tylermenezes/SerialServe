using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using System.Threading;
using System.IO;

namespace SerialServe
{
    class Program
    {
        static SerialBus bus = new SerialBus();
        static AssociativeArray<int, AssociativeArray<string, List<IHttpResponse>>> longpollConnections = new AssociativeArray<int, AssociativeArray<string, List<IHttpResponse>>>();

        static void Main(string[] args)
        {
            HttpListener listener = HttpListener.Create(System.Net.IPAddress.Loopback, 9981);
            listener.RequestReceived += new EventHandler<RequestEventArgs>(listener_RequestReceived);
            listener.Start(5);

            bus.OnIncomingSerialLine += new IncomingSerialLineHandler(bus_OnIncomingSerialLine);


            while (true)
            {
                Thread.Sleep(9000000);
            }
        }

        static void bus_OnIncomingSerialLine(int port, string line)
        {
            if (longpollConnections.ContainsKey(port))
            {
                foreach (List<IHttpResponse> responses in longpollConnections[port])
                {
                    while (responses.Count > 0)
                    {
                        try
                        {
                            StreamWriter writer = new StreamWriter(responses[0].Body);
                            writer.WriteLine("{'response':'" + line + "'}");
                            writer.Flush();
                            responses[0].Send();
                        }
                        catch
                        {
                            responses.RemoveAt(0);
                        }
                    }
                }
            }
        }


        static void listener_RequestReceived(object sender, RequestEventArgs e)
        {
            IHttpClientContext context = (IHttpClientContext)sender;
            IHttpRequest request = e.Request;
            IHttpResponse response = request.CreateResponse(context);
            StreamWriter writer = new StreamWriter(response.Body);

            response.AddHeader("Content-type", "text/plain");
            response.AddHeader("Access-Control-Allow-Methods", "*");
            response.AddHeader("Access-Control-Allow-Origin", "*");

            string endpoint = (request.UriParts.Length > 0? request.UriParts[0] : "");
            int to = 0;
            int.TryParse((request.UriParts.Length > 1 ? request.UriParts[1] : ""), out to);
            string id = "";
            AsyncSerial toSerial = null;

            if (to != 0)
            {
                if (request.QueryString.Contains("baud") || request.QueryString.Contains("parity") || request.QueryString.Contains("dataBits") ||
                            request.QueryString.Contains("stopBits") || request.QueryString.Contains("newLine"))
                {
                    // TODO
                    toSerial = bus.Connect(to);
                }
                else
                {
                    toSerial = bus.Connect(to);
                }
            }

            switch (endpoint)
            {
                case "":
                    writer.WriteLine("SerialServe is running! Check out the documentation on GitHub.");
                    writer.Flush();
                    response.Send();
                    break;
                case "list":
                    writer.WriteLine("[" + string.Join(",", bus.Ports) + "]");
                    writer.Flush();
                    response.Send();
                    break;
                case "write":
                    try
                    {
                        toSerial.Write(request.QueryString["toWrite"].ToString());
                        writer.WriteLine("{'success':true}");
                        writer.Flush();
                        response.Send();
                    }
                    catch
                    {
                        response.Status = System.Net.HttpStatusCode.BadRequest;
                        writer.WriteLine("{'error':'Could not write to the requested port.'}");
                        writer.Flush();
                        response.Send();
                    }
                    break;
                case "enable":
                case "disable":
                    try
                    {
                        if (endpoint == "enable")
                        {
                            toSerial.DtsEnable();
                        }
                        else
                        {
                            toSerial.DtsDisable();
                        }
                        writer.WriteLine("{'success':true}");
                        writer.Flush();
                        response.Send();
                    }
                    catch
                    {
                        response.Status = System.Net.HttpStatusCode.BadRequest;
                        writer.WriteLine("{'error':'Could not change the state of the requested port.'}");
                        writer.Flush();
                        response.Send();
                    }
                    break;
                case "read":
                    if (!longpollConnections.ContainsKey(to))
                    {
                        longpollConnections[to] = new AssociativeArray<string, List<IHttpResponse>>();
                    }

                    if (!longpollConnections[to].ContainsKey(id))
                    {
                        longpollConnections[to][id] = new List<IHttpResponse>();
                    }

                    longpollConnections[to][id].Add(response);
                    break;
                default:
                    response.Status = System.Net.HttpStatusCode.NotFound;
                    writer.WriteLine("Not found!");
                    writer.Flush();
                    response.Send();
                    break;
            }
        }
    }
}
