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
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Habbo.Web;

namespace Sulakore
{
    /// <summary>
    /// Provides static methods for extracting public information from a hotel.
    /// </summary>
    public static class SKore
    {
        private const string USER_API_SUFFIX = "/api/public/users?name=";
        private const string PROFILE_API_FORMAT = "{0}/api/public/users/{1}/profile";
        private const string IP_COOKIE_PREFIX = "YPF8827340282Jdskjhfiw_928937459182JAX666";

        private static string _ipCookie;

        private static readonly Array _randomThemes;
        private static readonly HttpClient _httpClient;
        private static readonly HttpClientHandler _httpClientHandler;
        private static readonly Random _randomSignGen, _randomThemeGen;
        private static readonly IDictionary<HHotel, IDictionary<string, string>> _uniqueIds;

        static SKore()
        {
            _randomSignGen = new Random();
            _randomThemeGen = new Random();

            _randomThemes = Enum.GetValues(typeof(HTheme));

            _httpClientHandler = new HttpClientHandler() { UseProxy = false };
            _httpClient = new HttpClient(_httpClientHandler, true);

            _uniqueIds = new Dictionary<HHotel, IDictionary<string, string>>();
        }

        /// <summary>
        /// Returns your external Internet Protocol (IP) address that is required to successfully send GET/POST request to the specified <seealso cref="HHotel"/> in an asynchronous operation.
        /// </summary>
        /// <param name="hotel">The hotel you wish to retrieve the cookie containing your external Internet Protocol (IP) address from.</param>
        /// <returns></returns>
        public static async Task<string> GetIPCookieAsync(HHotel hotel)
        {
            if (!string.IsNullOrEmpty(_ipCookie)) return _ipCookie;
            string body = await _httpClient.GetStringAsync(hotel.ToUrl())
                .ConfigureAwait(false);

            string ip = body.GetChild(IP_COOKIE_PREFIX + "', '", '\'');
            return _ipCookie = (IP_COOKIE_PREFIX + "=" + ip);
        }
        /// <summary>
        /// Returns the <seealso cref="HUser"/> from the specified hotel associated with the given name in an asynchronous operation.
        /// </summary>
        /// <param name="name">The name of the player you wish to retrieve the <seealso cref="HUser"/> from.</param>
        /// <param name="hotel">The <seealso cref="HHotel"/> that the target user is located on.</param>
        /// <returns></returns>
        public static async Task<HUser> GetUserAsync(string name, HHotel hotel)
        {
            string userJson = await _httpClient.GetStringAsync(
                hotel.ToUrl() + USER_API_SUFFIX + name);

            return HUser.Create(userJson);
        }
        /// <summary>
        /// Returns the unique identifier from the specified <seealso cref="HHotel"/> associated with the given name in an asynchronous operation.
        /// </summary>
        /// <param name="name">The name of the player you wish to retrieve the unique identifier from.</param>
        /// <param name="hotel">The <seealso cref="HHotel"/> that the target user is located on.</param>
        /// <returns></returns>
        public static async Task<string> GetUniqueIdAsync(string name, HHotel hotel)
        {
            bool hotelInitialized = _uniqueIds.ContainsKey(hotel);

            if (!hotelInitialized)
                _uniqueIds.Add(hotel, new Dictionary<string, string>());
            else if (_uniqueIds[hotel].ContainsKey(name))
                return _uniqueIds[hotel][name];

            string uniqueId = (await GetUserAsync(name, hotel)).UniqueId;

            _uniqueIds[hotel][name] = uniqueId;
            return uniqueId;
        }
        /// <summary>
        /// Returns the <seealso cref="HProfile"/> from the specified hotel associated with the given unique identifier in an asynchronous operation.
        /// </summary>
        /// <param name="uniqueId">The unique identifier of the player you wish to retrieve the <see cref="HProfile"/> from.</param>
        /// <param name="hotel">The <seealso cref="HHotel"/> that the target user is located on.</param>
        /// <returns></returns>
        public static async Task<HProfile> GetProfileAsync(string uniqueId, HHotel hotel)
        {
            string profileJson = await _httpClient.GetStringAsync(
                string.Format(PROFILE_API_FORMAT, hotel.ToUrl(), uniqueId));

            return HProfile.Create(profileJson);
        }

        /// <summary>
        /// Returns the primitive value for the specified <see cref="HSign"/>.
        /// </summary>
        /// <param name="sign">The <see cref="HSign"/> you wish to retrieve the primitive value from.</param>
        /// <returns></returns>
        public static int Juice(this HSign sign)
        {
            if (sign != HSign.Random)
                return (int)sign;

            return _randomSignGen.Next(0, 19);
        }
        /// <summary>
        /// Returns the primitive value for the specified <see cref="HBan"/>.
        /// </summary>
        /// <param name="ban">The <see cref="HBan"/> you wish to retrieve the primitive value from.</param>
        /// <returns></returns>
        public static string Juice(this HBan ban)
        {
            switch (ban)
            {
                default:
                case HBan.Day: return "RWUAM_BAN_USER_DAY";

                case HBan.Hour: return "RWUAM_BAN_USER_HOUR";
                case HBan.Permanent: return "RWUAM_BAN_USER_PERM";
            }
        }
        /// <summary>
        /// Returns the primitive value for the specified <see cref="HTheme"/>.
        /// </summary>
        /// <param name="theme">The <see cref="HTheme"/> you wish to retrieve the primitive value from.</param>
        /// <returns></returns>
        public static int Juice(this HTheme theme)
        {
            if (theme != HTheme.Random)
                return (int)theme;

            int randomIndex = _randomThemeGen.Next(0,
                _randomThemes.Length - 1);

            return (int)_randomThemes
                .GetValue(randomIndex);
        }

