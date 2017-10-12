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
        static string baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        //Sound variables
        static string[] soundFileExtensionsToReplace = { ".wav", ".mp3" };
        static string soundInputFolder = baseDirectory + "/AssetRandomizerFiles/Sounds/Input/";
        static string soundTempFolder = baseDirectory + "/AssetRandomizerFiles/Sounds/Temp/";
        static string soundOutputFolder = baseDirectory + "/AssetRandomizerFiles/Sounds/Output/";
        static int soundSmallFileThreshold = 70000;
        static int soundMediumFileThreshold = 700000;
        //sounds need to be seperated by size or game can refuse to load
        static List<string> soundSmallFiles = new List<string>();
        static List<string> soundMediumFiles = new List<string>();
        static List<string> soundLargeFiles = new List<string>();

        static int numberOfSounds = 0;

        static string soundModInputFolderPath = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/INPUT/";
        static string soundModOutputFolderPath = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/OUTPUT/";
        static string soundModBatchFile = baseDirectory + "/AssetRandomizerFiles/MUSIC_MOD/DSSI.bat";

        //Texture variables
        static string[] textureFileExtensionsToReplace = { ".png", ".dds", ".jpg", ".tga" };
        static string textureInputFolder = baseDirectory + "/AssetRandomizerFiles/Textures/Input/";
        static string textureTempFolder = baseDirectory + "/AssetRandomizerFiles/Textures/Temp/";
        static string textureOutputFolder = baseDirectory + "/AssetRandomizerFiles/Textures/Output/";
        static string[] uiTextures = {  "d39537ab.tga",
                                        "db8a58fa.tga",
                                        "f9d8db89.tga",
                                        "6b0e84c1.tga",
                                        "e3e2582d.tga"
                                    };

        static List<string> textureFiles = new List<string>();

        static int numberOfTextures = 0;

        static string mainSoundFileInputLocation = baseDirectory + "/AssetRandomizerFiles/Sounds/Input/fsb.frpg_main/";
        static string mainSoundFileOutputLocation = baseDirectory + "/AssetRandomizerFiles/Sounds/Output/frpg_main.fsb";

        static int minMainSoundFileSize = 5500000;
        static int maxMainSoundFileSize = 8400000;

        static void Main(string[] args)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1 - Randomize Sounds and Textures");
            Console.WriteLine("2 - Randomize Sounds");
            Console.WriteLine("3 - Randomize Textures");
            Console.WriteLine("4 - Redo Main Sound File (troubleshooting)");
            Console.WriteLine("5 - Use Extra files for ALL sounds");
            Console.WriteLine("6 - Use Extra files for ALL textures");
            string selection = Console.ReadLine();

            bool randomizeUiTextures = false;
            
            if (selection == "1" || selection == "3" || selection == "6")
            {
                Console.WriteLine("Do you want to randomize the UI textures? If you do, you wont be able to read a damn thing. (Y/N)");
                string userInput = Console.ReadLine();
                if (userInput.ToUpper().StartsWith("Y"))
                {
                    randomizeUiTextures = true;
                }
            }           

            if (selection == "1")
            {
                EmptyTempFolders();
                RandomizeSound();
                FixMainSoundFile();
                RandomizeTextures(randomizeUiTextures);
            }
            else if (selection == "2")
            {
                EmptyTempFolders();
                RandomizeSound();
                FixMainSoundFile();
            }
            else if (selection == "3")
            {
                EmptyTempFolders();
                RandomizeTextures(randomizeUiTextures);
            }
            else if (selection == "4")
            {
                EmptyTempFolders();
                FixMainSoundFile();
            }
            else if (selection == "5")
            {
                EmptyTempFolders();
                ReplaceAllSoundsWithExtra();
            }
            else if (selection == "6")
            {
                EmptyTempFolders();
                ReplaceAllTexturesWithExtra(randomizeUiTextures);
            }
            else
            {
                return;
            }

            //cleanup
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
        }

        static void EmptyTempFolders()
        {
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

                //clear sound inserter input folder, can cause problems if it stalls and doesnt clear itself
                foreach (var item in Directory.GetFiles(soundModInputFolderPath))
                {
                    if (item != "fsblist.lst")
                    {
                        File.Delete(item);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Could not clear Output folders.");
                Console.WriteLine("Manually empty the Output and Temp folders in /AssetRandomizerFiles/Sounds/ and /AssetRandomizerFiles/Textures/, as well as the INPUT folder in /AssetRandomizerFiles/MUSIC_MOD/ and try again.");
                Console.ReadLine();
                return;
            }
        }

        static void RandomizeSound()
        {
            Console.WriteLine("Looking at sound files.");

            //Check that sound folders exist in case people are using the download that doesnt come with sound files
            if (Directory.GetDirectories(soundInputFolder).Count() == 0)
            {
                Console.WriteLine("No sound files detected! Did you download the file directly from GitHub? Check the instructions on GitHub for the download that contains the sound files.");
                Console.ReadLine();
                return;
            }

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

                            File.Copy(file, soundTempFolder + soundSmallFiles[i], true);
                            soundSmallFiles.RemoveAt(i);
                        }
                        else if (length < soundMediumFileThreshold)
                        {
                            int i = r.Next(soundMediumFiles.Count);
                            File.Copy(file, soundTempFolder + soundMediumFiles[i], true);
                            soundMediumFiles.RemoveAt(i);
                        }
                        else
                        {
                            //Not a sound file, copy as is to output folder
                            int i = r.Next(soundLargeFiles.Count);
                            File.Copy(file, soundTempFolder + soundLargeFiles[i], true);
                            soundLargeFiles.RemoveAt(i);
                        }
                    }
                    else
                    {
                        File.Copy(file, file.Replace(soundInputFolder, soundTempFolder), true);
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
                File.Copy(file, file.Replace(soundModOutputFolderPath, soundOutputFolder), true);
                File.Delete(file);
            }
        }

        static void RandomizeTextures(bool randomizeUiTextures)
        {
            Console.WriteLine("Looking at texture files.");

            //Build a list of file names
            foreach (var folder in Directory.GetDirectories(textureInputFolder))
            {
                Directory.CreateDirectory(folder.Replace(textureInputFolder, textureTempFolder));

                foreach (var file in Directory.GetFiles(folder))
                {
                    string fileName = Path.GetFileName(file);

                    if (textureFileExtensionsToReplace.Any(fileName.EndsWith) && (randomizeUiTextures || !uiTextures.Contains(fileName)))
                    {
                        //copy other file to folder
                        textureFiles.Add(file.Replace(textureInputFolder, ""));
                    }
                }
            }

            numberOfTextures = textureFiles.Count;
            int counter = 1;

            foreach (var folder in Directory.GetDirectories(textureInputFolder))
            {
                var files = Directory.GetFiles(folder);

                foreach (var file in files)
                {
                    //Check if we want to copy the ui files
                    if (!randomizeUiTextures && uiTextures.Contains(Path.GetFileName(file)))
                    {
                        File.Copy(file, file.Replace(textureInputFolder, textureTempFolder), true);
                    }
                    else if (textureFileExtensionsToReplace.Any(file.EndsWith))
                    {
                        Console.WriteLine("Randomizing texture " + counter + " of " + numberOfTextures + ".");
                        counter++;
                        Random r = new Random();
                        int i = r.Next(textureFiles.Count);

                        File.Copy(file, textureTempFolder + textureFiles[i], true);
                        textureFiles.RemoveAt(i);
                    }
                }
            }

            Console.WriteLine("Copying to Output folder.");
            foreach (var file in Directory.GetFiles(textureTempFolder + "Textures"))
            {
                File.Copy(file, textureOutputFolder + Path.GetFileName(file), true);                
            }
        }

        static void FixMainSoundFile()
        {
            Console.WriteLine("Checking main sound file.");

            bool isMainFileValid = false;

            if (File.Exists(mainSoundFileOutputLocation))
            {
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

            while (!isMainFileValid)
            {
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

                            //Decrease lower file size limit for main file due to the number of sounds
                            if (length < (soundSmallFileThreshold - 5000))
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
                        Random r = new Random();
                        int i = r.Next(soundSmallFiles.Count);

                        File.Copy(soundInputFolder + soundSmallFiles[i], soundModInputFolderPath + fileName, true);
                        soundSmallFiles.RemoveAt(i);
                    }
                    else
                    {
                        File.Copy(file, soundModInputFolderPath + fileName, true);
                    }
                }

                Console.WriteLine("Running batch file");

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
                foreach (var item in Directory.GetFiles(soundModInputFolderPath))
                {
                    if (item != "fsblist.lst")
                    {
                        File.Delete(item);
                    }
                }

                //Check file size is valid
                long mainFileSize = new FileInfo(mainSoundFileOutputLocation.Replace(soundOutputFolder, soundModOutputFolderPath)).Length;
                if (mainFileSize > minMainSoundFileSize && mainFileSize < maxMainSoundFileSize)
                {
                    isMainFileValid = true;
                }
                else
                {
                    Console.WriteLine("Main sound file is invalid, re-randomizing.");
                }
            }

            //Move files from sound mod output directory and delete them
            foreach (var file in Directory.GetFiles(soundModOutputFolderPath))
            {
                File.Copy(file, file.Replace(soundModOutputFolderPath, soundOutputFolder), true);
                File.Delete(file);
            }

            Console.WriteLine("Main sound file OK.");
        }

        static void ReplaceAllSoundsWithExtra()
        {
            //Get list of replacing files
            string[] replacingFiles = Directory.GetFiles(soundInputFolder + "_Extra/").Where(a => soundFileExtensionsToReplace.Any(a.EndsWith)).ToArray();
            int counter = 1;
            Random r = new Random();

            foreach (var folder in Directory.GetDirectories(soundInputFolder))
            {
                if (!folder.EndsWith("_Extra"))
                {
                    var files = Directory.GetFiles(folder);

                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);

                        //Check this is a sound file
                        if (soundFileExtensionsToReplace.Any(fileName.EndsWith))
                        {
                            string replacingFile = replacingFiles[r.Next(replacingFiles.Length)];
                            //copy random file to folder
                            File.Copy(replacingFile, soundModInputFolderPath + fileName, true);
                        }
                        else
                        {
                            File.Copy(file, soundModInputFolderPath + fileName, true);
                        }
                    }

                    //Run batch file
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
                    System.Threading.Thread.Sleep(5000);

                    Console.WriteLine("Completed file " + counter + " of " + (Directory.GetDirectories(soundInputFolder).Count() -1));
                    counter++;

                    //clear files from mod input folder
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);

                        if (File.Exists(soundModInputFolderPath + fileName))
                        {
                            File.Delete(soundModInputFolderPath + fileName);
                        }
                    }
                }
            }

            Console.WriteLine("Copying files to Output folder.");

            //Move files from sound mod output directory and delete them
            foreach (var file in Directory.GetFiles(soundModOutputFolderPath))
            {
                File.Copy(file, file.Replace(soundModOutputFolderPath, soundOutputFolder), true);
                File.Delete(file);
            }

            long length = new FileInfo(mainSoundFileOutputLocation).Length;

            if (length > maxMainSoundFileSize || length < minMainSoundFileSize)
            {
                Console.WriteLine();
                Console.WriteLine("Warning: frpg_main.fsb is larger than the game can support and will probably cause the game to not load.");
                Console.WriteLine();
                Console.WriteLine("Do you want to copy this file to the Output folder anyway? (Y/N)");

                string userInput = Console.ReadLine();

                if (userInput.ToUpper().StartsWith("N"))
                {
                    File.Delete(mainSoundFileOutputLocation);
                }
                else
                {
                    Console.WriteLine("Try deleting the frpg_main.fsb file if your game fails to load.");
                }
            }

        }

        static void ReplaceAllTexturesWithExtra(bool randomizeUiTextures)
        {
            //Get list of replacing files
            string[] replacingFiles = Directory.GetFiles(textureInputFolder + "_Extra/").Where(a => textureFileExtensionsToReplace.Any(a.EndsWith)).ToArray();
            int counter = 1;
            Random r = new Random();

            foreach (var file in Directory.GetFiles(textureInputFolder + "Textures/"))
            {
                string fileName = Path.GetFileName(file);

                if (!randomizeUiTextures && uiTextures.Contains(Path.GetFileName(file)))
                {
                    File.Copy(file, textureOutputFolder + fileName, true);
                }
                if (textureFileExtensionsToReplace.Any(fileName.EndsWith))
                {
                    string replacingFile = replacingFiles[r.Next(replacingFiles.Length)];
                    //copy random file to folder
                    File.Copy(replacingFile, textureOutputFolder + fileName, true);
                }
                else
                {
                    File.Copy(file, textureOutputFolder + fileName, true);
                }
            }

            Console.WriteLine("Completed file " + counter + " of " + Directory.GetFiles(textureInputFolder + "Textures/").Count());
            counter++;
        }

    }
}