using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceiptManager;
using TestConsole;
using System.Net.Sockets;
using System.Drawing;
using System.IO;

namespace PicSend
{
    internal class PictureCommunication : IShutdown
    {
        const string _endOfMessage = ";;;";
        BroadCastClientSocket _broadCastClient;
        ServerSocket _serverSocket;

        SettingsUC.AppSettings _appSettings;

        Dictionary<Orientation, RotateFlipType> _rotationActions = new Dictionary<Orientation, RotateFlipType> { 
            { Orientation.FlipX, RotateFlipType.RotateNoneFlipX },
                { Orientation.FlipXY, RotateFlipType.RotateNoneFlipXY },
                { Orientation.FlipY, RotateFlipType.RotateNoneFlipY },
                { Orientation.Rotate90FlipX, RotateFlipType.Rotate90FlipX },
                { Orientation.Rotate90, RotateFlipType.Rotate90FlipNone },
                { Orientation.Rotate90FlipY, RotateFlipType.Rotate90FlipY },
                { Orientation.Rotate90FlipXY, RotateFlipType.Rotate90FlipXY },
            };

        public event EventHandler<ServerSocket.ConnectionChangedEventArgs.ConnectionState> ConnectionChanged;

        public PictureCommunication(SettingsUC.AppSettings settings)
        {
            _broadCastClient = new BroadCastClientSocket(23499, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp, _endOfMessage);
            _serverSocket =  new ServerSocket(23399, SocketType.Stream, ProtocolType.Tcp, _endOfMessage);
            _appSettings = settings;

            _serverSocket.OnConnectionStateChanged += OnCommunicationStateChange;
            _serverSocket.OnPictureReceived += OnPictureReceived;
        }

        public void Start()
        {
            _broadCastClient.StartPollingMessage(_endOfMessage);
            _serverSocket.StartListening();
        }

        public void Shutdown()
        {
            _broadCastClient.ShutDown();
            _serverSocket.ShutDown();
        }

     

        private void OnCommunicationStateChange(object? sender, ServerSocket.ConnectionChangedEventArgs.ConnectionState state)
        {
            ConnectionChanged?.Invoke(sender, state);

            if (state.Equals(ServerSocket.ConnectionChangedEventArgs.ConnectionState.Connected))
            {
                _broadCastClient.ShutDown();
            }
            else
            {
                _broadCastClient = new BroadCastClientSocket(23499, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp, _endOfMessage);
                _broadCastClient.StartPollingMessage(_endOfMessage);
            }
        }


        private void OnPictureReceived(object? sender, PictureData data)
        {
            CreateAndSaveImage(data);

            

            
        }


        private void CreateAndSaveImage(PictureData picData)
        {
            
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(picData.PicData, 0, picData.PicData.Length);

                Image img = Image.FromStream(stream);

                RotateImageToCorrectOrientation(img, picData.OrientationByte);

                img.Save(Path.Combine(_appSettings.PictureFolderPath, picData.Name + ".png"));
            }

            

        }
        
        private void RotateImageToCorrectOrientation(Image image, byte imageOrientation)
        {
            if (!Enum.IsDefined(typeof(Orientation), imageOrientation))
            {
                return;
            }

            image.RotateFlip(_rotationActions[(Orientation)imageOrientation]);
        }

        private enum Orientation : byte
        {
            
            FlipX = 2,
            FlipXY = 3,
            FlipY = 4,
            Rotate90FlipX = 5,
            Rotate90 = 6,
            Rotate90FlipY = 7,
            Rotate90FlipXY = 8,
        }
    }
}
