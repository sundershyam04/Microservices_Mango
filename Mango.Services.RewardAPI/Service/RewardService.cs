﻿using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text;

namespace Mango.Services.RewardAPI.Service
{
    public class RewardService : IRewardService
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }
    
        public async Task RewardsUpdate(RewardsMessage rewardsMessage)
        {
            try
            {
                Rewards rewards = new()
                {
                    UserId = rewardsMessage.UserId,
                    OrderId = rewardsMessage.OrderId,
                    RewardsActivity = rewardsMessage.RewardsActivity,
                    RewardsDate = DateTime.Now
                };
                await using var _db = new AppDbContext(_dbOptions);
                await _db.Rewards.AddAsync(rewards);
                await _db.SaveChangesAsync();              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error on uoppdating rewaards in db: {ex.Message}");
            }
        }
      
        }
    }
