namespace Mango.Services.OrderAPI.Models.Dto
{
    public class RewardsDto
    {
        public string UserId { get; set; }
        public int OrderId { get; set; }
       // Reward points
       // 1 USD = 1 points (reward)
        public int RewardsActivity { get; set; }
    }
}
