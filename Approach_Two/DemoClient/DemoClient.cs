//////////////////////////////////////////////////////////////////////
// DemoClient.cs - Commservice client that sends and receives      //
//                 messages(write,read,persist operations)         //
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
/*
 *Package operations: To perform read,write,and persist operations on 
 *                      Database by sending WCF messages.
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
        string localUrl { get; set; } = "http://localhost:8082/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        //----< retrieve urls from the CommandLine if there are any >--------

        public Action define_receive_action(Receiver rcvr, bool log_messages)
        {
            Action client_receive_action = () =>
            {
                Message rcv_msg = null;
                while (true) {
                    rcv_msg = rcvr.getMessage();
                    if (rcv_msg.content == "closeReceiver")
                        break;
                    if (rcv_msg.content == "connection start message")
                        continue;
                    if (rcv_msg.content == "done")  {
                        Console.WriteLine("\n  ********************************\n  All responses received from server.");
                        Console.WriteLine("  Enter any key on the TestExecutive window to launch all the readers and writers.");
                        Console.WriteLine("  Enter any key on this window to close sender and receiver of this Demo Client.");
                        continue;
                    }
                    try
                    {
                        XElement response_msg = XElement.Parse(rcv_msg.content);
                        Console.WriteLine("\n  Received response for Request with ID = {0}", response_msg.Element("Request_ID").Value);
                        DateTime tm_start = DateTime.Parse(response_msg.Element("Request_Time_Stamp").Value);
                        DateTime tm_now = DateTime.Now;
                        TimeSpan lat_ts = tm_now - tm_start;
                        Console.WriteLine("  Latency time for the Request = {0} milli seconds", lat_ts.TotalMilliseconds);
                        if (response_msg.Element("Query_Response") != null)
                        {
                            XElement query_response = response_msg.Element("Query_Response");
                            Console.WriteLine("  Query type = {0}", query_response.Element("Query_Type").Value);
                            if (query_response.Element("Result").Value != "Success")
                                Console.WriteLine("  Result of request:\n  {0}", query_response.Element("Result").Value);
                            if (query_response.Element("Partial") != null)
                                Console.WriteLine(query_response.Element("Partial").Value);
                            if (query_response.Element("Complete") != null)
                                Console.WriteLine(query_response.Element("Complete").Value);
                        }
                        else
                            Console.WriteLine("  Result of request:\n  {0}", response_msg.Element("Result").Value);
                    }
                    catch
                    {
                        Console.WriteLine("\n  XML format of response message is not in the expected format. A particular tag could not be found.");
                    }
                }
            };
            return client_receive_action;
        }


        public void send_messages(Sender sndr, Message msg, string file_name, bool log_messages)
        {
            Message new_message = new Message();
            XDocument xml_file = XDocument.Load(file_name);
            XElement root = xml_file.Element("Client");
            XElement read_client = root.Element("DemoClient");
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
                    //  if (log_messages)
                    //   Console.WriteLine("*************\nSending message from client = \n{0}\n***************", str_msg);
                    new_message.content = str_msg;
                    if (!sndr.sendMessage(new_message))
                        return;
                    Thread.Sleep(150);
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
        static void Main(string[] args)
        {
            Console.WriteLine("  The intent of the demo client is to demonstarte that all the operations of Project-2 can be executed remotely.");
            Console.WriteLine("  All the request messages stored in /SavedXMLFiles/Demo_Client_XML.xml are sent to server sequentially.");
            Console.WriteLine("  The Demo_Client_XML.xml file consists of Write operations, Read operations and Persist operations.");
            Console.WriteLine("  Write operations - Insert, Delete, Edit Name, Edit Description and Add Children");
            Console.WriteLine("  Read operations - Get Value, Get Children, Get Keys with string pattern in metadata");
            Console.WriteLine("  PErsist operations - Persist database, Load Database");
            Console.WriteLine("\n  The DB contents are displayed on the server only in case of demo client to show the successful operations on DB.");
            Console.WriteLine("  These are displayed to show to user that operations are successfully performed on the database.");
            Console.WriteLine("\n  ******************\n  To launch the readers and writers enter any key on the TestExecutive window.");
            Console.WriteLine("  You can verify all types of operations done and displayed on server window, before launching readers and writers.");
            Console.Write("\n  Starting CommService for demo client");
            Console.Write("\n ================================\n");
            Thread.Sleep(1000);
            bool log_message = true;
            Client clnt = new Client();
            clnt.processCommandLine(args);
            Receiver rcvr = new Receiver(Util.urlPort(clnt.localUrl), Util.urlAddress(clnt.localUrl));
            string win_title = "Demo Client - " + Util.urlPort(clnt.localUrl);
            Console.Title = win_title;
            Action client_receive_action = clnt.define_receive_action(rcvr, log_message);
            if (rcvr.StartService())
                rcvr.doService(client_receive_action);
            Sender sndr = new Sender(clnt.localUrl);  // Sender needs localUrl for start message
            Message msg = new Message();
            msg.fromUrl = clnt.localUrl;
            msg.toUrl = clnt.remoteUrl;
            Console.Write("\n  Demo client local url is {0}", msg.fromUrl);
            Console.Write("\n  Attempting to connect to server on {0}\n", msg.toUrl);
            if (!sndr.Connect(msg.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                sndr.shutdown();
                rcvr.shutDown();
                return;
            }
            clnt.send_messages(sndr, msg, "./../../../../SavedXMLFiles/Demo_Client_XML.xml", log_message);
            Thread.Sleep(100);
            msg.content = "done";
            sndr.sendMessage(msg);
            // Wait for user to press a key to quit. Ensures that client has gotten all server replies.
            Util.waitForUser();
            // shut down this client's Receiver and Sender by sending close messages
            rcvr.shutDown();
            sndr.shutdown();
            Console.Write("\n\n");
        }
    }
}

