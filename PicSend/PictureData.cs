using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicSend
{
    [Serializable]
    public class PictureData
    {
        public byte OrientationByte {  get;  set; }
        public byte[] PicData {  get;  set; }

        public string Name { get;  set; }

        public PictureData(byte orientationByte, byte[] picture, string name)
        {
            OrientationByte = orientationByte;
            PicData = picture;
            Name = name;
        }

        [JsonConstructor]
        public PictureData()
        {
            Debug.WriteLine("Json Created PictureData Object!");
        }
    }
}
