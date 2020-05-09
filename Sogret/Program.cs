using ConsoleTables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sogret
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = Path.Combine(Environment.CurrentDirectory, args.Last());
            if (!Directory.Exists(directory))
                return;

            var files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .GroupBy(m =>
                {
                    var d = Path.GetExtension(m);
                    var f = m.Replace(d, "");
                    return f;
                })
                .Where(m => m.Count() >= 2 && m.Any(n => Path.GetExtension(n) == ".json"))
                .Select(m => m.Where(n => Path.GetExtension(n) == ".json").First())
                .AsParallel()
                .Select(m =>
                {
                    using (StreamReader reader = new StreamReader(m))
                    {
                        var json = reader.ReadToEnd();
                        var infos = CompileTimeInfo.ParseFromJsonString(ref json);
                        var profile = new CompileTimeInfoContainer()
                        {
                            fileName = m,
                            infos = infos
                        };
                        return profile;
                    }
                })
                .OrderBy(m => m.fileName)
                .ToList();
            Analyze(files);
        }

        static void Analyze(List<CompileTimeInfoContainer> container)
        {
            var ex = container
                .SelectMany(m => m.infos)
                .Where(m => m.compileGroupName == "Source")
                .GroupBy(m => m.name)
                .OrderByDescending(m => m.Aggregate(0.0f, (m, n) => m + n.durationMilliSeconds))
                .Select(m => { 
                    var file = Path.GetFileName(m.Key);
                    var totalSecounds = MathF.Ceiling(m.Aggregate(0.0f, (n, o) => n + o.durationMilliSeconds) / 10000) / 100;
                    return (file, m.Count(), totalSecounds);
                });

            var table = new ConsoleTable("File", "Count", "Secounds");
            var e = ex.Select(m => { table.AddRow(m.file, m.Item2, m.totalSecounds); return 0; }).ToList();
            table
                .Configure(m => m.EnableCount = true)
                .Configure(m => m.NumberAlignment = Alignment.Right)
                .Write(Format.Minimal);
            var totalExecute = MathF.Ceiling(container
                .SelectMany(m => m.infos)
                .Where(n => n.compileGroupName == "Total ExecuteCompiler")
                .Aggregate(0.0f, (o, p) => o + p.durationMilliSeconds) / 10000) / 100;

            Console.WriteLine($"Total ExecuteCompiler: {totalExecute}");

        }
    }
}
