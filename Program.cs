using LiveSplit.Model.Comparisons;
using LiveSplit.Model.RunFactories;
using System;
using System.IO;
using System.Linq;
using LiveSplit.Model;
using LiveSplit.Model.RunSavers;

namespace OoTLivesplitIntroPatcher
{
    internal class Program
    {
        private static TimeSpan introDurationTimeSpan = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(52);
        private static Time introDurationTime = new Time(TimingMethod.RealTime, introDurationTimeSpan);

        private static void Main(string splitPath = null, string outputPath = null)
        {
            try
            {
                if(string.IsNullOrEmpty(splitPath))
                {
                    Console.Error.WriteLine("--split-path argument must be provided");
                    return;
                }

                if(!File.Exists(splitPath))
                {
                    Console.Error.WriteLine("The path provided does not exist");
                    return;
                }

                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = $"{Path.GetDirectoryName(splitPath)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(splitPath)}_patched.lss";
                }

                Console.WriteLine($"Reading split file: {splitPath}");
                using (var filestream = File.Open(splitPath, FileMode.Open))
                {
                    IRun run = null;
                    try
                    {
                        var runFactory = new StandardFormatsRunFactory(filestream, splitPath);
                        var comparisonGeneratorsFactory = new StandardComparisonGeneratorsFactory();
                        run = runFactory.Create(comparisonGeneratorsFactory);
                    }
                    catch(Exception)
                    {
                        Console.Error.WriteLine("Unable to parse split file. Are you sure it's a liveplit file ?");
                        return;
                    }

                    Console.WriteLine("Computing new split file");

                    // Patch attempt history
                    run.AttemptHistory = run.AttemptHistory.Select(a => new Attempt()
                    {
                        Ended = a.Ended,
                        Index = a.Index,
                        PauseTime = a.PauseTime,
                        Time = a.Time - introDurationTime,
                        Started = new AtomicDateTime(a.Started.Value.Time + introDurationTimeSpan, false),
                    }).ToList();

                    // Patch segment
                    foreach(var segment in run)
                    {
                        if(segment == run.First())
                        {
                            segment.BestSegmentTime = new Time(segment.BestSegmentTime - introDurationTime);

                            foreach (var id in segment.SegmentHistory.Keys.ToArray())
                            {
                                segment.SegmentHistory[id] -= introDurationTime;
                            }
                        }

                        segment.PersonalBestSplitTime = new Time(segment.PersonalBestSplitTime - introDurationTime);
                    }

                    // Save the patched file
                    Console.WriteLine($"Saving patched file at path {outputPath}");
                    var runSaver = new XMLRunSaver();
                    using (var memoryStream = new MemoryStream())
                    {
                        runSaver.Save(run, memoryStream);

                        using (var stream = File.Open(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            var buffer = memoryStream.GetBuffer();
                            stream.Write(buffer, 0, (int)memoryStream.Length);
                        }
                    }

                    Console.WriteLine($"Done !");
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine($"Unexpected error: {e.Message}");
            }
        }
    }
}
