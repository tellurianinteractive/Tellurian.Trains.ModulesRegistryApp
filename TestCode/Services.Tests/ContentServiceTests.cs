using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Tests
{
    [TestClass]
    public class ContentServiceTests
    {
        [TestMethod]
        public async Task ReadsMaxDate()
        {
            var target = new ContentService(new NoOpHttpClientFactory(), @"..\..\..\..\..\SourceCode\App\Content\Markdown");
            Assert.AreEqual(DateTimeOffset.Parse("2021-03-04 18:31:21 +00:00"), await target.GetLastModifiedTimeOfTextContent("termsofuse"));
        }

        private class NoOpHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name)
            {
                throw new NotImplementedException();
            }
        }
    }
}
