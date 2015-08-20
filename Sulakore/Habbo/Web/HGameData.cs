﻿/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sulakore.Habbo.Web
{
    public class HGameData : Dictionary<string, string>
    {
        private static readonly string _clientLoaderFormat;
        private static readonly string[] _flashValuesSeparator;
        private static readonly Regex _flashVarsSingleLineMatcher;

        public string Host
        {
            get
            {
                if (!ContainsKey("connection.info.host"))
                    Add("connection.info.host", "");

                return this["connection.info.host"];
            }
            set { this["connection.info.host"] = value; }
        }
        public string Port
        {
            get
            {
                if (!ContainsKey("connection.info.port"))
                    Add("connection.info.port", "0");

                return this["connection.info.port"];
            }
            set { this["connection.info.port"] = value; }
        }

        public string BaseUrl { get; set; }
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
                string movieUrl = isSwfObject ?
                    source.GetChild(".embedSWF(", ',') :
                    source.GetChild("<param name=\"movie\" value=\"", '"');

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
                    {
                        MovieName = MovieName.GetParent(".swf");
                    }
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