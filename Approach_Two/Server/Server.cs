//////////////////////////////////////////////////////////////////////
// Server.cs - CommService server                                  //
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
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - This server now receives and then sends back received messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
 *
 *
 ** Maintenance:
 * ------------
 * Required Files: ICommService.cs, Utilities.cs
 *                  Sener.cs, Receiver.cs, DatabaseLibrary,
 *                  DBProcessor.cs, HighResTimer.cs
 *
 * Build Process:  devenv Project4Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 2.3 : 29 Oct 2015
 * - added handling of special messages: 
 *   "connection start message", "done", "closeServer"
 * ver 2.2 : 25 Oct 2015
 * - minor changes to display
 * ver 2.1 : 24 Oct 2015
 * - added Sender so Server can echo back messages it receives
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 2.0 : 20 Oct 2015
 * - Defined Receiver and used that to replace almost all of the
 *   original Server's functionality.
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace Project4Starter
{
    using Util = Utilities;

    class Server
    {
        string address { get; set; } = "localhost";
        string port { get; set; } = "8080";
        // public bool log_messages { get; set; } = false;
        //----< quick way to grab ports and addresses from commandline >-----
        private Dictionary<string, List<double>> diction = new Dictionary<string, List<double>>();

        public Server()
        {
            diction["Insert"] = new List<double>();
            diction["Delete"] = new List<double>();
            diction["Add Children"] = new List<double>();
            diction["Edit Name"] = new List<double>();
            diction["Edit Description"] = new List<double>();
            diction["Get Value"] = new List<double>();
            diction["Get Children"] = new List<double>();
            diction["Get Keys metadata"] = new List<double>();
            diction["Persist Database"] = new List<double>();
            diction["Load Database"] = new List<double>();
            diction["Show DB"] = new List<double>();
        }



        public Action define_server_receiveaction(Receiver rcvr, Sender sndr, bool log_messages)
        {
            HiResTimer hTimer = new HiResTimer();
            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            Action serviceAction = () =>
            {
                Message msg = new Message();
                DBProcessor db_process = new DBProcessor();
                while (true)
                {
                    msg = rcvr.getMessage();   // note use of non-service method to deQ messages
                    if (msg.content == "connection start message" || msg.content == "done" || msg.content == "closeServer") {
                        Console.Write("\n  Received message:");
                        Console.Write("\n  Sender is {0}", msg.fromUrl);
                        Console.Write("\n  content is {0}\n", msg.content); }
                    if(msg.content == "connection start message")
                        continue; // don't send back start message                   
                    if (msg.content == "done")
                    {
                        Console.Write("\n  client has finished\n");
                        Util.swapUrls(ref msg);
                        sndr.sendMessage(msg);
                        continue;
                    }
                    if (msg.content == "closeServer") {
                        Console.Write("received closeServer");
                        break; }
                    hTimer.Start();
                    XElement result = db_process.process_msg<int, DBElement<int, string>, string>(msg.content, db);
                    hTimer.Stop();
                    if (log_messages)
                        Console.WriteLine("  Time taken to process this request = {0} microseconds", hTimer.ElapsedMicroseconds);
                    double tm = (double)hTimer.ElapsedMicroseconds;
                    XElement request_mesg = XElement.Parse(msg.content);
                    XElement request_id = request_mesg.Element("Request_ID");
                    XElement request_time = request_mesg.Element("Request_Time_Stamp");
                    string method = request_mesg.Element("Request_Type").Value;
                    send_wpf_processing(hTimer.ElapsedMicroseconds, method, sndr, port);
                    diction[method].Add(tm);
                    if (diction[method].Count % 10 == 0)
                        send_wpf_average(method, sndr, port);
                    XElement response_server = new XElement("Response_Message");
                    response_server.Add(request_id);
                    response_server.Add(request_time);
                    response_server.Add(result);
                    msg.content = response_server.ToString();
                    Util.swapUrls(ref msg);
                    sndr.sendMessage(msg);
                }
            };
            return serviceAction;
        }

        public void send_wpf_processing(ulong proc_time, string query_type, Sender sndr, string localport)
        {
            Message wpf_message = new Message();
            wpf_message.fromUrl = "http://localhost:8080/CommService";
            wpf_message.toUrl = "http://localhost:8081/CommService";

            string msg = "Server(" + localport + "): Processing time for request of type " + query_type + " = " + proc_time + " microseconds";

            wpf_message.content = msg;
            sndr.sendMessage(wpf_message);
            Thread.Sleep(150);
            //wpf_message = new Message();
        }

        public void send_wpf_average(string query_type, Sender sndr, string localport)
        {
            Message wpf_message = new Message();
            wpf_message.fromUrl = "http://localhost:8080/CommService";
            wpf_message.toUrl = "http://localhost:8081/CommService";

            string msg = "Server(" + localport + "): Average processing time for " + diction[query_type].Count + " operations of type " + query_type + " = " + diction[query_type].Average() + " microseconds";

            wpf_message.content = msg;
            sndr.sendMessage(wpf_message);
            Thread.Sleep(150);
            //wpf_message = new Message();
        }

        public void ProcessCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
                port = args[0];
            }
            if (args.Length > 1)
            {
                address = args[1];
            }

        }
        static void Main(string[] args)
        {
            Util.verbose = false;
            Server srvr = new Server();
            srvr.ProcessCommandLine(args);

            bool log_messages = true;

            string srv_win = "Server - " + srvr.port;
            Console.Title = srv_win;
            Console.Write(String.Format("\n  Starting CommService server listening on port {0}", srvr.port));
            Console.Write("\n ====================================================\n");
            //Console.WriteLine("Going to create a DB and add a single element to it:");


            Sender sndr = new Sender(Util.makeUrl(srvr.address, srvr.port));
            //Sender sndr = new Sender();
            Receiver rcvr = new Receiver(srvr.port, srvr.address);

            // - serviceAction defines what the server does with received messages
            // - This serviceAction just announces incoming messages and echos them
            //   back to the sender.  
            // - Note that demonstrates sender routing works if you run more than
            //   one client.


            Action serviceAction = srvr.define_server_receiveaction(rcvr, sndr, log_messages);

            if (rcvr.StartService())
            {
                rcvr.doService(serviceAction); // This serviceAction is asynchronous,
            }                                // so the call doesn't block.

            Thread.Sleep(10000);
            Util.waitForUser();
        }
    }
}
