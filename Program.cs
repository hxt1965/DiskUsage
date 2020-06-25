// Name: Harsh Tagotra 
// Username: hxt1965
// Date: 2/7/2020

/*
 * Notes:
 * To run this program with the parallel argument, use the following command 
 * dotnet run -- -p "C:/"
 * The double hyphen ensures that dotnet does not identify the path mentioned as the path of the project
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace du
{
    /// <summary>
    /// Main Program class instantiates all objects necessary for directory search 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var arr = new string[3] { "-p", "-s", "-b" };
            if (args.Length != 2)
            {
                Console.WriteLine("Not enough arguments provided!\n");
                Console.WriteLine(args.Length);
                ShowHelp();
                return;
            }

            var path = args[1];
            var param = args[0];
            var dirObj = new DiskLocation(path);

            if (!Directory.Exists(path))
            {
                Console.WriteLine("Invalid path provided!");
                ShowHelp();
                return;
            }

            //directory queue needed to be traversed through to collect information 
            List<DirectoryInfo> dirQueue = new List<DirectoryInfo>();
            dirQueue.Add(new DirectoryInfo(dirObj.getPath()));
            var timer = new Stopwatch();

            
            
            if (String.Equals(param, "-p"))
            {
                timer.Start();
                dirObj.RunParallel(dirQueue);
                timer.Stop();
                PrintMessage("\nParallel", dirObj.GetMessage(), timer.Elapsed.TotalSeconds.ToString());
            } 
            else if (String.Equals(param, "-s"))
            {
                timer.Start();
                dirObj.RunSequential(dirQueue);
                timer.Stop();
                PrintMessage("\nSequential", dirObj.GetMessage(), timer.Elapsed.TotalSeconds.ToString());
            } 
            else if (String.Equals(param, "-b"))
            {
                var dirObj2 = new DiskLocation(path);
                List<DirectoryInfo> dirQueue2 = new List<DirectoryInfo>();
                dirQueue2.Add(new DirectoryInfo(dirObj2.getPath()));

                timer.Start();
                dirObj.RunParallel(dirQueue);
                timer.Stop();
                PrintMessage("\nParallel", dirObj.GetMessage(), timer.Elapsed.TotalSeconds.ToString());

                timer.Start();
                dirObj2.RunSequential(dirQueue2);
                timer.Stop();
                PrintMessage("\nSequential", dirObj2.GetMessage(), timer.Elapsed.TotalSeconds.ToString());
            }
            else
            {
                Console.WriteLine("Invalid parameter provided!");
                ShowHelp();
            }
        }

        /// <summary>
        /// Generates Help instructions in case of invalid parameters
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Usage: du [-s] [-p] [-b] <path>");
            Console.WriteLine("where\n-s\tRun in single threaded mode\n-p\t" +
                "Run in parallel mode (uses all available processors)\n-b\tRun in " + 
                "both parallel and single threaded mode");
        }

        /// <summary>
        /// Formats Output string 
        /// </summary>
        /// <param name="type">Type of function run (sequential/parallel) </param>
        /// <param name="fileDetails"> string representation of folders, files and size</param>
        /// <param name="timer"> string representation of time taken to traverse through all directories </param>
        private static void PrintMessage(string type, string fileDetails, string timer)
        {
            Console.WriteLine("{0} calculated in: {1} s\n{2}", type, timer, fileDetails);
        }
    }

    /// <summary>
    /// The DiskLocation classes is given a starting path, and three variables to keep track of 
    /// all the files and folders contained in the respective sub directories 
    /// </summary>
    class DiskLocation
    {
        private string path;
        //total size in bytes 
        private long fileSize;
        //total number of files 
        private long noOfFiles ;
        //total number of folders/directories 
        private long noOfFolders;


        private object myLock = new object();

        public DiskLocation(string _path)
        {
            path = _path;
            this.fileSize = 0;
            this.noOfFiles = 0;
            this.noOfFolders = 0;
        }

        public string getPath()
        {
            return this.path;
        }

        /// <summary>
        /// 
        /// This method goes through the target directories, and adds all the sub-directories at the 
        /// end of the diretorry queue, so as to make sure to go through each one of them and collect 
        /// information on the contained files and folders 
        /// 
        /// </summary>
        /// <param name="dirQueue"> Directory Queue containing information about all directories inside
        /// target folder </param>
        public void RunSequential(List<DirectoryInfo> dirQueue)
        {
            //reference for goto statement 
            //if met with any exception, we want the program to start at the front of the queue 
            StartOver:
            try
            {
                foreach (DirectoryInfo currDir in dirQueue)
                {
                    this.noOfFolders++;


                    var files = currDir.GetFiles();
                    if (files.Length != 0)
                    {
                        foreach (var file in files)
                        {
                            fileSize += file.Length;
                            noOfFiles++;
                        }
                    }

                    // adding sub directories to the back of the queue
                    foreach (DirectoryInfo dir in currDir.GetDirectories())
                        dirQueue.Add(dir);
                  
                    //popping of target directory because we have collected all the information we need to
                    dirQueue.RemoveAt(0);
                    //no we can go to the beginning of the queue to search the next sub-directory, which now 
                    //becomes the new target directory
                    goto StartOver;
                }
            } catch (UnauthorizedAccessException )
            {
                dirQueue.RemoveAt(0);
                goto StartOver;
            }
            
         
        }

        /// <summary>
        /// This method recursively searches all directories and sub directories and collects information 
        /// about the contained files and folders 
        /// </summary>
        /// <param name="dirQueue"></param>
        public void RunParallel(List<DirectoryInfo> dirQueue)
        {
            
            Parallel.ForEach(dirQueue, currDir =>
            {
                try
                {
                    
                    lock (myLock)
                    {
                        this.noOfFolders++;
                        this.noOfFiles += currDir.GetFiles().Length;
                    }
                    Parallel.ForEach(currDir.GetFiles(), file => { this.fileSize += file.Length; });

                    // For every directory, a recursive call makes sure the program searches all sub-directories
                    // as well
                    if (currDir.GetDirectories().Length > 0)
                        RunParallel(currDir.GetDirectories().ToList());
                }
                catch (UnauthorizedAccessException)
                {
                    //left empty because we dont want to interrupt the output in any way 
                }
            });
           
        }

        /// <summary>
        /// Formats the details into a string 
        /// </summary>
        /// <returns> The string representation of the details of all the folders </returns>
        public string GetMessage()
        {
            string msg = String.Format("{0:n0} folders, {1:n0} files, {2:n0} bytes",
                this.noOfFolders, noOfFiles, fileSize);
            return msg;
        }

    }
}
