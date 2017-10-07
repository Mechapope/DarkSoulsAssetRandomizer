using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsAssetRandomizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //Sound variables
            string[] soundFileExtensionsToReplace = { ".wav", ".mp3" };
            string soundInputFolder = baseDirectory + "/AssetRandomizerFiles/Sounds/Input/";
            string soundTempFolder = baseDirectory + "/AssetRandomizerFiles/Sounds/Temp/";
            string soundOutputFolder = baseDirectory + "/AssetRandomizerFiles/Sounds/Output/";
            int soundSmallFileThreshold = 70000;
            int soundMediumFileThreshold = 700000;
            //sounds need to be seperated by size or game can refuse to load
            List<string> soundSmallFiles = new List<string>();
            List<string> soundMediumFiles = new List<string>();
            List<string> soundLargeFiles = new List<string>();

            int numberOfSounds = 0;

            string soundModInputFolderPath = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/INPUT/";
            string soundModOutputFolderPath = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/OUTPUT/";
            string soundModBatchFile = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/DSSI.bat";

            //Texture variables
            string[] textureFileExtensionsToReplace = { ".png", ".dds", ".jpg", ".tga" };
            string textureInputFolder = baseDirectory + "/AssetRandomizerFiles/Textures/Input/";
            string textureTempFolder = baseDirectory + "/AssetRandomizerFiles/Textures/Temp/";
            string textureOutputFolder = baseDirectory + "/AssetRandomizerFiles/Textures/Output/";
            List<string> textureFiles = new List<string>();

            int numberOfTextures = 0;

            string mainSoundFileInputLocation = baseDirectory + "/AssetRandomizerFiles/Sounds/Input/fsb.frpg_main/";
            string mainSoundFileOutputLocation = baseDirectory + "/AssetRandomizerFiles/Sounds/Output/frpg_main.fsb";

            int minMainSoundFileSize = 5500000;
            int maxMainSoundFileSize = 8200000;

            Console.WriteLine("Clearing Output folders.");

            try
            {
                if (Directory.Exists(soundTempFolder))
                {
                    Directory.Delete(soundTempFolder, true);
                }

                if (Directory.Exists(soundOutputFolder))
                {
                    Directory.Delete(soundOutputFolder, true);
                }

                if (Directory.Exists(textureTempFolder))
                {
                    Directory.Delete(textureTempFolder, true);
                }

                if (Directory.Exists(textureOutputFolder))
                {
                    Directory.Delete(textureOutputFolder, true);
                }

                if (!Directory.Exists(soundOutputFolder))
                {
                    Directory.CreateDirectory(soundOutputFolder);
                }

                if (!Directory.Exists(textureOutputFolder))
                {
                    Directory.CreateDirectory(textureOutputFolder);
                }
            }
            catch
            {
                Console.WriteLine("Could not clear Output folders.");
                Console.WriteLine("Manually delete the Output and Temp folders in /AssetRandomizerFiles/Sounds/ and /AssetRandomizerFiles/Textures/ and try again.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Looking at sound files.");
            //Build a list of all sound files
            foreach (var folder in Directory.GetDirectories(soundInputFolder))
            {
                //Create directory in temp folder
                Directory.CreateDirectory(folder.Replace(soundInputFolder, soundTempFolder));

                foreach (var file in Directory.GetFiles(folder))
                {
                    string fileName = Path.GetFileName(file);
                    string thisFolder = folder.Replace(soundInputFolder, "");

                    //Check this is a sound file
                    if (soundFileExtensionsToReplace.Any(fileName.EndsWith) && !fileName.Contains("blank"))
                    {
                        long length = new FileInfo(file).Length;

                        if (length < soundSmallFileThreshold)
                        {
                            soundSmallFiles.Add(thisFolder + "/" + fileName);
                        }
                        else if (length < soundMediumFileThreshold)
                        {
                            soundMediumFiles.Add(thisFolder + "/" + fileName);
                        }
                        else
                        {
                            soundLargeFiles.Add(thisFolder + "/" + fileName);
                        }
                    }
                }
            }

            numberOfSounds = soundSmallFiles.Count + soundMediumFiles.Count + soundLargeFiles.Count;

            int counter = 1;

            //Copy files, change names
            foreach (var folder in Directory.GetDirectories(soundInputFolder))
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    string fileName = Path.GetFileName(file);

                    //Check this is a sound file
                    if (soundFileExtensionsToReplace.Any(fileName.EndsWith) && !fileName.Contains("blank"))
                    {
                        Console.WriteLine("Randomizing sound " + counter + " of " + numberOfSounds + ".");
                        counter++;

                        //Pick a random sound file name of the right size and copy
                        long length = new FileInfo(file).Length;
                        Random r = new Random();

                        if (length < soundSmallFileThreshold)
                        {
                            int i = r.Next(soundSmallFiles.Count);

                            File.Copy(file, soundTempFolder + soundSmallFiles[i]);
                            soundSmallFiles.RemoveAt(i);
                        }
                        else if (length < soundMediumFileThreshold)
                        {
                            int i = r.Next(soundMediumFiles.Count);
                            File.Copy(file, soundTempFolder + soundMediumFiles[i]);
                            soundMediumFiles.RemoveAt(i);
                        }
                        else
                        {
                            //Not a sound file, copy as is to output folder
                            int i = r.Next(soundLargeFiles.Count);
                            File.Copy(file, soundTempFolder + soundLargeFiles[i]);
                            soundLargeFiles.RemoveAt(i);
                        }
                    }
                    else
                    {
                        File.Copy(file, file.Replace(soundInputFolder, soundTempFolder));
                    }
                }
            }

            //Delete the _extra folder from temp directory, as it will cause errors in the batch file and it isn't needed anymore anyways
            if (Directory.Exists(soundTempFolder + "_Extra"))
            {
                Directory.Delete(soundTempFolder + "_Extra", true);
            }

            counter = 1;

            Console.WriteLine("Running batch file");
            foreach (var folder in Directory.GetDirectories(soundTempFolder))
            {
                var files = Directory.GetFiles(folder);

                foreach (var file in Directory.GetFiles(folder))
                {
                    string fileName = Path.GetFileName(file);

                    File.Copy(file, soundModInputFolderPath + fileName, true);
                }

                //Run the sound inserter batch file
                System.Threading.Thread.Sleep(2000);
                var processInfo = new ProcessStartInfo(soundModBatchFile);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.WorkingDirectory = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/";
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                var process = Process.Start(processInfo);

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("output>>" + e.Data);
                process.BeginOutputReadLine();

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("error>>" + e.Data);
                process.BeginErrorReadLine();

                process.WaitForExit();

                Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();

                Console.WriteLine("Completed file " + counter + " of " + Directory.GetDirectories(soundTempFolder).Count());
                counter++;

                //wait a short time or else batch file will give an error sometimes
                System.Threading.Thread.Sleep(5000);

                //clear files from input folder for next
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);

                    if (File.Exists(soundModInputFolderPath + fileName))
                    {
                        File.Delete(soundModInputFolderPath + fileName);
                    }
                }
            }

            //Move files from sound mod output directory and delete them
            foreach (var file in Directory.GetFiles(soundModOutputFolderPath))
            {
                File.Copy(file, file.Replace(soundModOutputFolderPath, soundOutputFolder));
                File.Delete(file);
            }

            Console.WriteLine("Checking main sound file.");

            bool isMainFileValid = false;

            while (!isMainFileValid)
            {
                if (Directory.Exists(soundTempFolder))
                {
                    Directory.Delete(soundTempFolder, true);
                }

                Directory.CreateDirectory(mainSoundFileInputLocation.Replace(soundInputFolder, soundTempFolder));

                //Build a list of all sound files
                foreach (var folder in Directory.GetDirectories(soundInputFolder))
                {
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        string fileName = Path.GetFileName(file);
                        string thisFolder = folder.Replace(soundInputFolder, "");

                        //Check this is a sound file
                        if (soundFileExtensionsToReplace.Any(fileName.EndsWith) && !fileName.Contains("blank"))
                        {
                            long length = new FileInfo(file).Length;

                            if (length < soundSmallFileThreshold)
                            {
                                soundSmallFiles.Add(thisFolder + "/" + fileName);
                            }
                            else if (length < soundMediumFileThreshold)
                            {
                                soundMediumFiles.Add(thisFolder + "/" + fileName);
                            }
                            else
                            {
                                soundLargeFiles.Add(thisFolder + "/" + fileName);
                            }
                        }
                    }
                }

                foreach (var file in Directory.GetFiles(mainSoundFileInputLocation))
                {
                    string fileName = Path.GetFileName(file);

                    //Check this is a sound file
                    if (soundFileExtensionsToReplace.Any(fileName.EndsWith) && !fileName.Contains("blank"))
                    {
                        //Pick a random sound file name of the right size and copy
                        long length = new FileInfo(file).Length;
                        Random r = new Random();

                        int i = r.Next(soundSmallFiles.Count);

                        File.Copy(soundInputFolder + soundSmallFiles[i], file.Replace(soundInputFolder, soundTempFolder), true);
                        soundSmallFiles.RemoveAt(i);
                    }
                    else
                    {
                        File.Copy(file, file.Replace(soundInputFolder, soundTempFolder), true);
                    }
                }

                Console.WriteLine("Running batch file");
                foreach (var folder in Directory.GetDirectories(soundTempFolder))
                {
                    var files = Directory.GetFiles(folder);

                    foreach (var file in Directory.GetFiles(folder))
                    {
                        string fileName = Path.GetFileName(file);

                        File.Copy(file, soundModInputFolderPath + fileName, true);
                    }

                    //Run the sound inserter batch file
                    System.Threading.Thread.Sleep(2000);
                    var processInfo = new ProcessStartInfo(soundModBatchFile);
                    processInfo.CreateNoWindow = true;
                    processInfo.UseShellExecute = false;
                    processInfo.WorkingDirectory = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/";
                    processInfo.RedirectStandardError = true;
                    processInfo.RedirectStandardOutput = true;

                    var process = Process.Start(processInfo);

                    process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                        Console.WriteLine("output>>" + e.Data);
                    process.BeginOutputReadLine();

                    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                        Console.WriteLine("error>>" + e.Data);
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    Console.WriteLine("ExitCode: {0}", process.ExitCode);
                    process.Close();

                    //wait a short time or else batch file will give an error sometimes
                    System.Threading.Thread.Sleep(5000);

                    //clear files from input folder for next
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);

                        if (File.Exists(soundModInputFolderPath + fileName))
                        {
                            File.Delete(soundModInputFolderPath + fileName);
                        }
                    }
                }

                //Move files from sound mod output directory and delete them
                foreach (var file in Directory.GetFiles(soundModOutputFolderPath))
                {
                    File.Copy(file, file.Replace(soundModOutputFolderPath, soundOutputFolder), true);
                    File.Delete(file);
                }

                long mainFileSize = new FileInfo(mainSoundFileOutputLocation).Length;
                if (mainFileSize > minMainSoundFileSize && mainFileSize < maxMainSoundFileSize)
                {
                    isMainFileValid = true;
                }
                else
                {
                    Console.WriteLine("Main sound file is invalid, re-randomizing.");
                }
            }

            Console.WriteLine("Main sound file OK.");

            //Texture randomizer
            Console.WriteLine("Looking at texture files.");

            //Build a list of file names
            foreach (var folder in Directory.GetDirectories(textureInputFolder))
            {
                Directory.CreateDirectory(folder.Replace(textureInputFolder, textureTempFolder));

                foreach (var file in Directory.GetFiles(folder))
                {
                    //var fileName = file.Substring();
                    string fileName = Path.GetFileName(file);

                    if (textureFileExtensionsToReplace.Any(fileName.EndsWith))
                    {
                        //copy other file to folder
                        textureFiles.Add(file.Replace(textureInputFolder, ""));
                    }
                }
            }

            numberOfTextures = textureFiles.Count;
            counter = 1;

            foreach (var folder in Directory.GetDirectories(textureInputFolder))
            {
                var files = Directory.GetFiles(folder);

                foreach (var file in files)
                {
                    if (textureFileExtensionsToReplace.Any(file.EndsWith))
                    {
                        Console.WriteLine("Randomizing texture " + counter + " of " + numberOfTextures + ".");
                        counter++;
                        Random r = new Random();
                        int i = r.Next(textureFiles.Count);

                        File.Copy(file, textureTempFolder + textureFiles[i]);
                        textureFiles.RemoveAt(i);
                    }
                }
            }

            foreach (var file in Directory.GetFiles(textureTempFolder + "Textures"))
            {
                if (!File.Exists(textureOutputFolder + Path.GetFileNameWithoutExtension(file) + ".png"))
                {
                    File.Copy(file, textureOutputFolder + Path.GetFileNameWithoutExtension(file) + ".png");
                }
            }

            try
            {
                //Delete temp folders
                if (Directory.Exists(soundTempFolder))
                {
                    Directory.Delete(soundTempFolder, true);
                }

                if (Directory.Exists(textureTempFolder))
                {
                    Directory.Delete(textureTempFolder, true);
                }
            }
            catch { }

            //Add some empty lines so the complete message stands out more
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Randomizing complete!");
            Console.ReadLine();

            //I should probably get rid of this before I release this lol
            Console.WriteLine("hi every1 im new!!!!!!! holds up spork my name is katy but u can call me t3h PeNgU1N oF d00m!!!!!!!! lol…as u can see im very random!!!! thats why i came here, 2 meet random ppl like me _… im 13 years old (im mature 4 my age tho!!) i like 2 watch invader zim w/ my girlfreind (im bi if u dont like it deal w/it) its our favorite tv show!!! bcuz its SOOOO random!!!! shes random 2 of course but i want 2 meet more random ppl =) like they say the more the merrier!!!! lol…neways i hope 2 make alot of freinds here so give me lots of commentses!!!! DOOOOOMMMM!!!!!!!!!!!!!!!! <--- me bein random again _^ hehe…toodles!!!!!");
            Console.WriteLine("love and waffles,");
            Console.WriteLine("t3h PeNgU1N oF d00m");
            Console.ReadLine();
        }
    }
}