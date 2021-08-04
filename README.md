# RainbowForge

RainbowForge is a set of libraries and tools for working with Rainbow Six: Siege's FORGE asset containers. The primary tool for utilizing this library is **DumpTool**.

# DumpTool

All of these commands which output files or directories put them in the **current working directory**. Therefore, it is not advised to run commands which generate a lot of output (i.e. `dumpall`, etc) in the folder where the program resides.

Any of these command usages can be accessed through the help interface:

Usage: `DumpTool.exe help`

## Commands

### `list`

List all of the UIDs and their types in the given container.

Usage: `DumpTool.exe list <forge file>`

> Example: `DumpTool.exe list "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge"`

### `find`

Find the forge file(s) in the given directory that contain an asset with the given UID

Usage: `DumpTool.exe find <directory of forge files> <uid>`

> Example: `DumpTool.exe find "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege" 123412341234`

### `inspect`

Get some basic details on a given UID from the perspective of the containing forge file

Usage: `DumpTool.exe inspect <forge file> <uid>`

> Example: `DumpTool.exe inspect "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge" 123412341234`

### `dump`

Dump a non-archive asset

Usage: `DumpTool.exe dump <forge file> <uid>`

> Example: `DumpTool.exe dump "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge" 123412341234`

### `dumpall`

Dumps all non-archive assets

Usage: `DumpTool.exe dumpall <forge file>`

> Example: `DumpTool.exe dumpall "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge"`

**This command is slow.**

### `dumpmeshprops`

Dump all of the MeshProperties containers a flat archive, using the search index to recursively dump all of the models and textures referenced by each container

Usage: `DumpTool.exe dumpmeshprops <index file> <forge file> <uid>`

> Example: `DumpTool.exe dumpallmeshprops "index.db" "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge" 123412341234`

See the `index` command for help on creating search indexes.

### `dumpallmeshprops`

Dumps all flat archives which contain MeshProperties containers

Usage: `DumpTool.exe dumpallmeshprops <index file> <forge file>`

> Example: `DumpTool.exe dumpallmeshprops "index.db" "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge"`

See the `index` command for help on creating search indexes.

**This command is slow.**

### `findallmeshprops`

Searches all of the MeshProperties containers in the given forge for ones that contain the given UID

Usage: `DumpTool.exe findallmeshprops <forge file> <uid>`

> Example: `DumpTool.exe findallmeshprops "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge" 123412341234`

**This command is slow.**

### `findallmeshpropsglobal`

Searches all of the MeshProperties containers in all of the forges in the given folder for ones that contain the given UID

Usage: `DumpTool.exe findallmeshpropsglobal <forge directory> <uid>`

> Example: `DumpTool.exe findallmeshpropsglobal "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege" 123412341234`

**This command is slow.**

### `index`

Create a search index of all of the forge files in a given directory. Required for some commands.

Usage: `DumpTool.exe index <directory of forge files> <output index filename>`

> Example: `DumpTool.exe index "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege" "index.db"`

The output index filename doesn't matter as long as you use the same one in commands that require it, although the `.db` extension is preferred. The produced databases are standard LiteDB databases and can be browsed or accessed using any standard LiteDB tools.

## Typical Usage Scenarios

All of these scenarios use filenames from Y6S1 (v15500403), but the process should be relevant to any recent version.

#### UID primer

Every mesh asset, texture asset, texture mipmap set container, metadata container, etc. has their on unique identifier, or UID, which is a very large number. These UIDs generally don't change between versions of the game so it's safe to use them to refer to specific assets.

### Get information on an asset

Let's say I have a texture file UID, `241888865002`, and know that it's contained within `datapc64_merged_bnk_textures3.forge`. We can get information about it using `inspect`:

```
DumpTool.exe inspect "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_merged_bnk_textures3.forge" 241888865002
```

which tells us some basic information about the file and where it's located within the forge:

```
UID: 241888865002
Offset: 0x75540000
End: 0x7559E892
Size: 0x5E892
Name Table:
        File Magic: Texture
        Timestamp: 1615470721
Container: Forge Asset
Has Metadata Block: True
```

### Dump one asset

Let's say I have a texture file UID, `241888865002`, and know that it's contained within `datapc64_merged_bnk_textures3.forge`. We can dump it using `dump`:

```
DumpTool.exe dump "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_merged_bnk_textures3.forge" 241888865002
```

It'll dump the asset into a file of similar name which also generally includes some relevant information, like asset type IDs.

There are also ways to dump _all_ assets in a forge, if that's your kind of thing: `dumpall`, which doesn't take a UID but otherwise has an identical command.

### Dump all related assets

Let's say I have a texture file which I'd like to find the accompanying assets for, `241888865002`. This texture happens to be the Dokkaebi elite tablet diffuse texture.

#### 1. Create an index of all of the assets

First, you need to create a search index of all of the assets and their UIDs so that commands that search for assets don't need to re-crawl every file. This is slow but you'll **only need to do it once** (and re-do it every time the game updates), because commands can re-use the same index. From now on you can start at Step 2.

We'll use the `index` command:

```
DumpTool.exe index "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege" "index.db"
```

The output filename, `index.db` can be whatever you want. I find it helpful to name it with the game version, like `index_v15500403.db`, so I can keep assets and indexes for multiple game versions. Whatever you name it, be sure to use the same name in later commands, because I'm going to refer to it as `index.db`.

The command will output some database mapping information that isn't really relevant but lets you know that it's working.

#### 2. Find all references to that texture in the assets

We'll find the metadata containers which reference that texture using `findallmeshpropsglobal`:

```
DumpTool.exe findallmeshpropsglobal "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege" 241888865002
```

This command will take some time, because it searches every asset file for references to the given UID, which is limited by disk speed.

The command output:
```
datapc64_ondemand.forge: 261653128116
datapc64_ondemand.forge: 261653128117
datapc64_ondemand.forge: 273514556351
datapc64_ondemand.forge: 273514558171
datapc64_ondemand.forge: 285448834278
```
which tells us that `datapc64_ondemand.forge` contains 5 different metadata containers which reference that texture, and their UIDs are given.

#### 3. Dump all of the assets in the metadata container

I'll pick one of the containers from the previous command's output at random to move on with, the process is still the same. You'll likely want to dump _all_ of them, so just repeat this step for each.

We can dump all of the assets referenced by the metadata container using `dumpmeshprops`. The file that contains the container and the UID, which are needed in this command, are both output by the previous command. It also references the database we made earlier.

```
DumpTool.exe dumpmeshprops "index.db" "X:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege\datapc64_ondemand.forge" 273514558171
```

This will output a bunch of stuff letting you know it's working, but the output generally isn't relevant -- it's just showing you all of the assets it discovered that are relevant.

It'll dump all of your assets into a folder called, in this case, `model_flatarchive_id273514558171`.

Volia!
