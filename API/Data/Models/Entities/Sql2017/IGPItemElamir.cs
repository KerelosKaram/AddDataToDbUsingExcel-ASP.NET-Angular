using System.ComponentModel.DataAnnotations;

namespace API.Data.Models.Entities.Sql2017
{
    public class IGPItemElamir : BaseEntity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Parameterless constructor for dynamic creation
        public IGPItemElamir()
        {
            // Manually initializing dependencies for cases when a parameterless constructor is needed
            _httpContextAccessor = new HttpContextAccessor();
            InsertUser = GetCurrentUserName(); // Set InsertUser here
        }

        public required string ItemCode { get; set; }
        public string? PSKUName { get; set; }
        public string? ElamirTradeChannelID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? InsertUser { get; set; }
        public DateTime? InsertDate { get; set; } = DateTime.Now;
        public bool? DeleteFlag { get; set; } = false;
        public string? DeleteUser { get; set; } = null;
        public DateTime? DeleteDate { get; set; } = null;
        public int? tempID { get; set; } = null;
        
        [Key]
        public int PSKUItemID { get; set; }

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
