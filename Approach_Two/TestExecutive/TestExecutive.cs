///////////////////////////////////////////////////////////////
// DBProcessor.cs - Methods for processing request messages  //
//                   and performing action on DB.            //
// Ver 1.0                                                   //
// Application: Demonstration for CSE681-SMA, Project#4      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 15, Core-i5, Windows 10        //
// Author:      Suhas Kamasetty Ramesh                       //
//              MS Computer Engineering, Syracuse University //
//              (315) 278-3888 skamaset@syr.edu              //
///////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
 * This package begins the demonstration of meeting requirements.
 */

/*
* Maintenance:
 * ------------
 * Required Files: TestExecutive.cs
 *   
 *
 * Build Process:  devenv CommPrototype.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *

 * Maintenance History:
 * ver 1.0 : 22 Nov 2015
 * -first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace TestExecutive
{
    class TestExecutive
    {
        private static int port_count = 3;

        public void launch_WPF()
        {
            ProcessStartInfo wpf_client = new ProcessStartInfo(Path.GetFullPath("..\\..\\..\\WpfClient\\bin\\Debug\\WpfApplication1.exe"));
            Process.Start(wpf_client);
        }

        public void launch_Server()
        {
            ProcessStartInfo server = new ProcessStartInfo(Path.GetFullPath("..\\..\\..\\Server\\bin\\Debug\\Server.exe"));
            Process.Start(server);
        }

        public void launch_DemoClient()
        {
            ProcessStartInfo demo_client = new ProcessStartInfo(Path.GetFullPath("..\\..\\..\\DemoClient\\bin\\Debug\\DemoClient.exe"));
            Process.Start(demo_client);
        }
        
        public void launch_Readers(string[] args)
        {
            int read_count = number_of_readers(args);
            string par_disp = "";
            if (processCommandLineForPartialLog(args))
                par_disp = " -Reader_Partial_Display";
            for (int i = 1; i <= read_count; i++)
            {
                ProcessStartInfo reader = new ProcessStartInfo(Path.GetFullPath("..\\..\\..\\Client2\\bin\\Debug\\Client2.exe"));
                reader.Arguments = "/L http://localhost:" + (8080 + port_count).ToString() + "/CommService" + par_disp;
                Process.Start(reader);

                ++port_count;
            }
        }

        public void launch_Writers(string[] args)
        {
            int write_count = number_of_writers(args);
            string send_log = "";
            if (processCommandLineForWriteLog(args))
                send_log = " -Writer_Send_Message_Log";
            
            for(int i=1;i<=write_count;i++)
            {
                ProcessStartInfo writer = new ProcessStartInfo(Path.GetFullPath("..\\..\\..\\Client\\bin\\Debug\\Client.exe"));
                writer.Arguments = "/L http://localhost:" + (8080 + port_count).ToString() + "/CommService" + send_log;
                Process.Start(writer);
                ++port_count;
            }
        }

        

        public int number_of_readers(string[] args)
        {
            int readers = 1;
            for (int i = 0; i < args.Length; ++i)
            {
                //Console.WriteLine("args = {0}", args[i]);
                //Console.WriteLine("args[{0}] = {1}", i, args[i]);
                if ((args.Length > i + 1) && args[i].ToUpper() == "-READER_COUNT")
                {
                    readers = Int32.Parse(args[i + 1]);
                }
            }
            return readers;
        }

        public int number_of_writers(string[] args)
        {
            int readers = 1;
            for (int i = 0; i < args.Length; ++i)
            {
                Console.WriteLine("args = {0}", args[i]);
                //Console.WriteLine("args[{0}] = {1}", i, args[i]);
                if ((args.Length > i + 1) && args[i].ToUpper() == "-WRITER_COUNT")
                {
                    readers = Int32.Parse(args[i + 1]);
                }
            }
            return readers;
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
            Console.Title = "Test Executive";
            Console.WriteLine("\n  Format of command line arguments are mentioned in the Readme.txt file in the root directory.");
            Console.WriteLine("  ***************** REQUIREMENT #2 ************************\n");
            Console.WriteLine("  My solution developed for project#2 is added to project #4 as a Class library(DatabaseLib).");
            Console.WriteLine("\n  ***************** REQUIREMENT #3 ************************\n");
            Console.WriteLine("  The clients and the server communicate using WCF");
            Console.WriteLine("\n  ***************** REQUIREMENT #4 ************************\n");
            Console.WriteLine("  The demo client sends requests for write,read and persist operations.\n  Database contents are displayed on server console window only in case of Demo client to show to user the successful operations performed on DB.");

            TestExecutive tst = new TestExecutive();
            tst.launch_WPF();
            Thread.Sleep(500);
            tst.launch_Server();
            Thread.Sleep(500);
            tst.launch_DemoClient();

            Console.WriteLine("  Presently only Demo client and server are launched to demonstarte that all the operations supported in project2 can be done remotely.");
            Console.WriteLine("  Enter any key to launch all the writers and readers.");
            Console.ReadKey();
            tst.launch_Writers(args);
            tst.launch_Readers(args);
            Console.WriteLine("\n  ***************** REQUIREMENT # 5 ************************\n");
            Console.WriteLine("  Write clients read from Write_Client_XML.xml file and is provided in the SavedXMLFiles folder of the root directory.");
            Console.WriteLine(" The number of messages is mentioned in the XML file with the \"Count\" tag. All request messages are sent to the server in XML format,which is processed by the server.");
            Console.WriteLine("  Latency time for each request message is displayed on the console window.");
            Console.WriteLine("\n  ***************** REQUIREMENT #6 ************************\n");
            Console.WriteLine("  A log option is provided, which when enabled displays messages on console as they are sent to server. Also response result is displayed if log option is enabled.");
            Console.WriteLine("  If log option is not enabled only latency time for each message is displayed on the console window and WPF GUI.");
            Console.WriteLine("  Once all response messages are received fom server, the write client will display the average latency time for each type of request on the WPF GUI.");
            Console.WriteLine("\n  ***************** REQUIREMENT #7 ************************");
            Console.Write("\n  The read client sends all requests mentioned in XML file which is read at startup.");
            Console.WriteLine("\n\n  ***************** REQUIREMENT #8 ************************");
            Console.WriteLine("  A partial log option is provided. If it is enabled a partial part of response message is displayed on the console window.");
            Console.WriteLine("  In case of DBElement-payload is displayed, in case of list of keys, the number of elements in list is displayed.");
            Console.WriteLine("  If partial log option is disabled, entire result is displayed on the console window.");
            Console.WriteLine("  Based on the count present in the XML file, once all response for requests are received, the average latency time for each request is displayed on the WPF GUI.");
            Console.ReadKey();
        }
    }
}
