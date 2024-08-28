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

namespace ReceiptManager
{
    public class ServerSocket
    {
        Socket _socket;
        IPEndPoint _ipEndPoint;
        SocketType _socketType;
        ProtocolType _protocolType;
        volatile bool _shutdown;
        List<Action<byte[]>>? _byteListeners;
        List<Action<Image, byte?>>? _listenersForImage;
        List<Action<string>>? _stringListeners;
        EventHandler<ConnectionChangedEventArgs>? _connectionHandler;
        EventHandler<IPReceivedEventArgs>? _ipReceivedHandler;
        ServerNotificationMode _mode;
        string _messageEnd;
        Thread _thread;
        byte[] _orientationTag;
        byte? _orientation;
        bool _connectedToApp;
        MemoryStream _streamForImage;

        private readonly static string IMGID = "IMG_";
        private readonly static string IPID = "IP_";

        Dictionary<byte[], string> _messageTypes;
        

        public ServerSocket(int port, SocketType SocketType, ProtocolType protocolType, ServerNotificationMode mode, string messageEnd)
        {
            _ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            _socketType = SocketType;
            _protocolType = protocolType;
            _socket = new Socket(_ipEndPoint.AddressFamily, _socketType, _protocolType);
            _socket.Blocking = true;
            _socket.Bind(_ipEndPoint);
            _shutdown = false;
            _byteListeners = new List<Action<byte[]>>();
            _listenersForImage = new List<Action<Image, byte?>>();
            _mode = mode;
            _messageEnd = messageEnd;
            _thread = new Thread(new ThreadStart(StartThread));
            _orientationTag = Encoding.UTF8.GetBytes("WO_");
            _orientation = null;
            _stringListeners = new List<Action<string>>();
            _connectedToApp = false;

            _messageTypes = new Dictionary<byte[], string> { { Encoding.UTF8.GetBytes(IMGID), IMGID},
                {Encoding.UTF8.GetBytes(IPID), IPID },
            };
        }

        private void StartThread()
        {
            List<byte[]> bytesReceived = new List<byte[]>();

            Socket? connection = TryAcceptConnection();

            string? messageType = null;

            while (!_shutdown)
            {
                if (!IsCurrentConnectionAlive(connection))
                {
                    if (_connectedToApp)
                    {
                        InformOnAppConnectionChange(ConnectionChangedEventArgs.ConnectionState.Disconnected);
                    }
                    
                    connection = TryAcceptConnection();
                    InformOnAppConnectionChange(ConnectionChangedEventArgs.ConnectionState.Connected);
                }

                if (IfBytesAvailable(connection))
                {
                    
                    byte[] data = GetAvailableData(connection); 

                    if (messageType is null)
                    {
                        messageType = CheckMessageType(data);
                    }

                    bytesReceived.Add(data);

                    if (CheckEndOfMessage(data))
                    {

                        connection.Send(Encoding.UTF8.GetBytes(_messageEnd));

                        byte[] finalBytes = JoinByteArrays(bytesReceived);

                        finalBytes = RemoveEndOfMessageFromBytes(finalBytes);

                        if (messageType == IMGID)
                        {
                            finalBytes = RemoveMessageTypeFromBytes(IMGID, finalBytes);
                            if (ContainsImageOrientationData(finalBytes))
                            {
                                _orientation = ExtractImageOrientationByte(finalBytes);
                                finalBytes =  RemoveImageOrientationData(finalBytes);

                            }

                            InformImageListeners(finalBytes, _orientation);
                        }
                        if (messageType == IPID)
                        {
                            finalBytes = RemoveMessageTypeFromBytes(IPID, finalBytes);
                            InformIPReceivedListeners(connection.RemoteEndPoint as IPEndPoint);
                        }
                       
                        InformByteListeners(finalBytes);

                        InformStringListeners(Encoding.UTF8.GetString(finalBytes));
                            
                            
                            
                        bytesReceived.Clear();
                        messageType = null;
                        
                    }
                }
                Thread.Sleep(100);
            }
        }

