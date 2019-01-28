using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;


    public class CSVtype
    {
        public List<string> paraNames;
        public List<string> outNames;
        public List<List<double>> values;

        public CSVtype(List<string> paramNames, List <string> outputNames, List<List<double>> vals) 
        {
            paraNames = paramNames;
            outNames = outputNames;
            values = vals;              
        }
        
    
        
        public void writeCSV (string filepath)
        {

            List<string> header = new List<string>();

            foreach (string p in this.paraNames)
            {
                header.Add(p);
            }
            foreach (string o in this.outNames)
            {
                header.Add(o);
            }
        
            string[] lines = new string [this.values.Count+1];

            lines[0] = string.Join(",", header.ToArray());  // add csv header

            for (int row = 1; row < lines.Length; row++)

                {
                    // Create a string array with the lines of text
                    lines[row] = string.Join(",", this.values[row-1].ToArray());
                }

            // Set a variable to the Documents path.
            string docPath = filepath;
            //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "WriteLines.txt")))
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
            // function to write data to csv file stored in "filepath"
            
        }

    }

