using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Parser.FlowParser
{
    public class FlowSettings
    {

        public Action<IServiceCollection> ServiceConfiguration { get; set; }

        public bool FailOnUnknownAction { get; set; } = true;

        public List<string> IgnoreActions { get; set; } = new List<string>();

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