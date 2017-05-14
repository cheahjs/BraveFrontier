using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BfUnitPostProcess.Animation
{
    class SamReader
    {
        public static void Read(string url)
        {
            using (var wc = new WebClient())
            {
                var data = wc.DownloadData(url);
                using (var stream = new MemoryStream(data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        var fileSig = reader.ReadUInt32();
                        var version = reader.ReadUInt32();
                        var animRate = reader.ReadByte();
                        var x = reader.ReadUInt32();
                        var y = reader.ReadUInt32();
                        var width = reader.ReadUInt32();
                        var height = reader.ReadUInt32();
                        var numOfImages = reader.ReadUInt16();
                        for (int i = 0; i < numOfImages; i++)
                        {
                            var name = ReadString(reader);
                            //ignore stuff
                            reader.ReadBytes(24);
                        }
                        var numOfFrames = reader.ReadUInt16();
                        for (int i = 0; i < numOfFrames; i++)
                        {
                            var flags = reader.ReadByte();
                        }
                    }
                }
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            var stringBytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(stringBytes);
        }

 //       [Flags]
 //       public enum 
    }
}
