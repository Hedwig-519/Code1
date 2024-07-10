using SV20T1020362.DomainModels;

namespace SV20T1020362.Web.Models
{
    public class SupplierSearchResult : BasePaginationResult
    {
        public List<Supplier> Data { get; set; }
    }
}
