using SV20T1020362.DomainModels;
using System.ComponentModel;

namespace SV20T1020362.Web.Models
{
    public class ProductAttributePhotoSearchResult 
    {
        public Product Data { get; set; }
        public List<ProductAttribute>? Attribute {  get; set; }
        public List<ProductPhoto>? Photo {  get; set; }

    }
}
