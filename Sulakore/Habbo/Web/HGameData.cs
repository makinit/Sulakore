/* Copyright

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

namespace Sulakore.Habbo.Web
{
    public class HGameData
    {
        public int Port { get; private set; }
        public string Host { get; private set; }
        public string Texts { get; private set; }
        public int AccountId { get; private set; }
        public string UniqueId { get; private set; }
        public string Variables { get; private set; }
        public string OverrideTexts { get; private set; }
        public string ClientStarting { get; private set; }
        public string FlashClientUrl { get; private set; }
        public string FigurePartList { get; private set; }
        public string FlashClientBuild { get; private set; }
        public string FurniDataLoadUrl { get; private set; }
        public string OverrideVariables { get; private set; }
        public string ProductDataLoadUrl { get; private set; }

        public HGameData(string gameData)
        {
            string formattedGameData = gameData.Replace("\\/", "/").Replace("\"//", "\"http://")
                .Replace("'//", "'http://").Replace("\r\n", string.Empty).Replace("\t", string.Empty);

            string flashVars = formattedGameData.GetChild("flashvars", '}')
                .GetChild("{").Replace("\" : ", "\":").Trim();

            if (string.IsNullOrWhiteSpace(flashVars) &&
                formattedGameData.Contains("connection.info.host"))
            {
                // SWFOBject
                FlashClientUrl = ExtractFlashClientUrl(formattedGameData, "new SWFObject(", ',');
                while (formattedGameData.Contains(".addVariable"))
                {
                    string addVarArgs = formattedGameData.GetChild(".addVariable(", ')')
                        .Replace("\"", string.Empty).Replace(", ", ":");

                    string varName = addVarArgs.GetParent(":");
                    string varValue = addVarArgs.GetChild(":");

                    string lineToRemove = string.Format(".addVariable(\"{0}\"", varName);
                    formattedGameData = formattedGameData.Replace(lineToRemove, string.Empty);

                    if (!string.IsNullOrWhiteSpace(varValue))
                        flashVars += (addVarArgs + "\r");
                }
            }
            else if (!string.IsNullOrWhiteSpace(flashVars))
            {
                // embedSWF
                string tempFlashVars = string.Empty;
                while (flashVars.Contains("\":"))
                {
                    string varName = flashVars.GetChild("\"", '\"');
                    string flashArgParent = string.Format("\"{0}\":", varName);

                    string flashArgChild = flashVars.GetChild(flashArgParent);
                    bool isVariable = (flashArgChild[0] != '"');

                    string varValue = isVariable ?
                        flashArgChild.GetParent(",") : flashArgChild.GetChild("\"", '"');

                    string flashArg = string.Format("\"{0}\":{2}{1}{2}",
                        varName, varValue, !isVariable ? "\"" : string.Empty);

                    tempFlashVars += string.Format("{0}:{1}\r", varName, varValue);
                    flashVars = flashVars.Replace(flashArg, string.Empty);
                }
                flashVars = tempFlashVars;
            }

            string[] flashVarArgs = flashVars.Split(new[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string flashArg in flashVarArgs)
            {
                string varName = flashArg.GetParent(":").Trim();
                string varValue = flashArg.GetChild(":").Trim();
                #region Switch Statement: varName
                switch (varName)
                {
                    case "unique_habbo_id": UniqueId = varValue; break;
                    case "external.texts.txt": Texts = varValue; break;
                    case "connection.info.host": Host = varValue; break;
                    case "client.starting": ClientStarting = varValue; break;
                    case "account_id": AccountId = int.Parse(varValue); break;
                    case "external.variables.txt": Variables = varValue; break;
                    case "furnidata.load.url": FurniDataLoadUrl = varValue; break;
                    case "connection.info.port": Port = int.Parse(varValue.Split(',')[0]); break;
                    case "productdata.load.url": ProductDataLoadUrl = varValue; break;
                    case "external.override.texts.txt": OverrideTexts = varValue; break;
                    case "external.figurepartlist.txt": FigurePartList = varValue; break;
                    case "external.override.variables.txt": OverrideVariables = varValue; break;
                    case "flash.client.url":
                    {
                        if (string.IsNullOrWhiteSpace(FlashClientUrl))
                            FlashClientUrl = ExtractFlashClientUrl(formattedGameData, "embedSWF(", ',');

                        string[] segments = FlashClientUrl.GetChild("//")
                            .GetChild("/").Split('/');

                        int buildSegmentIndex = segments.Length > 1 ?
                            segments.Length - 2 : 0;

                        FlashClientBuild = segments[buildSegmentIndex];

                        if (buildSegmentIndex == 0)
                            FlashClientBuild = FlashClientBuild.GetParent(".swf");
                        break;
                    }
                }
                #endregion
            }
        }

        private string ExtractFlashClientUrl(string source, string parent, char delimiter)
        {
            if (!source.Contains(parent))
                return string.Empty;

            string possibleFlashClientUrl =
                source.GetChild(parent, delimiter);

            if (string.IsNullOrWhiteSpace(possibleFlashClientUrl))
                return string.Empty;

            char flashUrlStartChar =
                possibleFlashClientUrl[0];

            bool isVariable =
                (flashUrlStartChar != '"' && flashUrlStartChar != '\'');

            if (!isVariable)
            {
                possibleFlashClientUrl = possibleFlashClientUrl.Replace(
                    flashUrlStartChar.ToString(), string.Empty);
            }
            else
            {
                string suffix = string.Empty;
                if (possibleFlashClientUrl.Contains("+"))
                {
                    suffix = possibleFlashClientUrl.GetChild("\"", '"');
                    if (!string.IsNullOrWhiteSpace(suffix))
                    {
                        possibleFlashClientUrl =
                            possibleFlashClientUrl.GetParent("+").Trim();
                    }
                }
                possibleFlashClientUrl =
                    source.GetChild(possibleFlashClientUrl).GetChild("\"", '"');

                if (!string.IsNullOrWhiteSpace(suffix) &&
                    possibleFlashClientUrl.EndsWith("/") &&
                    suffix.StartsWith("/"))
                {
                    possibleFlashClientUrl +=
                        suffix.Substring(1);
                }
            }

            if (!string.IsNullOrWhiteSpace(possibleFlashClientUrl) &&
                possibleFlashClientUrl.Contains(".swf"))
            {
                possibleFlashClientUrl =
                    possibleFlashClientUrl.Split('?')[0];
            }

            return possibleFlashClientUrl.EndsWith(".swf") ?
                possibleFlashClientUrl : string.Empty;
        }
    }
}