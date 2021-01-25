using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVis
{
    class Program
    {
        static void Main(string[] args)
        {
            string startingPoint = "C:\\Myfiles";

            var visitor = new FileSystemVisitor(startingPoint, (info) => true);

            visitor.Start += (s, e) => { Console.WriteLine("Start iteration"); };

            visitor.Finish += (s, e) => { Console.WriteLine("Finish iteration"); };

            visitor.FileFinded += (s, e) => { Console.WriteLine("Finded file: " + e.FoundItem.Name); };

            visitor.FilteredFileFinded += (s, e) => { Console.WriteLine("Finded filtered file: " + e.FoundItem.Name); };

            visitor.DirectoryFinded += (s, e) => { Console.WriteLine("Finded directory: " + e.FoundItem.Name); };

            visitor.FilteredDirectoryFinded += (s, e) => { Console.WriteLine("Finded filtered directory: " + e.FoundItem.Name); };

            foreach (var fileSysInfo in visitor.GetFileInfoSequence())
            {
                Console.WriteLine(fileSysInfo);
                Console.ReadKey();
            }
        }
    }
}
