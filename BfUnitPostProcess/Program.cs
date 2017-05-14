using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BfUnitPostProcess
{
    class Program
    {
        public static string DatFolder;
        public static string CdnUrl;

        // args[0] = dat files
        // args[1] = cdn url
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Bad syntax. BfUnitPostProcess <dat folder> <cdn url>");
                return;
            }
            DatFolder = args[0];
            CdnUrl = args[1];
            
            //Animation.Run();
        }
    }
}
