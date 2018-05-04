using System;
using System.Linq;
using Xunit;

namespace WpXmlToGhostMigrator.Tests
{
    public class NodeParserTests
    {
        [Fact]
        public void TestNodeNameParsed()
        {
            var document = HtmlParser.Parse("<test>some text</test>");

            Assert.Single(document.Nodes);
        }
    }
}
