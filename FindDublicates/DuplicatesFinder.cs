using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FindDublicates
{
    class DuplicatesFinder
    {
        // All dublicates of start directory
        Dictionary<string, List<string>> dublicates;
        // Errors when opening directory without access violation
        int countError;

        public int ErrorDuringLookingForDuplicates
        {
            get { return countError; }
            set { countError = value; }
        }

        public DuplicatesFinder()
        {
            dublicates = new Dictionary<string, List<string>>();
            countError = 0;
        }

        // Looking for dublicates in current directory
        public void GetFilesDuplicates(string path)
        {
            Dictionary<long, List<string>> filelist = new Dictionary<long, List<string>>();
            var dir = new DirectoryInfo(path);
            try
            {
                // Group all files by length of files
                filelist = dir.GetFiles("*.*", SearchOption.AllDirectories)
                              .AsParallel()
                              .GroupBy(t => t.Length)
                              .ToDictionary(y => y.Key, y => y.Select(f => f.FullName).ToList());

                // Looking for at files with same length files which has same hashes
                Parallel.ForEach(filelist.Where(it => it.Value.Count > 1), (currentgroup) =>
                {
                    Parallel.ForEach(currentgroup.Value, (filename) =>
                    {
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(filename))
                            {
                                byte[] checkSum = md5.ComputeHash(stream);
                                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                                if (dublicates.ContainsKey(result))
                                    lock (dublicates) { dublicates[result].Add(filename); }
                                else
                                    lock (dublicates) { dublicates.Add(result, new List<string> { filename }); }
                            }
                        }
                    });
                });

            }
            catch (Exception)
            {
                ErrorDuringLookingForDuplicates ++;
            }
        }

        // Looking for dublicates in current path
        public Dictionary<string, List<string>> Find(string path)
        {
            dublicates = new Dictionary<string, List<string>>();
            countError = 0;

            // if getting path to logical drive
            if (Path.GetDirectoryName(path) == null)
            {
                var drive = new DirectoryInfo(path);

                // Looking for dublicates in each directory of locical drive
                Parallel.ForEach(drive.GetDirectories(), (currentdir) =>
                {
                    GetFilesDuplicates(currentdir.FullName);
                });
            }
            else
                GetFilesDuplicates(path);

            return dublicates;
        }

        // Write dublicates to report file
        public void WriteReportToStream(string filepath)
        { 
            using (StreamWriter file = new StreamWriter(filepath))
            {
                foreach (var hash in dublicates)
                {
                    if (hash.Value.Count > 1)
                    {
                        foreach (var dubfile in hash.Value)
                        {
                            file.WriteLine("{0}: {1}", hash.Key, dubfile);
                        }
                        file.WriteLine();
                    }
                }
                if (ErrorDuringLookingForDuplicates > 0)
                    file.WriteLine("{0} missed directory by reason of an access violation", ErrorDuringLookingForDuplicates);
            }
        }

    }
}
