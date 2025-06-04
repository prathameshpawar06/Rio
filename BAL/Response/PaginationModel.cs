namespace BAL.Response
{
    public abstract partial class PaginationModel
    {
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage { get; set; }
    }
}
