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
            var document = HtmlParser.Parse(@"<p>So, I promised this post a long time ago to <a href=""http://www.okorn.net/"">Bojan</a> and <a href=""http://slemc.org/blog/"">Tadej</a>. At most of the concerts, the main attention is centered towards the lead singer, or in my case, the guitarists :-). I can't help it, it's a professional deformation. Well, anyway, we talked about the backvolasits (the female ones :) ), at Rumena noč (<a href=""http://blog.vodovnik.com/2007/07/31/anavrin-da-phenomena-and-ank-rock/"">click</a>, <a href=""http://blog.vodovnik.com/2007/07/29/mclaren-slon-in-sadez-neisha-and-tinkara-kovac/"">click</a>, <a href=""http://blog.vodovnik.com/2007/07/26/siddharta-dan-d/"">click</a>) and afterwards there was some confusion between Neža (from Neisha's band) and Urška (from McLaren).</p>");

            //Assert.Single(document.Nodes);
        }
    }
}


