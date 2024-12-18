namespace API.Data.Models.Entities.Sql2017
{
    public class ItemActiveDistElamir : BaseEntity
    {
        private readonly HttpContextAccessor _httpContextAccessor;
        public ItemActiveDistElamir()
        {
            _httpContextAccessor = new HttpContextAccessor();
            InsertUser = GetCurrentUserName();
        }

        public string ItemCode { get; set; }
        public string? PSKUName { get; set; }
        public string ElamirTradeChannelID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string InsertUser { get; set; }
        public DateTime InsertDate { get; set; } = DateTime.Now;
        public bool DeleteFlag { get; set; } = false;
        public string? DeleteUser { get; set; } = null;
        public DateTime? DeleteDate { get; set; } = null;
        public long? TempID { get; set; } = null;
        public long ItemActiveDistID { get; set; }
        public int? MOQ { get; set; }

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