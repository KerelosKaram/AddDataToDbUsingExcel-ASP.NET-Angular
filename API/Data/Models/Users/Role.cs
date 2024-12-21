namespace API.Data.Models.Users
{
    public class Role
    {
        public int RoleID { get; set; }
        public required string RoleName { get; set; }
        public string? RoleDescription { get; set; }
    }
}