using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceiptManager;
using TestConsole;
using System.Net.Sockets;

namespace PicSend
{
    internal class PictureCommunication
    {
        const string _endOfMessage = ";;;";
        BroadCastClientSocket _broadCastClient;
        ServerSocket _serverSocket;
        CommunicationMode _currentCommunication;

        public PictureCommunication()
        {
            _broadCastClient = new BroadCastClientSocket(25555, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.IPv4, _endOfMessage);
            _serverSocket =  new ServerSocket(35555, SocketType.Stream, ProtocolType.Tcp, ServerSocket.ServerNotificationMode.OnCompleteMessage, _endOfMessage);
            

        }

        public void Start()
        {
            _broadCastClient.StartPollingMessage(_endOfMessage);
            _currentCommunication = CommunicationMode.BroadCast;
        }




        private enum CommunicationMode
        {
            None = 0,
            BroadCast = 1,
            Server = 2,
        }

        
    }
}
