# DiskUsage

An imitation of the disk usage tool found in Linux/Unix operating systems, designed to tell the user what files and folders are using the space on the system. 
While possible to run on the entirety of the disk, it is more common to run it in a specific directory. 

The [.Net]() framework is used, so this program will run equally well on Linux/Mac/Windows. 

## Usage 

After cloning the repository, use the following command to run the program with the following command

`dotnet run [-s] [-p] [-b] <path>`

 - Pass in -s to perform Sequential parsing of files 
 - Pass -p to parse through the files in parallel using the `System.Threading` library 
 - Pass in -b to do both 
 
The number of folders, files and directories along with the time taken for computation is displayed 
