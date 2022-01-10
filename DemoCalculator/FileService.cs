using System;
using System.IO;

namespace CalculatorNS
{
    public class FileService : IProduct
    {
        private const string _enterFilePath = "Enter file path: ";

        public void WriteCalculatedNumber()
        {
            Console.Write(_enterFilePath);
            WriteCalculatedNumber(Console.ReadLine());
        }

        public void WriteCalculatedNumber(string path)
        {
            int fileNameIndex = path.LastIndexOf('/') + 1; 
            string writePath = "Calculated " + path[fileNameIndex..];

            using StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            string line;

            File.WriteAllText(writePath, string.Empty);

            while ((line = sr.ReadLine()) != null)
            {
                using StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default);
                try
                {
                    sw.WriteLine($"{line} = {new Calculator(line).CalculatedNumber}");
                }
                catch (Exception e)
                {
                    sw.WriteLine(line == "" ? "" : $"{line} = {e.Message}");
                }
            }

            Console.WriteLine($"Expressions in {path} file are calculated.");
        }
    }
}