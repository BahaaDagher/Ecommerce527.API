namespace Ecommerce527.API.Models
{
    public enum TransactionType
    {
        Cash  ,
        Card
    }
    public enum OrderStatus
    {
        Pending,
        InProgress , 
        Shipped , 
        Completed , 
        Canceled
    }
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime CreatedAt { get; set;  } = DateTime.UtcNow;
        public TransactionType TransactionType { get; set; } = TransactionType.Card;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public decimal TotalPrice { get; set; }
        public string? SessionId { get; set; }
        public string? TransactionId { get; set; }

    }
}
