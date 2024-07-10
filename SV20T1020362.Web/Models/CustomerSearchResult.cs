using SV20T1020362.DomainModels;

namespace SV20T1020362.Web.Models
{
    public class CustomerSearchResult : BasePaginationResult
    {
        public List<Customer> Data { get; set; }
    }
}
