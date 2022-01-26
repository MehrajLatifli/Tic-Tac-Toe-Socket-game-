using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Windows.Threading;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static private int numOfLastChangedBtn = 0;
        static private int Turn = 1;

        static private bool firstUsr = false;
        static private bool secondUsr = false;

        string ip = string.Empty;
        string port = string.Empty;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(200.0);
            dispatcherTimer.Tick += DispatcherTimer_Tick;

            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {

            ip = IPAdressTextBox.Text;
            port = PortTextBox.Text;

        }

        private void answerWhoose(Socket listener)
        {
     
                    listener.Send(Encoding.UTF8.GetBytes(Turn.ToString() + " " + numOfLastChangedBtn.ToString()));

 
        }

        private void initUser(Socket listener)
        {
          
                    if (!firstUsr)
                    {
                        firstUsr = true;
                        listener.Send(Encoding.UTF8.GetBytes("2"));
                    }
                    else
                    {
                        secondUsr = true;
                        listener.Send(Encoding.UTF8.GetBytes("1"));
                    }

         
        }

        private static string GetMessage(Socket listener)
        {

            byte[] buffer = new byte[256];
            StringBuilder data = new StringBuilder();

            do
            {
                var size = listener.Receive(buffer);
                data.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (listener.Available > 0);
            return data.ToString();

        }

        private static void sendTurn(string msg)
        {

            String[] spearator = { " " };

            String[] parsedLine = msg.Split(spearator, 9,
                                  StringSplitOptions.RemoveEmptyEntries);
            numOfLastChangedBtn = int.Parse(parsedLine[2]);
            if (parsedLine[1].Equals("1")) Turn = 2;
            else Turn = 1;

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
                    {
                        try
                        {


                            var Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            Socket.Bind(new IPEndPoint(IPAddress.Parse(ip), int.Parse(port)));
                            Socket.Listen(2);



                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {

                                ConnectButton.Content = "Connected";
                                ConnectButton.IsEnabled = false;

                           
                            }));


                            while (true)
                            {
                                var listener = Socket.Accept();
                                String msg = GetMessage(listener);


                                if (msg.StartsWith("get_id")) initUser(listener);
                                else if (msg.StartsWith("send_turn")) sendTurn(msg);
                                else if (msg.StartsWith("whose_move")) answerWhoose(listener);

                                listener.Shutdown(SocketShutdown.Both);
                                listener.Close();
                            }
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show($"{ex.Message}");
                        }
                    }

                });

            }
            catch (Exception ex)
            {

                MessageBox.Show($"{ex.Message}");
            }

        }
    }
}
