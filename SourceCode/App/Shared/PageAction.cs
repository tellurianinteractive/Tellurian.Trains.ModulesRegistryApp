namespace ModulesRegistry.Shared
{
    public enum PageAction
    {
        Unknown,
        List,
        Add,
        Edit,
        Delete,
        Error,
        Tools
    }

    public static class PageActionExtensions
    {
        public static PageAction ToAddOrEditPageAction(this int id) => id == 0 ? PageAction.Add : PageAction.Edit;
    }
}
