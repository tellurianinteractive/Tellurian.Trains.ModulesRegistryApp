using Microsoft.Extensions.Time.Testing;
using ModulesRegistry.Notifications;

namespace ModulesRegistry.AppTests.Notifications;

[TestClass]
public sealed class NotificationServiceTests
{
    private FakeTimeProvider _time = null!;
    private NotificationService _target = null!;
    private int _changedCount;

    [TestInitialize]
    public void Initialize()
    {
        _time = new FakeTimeProvider();
        _target = new NotificationService(_time);
        _changedCount = 0;
        _target.Changed += () => _changedCount++;
    }

    [TestCleanup]
    public void Cleanup() => _target.Dispose();

    [TestMethod]
    public void ShowAddsNotificationWithLevelAndMessage()
    {
        _target.ShowInfo("Info");
        _target.ShowSuccess("Success");
        _target.ShowWarning("Warning");
        _target.ShowError("Error");

        var actual = _target.Notifications.Select(n => (n.Level, n.Message)).ToList();
        CollectionAssert.AreEqual(
            new[] { (NotificationLevel.Info, "Info"), (NotificationLevel.Success, "Success"), (NotificationLevel.Warning, "Warning"), (NotificationLevel.Error, "Error") },
            actual);
        Assert.AreEqual(4, _changedCount);
    }

    [TestMethod]
    public void NotificationsHaveUniqueIds()
    {
        _target.ShowSuccess("Same");
        _target.ShowSuccess("Same");

        Assert.AreEqual(2, _target.Notifications.Select(n => n.Id).Distinct().Count());
    }

    [TestMethod]
    public void NotificationIsRemovedAfterDisplayTime()
    {
        _target.ShowSuccess("Saved");

        _time.Advance(NotificationService.DisplayTime - TimeSpan.FromMilliseconds(1));
        Assert.HasCount(1, _target.Notifications);

        _time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.IsEmpty(_target.Notifications);
        Assert.AreEqual(2, _changedCount);
    }

    [TestMethod]
    public void EachNotificationHasItsOwnDisplayTime()
    {
        _target.ShowSuccess("First");
        _time.Advance(TimeSpan.FromSeconds(1));
        _target.ShowSuccess("Second");

        _time.Advance(TimeSpan.FromSeconds(1));

        Assert.AreEqual("Second", _target.Notifications.Single().Message);
    }

    [TestMethod]
    public void RemoveRemovesOnlyThatNotification()
    {
        _target.ShowSuccess("First");
        _target.ShowError("Second");
        var first = _target.Notifications[0];

        _target.Remove(first.Id);

        Assert.AreEqual("Second", _target.Notifications.Single().Message);
        Assert.AreEqual(3, _changedCount);
    }

    [TestMethod]
    public void RemovedNotificationIsNotRemovedAgainWhenTimeExpires()
    {
        _target.ShowSuccess("Saved");
        _target.Remove(_target.Notifications[0].Id);
        var changedCountAfterRemove = _changedCount;

        _time.Advance(NotificationService.DisplayTime);

        Assert.AreEqual(changedCountAfterRemove, _changedCount);
    }

    [TestMethod]
    public void RemoveUnknownIdDoesNothing()
    {
        _target.ShowSuccess("Saved");

        _target.Remove(Guid.NewGuid());

        Assert.HasCount(1, _target.Notifications);
        Assert.AreEqual(1, _changedCount);
    }

    [TestMethod]
    public void ClearRemovesAllNotificationsAndNotifiesOnce()
    {
        _target.ShowSuccess("First");
        _target.ShowError("Second");

        _target.Clear();

        Assert.IsEmpty(_target.Notifications);
        Assert.AreEqual(3, _changedCount);
    }

    [TestMethod]
    public void ClearWithoutNotificationsDoesNotNotify()
    {
        _target.Clear();

        Assert.AreEqual(0, _changedCount);
    }

    [TestMethod]
    public void ClearedNotificationsAreNotRemovedAgainWhenTimeExpires()
    {
        _target.ShowSuccess("Saved");
        _target.Clear();

        _time.Advance(NotificationService.DisplayTime);

        Assert.AreEqual(2, _changedCount);
    }

    [TestMethod]
    public void NotificationsIsASnapshot()
    {
        _target.ShowSuccess("First");
        var snapshot = _target.Notifications;

        _target.ShowSuccess("Second");

        Assert.HasCount(1, snapshot);
    }
}