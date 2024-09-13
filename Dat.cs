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

    public class DatReader : IDisposable {
        public int rowCount;
        public int rowWidth;

        int varyingOffset;
        BinaryReader r;

        public DatReader(string path) {
            r = new BinaryReader(File.OpenRead(path));
            rowCount = r.ReadInt32();
            while (true) {
                r.BaseStream.Seek(4 + rowWidth * rowCount, SeekOrigin.Begin);
                if (r.ReadUInt64() == 0xbbbbbbbbbbbbbbbb) break;
                rowWidth++;
            }
            varyingOffset = 4 + rowWidth * rowCount;
        }

        public void SeekRow(int i) {
            r.BaseStream.Seek(4 + rowWidth *  i, SeekOrigin.Begin);
        }

        public string String() {
            long offset = r.ReadInt64();
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            string s = r.ReadWStringNullTerminated();
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return s;
        }

        public string[] StringArray() {
            int count = (int)r.ReadInt64();
            long[] stringOffsets = new long[count];
            string[] strings = new string[count];
            long offset = r.ReadInt64();
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                stringOffsets[i] = r.ReadInt64();
            }
            for (int i = 0; i < count; i++) {
                r.BaseStream.Seek(varyingOffset + stringOffsets[i], SeekOrigin.Begin);
                strings[i] = r.ReadWStringNullTerminated();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return strings;
        }

        public int Int() {
            return r.ReadInt32();
        }
        public bool Bool() {
            return r.ReadBoolean();
        }
        public int Ref() {
            int refnum = r.ReadInt32();
            r.Seek(12);
            return refnum;
        }

        public int[] RefArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] offsets = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                offsets[i] = r.ReadInt32();
                r.Seek(12);
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return offsets;
        }

        public void Dispose() {
            r.Close();
        }
    }
}
