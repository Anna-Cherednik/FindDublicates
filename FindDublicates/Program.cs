using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDublicates
{
    class Program
    {
        static void Main(string[] args)
        {
            DuplicatesFinder dublicates = new DuplicatesFinder();

            Console.Write("Command 'exit' or directory> ");
            string command = Console.ReadLine();

            while (command.ToLower() != "exit")
            {
                // Ignoring invalid input
                if (!new DirectoryInfo(command).Exists)
                {
                    Console.WriteLine("Path does not exist!");
                }
                else
                {
                    Console.WriteLine("Waiting...");
                    var start = DateTime.Now;
                    var dubFiles = dublicates.Find(command);
                    int countRecords = dubFiles.Count;

                    // Getting name of report file
                    string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string filepath = string.Format("dublicates {0}.txt", start.ToString("yyyy.MM.dd-HH.mm.ss"));
                    filepath = Path.Combine(mydocpath, filepath);

                    dublicates.WriteReportToStream(filepath);

                    // if input is too much, write dublicates only to report file
                    if (countRecords > 50 )
                        Console.WriteLine("Report of finding dublicates write to file {0}", filepath);
                    // write dublicates to console
                    else
                    {
                        foreach (var hash in dubFiles)
                        {
                            if (hash.Value.Count > 1)
                            {
                                foreach (var dubfile in hash.Value)
                                {
                                    Console.WriteLine("{0}: {1}", dubfile);
                                }
                                Console.WriteLine();
                            }
                        }
                        Console.WriteLine("Report of finding duplicates also write to file {0}", filepath);
                    }

                    // Show if we get some error looking for dublicates in directory
                    if (dublicates.ErrorDuringLookingForDuplicates > 0)
                        Console.WriteLine("{0} missed directory by reason of an access violation", 
                                                                          dublicates.ErrorDuringLookingForDuplicates);

                    var finish = DateTime.Now;
                    Console.WriteLine("Executing time: {0}", finish.Subtract(start));
                }

                Console.Write("\nCommand 'exit' or directory> ");
                command = Console.ReadLine();

            }
        }
    }
}
