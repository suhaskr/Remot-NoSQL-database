/////////////////////////////////////////////////////////////////////
// WriteClient.cs - Commservice client that sends and receives     //
//                 messages(write operations) and measure latency  //
// Ver 2.1                                                         //
// Application: Demonstration for CSE681-SMA, Project#4            //
// Language:    C#, ver 6.0, Visual Studio 2015                    //
// Platform:    Dell Inspiron 15, Core-i5, Windows 10              //
// Author:      Suhas Kamasetty Ramesh                             //
//              MS Computer Engineering, Syracuse University       //
//              (315) 278-3888 skamaset@syr.edu                    //
// Source:      Jim Fawcett, CST 4-187, Syracuse University        //
//              (315) 443-3948, jfawcett@twcny.rr.com              //
/////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added using System.Threading
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - in this incantation the client has Sender and now has Receiver to
 *   retrieve Server echo-back messages.
 * - If you provide command line arguments they should be ordered as:
 *   remotePort, remoteAddress, localPort, localAddress
 */
/*
 *Package operations: To perform write operations on DB
 *                      and measure latency time.
 * Maintenance:
 * ------------
 * Required Files: ICommService.cs, Utilities.cs
 *                  Sener.cs, Receiver.cs, WriteClient.cs
 *
 * Build Process:  devenv Project4Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 */

