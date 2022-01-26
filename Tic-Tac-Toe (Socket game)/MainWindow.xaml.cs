using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Tic_Tac_Toe__Socket_game_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client client;
        private Button[] buttons=null;
        string[,] Array = new string[3,3];
        private int userID;
        private String symbol;
        FilterInfoCollection captureDevice;
        VideoCaptureDevice videoDevices;
        private static object syncRoot = new Object();

       public Semaphore semaphore = new Semaphore(1, 100, "Semaphore");

        List<string> cameralist = new List<string>();
        private object obj = new object();
        int bc = 0;
        public MainWindow()
        {
            InitializeComponent();

            getAllcameraList();

           
        }

        void getAllcameraList()
        {
            captureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo device in captureDevice)
            {
                cameralist.Add(device.Name);
            }

        

        }
        private void InitButtons()
        {
            Button_1.Content = string.Empty;
            Button_2.Content = string.Empty;
            Button_3.Content = string.Empty;
            Button_4.Content = string.Empty;
            Button_5.Content = string.Empty;
            Button_6.Content = string.Empty;
            Button_7.Content = string.Empty;
            Button_8.Content = string.Empty;
            Button_9.Content = string.Empty;



            buttons = new Button[] { this.Button_1, this.Button_2, this.Button_3, this.Button_4, this.Button_5, this.Button_6, this.Button_7, this.Button_8, this.Button_9 };

                    if (userID == 2)
                    {
                        foreach (Button btn in buttons)
                            btn.IsEnabled = false;
                    }
          
        }

        private void InitClient(string ip, int port)
        {
            this.client = new Client(ip, port);
            this.userID = client.getMyID();
            if (this.userID == 2)
            {
                this.symbol = "O";
            }
            else
            {
                this.symbol = "X";
            }
        }


        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {


            bc++;

                Task.Run(() =>
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        try
                        {
                            if (bc % 1 == 0)
                            {
                                ConnectButton.Content = "Web cam open";
                            }
                            if (bc % 2 == 0)
                            {
                                videoDevices = new VideoCaptureDevice(captureDevice[0].MonikerString);
                                videoDevices.NewFrame += new AForge.Video.NewFrameEventHandler(NewVideoFrame);

                                videoDevices.Start();

                                ConnectButton.Content = "capture image";
                            }
                            if (bc >= 3)
                            {
                                if (!string.IsNullOrEmpty(IPAdressTextBox.Text) && !string.IsNullOrEmpty(PortTextBox.Text))
                                {
                                    String ip = IPAdressTextBox.Text;
                                    int port = int.Parse(PortTextBox.Text);

                                    InitClient(ip, port);
                                    InitButtons();


                                    ConnectButton.Content = "conected";

                                    try
                                    {


                                        if (this.userID == 2)
                                        {
                                            MessageBox.Show($"Wait for the opponent to start");

                                            WaitTurn();
                                        ConnectButton.IsEnabled = false;
                                        }
                                        else
                                        {
                                            MessageBox.Show($"You go first");


                                            ConnectButton.IsEnabled = true;

                                        }


                                    }
                                    catch (Exception)
                                    {


                                    }




                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show($"{ex.Message}");
                        }


                    }));

                });
            

        }

        private void NewVideoFrame(Object sender, NewFrameEventArgs eventArgs)
        {


            System.Drawing.Image imgforms = (Bitmap)eventArgs.Frame.Clone();

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            MemoryStream ms = new MemoryStream();
            imgforms.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();

            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                yourImage.Source = image;

            }));


            int count = 0;


            if (bc % 3 == 0)
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate
            {

                Task.Delay(1000);

                imgforms.Save($"../../ yourImage.Source {Guid.NewGuid()}.jpeg", ImageFormat.Jpeg);
                  videoDevices.Stop();

            }));


            }



        }

        public BitmapImage Convert(Bitmap src)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;

        }

        private void WaitTurn()
        {

                    int numChangedBtn = client.waitTurn(userID);
                    if (symbol.Equals("O")) buttons[numChangedBtn - 1].Content = "X";
                    else buttons[numChangedBtn - 1].Content = "O";

                    string isOver = gameOver();
                    if (isOver == null)
                    {
                        int fillBtns = 0;
                        foreach (Button btn in buttons)
                        {
                            btn.IsEnabled = true;
                            if (btn.Content != "") fillBtns++;
                        }

                        if (fillBtns == 9) MessageBox.Show($"Draw");
                        //    else this.turnLabel.Text = "";

                    }
                    else
                    {
                        if (isOver == symbol) MessageBox.Show($"You won");
                        else MessageBox.Show($"You lost");
                        foreach (Button btn in buttons)
                            btn.IsEnabled = false;
                    }
           
        }

        private string gameOver()
        {
    


            // Horizontal
            if (Button_1.Content == Button_2.Content && Button_2.Content == Button_3.Content && Button_1.Content != "") return Button_1.Content.ToString();
            if (Button_4.Content == Button_5.Content && Button_4.Content == Button_6.Content && Button_4.Content != "") return Button_4.Content.ToString();
            if (Button_7.Content == Button_8.Content && Button_7.Content == Button_9.Content && Button_7.Content != "") return Button_7.Content.ToString();

            // Vertical
            if (Button_1.Content == Button_4.Content && Button_1.Content == Button_7.Content && Button_1.Content != "") return Button_1.Content.ToString();
            if (Button_2.Content == Button_5.Content && Button_2.Content == Button_8.Content && Button_2.Content != "") return Button_2.Content.ToString();
            if (Button_3.Content == Button_6.Content && Button_3.Content == Button_9.Content &&

            Button_3.Content != "") return Button_3.Content.ToString();

            // Diagonal
            if (Button_1.Content == Button_5.Content && Button_1.Content == Button_9.Content && Button_1.Content != "") return Button_1.Content.ToString();
            if (Button_3.Content == Button_5.Content && Button_3.Content == Button_7.Content && Button_3.Content != "") return Button_3.Content.ToString();

            // Not over
            return null;
        }



        private void SendTurn(string buttonNum)
        {

            client.sendTurn("send_turn" + " " + userID + " " + buttonNum);

            string isOver = gameOver();
            if (isOver == null)
            {
                //  this.turnLabel.Text = "";
                foreach (Button btn in buttons)
                    btn.IsEnabled = false;
                WaitTurn();
            }
            else
            {
                if (isOver == symbol) MessageBox.Show($"You won");
                else MessageBox.Show($"You lost");
                foreach (Button btn in buttons)
                    btn.IsEnabled = false;
            }


        }

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
 
               Button_1.Content = symbol;
               SendTurn("1");



        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {

    
                    Button_2.Content = symbol;
                    SendTurn("2");

        }

        private void Button_3_Click(object sender, RoutedEventArgs e)
        {

                    Button_3.Content = symbol;
                    SendTurn("3");


        }

        private void Button_4_Click(object sender, RoutedEventArgs e)
        {

                    Button_4.Content = symbol;
                    SendTurn("4");


        }
        private void Button_5_Click(object sender, RoutedEventArgs e)
        {

                    Button_5.Content = symbol;
                    SendTurn("5");


        }


        private void Button_6_Click(object sender, RoutedEventArgs e)
        {

                    Button_6.Content = symbol;
                    SendTurn("6");
  

        }

        private void Button_7_Click(object sender, RoutedEventArgs e)
        {

                    Button_7.Content = symbol;
                    SendTurn("7");


        }

        private void Button_8_Click(object sender, RoutedEventArgs e)
        {

                    Button_8.Content = symbol;
                    SendTurn("8");


        }

        private void Button_9_Click(object sender, RoutedEventArgs e)
        {

                    Button_9.Content = symbol;
                    SendTurn("9");
       

        }
    }
}
