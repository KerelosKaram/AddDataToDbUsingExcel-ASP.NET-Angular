namespace API.Data.Models.Entities.Sql2017
{
    public class IGPSalesmanTarget : BaseEntity
    {
        // delete before
        public string? SalesmanCode { get; set; }
        public string? TradeChannel { get; set; }
        public string? PSKUName { get; set; }
        public int? NoCustomerTarget { get; set; }
    }
}