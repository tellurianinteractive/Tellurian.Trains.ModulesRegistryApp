namespace ModulesRegistry.Shared
{
    public enum PageAction
    {
        Unknown,
        List,
        Add,
        Edit,
        Delete,
        Error
    }

    public static class PageActionExtensions
    {
        public static PageAction ToPageAction(this int id) => id == 0 ? PageAction.Add : PageAction.Edit;
    }
}
