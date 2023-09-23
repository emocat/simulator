## Introduction

We will introduce how to run a simulation with Apollo 6.0 **totally locally** in this repo. We mainly tested on **API Only mode** under 2021.2.2 but it should applies to other versions and other AD systems.

## Technical principle

Everytime when LGSVL simulator tries to query an api configuration file (e.g., map or vehicle sensor configuration) from remote website, we force it first to look up files locally.

In this way, we can add any assets and configurations without connection to remote wise websites.

### Steps to load a map

1. Query user’s map library → We can save the library file in local path
2. Get corresponding map **id** from library according to map **name**
3. Query mapData according to map **id** → We can save the mapData in local path
4. Get corresponding map **AssetGuid** from mapData
5. Lookup **LocalPath** in local database according to **AssetGuid** → We can save the entry in local database
6. Load map asset according to **LocalPath** → We can save the map asset in local path 

### Steps to add an agent

1. Query vehicleData according to vehicle sensor configuration **id** → We can save the vehicleData in local path
2. Get bridge and sensor lists from vehicleData
3. Lookup **LocalPath** in local database according for bridge and sensors according to **AssetGuid** → We can save the entries in local database
4. Load assets according to **LocalPath** → We can save the map asset in local path

## Usage

### How to run the modified simulator?

You need to specify the data directory under 2021.2-wise.

```bash
./simulator --data ./2021.2-wise
```

### How to add an asset locally?

We provide a script named `add_asset.py`. Once a developer build an asset (map, sensor or bridge), the asset can be add to simulator as follows:

```bash
python3 add_asset.py <assets>
```

The script will extract the asset’s name/type/guid, copy asset file to local path and insert corresponding entry to the database.

### How to modify a vehicle’s sensor configuration?

Vehicle’s sensor configurations will be stored at `wise/api/v1/vehicles`. Once you want to add a new sensor configuration, you can make a copy of previous configuration file. Then, if you want to change a specific sensor or bridge, you can directly modify the `assetGuid` field.