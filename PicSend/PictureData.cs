using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicSend
{
    public class PictureData
    {
        public byte OrientationByte {  get; private set; }
        public byte[] PicData {  get; private set; }

        public string Name { get; private set; }

        public PictureData(byte orientationByte, byte[] picture, string name)
        {
            OrientationByte = orientationByte;
            PicData = picture;
            Name = name;
        }

        [JsonConstructor]
        private PictureData()
        {
           
        }
    }
}
