using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WECOverlay
{
    public class Logger
    {
        private string path;

        public Logger(string _path)
        {
            this.path = _path;
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
        }

        public void Log(string title, string log)
        {
            

            using (StreamWriter w = File.AppendText(path))
            {
                WriteOutput(title, log, w);
            }

            using (StreamReader r = File.OpenText(path))
            {
                DumpLog(r);
            }
        }

        private void WriteOutput(string title, string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine($"  :{title}");
            w.WriteLine("  :");
            w.WriteLine($"  {logMessage}");
            w.WriteLine("-------------------------------");
        }

        private void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
