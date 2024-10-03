using PoeFormats.Util;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System;

namespace PoeFormats {
    public class Dat {

        public int rowCount;
        public int rowWidth;
        public byte[] data;
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
                data = r.ReadBytes(rowCount * rowWidth);

                varying = r.ReadBytes((int)r.BaseStream.Length - rowWidth * rowCount - 4);
                
            }
        }

        public string[] Column(Schema.Column column) {
            string[] values = new string[rowCount];
            int size = column.TypeSize();
            int offset = column.offset;

            if ((column.array ? 16 : size) + column.offset > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "ROW OOB";
                }
                return values;
            }
            if(column.array) {
                for (int row = 0; row < rowCount; row++) {
                    int count = BitConverter.ToInt32(data, offset + rowWidth * row);
                    int varyingOffset = BitConverter.ToInt32(data, offset + rowWidth * row + 8);
                    StringBuilder s = new StringBuilder("[");
                    int end = varyingOffset + count * size;
                    if (varyingOffset < 0 || end >= varying.Length) {
                        s.Append($"OOB {varyingOffset}, ");
                    } else {
                        switch (column.type) {
                            case Schema.Column.Type.i32:
                                for (int i = 0; i < count; i++) {
                                    s.Append(BitConverter.ToInt32(varying, varyingOffset + i * column.TypeSize()).ToString());
                                    s.Append(", ");
                                }
                                break;
                            case Schema.Column.Type.rid:
                            case Schema.Column.Type.Row:
                            case Schema.Column.Type.Enum: //TODO ENUM REF
                                for (int i = 0; i < count; i++) {
                                    int val = BitConverter.ToInt32(varying, varyingOffset + i * column.TypeSize());
                                    s.Append(val == -16843010 ? "null" : val.ToString());
                                    s.Append(", ");
                                }
                                break;

                            case Schema.Column.Type.f32:
                                for (int i = 0; i < count; i++) {
                                    s.Append(BitConverter.ToSingle(varying, varyingOffset + i * 4).ToString());
                                    s.Append(", ");
                                }
                                break;
                            case Schema.Column.Type.@bool:
                                for (int i = 0; i < count; i++) {
                                    s.Append(varying[varyingOffset + i] == 0 ? "False, " : "True, ");
                                }
                                break;
                            case Schema.Column.Type.@string:
                                for (int i = 0; i < count; i++) {
                                    int strOffset = BitConverter.ToInt32(varying, varyingOffset + i * column.TypeSize());
                                    if (strOffset < 0 || strOffset >= varying.Length) {
                                        s.Append($"OOB {strOffset}, ");
                                    }
                                    else {
                                        s.Append(CleanString(ReadWStringNullTerminated(varying, strOffset)));
                                        s.Append(", ");
                                    }

                               
                                    
                                }
                                break;
                            case Schema.Column.Type.Unknown:
                                s.Append("UNKNOWN ARRAY TYPE");
                                break;
                        }
                    }
                    
                    if(s.Length > 2) s.Remove(s.Length - 2, 2);
                    s.Append(']');
                    values[row] = s.ToString();

                }


            } else {
                switch (column.type) {
                    case Schema.Column.Type.i32:
                        for (int i = 0; i < rowCount; i++) {
                            values[i] = BitConverter.ToInt32(data, offset + rowWidth * i).ToString();
                        }
                        break;
                    case Schema.Column.Type.Enum: //TODO ENUM REF
                    case Schema.Column.Type.rid:
                    case Schema.Column.Type.Row:
                        for (int i = 0; i < rowCount; i++) {
                            int val = BitConverter.ToInt32(data, offset + rowWidth * i);
                            values[i] = val == -16843010 ? "null" : val.ToString();
                        }
                        break;
                    case Schema.Column.Type.f32:
                        for (int i = 0; i < rowCount; i++) {
                            values[i] = BitConverter.ToSingle(data, offset + rowWidth * i).ToString();
                        }
                        break;
                    case Schema.Column.Type.@bool:
                        for (int i = 0; i < rowCount; i++) {
                            values[i] = data[offset + rowWidth * i] == 0 ? "False" : "True";
                        }
                        break;
                    case Schema.Column.Type.@string:
                        for (int i = 0; i < rowCount; i++) {
                            int strOffset = BitConverter.ToInt32(data, offset + rowWidth * i);
                            if (strOffset < 0 || strOffset >= varying.Length) {
                                values[i] = $"OOB {strOffset} ";
                            } else {
                                values[i] = CleanString(ReadWStringNullTerminated(varying, strOffset));
                            }
                        }
                        break;
                    case Schema.Column.Type.Unknown:
                        for (int i = 0; i < rowCount; i++) {
                            values[i] = "UNKNOWN ROW TYPE";
                        }
                        break;
                }
            }

            return values;
        }

        string CleanString(string s) {
            if (s.IndexOf("\r\n") != -1) s = s.Replace("\r\n", " ");
            if (s.IndexOf('%') != -1) s = s.Replace("%", "%%");
            return s;
        }

        public string[] ColumnInt(int offset) {
            string[] values = new string[rowCount];
            if(offset + 4 > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "OOB";
                }
            } else {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = BitConverter.ToInt32(data, offset + rowWidth * i).ToString();
                }
            }

            return values;
        }

        public string[] ColumnFloat(int offset) {
            string[] values = new string[rowCount];
            if (offset + 4 > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "OOB";
                }
            } else {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = BitConverter.ToSingle(data, offset + rowWidth * i).ToString();
                }
            }
            return values;
        }

        


        public string[] ColumnString(int offset) {
            string[] values = new string[rowCount];
            if (offset + 8 > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "OOB";
                }
            }
            for (int i = 0; i < rowCount; i++) {
                int strOffset = BitConverter.ToInt32(data, offset + rowWidth * i);
                values[i] = ReadWStringNullTerminated(varying, strOffset);
            }
            
            return values;
        }

        static string ReadWStringNullTerminated(byte[] d, int offset) {
            int length = 0;
            while (d[offset + length] != 0) {
                length += 2;
            }
            return Encoding.Unicode.GetString(new ReadOnlySpan<byte>(d, offset, length));
        }


    }

    public class DatReader : IDisposable {
        public int rowCount;
        public int rowWidth;

        int varyingOffset;
        public BinaryReader r;

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
        public int[] IntArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] values = new int[count];
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadInt32();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return values;
        }

        public float Float() {
            return r.ReadSingle();
        }

        public float[] FloatArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            float[] values = new float[count];
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadSingle();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return values;
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
            int[] values = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadInt32();
                r.Seek(12);
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return values;
        }

        public int Row() {
            int test = (int)r.ReadInt64();
            return test;
            return (int)r.ReadInt64();
        }

        public int[] RowArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] offsets = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                offsets[i] = (int)r.ReadInt64();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return offsets;
        }

        public void Dispose() {
            r.Close();
        }

        public void UnknownArray() {
            r.Seek(16);
        }

        public T Enum<T>() where T : struct, IConvertible {
            int i = r.ReadInt32();
            return Unsafe.As<int, T>(ref i);
        }

        public T[] EnumArray<T>() where T : struct, IConvertible {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] values = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadInt32();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return Unsafe.As<int[], T[]>(ref values);
        }

    }
}
