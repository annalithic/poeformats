using System.Collections.Generic;
using System.IO;
using PoeTerrain.Util;

namespace PoeTerrain {
    public class Graph {

        public string path;
        public bool isTerrain;

        public int version;
        public int sizeX;
        public int sizeY;
        public string masterFile;
        public string groundTypeTop;
        public string groundTypeMid;
        public string groundTypeBot;
        public string dungeonDefaultPercent;

        public Node[] nodes;
        public Edge[] edges;

        public Graph(string path) {
            this.path = path;

            isTerrain = path.EndsWith(".tgr");
            using(TextReader r = new StreamReader(File.OpenRead(path))) {
                //r.Delimiters = new string[] { " " };
                //r.HasFieldsEnclosedInQuotes = true;

                WordReader w = new WordReader(r.ReadLine(), 1); 
                version = w.ReadInt(); w.Read(r.ReadLine(), 1);
                sizeX = w.ReadInt(); sizeY = w.ReadInt(); w.Read(r.ReadLine(), 1);

                masterFile = w.ReadString(); w.Read(r.ReadLine(), 1);

                nodes = new Node[w.ReadInt()]; w.Read(r.ReadLine(), 1);
                edges = new Edge[w.ReadInt()];

                groundTypeTop = r.ReadLine().Trim('"');
                groundTypeMid = r.ReadLine().Trim('"');
                groundTypeBot = r.ReadLine().Trim('"');

                if (!isTerrain) dungeonDefaultPercent = r.ReadLine();

                for (int i = 0; i < nodes.Length; i++) nodes[i] = new Node(r.ReadLine().SplitQuotes(), isTerrain);
                for (int i = 0; i < edges.Length; i++) edges[i] = new Edge(r.ReadLine().SplitQuotes(), version, isTerrain);
            }
        }

        public struct Node {
            public int x;
            public int y;
            public int[] edgeConnections;
            public string tileType;
            public string orientation;
            public string[] descriptions;
            public int existance;
            public int height;

            public char dungeonUnk;
            public int lockPositionX; //?
            public int lockPositionY; //?

            public Node(string[] words, bool isTerrain) {
                WordReader r = new WordReader(words);
                x = r.ReadInt(); y = r.ReadInt();
                edgeConnections = r.ReadIntArray();
                tileType = r.ReadString();
                orientation = r.ReadString(false);
                descriptions = r.ReadStringArray();
                existance = r.ReadInt();
                height = r.ReadInt();

                if(isTerrain) {
                    lockPositionX = r.ReadInt(); lockPositionY = r.ReadInt();
                    dungeonUnk = '0';
                } else {
                    dungeonUnk = r.ReadChar();
                    lockPositionX = 0; lockPositionY = 0;
                }

             }


        }

        public class Edge {
            public int start;
            public int end;
            public int[] unkPos;
            public int unk2;
            public int unk3;
            public string edgeType;
            public int unk4;
            public int unk5;

            public string dungeonName;
            public int dungeonUnk1;
            public int dungeonUnk2;
            public string dungeonName2;
            public int dungeonUnk3;
            public char dungeonUnk4;
            public char dungeonUnk5;
            public int dungeonUnk6;

            public Edge(string[] words, int version, bool isTerrain) {
                WordReader r = new WordReader(words);
                start = r.ReadInt();
                end = r.ReadInt();
                unkPos = new int[r.ReadInt() * 2]; for (int i = 0; i < unkPos.Length; i++) unkPos[i] = r.ReadInt();
                unk2 = r.ReadInt();
                unk3 = r.ReadInt();
                edgeType = r.ReadString();
                unk4 = r.ReadInt();
                unk5 = r.ReadInt();

                if(!isTerrain) {
                    dungeonName = r.ReadString();
                    dungeonUnk1 = r.ReadInt();
                    if (version > 19) dungeonName2 = r.ReadString();
                    dungeonUnk2 = r.ReadInt();
                    dungeonUnk3 = r.ReadInt();
                    dungeonUnk4 = r.ReadChar();
                    dungeonUnk5 = r.ReadChar();
                    dungeonUnk6 = r.ReadInt();
                }
            }
        }
    }
}
