namespace API.Data.Models.Entities.Sql2017
{
    public class PSKUItemElamir : BaseEntity
    {
        private readonly HttpContextAccessor _httpContextAccessor;
        public PSKUItemElamir()
        {
            // Manually initializing dependencies for cases when a parameterless constructor is needed
            _httpContextAccessor = new HttpContextAccessor();
            InsertUser = GetCurrentUserName(); // Set InsertUser here
        }

        // delete before
        public required string ItemCode { get; set; }
        public int? CSKUID { get; set; }
        public string? CSKUName { get; set; }
        public int? PSKUID { get; set; }
        public string? PSKUName { get; set; }
        public required string ElamirTradeChannelID { get; set; }
        public DateTime FromDate { get; set; } 
        public DateTime ToDate { get; set; }
        public required string InsertUser { get; set;} // user
        public DateTime? InsertDate { get; set; } = DateTime.Now;
        public bool DeleteFlag { get; set; } = false; // 0;
        public string? DeleteUser { get; set; } = null; // null
        public DateTime? DeleteDate { get; set; } = null; // null
        public int? tempID { get; set; } = null; // null
        public int PSKUItemID { get; set; } // auto
        public int? MOQ { get; set; }

        // Method to get the username from the JWT token
        public string GetCurrentUserName()
        {
            // Access the ClaimsPrincipal from the current HttpContext
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            if (claimsPrincipal == null)
            {
                Console.WriteLine("ClaimsPrincipal is null. HttpContext might not be populated.");
                return "Unknown";
            }

            // Attempt to retrieve the 'sub' claim
            var userName = claimsPrincipal.Claims
                            .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" || c.Type == "sub")
                            ?.Value ?? "Unknown";

            return userName;
        }
    }

}