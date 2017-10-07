# Dark Souls Asset Randomizer

This tool includes the Sound Inserter mod by /u/RavagerChris37 found [here](http://www.nexusmods.com/darksouls/mods/1193).

What is it?
* This tool will randomize all of the sound and texture files of Dark Souls, with the option to include your own in the mix. Inspired by the (now defunct?) Source Engine Randomizer mod.

What do I need?
* An unpacked Dark Souls installation. You can use [this handy tool](https://github.com/HotPocketRemix/UnpackDarkSoulsForModding) by HotPocketRemix to accomplish this.
* DSfix for the texture replacer. Someday I may look into repacking the textures into the game files to go around DSfix, but for now this requires DSfix.

Instructions:
* Download and extract the mod somewhere you can find it.
* Unpack your installation of Dark Souls.
* If you want to include your own sounds or textures in the randomizer, add your textures to the '/DarkSoulsAssetRandomizer/AssetRandomizerFiles/Textures/Input/_Extra' and your sounds to the '/DarkSoulsAssetRandomizer/AssetRandomizerFiles/Sounds/Input/_Extra' folder.
* Unpacked sounds and textures can't be included in this repo due to size constraints, but a copy with these files can be found [here](https://drive.google.com/file/d/0B5Z4vOoakC78TkliRi15NWFpcVk/view?usp=sharing).
* Run the DarkSoulsAssetRandomizer.exe. This may take a while to complete. Nothing I can do, sorry ¯\_(ツ)_/¯
* When the tool has finished, copy the .bnd files from /AssetRandomizerFiles/Sounds/Output/ to the sound folder in your Dark Souls/DATA folder.
* Copy the textures from /AssetRandomizerFiles/Textures/Output/ to your DSfix tex_override folder.

Complete!

NOTE: Due to some restrictions with the game, this mod will only swap sound files with other files of a similar size. If the bnd file sizes are drastically different than vanilla, the game will not load. This can occasionally happen as is, if it happens you can try running the randomizer again.