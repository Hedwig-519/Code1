using SV20T1020362.DomainModels;

namespace SV20T1020362.Web.Models
{
    public class ProductSearchResult : BasePaginationResult
    {
        public List<Product> Data { get; set; }
    }
}
