using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text;

namespace Mango.Services.EmailAPI.Service
{
    public class EmailService : IEmailService
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }
        public async Task EmailAndLogCart(CartDto cart)
        {
            StringBuilder message = new();

            message.AppendLine("<br/>Cart Email Requested ");
            message.Append("<br/>Total " + cart.CartHeader.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach (var item in cart.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product?.Name + " $"+ Convert.ToString(item.Product?.Price) + " x " + item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            await LogAndEmail(message.ToString(), cart.CartHeader.Email);
        }

        public async Task EmailAndLogConfirmedOrder(OrderConfirmationMessage message)
        {
            string messageLog = $"Order confirmed with OrderId: {message.OrderId} for UserId: {message.UserId}";
            await LogAndEmail(messageLog, "admin_support@mangocart.com");
        }

        public async Task EmailAndLogRegisterUser(string message)
        {
            string finalMsg = $"User registeration successful.<br/> Email: {message}";
            await LogAndEmail(finalMsg, "admin_support@mangocart.com");
        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                await using var _db = new AppDbContext(_dbOptions);

                EmailLogger emailLogger = new()
                {
                    Email = email,
                    Message = message,
                    EmailSent = DateTime.Now
                };
                await _db.EmailLoggers.AddAsync(emailLogger);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }           
        }
        

        }
    }

