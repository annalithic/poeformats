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

        public class Parameter {
            public string path;
            public bool srgb;
            public float value;
            public bool hasValue;

            public Parameter() {
                path = null;
                srgb = false;
                value = 0.0f;
                hasValue = false;
            }
        }

        public class GraphInstance {
            public string parent;
            public Dictionary<string, Parameter> parameters;
            public string baseTex;

            public GraphInstance(JsonNode node) {
                parent = node["parent"].ToString();
                parameters = new Dictionary<string, Parameter>();
                if (node["custom_parameters"] != null) {
                    foreach (var param in node["custom_parameters"].AsArray()) {
                        string paramName = param["name"].ToString();
                        Parameter parameter = new Parameter();
                        foreach (var paramValue in param["parameters"].AsArray()) {
                            if (paramValue["path"] != null) {
                                string paramPath = paramValue["path"].ToString();
                                Console.WriteLine("PATH FOUND " + paramPath);
                                parameter.path = paramPath;

                                if (colorTexNames.Contains(paramName)) {
                                    baseTex = paramPath;
                                }
                            } 
                            if (paramValue["srgb"] != null) {
                                parameter.srgb = paramValue["srgb"].ToString() == "true";
                            }
                            if (paramValue["value"] != null) {
                                string valueStr = paramValue["value"].ToString();
                                if (float.TryParse(valueStr, out float value)) {
                                    parameter.hasValue = true;
                                    parameter.value = value;
                                } else if (valueStr == "true") {
                                    parameter.hasValue = true;
                                    parameter.value = 1;
                                } else if (valueStr == "false") {
                                    parameter.hasValue = true;
                                    parameter.value = 0;
                                }

                            }
                        }
                        parameters[paramName] = parameter;
                    }
                }
            }

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
