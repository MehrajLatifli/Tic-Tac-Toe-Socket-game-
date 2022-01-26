using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tic_Tac_Toe__Socket_game_
{
    public class Client
    {
        private byte[] buffer = new byte[256];
        private StringBuilder answer = new StringBuilder();
        public static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        String host; int port;

        public Client(String host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public int getMyID()
        {
            try
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(host), port));
                socket.Send(Encoding.UTF8.GetBytes("get_id"));

                do
                {
                    var size = socket.Receive(buffer);
                    answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (socket.Available > 0);
            }
            catch (Exception)
            {


            }

            string id = answer.ToString();
            answer.Clear();


            return int.Parse(id);
        }

        public void sendTurn(String msg)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(host), port));
            socket.Send(Encoding.UTF8.GetBytes(msg));
        }

        public int waitTurn(int id)
        {
            while (true)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(host), port));
                socket.Send(Encoding.UTF8.GetBytes("whose_move"));
                do
                {
                    var size = socket.Receive(buffer);
                    answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (socket.Available > 0);

                string[] parsedLine = answer.ToString().Split(' ');
                answer.Clear();
                if (int.Parse(parsedLine[0]) == id) return int.Parse(parsedLine[1]);
                else Thread.Sleep(500);
            }
        }
    }
}
