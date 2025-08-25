using System;
using System.Collections.Generic;
using System.IO;
using PoeFormats.Util;

namespace PoeFormats {
    public class Tdt {
        public int version;
        public List<string> strings;

        int strChars;

        public string inherits;
        public string tgt;
        public string feature;
        public string edgeTypeDown;
        public string edgeTypeRight;
        public string edgeTypeUp;
        public string edgeTypeLeft;
        public byte sizeX;
        public byte sizeY;
        public string groundTypeDownLeft;
        public string groundTypeDownRight;
        public string groundTypeUpRight;
        public string groundTypeUpLeft;
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public byte unk5;
        public byte edgeDistDown;
        public byte edgeDistDown2;
        public byte edgeDistRight;
        public byte edgeDistRight2;
        public byte edgeDistUp;
        public byte edgeDistUp2;
        public byte edgeDistLeft;
        public byte edgeDistLeft2;

        public Tdt(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                version = r.ReadInt32();
                strChars = r.ReadInt32();
                int strEnd = 8 + strChars * 2;
                strings = new List<string>();
                while (r.BaseStream.Position < strEnd && strings.Count < 1000) {
                    string s = r.ReadWStringNullTerminated();
                    //TODO
                    if(s.IndexOf(';') != -1) {
                        foreach(string s2 in s.Split(';')) strings.Add(s2);
                    } else {
                        strings.Add(s);
                    }
                    r.Seek(2);
                }

                inherits = ReadStr(r);
                if (inherits != null && inherits.Length > 0) {
                    tgt = null;
                    r.Seek(1);
                } else {
                    tgt = ReadStr(r);
                    feature = ReadStr(r);
                    edgeTypeDown = ReadStr(r);
                    edgeTypeRight = ReadStr(r);
                    edgeTypeUp = ReadStr(r);
                    edgeTypeLeft = ReadStr(r);
                    sizeX = r.ReadByte();
                    sizeY = r.ReadByte();
                    groundTypeDownLeft = ReadStr(r);
                    groundTypeDownRight = ReadStr(r);
                    groundTypeUpRight = ReadStr(r);
                    groundTypeUpLeft = ReadStr(r);
                    unk2 = r.ReadByte();
                    unk3 = r.ReadByte();
                    unk4 = r.ReadByte();
                    unk5 = r.ReadByte();
                    edgeDistDown = r.ReadByte();
                    edgeDistDown2 = r.ReadByte();
                    edgeDistRight = r.ReadByte();
                    edgeDistRight2 = r.ReadByte();
                    edgeDistUp = r.ReadByte();
                    edgeDistUp2 = r.ReadByte();
                    edgeDistLeft = r.ReadByte();
                    edgeDistLeft2 = r.ReadByte();
                }
            }
        }

        string ReadStr(BinaryReader r) {
            int i = r.ReadInt32();
            //Console.WriteLine(i);
            if (i < 0 || i >= strChars) return null;
            var pos = r.BaseStream.Position;
            r.BaseStream.Seek(8 + i * 2, SeekOrigin.Begin);
            string str = r.ReadWStringNullTerminated();
            r.BaseStream.Seek(pos, SeekOrigin.Begin);
            return str;
        }

        public void ReadTileGeometry(string gamePath, HashSet<string> geometries) {

            if (inherits != null && inherits.Length > 0) ReadTileGeometry(gamePath, inherits, geometries);

            if (tgt != null) {
                //TODO tdt that extends tdt dont work
                if (!tgt.EndsWith(".tgt")) return;
                foreach (string geom in tgt.Split(';')) {
                    geometries.Add(geom);
                }
            }
        }

        public static void ReadTileGeometry(string gamePath, string tdtPath, HashSet<string> geometries) {
            string fullPath = Path.Combine(gamePath, tdtPath.ToLower());

            Tdt tdt = new Tdt(fullPath);

            if (tdt.inherits != null && tdt.inherits.Length > 0) ReadTileGeometry(gamePath, tdt.inherits, geometries);

            if (tdt.tgt != null) {
                //TODO tdt that extends tdt dont work
                if (!tdt.tgt.EndsWith(".tgt")) return;
                foreach (string geom in tdt.tgt.Split(';')) {
                    geometries.Add(geom);
                }
            }
        }
    }
}
