/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace QuantConnect.Data.Custom.Tiingo
{
    /// <summary>
    /// Helper json converter class used to convert a list of Tiingo news data
    /// into <see cref="List{TiingoNewsData}"/>
    /// </summary>
    public class TiingoNewsJsonConverter : JsonConverter
    {
        private readonly Symbol _symbol;
        private readonly DateTimeZone _exchangeTimeZone;

        /// <summary>
        /// Creates a new instance of the json converter
        /// </summary>
        /// <param name="symbol">The <see cref="Symbol"/> instance associated with this news</param>
        /// <param name="exchangeTimeZone">The exchange time zone used to set the <see cref="BaseData.Time"/>
        /// correctly</param>
        public TiingoNewsJsonConverter(Symbol symbol,
            DateTimeZone exchangeTimeZone)
        {
            _symbol = symbol;
            _exchangeTimeZone = exchangeTimeZone;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("TiingoNewsJsonConverter.WriteJson(): is not implemented");
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var tokens = JToken.Load(reader);

            var result = new List<TiingoNewsData>();
            foreach (var token in tokens)
            {
                // just in case we add some default values for these
                var title = token["title"]?.ToString() ?? "";
                var source = token["source"]?.ToString() ?? "";
                var url = token["url"]?.ToString() ?? "";
                var tags = token["tags"]?.ToObject<List<string>>() ?? new List<string>();
                var description = token["description"]?.ToString() ?? "";

                var publishedDate = token["publishedDate"].ToObject<DateTime>();
                var articleID = token["id"].ToString();
                var crawlDate = token["crawlDate"].ToObject<DateTime>();
                var tickers = token["tickers"];

                var symbols = new List<Symbol>();
                foreach (var tiingoTicker in tickers)
                {
                    var ticker = TiingoSymbolMapper.GetLeanTicker(tiingoTicker.ToString());

                    var sid = SecurityIdentifier.GenerateEquity(
                        ticker,
                        QuantConnect.Market.USA,
                        mappingResolveDate: crawlDate);
                    symbols.Add(new Symbol(sid, ticker));
                }

                result.Add(new TiingoNewsData
                {
                    ArticleID = articleID,
                    CrawlDate = crawlDate,
                    Description = description,
                    PublishedDate = publishedDate,
                    Source = source,
                    Tags = tags,
                    Symbols = symbols,
                    Url = url,
                    Title = title,

                    Symbol = _symbol,
                    // CrawlDate set by Tiingo is used as Time but its in UTC
                    // Lean expects Time to be in exchange time zone
                    Time = crawlDate.ConvertFromUtc(_exchangeTimeZone)
                });
            }

            return result;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<TiingoNewsData>);
        }
    }
}
