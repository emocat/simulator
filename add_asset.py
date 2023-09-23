import zipfile
import json
import sys, os
import sqlite3
import datetime
import uuid

asset = sys.argv[1]
sim_path = os.path.expanduser("~/Build_Local")
data_path = os.path.join(sim_path, "2021.2-wise")
config_path = os.path.join(sim_path, "wise/api/v1")

typeMap = {
    "bridge": "Bridge",
    "sensor": "Sensor",
    "map": "Environment",
}

# Get asset information
with zipfile.ZipFile(asset, "r") as zipr:
    with zipr.open("manifest.json") as f:
        manifest = json.load(f)
assetName = manifest["assetName"]
if len(sys.argv) == 3:
    assetName = sys.argv[2]
assetType = manifest["assetType"]
assetGuid = manifest["assetGuid"]
print("[+] {} {} {}".format(assetGuid, assetType, assetName))

# Copy the file to data path
assetPath = os.path.join(data_path, typeMap[assetType] + "s", assetGuid)
print("[+] Checking {}".format(assetPath))
if not os.path.exists(assetPath):
    os.system("cp {} {}".format(asset, assetPath))
else:
    print("[!] Local file already exist, not copying")

# Insert into database
conn = sqlite3.connect(os.path.join(data_path, "data.db"))
cursor = conn.execute("select * from assets where assetGuid = '{}';".format(assetGuid))
res = cursor.fetchone()
if res == None:
    cmd = 'INSERT INTO assets VALUES ("{}","{}","{}","{}","{}")'.format(
        assetGuid,
        typeMap[assetType],
        assetName,
        assetPath,
        datetime.datetime.now().strftime("%m/%d/%Y %I:%M:%S %p"),
    )
    print(cmd)
    conn.execute(cmd)
    conn.commit()
else:
    print("[!] Duplicate asset in database")

# Map configuration
if assetType == "map":
    map_library_path = os.path.join(config_path, "maps_display-sim+limit-50+offset-0")
    with open(map_library_path, "r") as f:
        map_list = json.load(f)
    if assetName == "SanFrancisco":
        assetName = "SanFrancisco202006"
    if assetName not in list(map(lambda x: x["name"], map_list["rows"])):
        # generator a new uuid as map configuration id
        map_config_uuid = str(uuid.uuid4())
        print("[+] New map configuration uuid {}".format(map_config_uuid))
        map_list["rows"].append(
            dict(assetGuid=assetGuid, id=map_config_uuid, name=assetName)
        )
        with open(map_library_path, "w") as f:
            json.dump(map_list, f, indent=4)

        map_config_path = os.path.join(config_path, "maps", map_config_uuid)
        if os.path.exists(map_config_path):
            print("[!] map config already exists")
        else:
            with open(map_config_path, "w") as f:
                json.dump(dict(assetGuid=assetGuid, id=map_config_uuid, name=assetName), f, indent=4)
    else:
        print("[!] map already in the library")


conn.close()
