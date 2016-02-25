using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sulakore.Habbo.Web
{
    public class HGameData
    {
        private static readonly Regex _variableGrabber;
        private readonly Dictionary<string, string> _variables;

        public string InfoHost => _variables["connection.info.host"];
        public string InfoPort => _variables["connection.info.port"];

        public string this[string key]
        {
            get { return _variables[key]; }
        }

        static HGameData()
        {
            _variableGrabber = new Regex("\"(?<key>.*)\"(:| :|: | : )\"(?<value>.*)\"");
        }
        public HGameData(string source)
        {
            MatchCollection matches = _variableGrabber.Matches(source);
            _variables = new Dictionary<string, string>(matches.Count);

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