namespace API.Data.Models.Entities.OneNineTwo
{
    public class MessageOut : BaseEntity
    {
        public int Id { get; set; }
        public string? MessageTo { get; set; }
        public string? MessageFrom { get; set; }
        public string? MessageText { get; set; }
        public string? MessageType { get; set; }
        public string? Gateway { get; set; }
        public string? UserId { get; set; }
        public string? UserInfo { get; set; }
        public int? Priority { get; set; }
        public DateTime? Scheduled { get; set; }
        public int? ValidityPeriod { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, " +
                   $"MessageTo: {MessageTo}, " +
                   $"MessageFrom: {MessageFrom}, " +
                   $"MessageText: {MessageText}, " +
                   $"MessageType: {MessageType}, " +
                   $"Gateway: {Gateway}, " +
                   $"UserId: {UserId}, " +
                   $"UserInfo: {UserInfo}, " +
                   $"Priority: {Priority}, " +
                   $"Scheduled: {Scheduled?.ToString("yyyy-MM-dd HH:mm:ss")}, " +
                   $"ValidityPeriod: {ValidityPeriod}, " +
                   $"IsRead: {IsRead}, " +
                   $"IsSent: {IsSent}";
        }
    }
}
