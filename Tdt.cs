using System;
using System.IO;
using PoeFormats.Util;

namespace PoeFormats {
    public class Tdt {
        public int version;
        public List<string> strings;

        public Tdt(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                version = r.ReadInt32();
                int strChars = r.ReadInt32();
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
            }
        }
    }
}
