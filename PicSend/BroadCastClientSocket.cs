using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace TestConsole
{
    public class BroadCastClientSocket
    {
        Socket _socket;
        readonly IPAddress _ipAddress = IPAddress.Parse("192.168.1.255");
        IPEndPoint _endPoint;
        SocketType _socketType;
        ProtocolType _protocolType;
        private volatile bool _isPolling;
        string _messageEnd;
        Task _taskForPoll;

       

        public bool IsPolling
        {
            get { return _isPolling; }
        }
        public BroadCastClientSocket(int port,  SocketType socketType, ProtocolType protocolType, string messageEnd)
        {
            
            _endPoint = new IPEndPoint(_ipAddress, port);
            _socketType = socketType;
            _protocolType = protocolType;
            _socket = new Socket(_endPoint.AddressFamily, _socketType, _protocolType);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            _socket.Connect(_endPoint);
            _isPolling = false;
            _messageEnd = messageEnd;
        }

        private bool SendMessage(string message)
        {
            if (!_socket.Connected)
            {
                _socket.Connect(_endPoint);
                if (!_socket.Connected)
                {
                    return false;
                }
                
            }
            byte[] messageInBytes = Encoding.UTF8.GetBytes(message + _messageEnd);
            _socket.Send(messageInBytes);
            
            return true;
        }

        public void StartPollingMessage(string message)
        {
            

            SetPollingON();
            _taskForPoll = new Task(() =>
            {
                while (_isPolling)
                {
                    SendMessage(message);
                    Thread.Sleep(500);
                }
            });
            _taskForPoll.Start();
        }

        private void StopPolling()
        {
            if (_isPolling)
            {
                SetPollingOFF();
                while (!_taskForPoll.IsCompleted)
                {
                    Thread.Sleep(100);
                }
            }
        }

        

        

        public void ShutDown()
        {
            StopPolling();
            _socket.Close();
        }


        


        private void SetPollingON()
        {
            _isPolling = true;
        }

        private void SetPollingOFF()
        {
            _isPolling = false;
        }

        


       
    }
}
