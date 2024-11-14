using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Service
{
    public interface IEmailService
    {
        Task EmailAndLogCart(CartDto cart);
        Task EmailAndLogRegisterUser(string message);
        Task EmailAndLogConfirmedOrder(OrderConfirmationMessage message);
    }
}
