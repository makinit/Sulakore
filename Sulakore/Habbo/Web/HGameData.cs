using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sulakore.Habbo.Web
{
    /* Spaghetti code galore up in here. */
    // TODO: Re-do all of this, again, someday, within the next decade, hopefully.
    public class HGameData : Dictionary<string, string>
    {
        private static readonly string _clientLoaderFormat;
        private static readonly string[] _flashValuesSeparator;
        private static readonly Regex _flashVarsSingleLineMatcher;

        public string Host
        {
            get { return this["connection.info.host"]; }
            set { this["connection.info.host"] = value; }
        }
        public string Port
        {
            get { return this["connection.info.port"]; }
            set { this["connection.info.port"] = value; }
        }

        public string BaseUrl { get; set; }
        public string UniqueId { get; set; }
        public string MovieUrl { get; set; }
        public string MovieName { get; set; }

        static HGameData()
        {
            _flashValuesSeparator = new string[] { "&amp;" };

            _flashVarsSingleLineMatcher = new Regex("<param name=\"flashVars\" value=\"(?<value>.*?)\"",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            var clientLoaderBuilder = new StringBuilder(860);
            clientLoaderBuilder.AppendLine("<html>");
            clientLoaderBuilder.AppendLine("<head>");
            clientLoaderBuilder.AppendLine("<meta http-equiv=\"Expires\" content=\"-1\"/>");
            clientLoaderBuilder.AppendLine("<meta http-equiv=\"Pragma\" content=\"no-cache, no-store\"/>");
            clientLoaderBuilder.AppendLine("<meta http-equiv=\"Cache-Control\" content=\"no-cache, no-store\"/>");
            clientLoaderBuilder.AppendLine("</head>");

            clientLoaderBuilder.AppendLine("<body class=\"flashclient\" style=\"margin: 0;\">");
            clientLoaderBuilder.AppendLine("<div id=\"flash-wrapper\">");
            clientLoaderBuilder.AppendLine("<object id=\"flash-container\" width=\"100%\" height=\"100%\">");
            clientLoaderBuilder.AppendLine("<param name=\"movie\" value=\"{1}\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"base\" value=\"{0}\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"allowScriptAccess\" value=\"always\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"menu\" value=\"false\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"flashvars\" value=\"{2}\"/>");

            clientLoaderBuilder.AppendLine("<!--[if !IE]>-->");
            clientLoaderBuilder.AppendLine("<object type=\"application/x-shockwave-flash\" data=\"{1}\" width=\"100%\" height=\"100%\">");
            clientLoaderBuilder.AppendLine("<param name=\"movie\" value=\"{1}\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"base\" value=\"{0}\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"allowScriptAccess\" value=\"always\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"menu\" value=\"false\"/>");
            clientLoaderBuilder.AppendLine("<param name=\"flashvars\" value=\"{2}\"/>");
            clientLoaderBuilder.AppendLine("</object>");
            clientLoaderBuilder.AppendLine("<!--<![endif]-->");

            clientLoaderBuilder.AppendLine("</object>");
            clientLoaderBuilder.AppendLine("</div>");
            clientLoaderBuilder.AppendLine("</body>");
            clientLoaderBuilder.AppendLine("</html>");

            _clientLoaderFormat = clientLoaderBuilder.ToString();
        }
        public HGameData(string source)
        {
            this["connection.info.host"] = string.Empty;
            this["connection.info.port"] = string.Empty;

            Match flashVarsMatch =
                _flashVarsSingleLineMatcher.Match(source);

            bool isSwfObject = false;
            if (isSwfObject = !flashVarsMatch.Success)
            {
                flashVarsMatch =
                    GetVariable(source, "flashvars");
            }

            string variables =
                flashVarsMatch.Groups["value"].Value;

            try
            {
                string movieUrl = string.Empty;
                if (!isSwfObject)
                {
                    string dataParam = "data=\"";
                    string movieParam = "<param name=\"movie\" value=\"";

                    movieUrl = (source.GetChild((source.Contains(
                        movieParam) ? movieParam : dataParam), '"'));

                    movieUrl = $"\"{movieUrl}\"";
                }
                else movieUrl = source.GetChild(".embedSWF(", ',');

                char charBegin = movieUrl[0];
                bool isFieldName = (charBegin != '\"' && charBegin != '\'');

                if (isSwfObject)
                {
                    Match paramsMatch = GetVariable(source, "params");
                    if (paramsMatch.Success)
                    {
                        string flashParams = paramsMatch.Groups["value"].Value
                            .GetChild("{", '}').Replace(" :", ":").Replace(": ", ":").Trim();

                        BaseUrl = FixUrlString(flashParams.GetChild("\"base\":\"", '"'));
                    }

                    if (isFieldName)
                    {
                        movieUrl = GetVariable(source, movieUrl).Groups["value"].Value;
                        charBegin = movieUrl[0];
                    }
                }

                if (!string.IsNullOrWhiteSpace(movieUrl))
                {
                    movieUrl = movieUrl.GetChild(charBegin.ToString(), charBegin);
                    MovieUrl = FixUrlString(movieUrl);

                    string[] segments = MovieUrl.GetChild("//")
                        .GetChild("/").Split('/');

                    int buildSegmentIndex = segments.Length > 1 ?
                        segments.Length - 2 : 0;

                    MovieName = segments[buildSegmentIndex];

                    if (buildSegmentIndex == 0)
                        MovieName = MovieName.GetParent(".swf");
                }
            }
            catch { }

            if (isSwfObject)
            {
                variables = variables.GetChild("{", '}')
                    .Replace(" :", ":").Replace(": ", ":").Trim();

                int previousLength = 0;
                string parsedVariables = string.Empty;
                while (variables.Contains("\""))
                {
                    string name = variables.GetChild("\"", '"');
                    string child = variables.GetChild($"\"{name}\":").Trim();

                    string value = string.Empty;
                    string fieldName = string.Empty;
                    bool isField = (child[0] != '"' && child[0] != '\'');
                    bool isEmpty = (child.Length > 1 && (child[1] == '"' || child[1] == '\''));

                    if (isField)
                    {
                        fieldName = child.GetParent(",");

                        Match variableMatch =
                            GetVariable(source, fieldName);

                        string fieldValue = variableMatch.Groups["value"].Value;
                        isEmpty = string.IsNullOrWhiteSpace(fieldValue);

                        if (!isEmpty)
                        {
                            child = child.Replace(fieldName,
                                variableMatch.Groups["value"].Value);
                        }
                    }

                    if (!isEmpty)
                        value = child.GetChild("\"", '"');

                    string toRemove = string.Format("\"{0}\":{1}{2}{1}",
                        name, (isField ? string.Empty : "\""), (isField ? fieldName : value));

                    variables = variables.Replace(toRemove, string.Empty);
                    parsedVariables += $"{name}={value}&amp;";
                    previousLength = variables.Length;
                }
                variables = parsedVariables;
            }

            if (!variables.Contains("&amp;"))
                variables = variables.Replace("&", "&amp;");

            string[] valuePairs = variables
                .Split(_flashValuesSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in valuePairs)
            {
                int separatorIndex = pair.IndexOf('=');
                string name = pair.Substring(0, separatorIndex);
                string value = pair.Substring(separatorIndex + 1);

                value = FixUrlString(value);
                this[name] = value;
            }
        }

        protected virtual string FixUrlString(string value)
        {
            if (value.StartsWith("\\/"))
                value = value.Replace("\\/", "/");

            return value;
        }
        protected virtual Match GetVariable(string source, string name)
        {
            var variableMatcher = new Regex($"var {name}=(?<value>.*?);",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            source = source.Replace($"{name} = ", $"{name}=")
                .Replace($"{name} =", $"{name}=").Replace($"{name}= ", $"{name}=");

            return variableMatcher.Match(source);
        }

        public override string ToString()
        {
            string flashvars = string.Empty;
            foreach (string key in Keys)
            {
                flashvars += $"{key}={this[key]}&amp;";
            }

            return string.Format(_clientLoaderFormat,
                BaseUrl, MovieUrl.Replace("http:", string.Empty), flashvars);
        }
    }
}