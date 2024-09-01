using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PicSend;
using System.Text.Json;
using System.Text.Unicode;

namespace ReceiptManager
{
    public class ServerSocket
    {
        Socket _socket;
        IPEndPoint _ipEndPoint;
        SocketType _socketType;
        ProtocolType _protocolType;
        volatile bool _shutdown;
        string _endOfMessage;
        Thread _commThread;
        StateHandler<ConnectionChangedEventArgs.ConnectionState> _stateHandler;
        JsonByteArrayConverter _jsonByteArrayConverter;
        JsonSerializerOptions _jsonOptions;

      

        public event EventHandler<ConnectionChangedEventArgs.ConnectionState> OnConnectionStateChanged;
        public event EventHandler<PictureData> OnPictureReceived;
       

        
        

        public ServerSocket(int port, SocketType SocketType, ProtocolType protocolType, string messageEnd)
        {
            _ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            _socketType = SocketType;
            _protocolType = protocolType;
            _socket = new Socket(_ipEndPoint.AddressFamily, _socketType, _protocolType);
            _socket.Blocking = true;
            _socket.Bind(_ipEndPoint);
            _shutdown = false;
            _endOfMessage = messageEnd;

            _stateHandler = new StateHandler<ConnectionChangedEventArgs.ConnectionState>();
            _stateHandler.Update(ConnectionChangedEventArgs.ConnectionState.Disconnected);

            _commThread = new Thread(new ThreadStart(Communication));

            _jsonByteArrayConverter = new JsonByteArrayConverter();
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(_jsonByteArrayConverter);
        }

        private void Communication()
        {
            List<byte[]> bytesReceived = new List<byte[]>();

            Socket? connection = null;


            while (!_shutdown)
            {

                if (_stateHandler.GetCurrentState().Equals(ConnectionChangedEventArgs.ConnectionState.Disconnected))
                {
                    connection = TryAcceptConnection();
                    _stateHandler.Update(ConnectionChangedEventArgs.ConnectionState.Connected);
                    OnConnectionStateChanged?.Invoke(this, ConnectionChangedEventArgs.ConnectionState.Connected);
                }
                
                
                if (!IsCurrentConnectionAlive(connection))
                {
                    _stateHandler.Update(ConnectionChangedEventArgs.ConnectionState.Disconnected);
                    OnConnectionStateChanged?.Invoke(this, ConnectionChangedEventArgs.ConnectionState.Disconnected);

                    continue;
                }

                if (IfBytesAvailable(connection))
                {
                    
                    byte[] data = GetAvailableData(connection); 

                    bytesReceived.Add(data);

                    if (CheckEndOfMessage(data))
                    {

                        connection.Send(Encoding.UTF8.GetBytes(_endOfMessage));

                        byte[] finalBytes = JoinByteArrays(bytesReceived);

                        finalBytes = RemoveEndOfMessageFromBytes(finalBytes);

                        string payload = GetStringFromBytes(finalBytes);

                        if (payload == string.Empty)
                        {
                            bytesReceived = new List<byte[]>();
                            continue;
                        }

                        PictureData? picData = null;

                        try
                        {
                            picData = JsonSerializer.Deserialize<PictureData>(payload, _jsonOptions);
                        }

                        catch(Exception e)
                        {
                            bytesReceived = new List<byte[]>();
                        }


                        bytesReceived = new List<byte[]>();

                        if (picData is null)
                        {
                            continue;
                        }

                        OnPictureReceived?.Invoke(this, picData);
                        
                    }
                }
                Thread.Sleep(100);
            }
        }

        private string GetStringFromBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

       
        public void StartListening()
        {
            _socket.Listen(100);
            _commThread.Start();
        }

        public void ShutDown()
        {
            _shutdown = true;
            _socket.Close();
        }

        private Socket? TryAcceptConnection()
        {
            try
            {
                Socket connection = _socket.Accept();
              

                return connection;
            }
            catch(SocketException e)
            {
                
                return null;
            }
        }

      

        
        


       

        

        private int GetAmountOfItemsInArray<T>(T[] array)
        {
            int amount = 0;
            foreach (T item in array)
            {
                if (item != null)
                {
                    amount++;
                }
            }
            return amount;
        }

        private int ChangeNumberToNotNegative(int number)
        {
            if (number < 0)
            {
                return 0;
            }
            return 0;
        }

        private bool IsCurrentConnectionAlive(Socket? connection)
        {
            if (connection is null)
            {

                return false;
            }
            if (connection.Poll(1000, SelectMode.SelectRead) & connection.Available == 0)
            {
                return false;
            }
            return true;
        }

        private bool IfBytesAvailable(Socket? connection)
        {
            if (connection is null)
            {
                return false;
            }
            return connection.Available > 0;
        }

      

        private byte[] GetAvailableData(Socket connection)
        {
            byte[] data = new byte[connection.Available];
            connection.Receive(data);
            return data;
        }


      

       



        private Image ConvertImageToJPEG(Image img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return Image.FromStream(stream);
            }
        }    

    private bool CheckEndOfMessage(byte[] data)
        {
            byte[] endOfMessage = Encoding.UTF8.GetBytes(_endOfMessage);
            int counter = 1;
            for(int i = data.Length -1; i > data.Length - endOfMessage.Length; i--)
            {
                if (endOfMessage[endOfMessage.Length - counter] != data[i])
                {
                    return false;
                }
                counter++;
            }
            
            return true;
        }

        private byte[] JoinByteArrays(List<byte[]> arrays)
        {
            byte[] finalBytes = new byte[arrays.Sum(a => a.Length)];
            int position = 0;
            foreach (byte[] array in arrays)
            {
                array.CopyTo(finalBytes, position);
                position += array.Length;
            }

            return finalBytes;
        }

        private byte[] RemoveEndOfMessageFromBytes(byte[] data)
        {
            byte[] endOfMessage = Encoding.UTF8.GetBytes(_endOfMessage);
            byte[] alteredData = data.Take(data.Length - endOfMessage.Length).ToArray();
            return alteredData;
        }

        private byte[] RemoveMessageTypeFromBytes(string type, byte[] data)
        {
            byte[] typeTag = Encoding.UTF8.GetBytes(type);
            byte[] alteredData = data.Skip(typeTag.Length).ToArray();
            return alteredData;
        }

      

     

       

     

        

        
       

        public class ConnectionChangedEventArgs : EventArgs
        {
            public ConnectionState State { get; private set; }

            public ConnectionChangedEventArgs(ConnectionState state)
            {
                State = state;
            }


            public enum ConnectionState
            {
                None = 0,
                Connected = 1,
                Disconnected = 2,
            }
        }

        public class IPReceivedEventArgs
        {
            private IPEndPoint _address;

            public IPEndPoint Address
            {
                get { return _address; }
            }

            public IPReceivedEventArgs(IPEndPoint address)
            {
                _address = address;
            }
        }
    }

    
}
