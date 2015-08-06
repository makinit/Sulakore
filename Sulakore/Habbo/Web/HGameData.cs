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
        public int Port { get; set; }
        public string Host { get; set; }
        public string FlashClientBuild { get; set; }

        public string Texts { get; }
        public int AccountId { get; }
        public string UserName { get; }
        public string UniqueId { get; }
        public string Variables { get; }
        public string BannerUrl { get; }
        public string OverrideTexts { get; }
        public string ClientStarting { get; }
        public string FlashClientUrl { get; }
        public string FigurePartList { get; }
        public string FurniDataLoadUrl { get; }
        public string OverrideVariables { get; }
        public string ProductDataLoadUrl { get; }

        public HGameData(string gameData)
        {
            gameData = gameData.Replace(
                "connections.info", "connection.info");

            string formattedGameData = gameData.Replace("\\/", "/").Replace("\"//", "\"http://")
                .Replace("'//", "'http://").Replace("\r\n", string.Empty).Replace("\t", string.Empty);

            string flashVars = formattedGameData.GetChild("flashvars")
                .GetParent("}");

            if (formattedGameData.ToLower().Contains("<param name=\"flashvars\""))
            {
                flashVars = flashVars.GetChild("\" value=\"").GetParent(">")
                    .Replace("&amp;", "\r").Replace('=', ':').Replace("\"", string.Empty).Trim();
            }
            else
            {
                flashVars = flashVars.GetChild("{").Replace("\" : ", "\":").Trim();
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

                        string lineToRemove = $".addVariable(\"{varName}\"";
                        formattedGameData = formattedGameData.Replace(lineToRemove, string.Empty);

                        if (!string.IsNullOrWhiteSpace(varValue))
                            flashVars += (addVarArgs + "\r");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(flashVars))
                {
                    // embedSWF
                    string tempFlashVars = string.Empty;
                    flashVars = flashVars.Replace(": ", ":");

                    while (flashVars.Contains("\":"))
                    {
                        string varName = flashVars.GetChild("\"", '\"');
                        string flashArgParent = $"\"{varName}\":";

                        string flashArgChild = flashVars.GetChild(flashArgParent).Trim();
                        bool isVariable = (flashArgChild[0] != '"');
                        bool isEmpty = (flashArgChild[1] == '"');

                        string varValue = isEmpty ? string.Empty : isVariable ?
                            flashArgChild.GetParent(",") : flashArgChild.GetChild("\"", '"');

                        string flashArg = string.Format("\"{0}\":{2}{1}{2}",
                            varName, varValue, !isVariable ? "\"" : string.Empty);

                        if (!isEmpty)
                            tempFlashVars += $"{varName}:{varValue}\r";

                        flashVars = flashVars.Replace(flashArg, string.Empty);
                    }
                    flashVars = tempFlashVars;
                }
            }

            if (formattedGameData.Contains("var habboName = "))
            {
                string possibleUserName = formattedGameData
                    .GetChild("var habboName = \"", '"');

                if (!string.IsNullOrWhiteSpace(possibleUserName))
                    UserName = possibleUserName;
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
                    case "hotelview.banner.url": BannerUrl = varValue; break;
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