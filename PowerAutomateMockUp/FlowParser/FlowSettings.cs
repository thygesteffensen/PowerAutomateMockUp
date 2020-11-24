using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Parser.FlowParser
{
    public class FlowSettings
    {
        public FlowSettings()
        {
            IgnoreActions = new List<string>();
        }

        public bool FailOnUnknownAction { get; set; } = true;

        public List<string> IgnoreActions { get; set; }

        public Stream GetAsJsonStream()
        {
            var m = new MemoryStream();
            var w = new StreamWriter(m);
            using var writer = new JsonTextWriter(w);

            var serializer = new JsonSerializer();

            serializer.Serialize(writer, this);
            writer.Flush();
            m.Position = 0;
            return m;
        }
    }
}