        private string CheckMessageType(byte[] data)
        {
            foreach (byte[] id in _messageTypes.Keys)
            {
                byte[] messageId = data.Take(new Range(0, id.Length)).ToArray();
                if (messageId.SequenceEqual(id))
                {
                    return _messageTypes[id];
                }
            }

            return string.Empty;
        }
        public void StartListening()
        {
            _socket.Listen(100);
            _thread.Start();
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
        


        public void RegisterListener(Action<byte[]> listener)
        {
            _byteListeners.Add(listener);
        }

        public void RegisterListener(Action<string> listener)
        {
            _stringListeners.Add(listener);
        }

        public void RegisterOnConnectionStateChangeListener(EventHandler<ConnectionChangedEventArgs> listener)
        {
            _connectionHandler += listener;
        }

        public void RegisterImageListener(Action<Image, byte?> listener)
        {
            _listenersForImage.Add(listener);
        }

        public void RegisterOnIPAddressReceivedListener(EventHandler<IPReceivedEventArgs> listener)
        {
            _ipReceivedHandler += listener;
        }

        public enum ServerNotificationMode
        {
            None = 0,
            OnIncompleteReceive = 1,
            OnCompleteMessage = 2,
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

        private bool IsImmediateNotification()
        {
            return _mode == ServerNotificationMode.OnIncompleteReceive;
        }

        private byte[] GetAvailableData(Socket connection)
        {
            byte[] data = new byte[connection.Available];
            connection.Receive(data);
            return data;
        }


        private void InformByteListeners(byte[] data)
        {
            if (_byteListeners is null)
            {
                return;
            }
            foreach (Action<byte[]> action in _byteListeners)
            {
                byte[] copy = new byte[data.Length];
                data.CopyTo(copy, 0);
                action.Invoke(copy);


            }
        }

        private void InformStringListeners(string data)
        {
            if (_stringListeners is null)
            {
                return;
            }
            foreach (Action<string> action in _stringListeners)
            {
               
                action.Invoke(data);
            }
        }

        private void InformImageListeners(byte[] data, byte? orientation)
        {
            if (_listenersForImage is null)
            {
                return;
            }

            Image img = CreateImage(data);
           
            foreach (Action<Image, byte?> action in _listenersForImage)
            {
                action.Invoke(img, orientation);
            }
            _orientation = null;
        }

        
        private Image CreateImage(byte[] bytes)
        {
                _streamForImage = new MemoryStream(bytes);
                _streamForImage.Seek(0, SeekOrigin.Begin);
                return Bitmap.FromStream(_streamForImage);
            
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
            byte[] endOfMessage = Encoding.UTF8.GetBytes(_messageEnd);
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
            byte[] endOfMessage = Encoding.UTF8.GetBytes(_messageEnd);
            byte[] alteredData = data.Take(data.Length - endOfMessage.Length).ToArray();
            return alteredData;
        }

        private byte[] RemoveMessageTypeFromBytes(string type, byte[] data)
        {
            byte[] typeTag = Encoding.UTF8.GetBytes(type);
            byte[] alteredData = data.Skip(typeTag.Length).ToArray();
            return alteredData;
        }

        private bool ContainsImageOrientationData(byte[] data)
        {
            
            if (data.Length < _orientationTag.Length)
            {
                return false;
            }
            byte[] dataTag = data.Take(_orientationTag.Length).ToArray();
            for (int i = 0; i < _orientationTag.Length; i++)
            {
                if (dataTag[i] != _orientationTag[i])
                {
                    return false;
                }
            }

            return true;
        }

        private byte ExtractImageOrientationByte(byte[] data)
        {
            byte[] withTag = data.Take(_orientationTag.Length + 1).ToArray();
            return withTag[withTag.Length - 1];
        }

        private byte[] RemoveImageOrientationData(byte[] data)
        {
            return data.Skip(_orientationTag.Length + 1).ToArray();
        }

        private void InformOnAppConnectionChange(ConnectionChangedEventArgs.ConnectionState state)
        {


            _connectedToApp = state.Equals(ConnectionChangedEventArgs.ConnectionState.Connected);
            
            _connectionHandler?.Invoke(this,
                                new ConnectionChangedEventArgs(state));
        }

        private void InformIPReceivedListeners(IPEndPoint addresss)
        {
            _ipReceivedHandler?.Invoke(this, new IPReceivedEventArgs(addresss));
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
