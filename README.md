# Custom Data Manager

A [Torch](https://torchapi.com/) plugin for Space Engineers that bulk edits block Custom Data outside the game.
**Export** writes the Custom Data of every matching block to a file named after the block, and **Import** reads those
files back and updates every block whose name matches a file.

## Settings

- **Storage location** — the folder the files are written to and read from.
- **Type ID filter** — only touch blocks with this exact TypeId (e.g. `MyObjectBuilder_TextPanel`). Empty means all.
- **Subtype ID filter** — the same, matched against the block's SubtypeId.
- **Line filter (regex)** — lines matching this regex are left out of the exported files.
- **Force restart** — turn each block off and back on after importing, so it picks up the new data.

Several blocks may share a name; export keeps the last one, import updates all of them.
