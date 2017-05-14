using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BfUnitPostProcess
{
    public class AnimationParser
    {
        private static ConcurrentDictionary<string, dynamic> AnimationInfo; 

        public static void Run()
        {
            ServicePointManager.DefaultConnectionLimit = 256;
            AnimationInfo = new ConcurrentDictionary<string, dynamic>();
            ParseCgsMst();

            File.WriteAllText(Path.Combine(Program.DatFolder, "animationpreprocess.json"), JsonConvert.SerializeObject(AnimationInfo));
        }

        private static void ParseUnitCgs(string id, string rawData)
        {
            Console.WriteLine($"Parsing {id}");
            var infoDict = new ConcurrentDictionary<string, dynamic>();
            if (rawData.Contains(".sam"))
            {
                infoDict.TryAdd("error", "sam file not supported");
            }
            else
            {
                var parts = rawData.Split('|');
                Parallel.ForEach(parts, (rawAnim) =>
                {
                    var tempParts = rawAnim.Split(':');
                    var type = tempParts[0];
                    var file = tempParts[1];
                    if (type == "1" || type == "2" || type == "3")
                    {
                        var url = Program.CdnUrl + "/unit/cgs/" + file;
                        try
                        {
                            using (var wc = new WebClient {Proxy = null})
                            {
                                var rawCgsFile = wc.DownloadString(url);
                                Console.WriteLine($"Got {url}");
                                var cgsLines = rawCgsFile.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                                var frames =
                                    cgsLines.Select(line => line.Split(','))
                                        .Where(frameParts => frameParts.Length >= 2)
                                        .Sum(frameParts => int.Parse(frameParts[3]) + 1);
                                infoDict.TryAdd(GetAnimationType(type), new Dictionary<string, int>
                                {
                                    {"total number of frames", frames}
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to get {url}, {ex.Message}");
                            infoDict.TryAdd(GetAnimationType(type), "missing data");
                        }
                    }
                });
            }
            AnimationInfo.TryAdd(id, infoDict);
        }

        private static void ParseCgsMst()
        {
            var mstFile = FindNewestCgsFile();
            if (File.Exists(mstFile))
            {
                var rawData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(File.ReadAllText(mstFile));
                Parallel.ForEach(rawData, (dict) =>
                {
                    ParseUnitCgs(dict["pn16CNah"], dict["Kn51uR4Y"]);
                });
            }
        }

        private static string FindNewestCgsFile()
        {
            var directory = new DirectoryInfo(Program.DatFolder);
            var latestFile = directory.GetFiles("*6rjmY7tV*").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (latestFile != null) return latestFile.FullName;
            return "";
        }

        private static string GetAnimationType(string type)
        {
            switch (type)
            {
                case "1":
                    return "idle";
                case "2":
                    return "move";
                case "3":
                    return "attack";
                case "4":
                    return "altattack";
                default:
                    return $"unknown ({type})";
            }
        }
    }
}
