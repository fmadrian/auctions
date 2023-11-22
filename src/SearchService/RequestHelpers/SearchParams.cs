namespace SearchService.RequestHelpers;
public class SearchParams
{
    // Search parameters.
    public string SearchTerm { get; set; }
    // Pagination options.
    public int PageNumber { get; set; } = 1;
    public int Size { get; set; } = 4;
    // Ordering and filtering parameters
    public string Seller { get; set; }
    public string Winner { get; set; }
    public string OrderBy { get; set; }
    public string FilterBy { get; set; }

}
