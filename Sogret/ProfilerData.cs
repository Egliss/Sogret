using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sogret
{
    public class CompileTimeInfoContainer
    {
        public string fileName { get; set; }
        public List<CompileTimeInfo> infos { get; set; } = new List<CompileTimeInfo>();
    }
    public class CompileTimeInfo
    {
        public int processId { get; set; } = -1;
        public int threadId { get; set; } = -1;
        public string phase { get; set; } = string.Empty;

        public string compileGroupName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public long beginMilliSeconds { get; set; } = -1;
        public long durationMilliSeconds { get; set; } = -1;

        // Node info
        public int parentInfoId { get; set; } = -1;
        public List<int> childInfoIds { get; set; } = new List<int>();

        public static List<CompileTimeInfo> ParseFromJsonString(ref string json)
        {
            var root = JObject.Parse(json);
            return root["traceEvents"].Select(m =>
            {
                var timeInfo = new CompileTimeInfo()
                {
                    processId = m.Value<int>("pid"),
                    threadId = m.Value<int>("tid"),
                    phase = m.Value<string>("ph"),
                    beginMilliSeconds = m.Value<long>("ts"),
                    durationMilliSeconds = m.Value<long>("dur"),
                    compileGroupName = m.Value<string>("name")
                };
                // optional args
                timeInfo.name = m
                    .SelectToken("args.detail")
                    ?.Value<string>()
                    .Replace("\\","/");

                // TODO add more optional args ...
                return timeInfo;
            }).ToList();
        }
    }

}
