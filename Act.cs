using PoeFormats.Util;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PoeFormats {
    public class Act {
        public string ao;
        public float movementSpeed;
        public Dictionary<string, string> animations;

        public Act(string path) { 
            animations = new Dictionary<string, string>();
            using(TextReader r = new StreamReader(path, System.Text.Encoding.Unicode)) {
                ao = r.ReadValueString("animated_object");
                if (r.ReadLine().Trim() != "{") Console.WriteLine(path + "  ?????????????????? no animated_object block start");
                string line = r.ReadLine().Trim();
                while(line != "}") {

                    if (line.StartsWith("movement_speed ")) {
                        movementSpeed = float.Parse(line.Split()[1]);
                    }

                    //ignore stances for now
                    else if(line.StartsWith("stance ")) {
                        if(!line.Contains('}')) {
                            char c = (char)r.Read();
                            while (c != '}') c = (char)r.Read();
                        }
                    }

                    else {
                        var words = line.SplitQuotes();
                        if(words.Length >= 3 && words[1] == "=") {
                            animations[words[0]] = words[2].TrimEnd(';');
                        }
                    }
                    line = r.ReadLine().Trim();
                }
            }
            string[] actions = animations.Keys.ToArray();

            foreach (string action in actions) {
                if (animations[action].StartsWith('@') && animations.ContainsKey(animations[action].Substring(1))) {
                    animations[action] = animations[animations[action].Substring(1)];
                }
            }

        }
    }
}
