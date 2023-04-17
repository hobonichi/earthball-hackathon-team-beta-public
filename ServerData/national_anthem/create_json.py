import json
import pandas as pd

df = pd.read_csv('./nation_flag_anthems_loc.csv')
import json
import pandas as pd
df = pd.read_csv('./nation_flag_anthems_loc.csv')

data = [{
    "id": "Globe",
    "overwrite": "true",
    "model": "Sphere",
    "material": "Unlit",
    "texture": "http://localhost:8080/globe_texture.jpg",
    "location": "0, 0, 0",
    "direction": "0, 0, 0",
    "scale": "1, 1, 1",
    "actions": [
      {
        "action": "add",
        "param": "globe_texture_alter.json"
      },
      {
        "action": "delete",
        "param": "Washington,WashingtonLabel,Fukui,FukuiLabel"
      }
    ]
  },]

for i, row in df.iterrows():
    data.append({
        "id": row["nation_name"],
        "model": "nation_flag",
        "material": "UnlitTransparentNoCull",
        "texture": row["img_url"],
        "location": f"{row['longitude']}, {row['latitude']}, 1",
        "direction": "0, 0, 0",
        "scale": "0.1",
        "mp3_url": row["nationalanthems_mp3"],
        "actions": [
            "get_mp3"
        ]
    })
with open("./nation_flag_anthems_loc.json", "w") as f:
    json.dump(data, f)