using System.Collections.Generic;
using System.IO;
using PoeFormats.Util;
namespace PoeFormats {
    public class Arm {
        public int version;

        public string[] dictionary;

        public int size1x; public int size1y;
        public int apax; public int apay;
        public string name;
        public int bepax; public int bepay;

        public TileKey roomKey;

        public string[] apaEntries;

        //     0,       1,     2,                3,              4,    5,     6,   7,    8,
        //9    monster, chest,                   unk,                              warp,
        //10   monster, chest, unk               monsterspawner, unk,                    warp
        //5    monster, chest, unk,              monsterspawner, warp, 
        //6    monster, chest, infestneggsgreen, monsterspawner, warp, objects


        public static Dictionary<int, int> versionEntityCounts = new Dictionary<int, int>() {
            {14, 9 }, {15, 9}, {16, 9},  {18, 9},  {19, 9},
            {20, 10 }, {21, 10}, {22, 10}, {23, 10}, {24, 10},
            {26, 5},   {27, 5 }, {28, 5 }, 
            {29, 6},   {30, 6 }, {31, 6}, {32, 6},
        };
        public string[][] entityLines; 

        public TileKey[,] tileKeys;

        public Doodad[] doodads;



        public Arm(string path) {
            using (TextReader r = new StreamReader(File.OpenRead(path))) {

                version = int.Parse(r.ReadLine().Substring(8));

                dictionary = new string[r.ReadLineInt()];
                for (int i = 0; i < dictionary.Length; i++) dictionary[i] = r.ReadLineString();

                r.ReadLineInt(out size1x, out size1y);
                r.ReadLineInt(out apax, out apay);

                name = r.ReadLineString();
                if (version == 22 && r.Peek() == 10) r.ReadLine();
                if (version > 15) r.ReadLineInt(out bepax, out bepay);
                else bepax = int.Parse(r.ReadLine());

                roomKey = new TileKey(new WordReader(r.ReadLine().Split(' ')), version);

                apaEntries = new string[(apax + apay) * 2];
                for (int i = 0; i < apaEntries.Length; i++) apaEntries[i] = r.ReadLine();

                entityLines = new string[versionEntityCounts[version]][];
                for(int entityType = 0; entityType < versionEntityCounts[version]; entityType++) {
                    if (version > 31) { //why did they do this???
                        List<string> entities = new List<string>();
                        string line = r.ReadLine();
                        while(line != "-1") {
                            entities.Add(line);
                            line = r.ReadLine();
                        }
                        entityLines[entityType] = entities.ToArray();
                    } else {
                        entityLines[entityType] = new string[r.ReadLineInt()];
                        for (int i = 0; i < entityLines[entityType].Length; i++) entityLines[entityType][i] = r.ReadLine();

                    }
                }

                tileKeys = new TileKey[roomKey.sizeX, roomKey.sizeY];
                for (int y = 0; y < roomKey.sizeY; y++) {
                    WordReader w = new WordReader(r.ReadLine().Split(' '));
                    for(int x = 0; x < roomKey.sizeX; x++) {
                        tileKeys[x, y] = new TileKey(w, version);
                    }
                }

                //doodads = new Doodad[r.ReadLineInt()]; for (int i = 0; i < doodads.Length; i++) doodads[i] = new Doodad(r.ReadLine().Split(' '));
            }
        }

        public class TileKey {
            public enum Type {
                k,
                s,
                n,
                o,
                f

            }

            public Type type;

            public int sizeX;
            public int sizeY;
            public int edgeTypeDown;
            public int edgeTypeRight;
            public int edgeTypeUp;
            public int edgeTypeLeft;
            public int edgeLengthDown;
            public int unk2;
            public int edgeLengthRight;
            public int unk4;
            public int edgeLengthUp;
            public int unk6;
            public int edgeLengthLeft;
            public int unk8;
            public int groundTypeDownLeft;
            public int groundTypeDownRight;
            public int groundTypeUpRight;
            public int groundTypeUpLeft;
            public int heightDownLeft;
            public int heightDownRight;
            public int heightUpRight;
            public int heightUpLeft;
            public int feature;
            public int origin;



            //public int[] values;

            public TileKey(WordReader r, int version) {
                type = (Type)System.Enum.Parse(typeof(Type), r.ReadString(false));
                if (type == Type.k) {
                    sizeX = r.ReadInt();
                    sizeY = r.ReadInt();
                    edgeTypeDown = r.ReadInt();
                    edgeTypeRight = r.ReadInt();
                    edgeTypeUp = r.ReadInt();
                    edgeTypeLeft = r.ReadInt();
                    edgeLengthDown = r.ReadInt();
                    unk2 = r.ReadInt();
                    edgeLengthRight = r.ReadInt();
                    unk4 = r.ReadInt();
                    edgeLengthUp = r.ReadInt();
                    unk6 = r.ReadInt();
                    edgeLengthLeft = r.ReadInt();
                    unk8 = r.ReadInt();
                    
                    groundTypeDownLeft = r.ReadInt();
                    groundTypeDownRight = r.ReadInt();
                    groundTypeUpRight = r.ReadInt();
                    groundTypeUpLeft = r.ReadInt();
                    heightDownLeft = r.ReadInt();
                    heightDownRight = r.ReadInt();
                    heightUpRight = r.ReadInt();
                    heightUpLeft = r.ReadInt();
                    feature = r.ReadInt();
                    if(version > 18) origin = r.ReadInt();
                    //values = new int[24];
                    //for (int i = 0; i < values.Length; i++) values[i] = r.ReadInt();
                } else if (type == Type.f) {
                    feature = r.ReadInt();
                }
            }

            public string MeshDescription() {
                return $"{sizeX}_{sizeY}_{edgeLengthDown}_{edgeLengthRight}_{edgeLengthUp}_{edgeLengthLeft}";
            }

        }



        public class Doodad {
            public int x;
            public int y;
            public float z;
            public float unk1;
            public float unk2;
            public float unk3;
            public float unk4;
            public int unk5;
            public int unk6;
            public float unk7;
            public float scale;
            public string artFile;
            public string objectFile;

            public Doodad(string[] words) {
                if (words.Length < 13) return;
                WordReader reader = new WordReader(words);
                x = reader.ReadInt();
                y = reader.ReadInt();
                z = reader.ReadFloat();
                unk1 = reader.ReadFloat();
                unk2 = reader.ReadFloat();
                unk3 = reader.ReadFloat();
                unk4 = reader.ReadFloat();
                unk5 = reader.ReadInt();
                unk6 = reader.ReadInt();
                if (reader.ReadInt() == 1) unk7 = reader.ReadFloat();
                scale = reader.ReadFloat();
                artFile = reader.ReadString();
                objectFile = reader.ReadString();
            }
        }
    }
}