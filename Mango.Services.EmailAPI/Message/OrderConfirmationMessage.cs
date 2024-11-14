namespace Mango.Services.EmailAPI.Message
{
    public class OrderConfirmationMessage
    {
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public int RewardsActivity { get; set; }
    }
}
