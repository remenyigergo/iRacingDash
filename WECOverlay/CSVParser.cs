using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WECOverlay
{
    class CSVParser
    {
        public SortedList<int,int> Parse(string csvPath, int trackId)
        {
            SortedList<int,int> turns = new SortedList<int,int>();
            var lines = File.ReadAllLines(csvPath);

            foreach (var track in lines)
            {
                var values = track.Split(';');

                var isItValidId = Int32.TryParse(values[0], out int ID);

                if (isItValidId && trackId == Int32.Parse(values[0]))
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        turns.Add(i,Int32.Parse(values[i]));
                    }
                    return turns;
                }
            }
            return null;
        }
    }
}
