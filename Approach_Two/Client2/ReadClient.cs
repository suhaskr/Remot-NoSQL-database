/////////////////////////////////////////////////////////////////////
// ReadClient.cs - Commservice client that sends and receives      //
//                 messages(read operations) and measure latency   //
// Ver 1.1                                                         //
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
 *Package operations: To perform read operations on DB
 *                      and measure latency time.
 * Maintenance:
 * ------------
 * Required Files: ICommService.cs, Utilities.cs
 *                  Sener.cs, Receiver.cs
 *
 * Build Process:  devenv Project4Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 *
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
        string localUrl { get; set; } = "http://localhost:8083/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        //----< retrieve urls from the CommandLine if there are any >--------
        private Dictionary<string, List<double>> diction = new Dictionary<string, List<double>>();

        public Client()
        {
            //Console.WriteLine("This is inside the constructor.");
            diction["Get Value"] = new List<double>();
            diction["Get Children"] = new List<double>();
            diction["Get Keys metadata"] = new List<double>();
        }

        public Action define_receive_action(Receiver rcvr, bool log_messages, Sender sndr, string localport)
        {
            //Try to implement service action specific to client.
            Action client_receive_action = () =>
            {
                Message rcv_msg = null;
                while (true)
                {
                    rcv_msg = rcvr.getMessage();
                    if (rcv_msg.content == "closeReceiver")
                        break;
                    if (rcv_msg.content == "connection start message")                   
                        continue;                    
                    if (rcv_msg.content == "done")
                    {
                        Console.WriteLine("\n  ***************************************\n  All responses received from server. Going to display average results on WPF GUI.");
                        send_wpf_final(sndr, localport);
                        Console.WriteLine("  Enter any key on this window to close sender and receiver of this client.");
                        continue;
                    }
                    try
                    {
                        XElement response_msg = XElement.Parse(rcv_msg.content);
                        XElement query_response = response_msg.Element("Query_Response");
                        Console.WriteLine("\n  Received response for Request with ID = {0} of type - {1}", response_msg.Element("Request_ID").Value,query_response.Element("Query_Type").Value);
                        DateTime tm_start = DateTime.Parse(response_msg.Element("Request_Time_Stamp").Value);
                        DateTime tm_now = DateTime.Now;
                        TimeSpan lat_ts = tm_now - tm_start;
                        Console.WriteLine("  Latency time for the Request = {0} milli seconds", lat_ts.TotalMilliseconds);
                        if (query_response.Element("Result").Value != "Success")
                        {
                            Console.WriteLine("  Result of request:\n  {0}", query_response.Element("Result").Value);
                        }
                        else
                        {
                            Console.WriteLine(query_response.Element("Partial").Value);
                            if (log_messages)
                                Console.WriteLine(query_response.Element("Complete").Value);
                        }
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
           // Console.WriteLine("Inside send loop");
            Message wpf_message = new Message();
            wpf_message.fromUrl = localUrl;
            wpf_message.toUrl = "http://localhost:8081/CommService";
            if (!sndr.Connect(wpf_message.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                return;
            }
            foreach (string k in diction.Keys)
            {
                wpf_message = new Message();
                //string msg2 = "Write Client(" + localport + "): Average latency time for " + k + " operations = " + diction[k].Average() + " milliseconds";
                if (diction[k].Count > 0)
                {
                    string msg2 = "Read Client(" + localport + "): Average latency time for " + diction[k].Count + " " + k + " operations = " + diction[k].Average() + " milliseconds";
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
            if (!sndr.Connect(wpf_message.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                return;
            }

                string msg = "Read Client(" + localport + ") Latency time for request of type " + query_type + " = " + latency + " milliseconds";

            wpf_message.content = msg;
            sndr.sendMessage(wpf_message);
            Thread.Sleep(100);
            //wpf_message = new Message();
        }


        public void send_messages(Sender sndr, Message msg, string file_name, bool log_messages)
        {
            Message new_message = new Message();
            XDocument xml_file = XDocument.Load(file_name);
            XElement root = xml_file.Element("Client");
            XElement read_client = root.Element("ReadClient");
            int count = Int32.Parse(read_client.Element("Count").Value);
            int number_req = 0;
            for (int i = 1; i <= count; i++)
            {
                foreach (var req_msg in read_client.Elements("Request_Message"))
                {
                    new_message = new Message();
                    new_message.toUrl = msg.toUrl;
                    new_message.fromUrl = msg.fromUrl;
                    Console.WriteLine("\n  Sending request with ID = {0}, of type {1}", ++number_req, req_msg.Element("Request_Type").Value);
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
                    
                    new_message.content = str_msg;
                    if (!sndr.sendMessage(new_message))
                        return;
                    Thread.Sleep(100);
                }
            }

        }

        public bool processCommandLineForPartialLog(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].ToUpper() == "-READER_PARTIAL_DISPLAY")
                {
                    return true;
                }
            }
            return false;
        }

        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;
            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
        }
        static void Main(string[] args)
        {
            Console.WriteLine("  The intent of the read client is to stress the database with read operations.\n  Read operations - Get DBElement with key, Get children of DBElement, Get keys of DBElements with particular pattern.");
            Console.WriteLine("\n  If partial log option is enabled, only a partial part of the response messsage is logged to this console window.(latency and part of result)\n  If Partial log option is not enabled, entire result is displayed on the GUI.");
            Console.WriteLine("  For all requests, latency time for each message is displayed on this window and WPF GUI.");
            Console.WriteLine("  Once all request messages are received, average latency times are updated in WPF GUI.");          
            Client clnt = new Client();
            clnt.processCommandLine(args);
            bool log_message = true;
            if (clnt.processCommandLineForPartialLog(args))
                log_message = false;
            if (!log_message)
                Console.WriteLine("  (Partial log option is enabled for this client)");
            else
                Console.WriteLine(" (Partial log option is not enabled for this client.)");

            Console.Write("\n  Starting CommService Read client");
            Console.Write("\n  =============================\n");
            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            Receiver rcvr = new Receiver(localPort, localAddr);
            string win_title = "Read Client - " + localPort;
            Console.Title = win_title;

            Sender sndr = new Sender(clnt.localUrl);  // Sender needs localUrl for start message
            Action client_receive_action = clnt.define_receive_action(rcvr, log_message,sndr,localPort);
            if (rcvr.StartService())
                rcvr.doService(client_receive_action);
            Message msg = new Message();
            msg.fromUrl = clnt.localUrl;
            msg.toUrl = clnt.remoteUrl;
            Console.Write("\n  sender's url is {0}", msg.fromUrl);
            Console.Write("\n  attempting to connect to {0}\n", msg.toUrl);
            if (!sndr.Connect(msg.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                sndr.shutdown();
                rcvr.shutDown();
                return;            }
            Console.WriteLine("\n  *****************************\n  Going to read messages present in XML file - Read_Client_XML.xml and send them sequentially to server.\n  *****************************");
            clnt.send_messages(sndr, msg, "./../../../../SavedXMLFiles/Read_Client_XML.xml", log_message);
            Thread.Sleep(100);
            msg.content = "done";
            sndr.sendMessage(msg); 
            Util.waitForUser();
            rcvr.shutDown();
            sndr.shutdown();
            Console.Write("\n\n");
        }
    }
}

