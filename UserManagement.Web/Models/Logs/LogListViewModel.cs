namespace UserManagement.Web.Models.Logs;

public class LogListViewModel
{
    public IEnumerable<LogListItemViewModel> Items { get; set; } = new List<LogListItemViewModel>();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
