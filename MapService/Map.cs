using System;
using System.IO;

namespace MapService
{
    public class Map<T>
    {
        public T[,] MapArray { get; set; } = new T[200, 200];

        public const string filePath = "map.csv"; // Ensure a valid file path is given

        public Func<string, T> ConvertFunction { get; set; }

        public Map(Func<string, T> convertFunction)
        {
            ConvertFunction = convertFunction;
            
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                LoadMapFromCSV(filePath);
            }
        }

        private void LoadMapFromCSV(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < Math.Min(200, lines.Length); i++)
            {
                string[] values = lines[i].Split(',');
                for (int j = 0; j < Math.Min(200, values.Length); j++)
                {

                        MapArray[i, j] = ConvertFunction(values[j]);

                }
            }
        }
    }
}
