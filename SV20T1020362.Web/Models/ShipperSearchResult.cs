using SV20T1020362.DomainModels;

namespace SV20T1020362.Web.Models
{
    public class ShipperSearchResult : BasePaginationResult
    {
        public List<Shipper> Data { get; set; }
    }
}
