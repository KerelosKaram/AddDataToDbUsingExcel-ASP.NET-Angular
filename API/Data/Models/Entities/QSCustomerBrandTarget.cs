namespace API.Data.Models.Entities.Sql2017
{
    public class QSCustomerBrandTarget : BaseEntity
    {
        public int ID { get; set; }
        public string SalesmanCode { get; set; }
        public string CustomerCode { get; set; }
        public string SubBrand { get; set; }
        public decimal target_LE { get; set; }
        public decimal Target_SU { get; set; }
        public decimal Target_Qty { get; set; }
        public int Month { get; set; }
        public int year { get; set; }
        public DateTime insertDate { get; set; } = DateTime.Now;
    }
}