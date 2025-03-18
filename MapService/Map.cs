using System;
using System.IO;

namespace MapService
{
    public class Map<T> 
    {
        public T[,] MapArray { get; set; } = new T[200, 200];

        public Map(string filePath, Func<string, T> parser)
        {
            LoadMapFromCSV(filePath, parser);
        }

        private void LoadMapFromCSV(string filePath, Func<string, T> parser)
        {
           string[] lines = File.ReadAllLines(filePath);
           for (int i = 0; i < 200; i++)
           {
               string[] values = lines[i].Split(',');
               for (int j = 0; j < 200; j++)
               {
                       MapArray[i, j] = parser(values[j]);

               }
           }
        }
    }
}