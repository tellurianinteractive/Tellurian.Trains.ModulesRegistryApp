namespace ModulesRegistry.Shared
{
    public enum PageAction
    {
        Unknown,
        List,
        Add,
        Edit,
        Delete
    }

    public static class PageActionExtensions
    {
        public static PageAction ToPageAction(this int id) =>  id == 0 ? PageAction.Add : PageAction.Edit;
    }
}
