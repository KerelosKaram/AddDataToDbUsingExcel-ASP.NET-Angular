namespace API.Data.Models.Entities.DBAX
{
    public class ElamirCustomerClassification2 : BaseEntity
    {
        public int ElamirCustomerClassificationID { get; set; }
        public string? CustomerCode { get; set; }
        public string? ElamirClassification { get; set; }
        public string? PGClassification { get; set; }
        public string? PGBranchCode { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}