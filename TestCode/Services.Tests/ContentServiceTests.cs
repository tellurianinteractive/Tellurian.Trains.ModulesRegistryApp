using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class ContentServiceTests
{
    [TestMethod]
    public async Task ReadsMaxDate()
    {
        var target = new ContentService(new NoOpHttpClientFactory(), @"..\..\..\..\..\SourceCode\App\Content\Markdown");
        Assert.AreEqual(DateTimeOffset.Parse("2023-09-29 09:53:32 +00:00"), await target.GetLastModifiedTimeOfTextContent("termsofuse"));
    }

    private class NoOpHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            throw new NotImplementedException();
        }
    }
}
