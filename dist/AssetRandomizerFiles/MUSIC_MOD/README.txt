Dark Souls Soundfile Inserter (DSSI V1.1.3)
-------------------------------------------

DSSI is a batch file, which runs the commands needed to swap out in-game sound effects & music contained in .fsb files with custom sound effects & music. Can be used to insert custom soundfiles into Dark Souls. Dark Souls soundfiles' real extension inside the .fsb is .mp3, NOT ".wav"! It's just part of the filename (and thus needed for the swap). 

Requirements:
-------------

To mod in-game sounds, you'll need to do a bunch of things first:
-Download Aezay's FSB extractor, to check which soundfiles inside the .fsb you want to swap.
&
-Extract the dvdbnd*.bdt libraries in your DATA folder.
-Hex Edit your DARKSOULS.exe to load files from outside the libraries.

Wulf2k does a great job explaining the latter two in his video, check it out:
https://www.youtube.com/watch?v=qaPKqLcqGmY
0:00-4:30 is what you'll need to do.

How to make it work:
--------------------

Video tutorial of this for Dark Souls https://www.youtube.com/watch?v=ci96ezZAIYk

1.) Insert the .fsb file you want to mod, into \INPUT\ (back it up first)
2.) Insert your .mp3 files into \INPUT\
3.) Make sure you rename your .mp3s to the in-game one's you want to swap out. Load your .fsb into Aezay's FSB extractor to check these names (don't forget to write .wav between the name and the .mp3 extension, for example: "m150100000.wav.mp3")
4.) Run DSSI.bat
5.) After DSSI finished, the modded .fsb file can be found in \OUTPUT\
6.) Place your modded .fsb file into ...\Dark Souls Prepare to Die Edition\DATA\sound\
7.) Enjoy your custom music playing in the game!

DSSI is a work in progress, I'm working on creating a faster and a more user friendly version of this.

CAUTION!
--------

-Only place one .fsb file into \INPUT\!
-Be sure you have the .mp3 names correct! Otherwise the modded .fsb will be corrupt!
-Don't put anything into \OUTPUT\, it may screw things up!
-That said: any .fsb files left in \OUTPUT\ will get deleted once DSSI is launched! Make sure you copied your modified .fsb into ...\DATA\sound\ before re-runnig DSSI!

Changelog:
----------

V1.1
-Hex editing no longer required. DSSI automatically sets the LoopStart value to 0, and LoopEnd value to default (end of the sound).
-The pitch of each inserted file gets corrected.
-Improved efficiency: DSSI reads data directly from the original & dummy .fsb, and no longer needs to extract them.
-fsbext.exe omitted. Replaced with inserter.exe.
-Sadly Dark Souls II .fsbs are no longer supported. This is due to how differently they are built up. Will fix this in an update.

V1.1.1
-Ninja fixed one of the error triggers.

V1.1.2
-Fixed an error where if an mp3's filename was also found in another mp3's name (like as "ghost.wav.mp3" and "blood-ghost.wav.mp3") then the one containing the former mp3's name would be swapped instead of the correct one.
-Detected an issue when inserting multiple mp3s. The fix for this issue will arrive shortly. Until then, it is advised to insert files one by one.

V1.1.3
-Fixed a data reading issue, where inserter.exe would read 32 bytes less from some mp3 files inside the dummy fsb file, causing the newly created fsb file to be corrupt.
-Fixed the multi-insert issue, where some mp3 files would be left out. This was caused by the order of the mp3 files inside of the dummy .fsb to be different than those inside the original fsb file.
-Added listrearranger.exe to solve the latter problem.

Notes:
------

Feel free to PM me on reddit (/u/ravagerchris37) if you'd want me to make a mod for you.

McJ0hns0n (Frenzowski) requested a custom "Dark Souls menu sounds mod" from me. If it wasn't for this request, I wouldn't have started working on this project (yet).