        /// <summary>
        /// Returns the full URL representation of the specified <seealso cref="HHotel"/>.
        /// </summary>
        /// <param name="hotel">The <seealso cref="HHotel"/> you wish to retrieve the full URL from.</param>
        /// <returns></returns>
        public static string ToUrl(this HHotel hotel)
        {
            const string HotelUrlFormat = "https://www.Habbo.";
            return HotelUrlFormat + hotel.ToDomain();
        }
        /// <summary>
        /// Returns the domain associated with the specified <see cref="HHotel"/>.
        /// </summary>
        /// <param name="hotel">The <see cref="HHotel"/> that is associated with the wanted domain.</param>
        /// <returns></returns>
        public static string ToDomain(this HHotel hotel)
        {
            string value = hotel.ToString().ToLower();
            return value.Length != 5 ? value : value.Insert(3, ".");
        }

        /// <summary>
        /// Returns the <see cref="HBan"/> associated with the specified value.
        /// </summary>
        /// <param name="ban">The string representation of the <see cref="HBan"/> object.</param>
        /// <returns></returns>
        public static HBan ToBan(string ban)
        {
            switch (ban)
            {
                default:
                case "RWUAM_BAN_USER_DAY": return HBan.Day;

                case "RWUAM_BAN_USER_HOUR": return HBan.Hour;
                case "RWUAM_BAN_USER_PERM": return HBan.Permanent;
            }
        }
        /// <summary>
        /// Returns the <see cref="HHotel"/> associated with the specified value.
        /// </summary>
        /// <param name="host">The string representation of the <see cref="HHotel"/> object.</param>
        /// <returns></returns>
        public static HHotel ToHotel(string host)
        {
            HHotel hotel = HHotel.Unknown;
            string identifier = host.GetChild("game-", '.')
                .Replace("us", "com");

            if (string.IsNullOrWhiteSpace(identifier))
            {
                identifier = host.GetChild("habbo.", '/')
                    .Replace(".", string.Empty);
            }
            else if (identifier == "tr" || identifier == "br")
                identifier = "com" + identifier;

            if (!string.IsNullOrWhiteSpace(identifier))
                Enum.TryParse(identifier, true, out hotel);

            return hotel;
        }
        /// <summary>
        /// Returns the <see cref="HGender"/> associated with the specified value.
        /// </summary>
        /// <param name="gender">The string representation of the <see cref="HGender"/> object.</param>
        /// <returns></returns>
        public static HGender ToGender(string gender)
        {
            return (HGender)gender.ToUpper()[0];
        }

        /// <summary>
        /// Iterates through an event's list of subscribed delegates, and begins to unsubscribe them from the event.
        /// </summary>
        /// <typeparam name="T">The type of the event handler.</typeparam>
        /// <param name="eventHandler">The event handler to unsubscribe the subscribed delegates from.</param>
        public static void Unsubscribe<T>(ref EventHandler<T> eventHandler) where T : EventArgs
        {
            if (eventHandler == null) return;
            Delegate[] subscriptions = eventHandler.GetInvocationList();

            foreach (Delegate subscription in subscriptions)
                eventHandler -= (EventHandler<T>)subscription;
        }

        /// <summary>
        /// Returns a new string that begins where the parent ends in the source.
        /// </summary>
        /// <param name="source">The string that contains the child.</param>
        /// <param name="parent">The string that comes before the child.</param>
        /// <returns></returns>
        public static string GetChild(this string source, string parent)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            int sourceIndex = source
                .IndexOf(parent ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            return sourceIndex >= 0 ?
                source.Substring(sourceIndex + parent.Length) : string.Empty;
        }
        /// <summary>
        /// Returns a new string that ends where the child begins in the source.
        /// </summary>
        /// <param name="source">The string that contains the parent.</param>
        /// <param name="child">The string that comes after the parent.</param>
        /// <returns></returns>
        public static string GetParent(this string source, string child)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            int sourceIndex = source
                .IndexOf(child ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            return sourceIndex >= 0 ?
                source.Remove(sourceIndex) : string.Empty;
        }
        /// <summary>
        /// Returns a new string that is between the delimiters, and the child in the source.
        /// </summary>
        /// <param name="source">The string that contains the parent.</param>
        /// <param name="child">The string that comes after the parent.</param>
        /// <param name="delimiters">The Unicode characters that will be used to split the parent, returning the last split value.</param>
        /// <returns></returns>
        public static string GetParent(this string source, string child, params char[] delimiters)
        {
            string parentSource = source.GetParent(child);
            if (!string.IsNullOrWhiteSpace(parentSource))
            {
                string[] childSplits = parentSource.Split(delimiters,
                    StringSplitOptions.RemoveEmptyEntries);

                return childSplits[childSplits.Length - 1];
            }
            else return string.Empty;
        }
        /// <summary>
        /// Returns a new string that is between the parent, and the delimiters in the source.
        /// </summary>
        /// <param name="source">The string that contains the child.</param>
        /// <param name="parent">The string that comes before the child.</param>
        /// <param name="delimiters">The Unicode characters that will be used to split the child, returning the first split value.</param>
        /// <returns></returns>
        public static string GetChild(this string source, string parent, params char[] delimiters)
        {
            string childSource = source.GetChild(parent);

            return !string.IsNullOrWhiteSpace(childSource) ?
                childSource.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[0] : string.Empty;
        }
    }
}