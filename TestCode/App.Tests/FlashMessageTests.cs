using Microsoft.AspNetCore.Http;
using ModulesRegistry.Extensions;

namespace ModulesRegistry.AppTests;

/// <summary>
/// One-time messages after a POST on a static page (Migration step 3.3): set as a cookie before the redirect, shown once on
/// the next page, so the address doesn't keep the result.
/// </summary>
[TestClass]
public sealed class FlashMessageTests
{
    [TestMethod]
    public void SetMessageIsTakenOnceOnTheNextRequest()
    {
        var post = new DefaultHttpContext();
        post.SetFlashMessage("DeletedSuccessfully", isSuccess: true);
        var setCookie = post.Response.Headers.SetCookie.ToString();

        var next = RequestWithCookie(CookieValue(setCookie));
        var message = next.TakeFlashMessage();

        Assert.AreEqual(new FlashMessageText("DeletedSuccessfully", true), message);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"{FlashMessageExtensions.CookieName}=;", next.Response.Headers.SetCookie.ToString(), "The cookie is deleted after it is taken.");
    }

    [TestMethod]
    public void FailureIsKept()
    {
        var post = new DefaultHttpContext();
        post.SetFlashMessage("DeleteFailed", isSuccess: false);

        var message = RequestWithCookie(CookieValue(post.Response.Headers.SetCookie.ToString())).TakeFlashMessage();

        Assert.AreEqual(new FlashMessageText("DeleteFailed", false), message);
    }

    [TestMethod]
    [DataRow(FlashMessageLevel.Warning)]
    [DataRow(FlashMessageLevel.Info)]
    public void WarningAndInformationAreKept(FlashMessageLevel level)
    {
        var post = new DefaultHttpContext();
        post.SetFlashMessage("NoModification", level);

        var message = RequestWithCookie(CookieValue(post.Response.Headers.SetCookie.ToString())).TakeFlashMessage();

        Assert.AreEqual(new FlashMessageText("NoModification", level), message);
        Assert.IsFalse(message!.IsSuccess);
    }

    [TestMethod]
    public void NoCookieGivesNoMessage()
    {
        var context = new DefaultHttpContext();

        Assert.IsNull(context.TakeFlashMessage());
        Assert.IsEmpty(context.Response.Headers.SetCookie.ToString());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("1:")]
    [DataRow("4:DeletedSuccessfully")]
    [DataRow("DeletedSuccessfully")]
    [DataRow("1:<script>")]
    [DataRow("1:Two words")]
    public void CookieThatIsNotAResourceKeyGivesNoMessage(string value)
    {
        var context = RequestWithCookie(Uri.EscapeDataString(value));

        Assert.IsNull(context.TakeFlashMessage());
    }

    [TestMethod]
    public void TextThatIsNotAResourceKeyIsNotSet()
    {
        var post = new DefaultHttpContext();

        post.SetFlashMessage("Deleted successfully!", isSuccess: true);

        Assert.IsEmpty(post.Response.Headers.SetCookie.ToString());
    }

    [TestMethod]
    public void NoHttpContextDoesNothing()
    {
        HttpContext? context = null;

        context.SetFlashMessage("DeletedSuccessfully", true);

        Assert.IsNull(context.TakeFlashMessage());
    }

    private static DefaultHttpContext RequestWithCookie(string value)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{FlashMessageExtensions.CookieName}={value}";
        return context;
    }

    // The value of "name=value; path=/; ..." as sent in Set-Cookie.
    private static string CookieValue(string setCookie) => setCookie.Split(';')[0].Split('=', 2)[1];
}