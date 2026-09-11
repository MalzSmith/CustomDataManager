# Custom Data Manager

A [Torch](https://torchapi.com/) plugin for Space Engineers that bulk edits block Custom Data outside the game.
**Export** writes the Custom Data of every matching block to a file named after the block, and **Import** reads those
files back and updates every block whose name matches a file.

The plugin is safe to add to and remove from your server at any point.

## Settings

- **Storage location** — the folder the files are written to and read from.
- **Type ID filter** — only touch blocks with this exact TypeId (e.g. `MyObjectBuilder_TextPanel`). Empty means all.
- **Subtype ID filter** — the same, matched against the block's SubtypeId.
- **Line filter (regex)** — lines matching this regex are left out of the exported files.
- **Force restart** — turn each block off and back on after importing, so it picks up the new data.

Several blocks may share a name; export keeps the last one, import updates all of them.

## How to build

Before building run the setup script or create a directory junction manually so that the referenced libraries can be found. After that click the build button in your IDE or run 
```
dotnet build
```
in the repo root. The output should be created as a `.zip` file in the `Build/` Directory

## What's the point of this thing?

On our server we have used this to set up [Simple Stores](https://steamcommunity.com/workshop/filedetails/?id=3243498681) remotely

## Disclaimer

The current implementation doesn't use batching, if you have a few hundred grids on your server it's probably going to freeze for a moment.
