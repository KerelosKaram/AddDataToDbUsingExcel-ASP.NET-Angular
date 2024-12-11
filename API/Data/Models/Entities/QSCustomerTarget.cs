namespace API.Data.Models.Entities.Sql2017
{
    public class QSCustomerTarget : BaseEntity
    {
        public int ID { get; set; }
        public string CustomerCode { get; set; }
        public decimal Target_LE { get; set; }
        public int Month { get; set; }
        public int year { get; set; }
        public DateTime insertDate { get; set; } = DateTime.Now;
        public string SalesmanCode { get; set; }
    }
}