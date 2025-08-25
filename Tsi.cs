using PoeFormats.Util;
using System;
using System.IO;
using System.Collections.Generic;

namespace PoeFormats {
    public class Tsi {

        string path;

        public int version;

        public bool allowInteriorOverlapFixes;
        public bool bottomStoreyOverride;
        public bool disableDoors;
        public bool doNotFillOutside;
        public bool forceOuterGT;
        public bool mergeInsideWalls;
        public bool outerFillHeightOverride;
        public bool randomiseUniqueMeshVariationsPerSubtile;
        public bool removeOuterEdge;
        public bool roomOcclusion;
        public bool roomOverlap;
        public bool useParentMinimapLayer;

        public int defaultKillZone;
        public int dungeonFillRoomHeightOverride;
        public int envMargin;
        public int outerBufferSize;
        public int roofFadeDistanceDownscreen;
        public int roomUnitSize;
        public int storeyHeight;
        public int tallWallActivationDistanceDownscreen;
        public int tallWallActivationDistanceUpscreen;
        public int topStoreyOverride;

        public string blendMaskOverride; //dds
        public string chestData; //cht
        public string critters; //clt
        public string decals; //dct
        public string doodads; //ddt
        public string edgeCombination; //ecf
        public string edgePoints; //edp (SERVER ONLY?)
        public string fileGroups; //fgp
        public string fillTiles; //gft
        public string materialsList; //mtd
        public string outerGroundType; //gt
        public string overlays; //toy
        public string roomSet; //rs
        public string tileMaterialOverrides; //tmo
        public string tileSet; //tst

        public string doorEdgeType;
        public string environment1;
        public string environment2;
        public string environment;
        public string environmentPreload;
        public string extendActivationDistance;
        public string overrideChestSpawns;
        public string overrideMonsterSpawns;
        public string overrideSpawns;
        public string reskin;
        public string restrictedGTs;
        public string roofFadeMode;
        public string uniqueMeshOverrideSet1;
        public string walkableGroundMaterialOverride;

        public string environmentSectorKey;
        public string environmentSectorValue;

        static char[] whiteSpaceChars = new char[] { ' ', '\t' };

        bool Bool(string s) {
            if (s == "1") return true;
            if (s == "0") return false;
            throw new Exception();
        }

