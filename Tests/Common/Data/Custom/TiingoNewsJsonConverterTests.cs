﻿/*
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
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Data.Custom.Tiingo;

namespace QuantConnect.Tests.Common.Data.Custom
{
    [TestFixture]
    public class TiingoNewsJsonConverterTests
    {
        [Test]
        public void DeserializeCorrectly()
        {
            var content = @"[{
    ""source"":""source"",
    ""crawlDate"":""2019-01-29T22:20:01.696871Z"",
    ""description"":""description"",
    ""url"":""url"",
    ""publishedDate"":""2019-01-29T22:17:00Z"",
    ""tags"":[ ""tag1"", ""tag2""],
    ""tickers"":[""aapl""],
    ""id"":1,
    ""title"":""title""
},
{
    ""source"":""source"",
    ""crawlDate"":""2019-01-29T22:20:01.696871Z"",
    ""publishedDate"":""2019-01-29T22:20:01.696871Z"",
    ""tickers"":[],
    ""id"":2,
    ""title"":""title""
}]";
            var result = JsonConvert.DeserializeObject<List<TiingoNewsData>>(content,
                new TiingoNewsJsonConverter(Symbols.SPY, TimeZones.NewYork));

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("1", result[0].ArticleID);
            Assert.AreEqual(
                DateTime.Parse("2019-01-29T22:20:01.696871", CultureInfo.InvariantCulture),
                result[0].CrawlDate);
            Assert.AreEqual(
                DateTime.Parse("2019-01-29T22:17:00", CultureInfo.InvariantCulture),
                result[0].PublishedDate);
            Assert.AreEqual("description", result[0].Description);
            Assert.AreEqual("source", result[0].Source);
            Assert.AreEqual(new List<string> { "tag1", "tag2" }, result[0].Tags);
            Assert.AreEqual(new List<Symbol> { Symbols.AAPL }, result[0].Symbols);
            Assert.AreEqual("title", result[0].Title);
            Assert.AreEqual("url", result[0].Url);
            Assert.AreEqual(Symbols.SPY, result[0].Symbol);

            Assert.AreEqual("2", result[1].ArticleID);
            Assert.AreEqual(
                DateTime.Parse("2019-01-29T22:20:01.696871", CultureInfo.InvariantCulture),
                result[1].CrawlDate);
            Assert.AreEqual(
                DateTime.Parse("2019-01-29T22:20:01.696871", CultureInfo.InvariantCulture),
                result[1].PublishedDate);
            Assert.AreEqual("title", result[1].Title);
            Assert.AreEqual(new List<Symbol>(), result[1].Symbols);
            Assert.AreEqual(new List<string>(), result[1].Tags);
        }
    }
}
