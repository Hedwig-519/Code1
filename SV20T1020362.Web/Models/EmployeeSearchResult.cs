using SV20T1020362.DomainModels;

namespace SV20T1020362.Web.Models
{
    public class EmployeeSearchResult : BasePaginationResult
    {
        public List<Employee> Data { get; set; }
    }
}