/*
 * Maintenance History:
 * --------------------
 * ver 2.1 : 29 Oct 2015
 * - fixed bug in processCommandLine(...)
 * - added rcvr.shutdown() and sndr.shutDown() 
 * ver 2.0 : 20 Oct 2015
 * - replaced almost all functionality with a Sender instance
 * - added Receiver to retrieve Server echo messages.
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Project4Starter
{
    using Util = Utilities;

    ///////////////////////////////////////////////////////////////////////
    // Client class sends and receives messages in this version
    // - commandline format: /L http://localhost:8085/CommService 
    //                       /R http://localhost:8080/CommService
    //   Either one or both may be ommitted

    class Client
    {
        string localUrl { get; set; } = "http://localhost:8085/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";
        private Dictionary<string, List<double>> diction = new Dictionary<string, List<double>>();
        //----< retrieve urls from the CommandLine if there are any >--------

        //public bool log_messages = false;

        public Client()
        {
            //Console.WriteLine("This is inside the constructor.");
            diction["Insert"] = new List<double>();
            diction["Delete"] = new List<double>();
            diction["Add Children"] = new List<double>();
            diction["Edit Name"] = new List<double>();
            diction["Edit Description"] = new List<double>();
        }


        public Action define_receive_action(Receiver rcvr, Sender sndr, bool log_messages, string localport)
        {
            //Try to implement service action specific to client.
            Action client_receive_action = () =>
            {
                Message rcv_msg = null;
                //Message wpf_message = new Message();
                while (true)
                {
                    rcv_msg = rcvr.getMessage();
                    //Console.Write("\n  Received message: with contetn = {0} ",rcv_msg.content);
                    if (rcv_msg.content == "closeReceiver")
                        break;

                    if (rcv_msg.content == "connection start message")
                        continue;
                    if (rcv_msg.content == "done")
                    {
                        Console.WriteLine("\n  ********************************\n  All responses received from server. Going to display average results on WPF GUI.");
                        send_wpf_final(sndr, localport);
                        Console.WriteLine("  Enter any key on this window to close sender and receiver of this client.");
                        continue;
                    }
                    try
                    {
                        XElement response_msg = XElement.Parse(rcv_msg.content);
                        DateTime tm_start = DateTime.Parse(response_msg.Element("Request_Time_Stamp").Value);
                        DateTime tm_now = DateTime.Now;
                        TimeSpan lat_ts = tm_now - tm_start;
                        XElement query_response = response_msg.Element("Query_Response");
                        Console.WriteLine("\n  Received response for Request with ID = {0} of type - {1}", response_msg.Element("Request_ID").Value, query_response.Element("Query_Type").Value);
                        Console.WriteLine("  Latency time for the Request = {0} milli seconds", lat_ts.TotalMilliseconds);

                        if (log_messages)
                            Console.WriteLine("  Result of request:\n  {0}", query_response.Element("Result").Value);
                        send_wpf_latency(lat_ts.TotalMilliseconds, query_response.Element("Query_Type").Value, sndr, localport);
                        diction[query_response.Element("Query_Type").Value].Add(lat_ts.TotalMilliseconds);

                    }
                    catch
                    {
                        Console.WriteLine("\n  XML format of response message is not in the expected format. A particular tag could not be found.");
                    }
                }

            };
            return client_receive_action;
        }

        public void send_wpf_final(Sender sndr, string localport)
        {
            Message wpf_message = new Message();
            foreach (string k in diction.Keys)
            {
                wpf_message = new Message();
                //string msg2 = "Write Client(" + localport + "): Average latency time for " + k + " operations = " + diction[k].Average() + " milliseconds";
                if (diction[k].Count > 0)
                {
                    string msg2 = "Write Client(" + localport + "): Average latency time for " + diction[k].Count + " " + k + " operations = " + diction[k].Average() + " milliseconds";
                    wpf_message.fromUrl = localUrl;
                    wpf_message.toUrl = "http://localhost:8081/CommService";
                    wpf_message.content = msg2;
                    sndr.sendMessage(wpf_message);

                }
            }
            //foreach 
        }
        public void send_wpf_latency(double latency, string query_type, Sender sndr, string localport)
        {
            Message wpf_message = new Message();
            wpf_message.fromUrl = localUrl;
            wpf_message.toUrl = "http://localhost:8081/CommService";

            string msg = "Write Client(" + localport + ") Latency time for request of type " + query_type + " = " + latency + " milliseconds";

            wpf_message.content = msg;
            sndr.sendMessage(wpf_message);
            Thread.Sleep(150);
            //wpf_message = new Message();
        }
        public void send_messages(Sender sndr, Message msg, string file_name, bool log_messages)
        {
            XDocument xml_file = XDocument.Load(file_name);
            XElement root = xml_file.Element("Client");
            XElement write_client = root.Element("WriteClient");
            int count = Int32.Parse(write_client.Element("Count").Value);
            int number_req = 0;
            //DateTime tm;
            for (int i = 1; i <= count; i++)
            {
                foreach (var req_msg in write_client.Elements("Request_Message"))
                {
                    ++number_req;
                    Message new_msg = new Message();
                    new_msg.toUrl = msg.toUrl;
                    new_msg.fromUrl = msg.fromUrl;
                    if (log_messages)
                        Console.WriteLine("\n  Sending request with ID = {0}, of type {1}", number_req, req_msg.Element("Request_Type").Value);
                    if (i == 1)
                    {
                        XElement req_id = new XElement("Request_ID", number_req);
                        XElement time_stamp = new XElement("Request_Time_Stamp", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                        req_msg.Add(req_id);
                        req_msg.Add(time_stamp);
                    }
                    else
                    {
                        req_msg.Element("Request_ID").Value = number_req.ToString();
                        req_msg.Element("Request_Time_Stamp").Value = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
                    }


                    string str_msg = req_msg.ToString();
                    new_msg.content = str_msg;
                    if (!sndr.sendMessage(new_msg))
                        return;
                    Thread.Sleep(100);
                }
            }
        }

        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;
            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
        }

        public bool processCommandLineForWriteLog(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].ToUpper() == "-WRITER_SEND_MESSAGE_LOG")
                {
                    return true;
                }
            }
            return false;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("  The intent of the write client is to stress the database with write operations.");
            Console.WriteLine("  Write operations - Insert, Delete, Edit name, Edit Description and Add Children.");
            Console.WriteLine("\n  If log option is enabled, messsages are logged to this console window as they are sent.");
            Console.WriteLine("  If log is enabled result of response is displayed as responses are received from server.");
            Console.WriteLine("  If log option is disabled, only latency time for each operation is disaplyed on the console window.");
            Client clnt = new Client();
            clnt.processCommandLine(args);
            bool log_message = clnt.processCommandLineForWriteLog(args);
            if (log_message)
                Console.WriteLine("  (Log option is Enabled for this client)");
            else
                Console.WriteLine("  (Log option is Disabled for this client)");
            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            Receiver rcvr = new Receiver(localPort, localAddr);
            string win_title = "Write Client - " + localPort;
            Console.Title = win_title;
            Sender sndr = new Sender(clnt.localUrl);  // Sender needs localUrl for start message
            Action client_receive_action = clnt.define_receive_action(rcvr, sndr, log_message, localPort);
            if (rcvr.StartService())
                rcvr.doService(client_receive_action);
            Message msg = new Message();
            msg.fromUrl = clnt.localUrl;
            msg.toUrl = clnt.remoteUrl;
            Console.WriteLine("\n\n  Starting CommService  Write client");
            Console.Write("  =============================");
            Console.Write("\n  Write client local url is {0}", msg.fromUrl);
            Console.Write("\n  Attempting to connect to server on {0}\n", msg.toUrl);
            if (!sndr.Connect(msg.toUrl))
            {
                Console.Write("\n  Could not connect to server in {0} attempts, closing application.", sndr.MaxConnectAttempts);
                sndr.shutdown();
                rcvr.shutDown();
                return;
            }
            Console.WriteLine("\n  *****************************\n  Going to read messages present in XML file - Write_Client_XML.xml and send them sequentially to server.\n  *****************************");
            clnt.send_messages(sndr, msg, "./../../../../SavedXMLFiles/Write_Client_XML.xml", log_message);
            Thread.Sleep(100);
            msg.content = "done";
            sndr.sendMessage(msg);
            Util.waitForUser();
            // shut down this client's Receiver and Sender by sending close messages
            rcvr.shutDown();
            sndr.shutdown();
            Console.Write("\n\n");
        }
    }
}
