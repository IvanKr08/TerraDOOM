# TerraDOOM
<sup>*Tweak dependencies before building*</sup>
![2037-155](https://github.com/user-attachments/assets/8da62dc6-83bd-4c8e-803d-2915a8104aef)
A slightly successful April Fool's joke from the "why not?" category.

## Features
1. Real-time gameplay
2. 4-bit 101x60 color display, HUD
3. Input (no strafe support)
4. Music
5. Competable (probably)
6. No tModLoader/Resource packs used
7. Swappable WADs

## How?
Technically, TerraDOOM is a chimera of [Managed Doom](https://github.com/sinshu/managed-doom) and Terraria. Terraria handles Doom update loop every frame, and Doom writes its screen buffer to the Terraria's world. The original version worked on top of decompiled Terraria. This one has a few differences and simply includes Terraria as a dependency, launching it from itself, having previously hooked into Main.OnPreDraw. This made it possible to increase stability, remove the need to explicitly get into the Terraria code and publish the port. The license is inherited from Managed Doom.

## Installation
Place all the TerraDOOM files in the folder with the Terraria installation, and you also need a WAD file from the desired game. You can get them from [Archive.org](https://archive.org/) or [Old-DOS.ru](http://old-dos.ru/). The release includes a WAD from the shareware version of Doom 1.

It is advisable to enable frame skipping in the settings and change the lighting to "trippy/special". With frame skipping disabled, Doom will run too fast, and with colored lighting it will run too slowly (unless you have an R9 9950X, but there may be another reason)

~~Launch the game via TerraDOOM Injector.exe...~~

No, it won't work. The problem is that the Steam version of Terarria has a built-in check that it is launched via Steam, and if it fails, it closes itself and forces Steam to open Terraria on its own. And since the patch is applied in runtime, restarting the game via Steam removes it. Workaround - add this to launch options in Steam settings<sup>(keeping quotes and %command%, but without brackets)</sup>
> "[REPLACE WITH FULL PATH TO THE TERRARIA DIR]\TerraDOOM Injector.exe" %command%

This makes Steam launch the patcher, and Terraria understands that it is running through Steam. If you have a version from GOG or a "client-oriented store with the letter "T", then you can simply run TerraDOOM Injector.exe. In these versions, checks are disabled.

(Theoretically, this may not be necessary if you have Steam open. Otherwise, the instructions are above in the text)

In addition, the patch also moves the saves to the game folder, in "TerraDOOM Saves". This is done to avoid accidentally corrupting the saves as a result of an error, plus I can immediately distribute the world without having to manually move it

There was also an idea to transfer all the logic to a dedicated server, which would allow you to create a server with Doom that works with vanilla clients and does not require any additional actions (and this would solve the problem with the license check), but this requires sorting out the netcode and serious optimizations. The naive version broke the sync of the server with the client and yielded about 10 FPS.

## TODO
- [ ] Bring the code to a decent state
- [x] ~~Find the cause of constant OOM when saving, as well as when loading medium and large worlds.~~ Terraria 1.4 is now compiled in AnyCPU, and trying to load it from an x86 assembly forcibly limits it to x86, and without LAA, thereby leading to idiotic restrictions on the size of objects. Fixed by moving the patcher to AnyCPU
- [ ] Version for servers (theoretically possible) + release for tModLoader
- [ ] Improve the input system
- [ ] More music. Perhaps a resource pack with original music and a filled texture of lamps for the screen
- [ ] Fix the screen palette (take it from FastDoom for EGA)
