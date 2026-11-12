using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Time.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Notifications;

namespace ModulesRegistry.AppTests.Notifications;

[TestClass]
public sealed class NotificationServiceExtensionsTests
{
    private NotificationService _target = null!;
    private readonly IStringLocalizer _localizer = new BracketLocalizer();

    [TestInitialize]
    public void Initialize() => _target = new NotificationService(new FakeTimeProvider());

    [TestCleanup]
    public void Cleanup() => _target.Dispose();

    [TestMethod]
    [DataRow(1, NotificationLevel.Success)]
    [DataRow(5, NotificationLevel.Success)]
    [DataRow(-1, NotificationLevel.Info)]
    [DataRow(0, NotificationLevel.Error)]
    public void ShowSuccessOrFailureChoosesLevelFromCount(int count, NotificationLevel expected)
    {
        _target.ShowSuccessOrFailure(_localizer, count, "Saved");

        var actual = _target.Notifications.Single();
        Assert.AreEqual(expected, actual.Level);
        Assert.AreEqual("[Saved]", actual.Message);
    }

    [TestMethod]
    public void ShowSuccessOrFailureAcceptsNullMessage()
    {
        _target.ShowSuccessOrFailure(_localizer, 1, null);

        Assert.AreEqual("[]", _target.Notifications.Single().Message);
    }

    [TestMethod]
    public void ShowNotFoundShowsLocalizedErrorWithTypeName()
    {
        _target.ShowNotFound<Meeting>(_localizer);

        var actual = _target.Notifications.Single();
        Assert.AreEqual(NotificationLevel.Error, actual.Level);
        Assert.AreEqual("[Meeting] [notfound]", actual.Message);
    }

    private sealed class BracketLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, $"[{name}]");
        public LocalizedString this[string name, params object[] arguments] => this[name];
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}