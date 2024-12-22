namespace API.Data.Models.Entities.DbElWagd
{
    public class PSKUItemElamir : BaseEntity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PSKUItemElamir()
        {
            _httpContextAccessor = new HttpContextAccessor();
            InsertUser = GetCurrentUserName(); // Set InsertUser here
        }

        public int PSKUItemID { get; set; }
        public required string ItemCode { get; set; }
        public long? CSKUID { get; set; }
        public string? CSKUName { get; set; }
        public long? PSKUID { get; set; }
        public string? PSKUName { get; set; }
        public required string ElamirTradeChannelID { get; set; }
        public required DateTime FromDate { get; set; }
        public required DateTime ToDate { get; set; }
        public required string InsertUser { get; set; }
        public required DateTime InsertDate { get; set; } = DateTime.Now;
        public required bool DeleteFlag { get; set; } = false;
        public string? DeleteUser { get; set; } = null;
        public DateTime? DeleteDate { get; set; } = null;

        public string GetCurrentUserName()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            if (claimsPrincipal == null)
            {
                Console.WriteLine("ClaimsPrincipal is null. HttpContext might not be populated.");
                return "Unknown";
            }

            var userName = claimsPrincipal.Claims
                            .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" || c.Type == "sub")
                            ?.Value ?? "Unknown";

            return userName;
        }
    }
}