/////////////////////////////////////////////////////////////////////////
// MainWindows.xaml.cs - CommService GUI Client                        //
// ver 2.0                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# WPF Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, MakeMessage, Utilities
 * - Added using Project4Starter
 *
 * Note:
 * - This client receives and sends messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.0 : 29 Oct 2015
 * - changed Xaml to achieve more fluid design
 *   by embedding controls in grid columns as well as rows
 * - added derived sender, overridding notification methods
 *   to put notifications in status textbox
 * - added use of MessageMaker in send_click
 * ver 1.0 : 25 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Project4Starter;

namespace WpfApplication1
{
  public partial class MainWindow : Window
  {
    static bool firstConnect = true;
    static Receiver rcvr = null;
    string localAddress = "localhost";
    string localPort = "8081";

    /////////////////////////////////////////////////////////////////////
    // nested class wpfSender used to override Sender message handling
    // - routes messages to status textbox
    public class wpfSender : Sender
    {
      TextBox lStat_ = null;  // reference to UIs local status textbox
      System.Windows.Threading.Dispatcher dispatcher_ = null;

      public wpfSender(TextBox lStat, System.Windows.Threading.Dispatcher dispatcher)
      {
        dispatcher_ = dispatcher;  // use to send results action to main UI thread
        lStat_ = lStat;
      }
      public override void sendMsgNotify(string msg)
      {
        Action act = () => { lStat_.Text = msg; };
        dispatcher_.Invoke(act);

      }
      public override void sendExceptionNotify(Exception ex, string msg = "")
      {
        Action act = () => { lStat_.Text = ex.Message; };
        dispatcher_.Invoke(act);
      }
      public override void sendAttemptNotify(int attemptNumber)
      {
        Action act = null;
        act = () => { lStat_.Text = String.Format("attempt to send #{0}", attemptNumber); };
        dispatcher_.Invoke(act);
      }
    }
    public MainWindow()
    {
      InitializeComponent();
      //lAddr.Text = localAddress;
      //lPort.Text = localPort;
      //rAddr.Text = remoteAddress;
      //rPort.Text = remotePort;
            string wpf_win = "WPF Client to display performance analysis - " + localPort;
      Title = wpf_win;
            setupChannel();
      //send.IsEnabled = false;
    }
    //----< trim off leading and trailing white space >------------------

    string trim(string msg)
    {
      StringBuilder sb = new StringBuilder(msg);
      for(int i=0; i<sb.Length; ++i)
        if (sb[i] == '\n')
          sb.Remove(i,1);
      return sb.ToString().Trim();
    }
    //----< indirectly used by child receive thread to post results >----

    public void postRcvMsg(string content)
    {
      TextBlock item = new TextBlock();
      item.Text = trim(content);
      item.FontSize = 16;
      //rcvmsgs.Items.Insert(0, item);
    }
    //----< used by main thread >----------------------------------------

        //-----<Post messages of avergae values of write operations
        public void postWriteAvgMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            write_avg_msgs.Items.Insert(0, item);
        }

        public void postWriteAllMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            write_all_msgs.Items.Insert(0, item);
        }
        public void postReadAllMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            read_all_msgs.Items.Insert(0, item);
        }
        public void postReadAvgMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            read_avg_msgs.Items.Insert(0, item);
        }
        public void postServerAllMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            server_all_msgs.Items.Insert(0, item);
        }
        public void postServerAvgMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            server_avg_msgs.Items.Insert(0, item);
        }

        public Action ret_act(string msg)
        {
            Action act = () => {
                int abc = 55;
                abc++;};
            if (msg.Contains("Write") && msg.Contains("Average"))
                act = () => { postWriteAvgMsg(msg); };
            else if (msg.Contains("Write") && msg.Contains("Latency"))
                act = () => { postWriteAllMsg(msg); };
            else if (msg.Contains("Server") && msg.Contains("Processing"))
                act = () => { postServerAllMsg(msg); };
            else if (msg.Contains("Server") && msg.Contains("Average"))
                act = () => { postServerAvgMsg(msg); };
            else if (msg.Contains("Read") && msg.Contains("Latency"))
                act = () => { postReadAllMsg(msg); };
            else if (msg.Contains("Read") && msg.Contains("Average"))
                act = () => { postReadAvgMsg(msg); };
            return act;
        }


    void setupChannel()
    {
      rcvr = new Receiver(localPort, localAddress);
      Action serviceAction = () =>
      {
          Message rmsg = null;
          while (true)
          {
                  rmsg = rcvr.getMessage();

                  /*
                  Action act = () => {
                      int abc = 55;
                      abc++;
                  };

                  if (rmsg.content.Contains("Write") && rmsg.content.Contains("Average"))
                      act = () => { postWriteAvgMsg(rmsg.content); };
                  else if (rmsg.content.Contains("Write") && rmsg.content.Contains("Latency"))
                      act = () => { postWriteAllMsg(rmsg.content); };
                  else if (rmsg.content.Contains("Server") && rmsg.content.Contains("Processing"))
                      act = () => { postServerAllMsg(rmsg.content); };
                  else if (rmsg.content.Contains("Server") && rmsg.content.Contains("Average"))
                      act = () => { postServerAvgMsg(rmsg.content); };
                  else if (rmsg.content.Contains("Read") && rmsg.content.Contains("Latency"))
                      act = () => { postReadAllMsg(rmsg.content); };
                  else if (rmsg.content.Contains("Read") && rmsg.content.Contains("Average"))
                      act = () => { postReadAvgMsg(rmsg.content); };
                  */
                  Action act = ret_act(rmsg.content);
                  Dispatcher.Invoke(act, System.Windows.Threading.DispatcherPriority.Background);

              }
      };
      if (rcvr.StartService())
      {
        rcvr.doService(serviceAction);
      }

      //sndr = new wpfSender(lStat, this.Dispatcher);
    }
    //----< set up channel after entering ports and addresses >----------

    private void start_Click(object sender, RoutedEventArgs e)
    {
            /*
      localPort = lPort.Text;
      localAddress = lAddr.Text;
      remoteAddress = rAddr.Text;
      remotePort = rPort.Text;
      */
      if (firstConnect)
      {
        firstConnect = false;
        if (rcvr != null)
          rcvr.shutDown();
        setupChannel();
      }
      /*
      rStat.Text = "connect setup";
      //send.IsEnabled = true;
      //connect.IsEnabled = false;
      lPort.IsEnabled = false;
      lAddr.IsEnabled = false;*/
    }
    //----< send a demonstraton message >--------------------------------
    
  }
}
