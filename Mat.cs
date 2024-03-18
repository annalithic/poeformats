using PoeFormats.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PoeFormats {
    public class Mat {

        static HashSet<string> colorTexNames = new HashSet<string> {
            "base_color_texture",
            "AlbedoTransparency_TEX",
            "AlbedoSpecMask_TEX",
            "ColourHeight_TEX"
        };

        public class GraphInstance {
            public string parent;
            public List<string> parameters;
            public string baseTex;

            public GraphInstance(JsonNode node) {
                parent = node["parent"].ToString();
                parameters = new List<string>();
                if (node["custom_parameters"] != null) {
                    foreach (var param in node["custom_parameters"].AsArray()) {
                        string paramName = param["name"].ToString();
                        if(colorTexNames.Contains(paramName)) {
                            foreach(var paramValue in param["parameters"].AsArray()) {
                                if (paramValue["path"] != null)
                                    baseTex = paramValue["path"].ToString();
                            }
                        }
                        parameters.Add(param["name"].ToString());
                    }
                }
            }

        }

        public class Param {
            public string name;
            public List<string> parameters;
        }

        public int version;
        public List<GraphInstance> graphs;

        public Mat(string gamePath, string path) : this(Path.Combine(gamePath, path)) { }

        public Mat(string path) {
            string jsonText = File.ReadAllText(path, System.Text.Encoding.Unicode);
            JsonNode json = JsonNode.Parse(jsonText);
            version = (int)json["version"];
            graphs = new List<GraphInstance>();
            if(json["graphinstances"] != null) {
                foreach(var graph in json["graphinstances"].AsArray()) {
                    graphs.Add(new GraphInstance(graph));
                }
            }

        }
    }




}
