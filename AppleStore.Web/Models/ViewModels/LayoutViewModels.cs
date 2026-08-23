namespace AppleStore.Web.Models.ViewModels;

public class PageHeaderCompactViewModel
{
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<PageHeaderBreadcrumbItemViewModel> Breadcrumbs { get; set; } = [];
}

public class PageHeaderBreadcrumbItemViewModel
{
    public PageHeaderBreadcrumbItemViewModel() { }

    public PageHeaderBreadcrumbItemViewModel(string label, string? url = null)
    {
        Label = label;
        Url = url;
    }

    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
}
