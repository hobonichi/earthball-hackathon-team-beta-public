// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using System.Globalization;
using UnityEngine;

namespace Hobonichi {
    [System.Serializable]
    public class ObjectInfo {
        public string id;
        public string model;
        public string material;
        public string texture;
        public string location = "0,0,0";
        public string direction = "0,0,0";
        public string scale = "1,1,1";
        public string label;
        public string color = "#ffffffff";
        public string from;
        public string to;
        public int split;
        public float width;
        public string overwrite = "";
        public string mp3_url = "";
        public string similar_countries = "";
        public ActionInfo[] actions;

        public string[] SimilarContries {
            get {
                char[] delimiterChars = { ',' };
                string similar_countries_with_self = similar_countries + "," + id;
                var tmp = similar_countries_with_self.Split(delimiterChars);
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = tmp[i].Replace(" ", "");
                }
                return tmp;
            }
        }

        public Vector3 Location {
            get {
                var values = location.Split(',');
                return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            }
        }

        public Vector3 Direction {
            get {
                var values = direction.Split(',');
                return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            }
        }

        public Quaternion Rotation {
            get {
                var values = direction.Split(',');
                return Quaternion.Euler(new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
            }
        }

        public Vector3 Scale {
            get {
                var values = scale.Split(',');
                return values.Length >= 3 ? new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])) : new Vector3(float.Parse(values[0]), float.Parse(values[0]), float.Parse(values[0]));
            }
        }

        public bool AutoScale {
            get {
                return scale.Split(',').Length == 1;
            }
        }

        public Vector3 From {
            get {
                var values = from.Split(',');
                return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            }
        }

        public Vector3 To {
            get {
                var values = to.Split(',');
                return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            }
        }

        public Color MainColor {
            get {
                if (!string.IsNullOrEmpty(color) && color.StartsWith("#")) {
                    if (color.Length == 7) {
                        return new Color(
                            int.Parse(color.Substring(1, 2), NumberStyles.HexNumber) / 255f,
                            int.Parse(color.Substring(3, 2), NumberStyles.HexNumber) / 255f,
                            int.Parse(color.Substring(5, 2), NumberStyles.HexNumber) / 255f);
                    }
                    else if (color.Length == 9) {
                        return new Color(
                            int.Parse(color.Substring(1, 2), NumberStyles.HexNumber) / 255f,
                            int.Parse(color.Substring(3, 2), NumberStyles.HexNumber) / 255f,
                            int.Parse(color.Substring(5, 2), NumberStyles.HexNumber) / 255f,
                            int.Parse(color.Substring(7, 2), NumberStyles.HexNumber) / 255f);
                    }
                }
                return Color.black;
            }
        }

        public bool OverwriteFlag {
            get {
                if (string.IsNullOrEmpty(overwrite)) return false;
                string value = overwrite.Trim().ToLower();
                return (value != "false" && value != "0");
            }
        }
    }

    [System.Serializable]
    public class ActionInfo {
        public string action;
        public string param = "";
    }
}
