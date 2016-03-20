using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("InfoHost: {InfoHost}, InfoPort: {InfoPort}")]
    public class HGameData
    {
        private readonly Dictionary<string, string> _variables;

        public string Source { get; private set; }
        public string InfoHost => _variables["connection.info.host"];
        public string InfoPort => _variables["connection.info.port"];

        public string this[string key]
        {
            get { return _variables[key]; }
        }

        public HGameData()
        {
            _variables = new Dictionary<string, string>();
        }
        public HGameData(string source)
            : this()
        {
            Update(source);
        }

        public void Update(string source)
        {
            Source = source;
            _variables.Clear();

            MatchCollection matches = Regex.Matches(source,
                "\"(?<key>.*)\"(:| :|: | : )\"(?<value>.*)\"", RegexOptions.Singleline);

            _variables["connection.info.host"] = string.Empty;
            _variables["connection.info.port"] = string.Empty;
            foreach (Match pair in matches)
            {
                string key = pair.Groups["key"].Value;
                string value = pair.Groups["value"].Value;

                if (value.Contains("\\/"))
                    value = value.Replace("\\/", "/");

                _variables[key] = value;
            }
        }
    }
}