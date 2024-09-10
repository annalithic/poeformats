using PoeFormats.Util;
using System;
using System.IO;

namespace PoeFormats {
    public class Dat {

        public int rowCount;
        public int rowWidth;
        public byte[][] rows;
        public byte[] varying;

        public Dat(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                rowCount = r.ReadInt32();
                while(true) {
                    r.BaseStream.Seek(4 + rowWidth * rowCount, SeekOrigin.Begin);
                    if (r.ReadUInt64() == 0xbbbbbbbbbbbbbbbb) break;
                    rowWidth++;
                }

                r.BaseStream.Seek(4, SeekOrigin.Begin);
                rows = new byte[rowCount][];
                for (int i = 0; i < rowCount; i++) {
                    rows[i] = r.ReadBytes(rowWidth);
                }

                varying = r.ReadBytes((int)r.BaseStream.Length - rowWidth * rowCount - 4);
            }
        }
    }
}