        public Tsi(string path) {
            this.path = path;

            char[] splitChars = new char[] { ' ', '\t' };
            foreach (string line in File.ReadAllLines(path)) {
                if (line.StartsWith("//")) continue;
                string[] sp = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                if (sp.Length != 2) {
                    continue;
                }
                sp[1] = sp[1].Trim('"');

                switch(sp[0]) {
                    case "version": version = int.Parse(sp[1]); break;
                    case "DefaultKillZone": defaultKillZone = int.Parse(sp[1]); break;
                    case "DungeonFillRoomHeightOverride": dungeonFillRoomHeightOverride = int.Parse(sp[1]); break;
                    case "EnvMargin": envMargin = int.Parse(sp[1]); break;
                    case "OuterBufferSize": outerBufferSize = int.Parse(sp[1]); break;
                    case "RoofFadeDistanceDownscreen": roofFadeDistanceDownscreen = int.Parse(sp[1]); break;
                    case "RoomUnitSize": roomUnitSize = int.Parse(sp[1]); break;
                    case "StoreyHeight": storeyHeight = int.Parse(sp[1]); break;
                    case "TallWallActivationDistanceDownscreen": tallWallActivationDistanceDownscreen = int.Parse(sp[1]); break;
                    case "TallWallActivationDistanceUpscreen": tallWallActivationDistanceUpscreen = int.Parse(sp[1]); break;
                    case "TopStoreyOverride": topStoreyOverride = int.Parse(sp[1]); break;

                    case "AllowInteriorOverlapFixes": allowInteriorOverlapFixes = Bool(sp[1]); break;
                    case "BottomStoreyOverride": bottomStoreyOverride = Bool(sp[1]); break;
                    case "DisableDoors": disableDoors = Bool(sp[1]); break;
                    case "DoNotFillOutside": doNotFillOutside = Bool(sp[1]); break;
                    case "ForceOuterGT": forceOuterGT = Bool(sp[1]); break;
                    case "MergeInsideWalls": mergeInsideWalls = Bool(sp[1]); break;
                    case "OuterFillHeightOverride": outerFillHeightOverride = Bool(sp[1]); break;
                    case "RandomiseUniqueMeshVariationsPerSubtile": randomiseUniqueMeshVariationsPerSubtile = Bool(sp[1]); break;
                    case "RemoveOuterEdge": removeOuterEdge = Bool(sp[1]); break;
                    case "RoomOcclusion": roomOcclusion = Bool(sp[1]); break;
                    case "RoomOverlap": roomOverlap = Bool(sp[1]); break;
                    case "UseParentMinimapLayer": useParentMinimapLayer = Bool(sp[1]); break;

                    case "BlendMaskOverride": blendMaskOverride = sp[1]; break;
                    case "ChestData": chestData = sp[1]; break;
                    case "Critters": critters = sp[1]; break;
                    case "Decals": decals = sp[1]; break;
                    case "Doodads": doodads = sp[1]; break;
                    case "EdgeCombination": edgeCombination = sp[1]; break;
                    case "EdgePoints": edgePoints = sp[1]; break;
                    case "FileGroups": fileGroups = sp[1]; break;
                    case "FillTiles": fillTiles = sp[1]; break;
                    case "MaterialsList": materialsList = sp[1]; break;
                    case "OuterGroundType": outerGroundType = sp[1]; break;
                    case "Overlays": overlays = sp[1]; break;
                    case "RoomSet": roomSet = sp[1]; break;
                    case "TileMaterialOverrides": tileMaterialOverrides = sp[1]; break;
                    case "TileSet": tileSet = sp[1]; break;

                    case "DoorEdgeType": doorEdgeType = sp[1]; break;
                    case "Environment": environment = sp[1]; break;
                    case "Environment1": environment1 = sp[1]; break;
                    case "Environment2": environment2 = sp[1]; break;
                    case "EnvironmentPreload": environmentPreload = sp[1]; break;
                    case "ExtendActivationDistance": extendActivationDistance = sp[1]; break;
                    case "OverrideChestSpawns": overrideChestSpawns = sp[1]; break;
                    case "OverrideMonsterSpawns": overrideMonsterSpawns = sp[1]; break;
                    case "OverrideSpawns": overrideSpawns = sp[1]; break;
                    case "Reskin": reskin = sp[1]; break;
                    case "RestrictedGTs": restrictedGTs = sp[1]; break;
                    case "RoofFadeMode": roofFadeMode = sp[1]; break;
                    case "UniqueMeshOverrideSet1": uniqueMeshOverrideSet1 = sp[1]; break;
                    case "WalkableGroundMaterialOverride": walkableGroundMaterialOverride = sp[1]; break;

                    case "EnvironmentSector": environmentSectorKey = sp[2]; environmentSectorValue = sp[1]; break;

                    default: 
                        Console.WriteLine("UNKNOWN TSI KEY " + sp[0] + " IN " + path); break;
                }
            }
        }

        public IEnumerable<string> GetTiles(string gamePath) {
            if(tileSet == null) return new string[0];

            string tstPath = path.Replace(Path.GetFileName(path), tileSet);
            HashSet<string> tiles = new HashSet<string>();
            
            foreach (string line in File.ReadAllLines(tstPath)) {
                if (line.StartsWith("//")) continue;
                foreach (string word in line.Split(whiteSpaceChars, StringSplitOptions.RemoveEmptyEntries)) {
                    if (word.EndsWith(".tdt\"")) {
                        tiles.Add(word.Trim('"'));
                    }
                }
            }

            return tiles;


        }

        public IEnumerable<string> GetTileGeometries(string gamePath) { 
            var tiles = GetTiles(gamePath);

            HashSet<string> tileGeometries = new HashSet<string>();

            foreach (string tdtPath in tiles) {
               Tdt.ReadTileGeometry(gamePath, tdtPath, tileGeometries);
            }
            return tileGeometries;

        }


        public Dictionary<string, string> GetMaterialOverrides() {
            Dictionary<string, string> materialOverrides = new Dictionary<string, string>();
            if(tileMaterialOverrides == null) return materialOverrides;
            string tmoPath = path.Replace(Path.GetFileName(path), tileMaterialOverrides);
            if (!File.Exists(tmoPath)) return null;
            foreach (string line in File.ReadAllLines(tmoPath)) {
                if (line.IndexOf("//") != -1) continue; //TODO comment at end of line
                string[] words = line.Split(whiteSpaceChars, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < words.Length - 1; i++) {
                    if (words[i].EndsWith(".mat\""))
                        if (words[i + 1].EndsWith(".mat\"")) {
                        materialOverrides[words[i].Trim('"').Replace('\\', '/')] = words[i + 1].Trim('"').Replace('\\', '/');
                    }
                }
           
            }
            return materialOverrides;
        }
    }
}