using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace str2csv
{
    class Program
    {
        static void Main(string[] args)
        {
            string versionNumber = "1.0.0-beta1";
            Console.WriteLine("Str2csv version " + versionNumber);

            string stripfile = "";
            string csvfile = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-str"))
                {
                    stripfile = args[i + 1];
                    i += 1;
                }
                else if (args[i].Equals("-csv"))
                {
                    csvfile = args[i + 1];
                    i += 1;
                }
            }

            if (stripfile != "" && csvfile != "")
            {
                var str2xls = new ReadAndWriteToXls(stripfile, csvfile);
                
            }
            else
            {
                Console.WriteLine("No files specified");
            }
                
        }
    }


    /// <summary>
    /// Class that reads a RELAP5 strip file and outputs it as an excel file
    /// </summary>
    class ReadAndWriteToXls
    {
        private List<string> Plotalfs = new List<string>();
        private List<string> Plotnums = new List<string>();
        private List<List<double>> Plotrecs = new List<List<double>>();

        public ReadAndWriteToXls(string StripFile, string CSVFile)
        {
            ReadStrFile(StripFile);
            WriteCSV(CSVFile);
        }

        /// <summary>
        /// Reads file and stores data in Plotalfs, Plotnums and Plotrecs
        /// </summary>
        /// <param name="Filename"></param>
        private void ReadStrFile(string Filename)
        {
            Console.WriteLine("Reading file: " + Filename);
            int currentField = -1;
            var currentPlotrec = new List<double>();
            var plotrecCount = 0;
            //Array.Resize(Plotrecs,)

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(Filename))
                {
                    // Read the stream to a string, and write the string to the console.
                    string line = String.Empty;
                    var word1 = "";
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        char[] seps = { ' ' };
                        string[] inputs = line.Trim().Split(seps, StringSplitOptions.RemoveEmptyEntries);
                        word1 = inputs[0];

                        // If trigger word read - set what fields are read at present
                        if (word1.Equals("plotinf"))
                            currentField = 0;
                        else if (word1.Equals("plotalf"))
                            currentField = 1;
                        else if (word1.Equals("plotnum"))
                            currentField = 2;
                        else if (word1.Equals("plotrec"))
                        {
                            currentField = 3;
                            if (plotrecCount > 0)
                                Plotrecs.Add(currentPlotrec);
                            plotrecCount += 1;
                            currentPlotrec = new List<double>();
                        }

                        // Add read words to appropriate storage
                        if (currentField == 1)   // plotalfs
                        {
                            for (int i = 0; i < inputs.Length; i++)
                            {
                                if (Plotalfs.Count == 0 && i == 0) continue;  // Disregard first word 'plotalf'
                                Plotalfs.Add(inputs[i]);
                            }

                        }
                        else if (currentField == 2)  // plotnums
                        {
                            for (int i = 0; i < inputs.Length; i++)
                            {
                                if (Plotnums.Count == 0 && i == 0) continue;  // Disregard first word 'plotnum'
                                Plotnums.Add(inputs[i]);
                            }
                        }
                        else if (currentField == 3)  // plotrecs
                        {
                            for (int i = 0; i < inputs.Length; i++)
                            {
                                if (currentPlotrec.Count == 0 && i == 0) continue;    // If first plotrec, disregard first word 'plotrec'

                                double newDbl;
                                if (!Double.TryParse(inputs[i], out newDbl))
                                    newDbl = -1;

                                currentPlotrec.Add(newDbl);
                            }
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
            
        /// <summary>
        /// Writes read data to CSV file
        /// </summary>
        /// <param name="CSVFile"></param>
        private void WriteCSV(string CSVFile)
        {
            Console.WriteLine("Writing to file: " + CSVFile);

            var nLines = 0;
            var nLinesMax = 3000;

            FileStream f = new FileStream(CSVFile, FileMode.Create);
            StreamWriter s = new StreamWriter(f);

            var fileOutput = new StringBuilder();

            foreach (var nw in Plotalfs.Zip(Plotnums, Tuple.Create))
            {
                fileOutput.Append(nw.Item1 + "-" + nw.Item2 + ";");

            }
            fileOutput.AppendLine("");
            nLines += 1;

            foreach (List<double> plotrec in Plotrecs)
            {
                fileOutput.AppendLine(String.Join(";", plotrec));
                nLines += 1;

                // Flush to file if max line count
                if (nLines > nLinesMax)
                {
                    nLines = 0;
                    s.WriteLine(fileOutput.ToString());
                    fileOutput = new StringBuilder();
                }
            }

            // Flush to file
            s.WriteLine(fileOutput.ToString());

            s.Close();
            f.Close();
            Console.WriteLine("File created successfully...");
        }
    }
}


