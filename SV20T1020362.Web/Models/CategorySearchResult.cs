using SV20T1020362.DomainModels;

namespace SV20T1020362.Web.Models
{
    public class CategorySearchResult : BasePaginationResult
    {
        public List<Category> Data { get; set; }
    }
}
