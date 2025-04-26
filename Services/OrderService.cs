using System;
using AllSet.Domain;
using Microsoft.EntityFrameworkCore;
using static AllSet.DTOs.DataTransferObjects;

namespace AllSet.Services
{
    public class OrderService
    {
        private readonly AllSetDbContext _dbContext;

        public OrderService(AllSetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> CreateOrder(CreateOrderRequestDto request)
        {
            Order o = new()
            {
                Id = Guid.NewGuid(),
                Status = "",
                UserId = request.UserId,
                CreatedOn = DateTime.UtcNow
            };
            await _dbContext.Orders.AddAsync(o);
            await _dbContext.SaveChangesAsync();
            return o;
        }

        public async Task<bool> UpdateOrderStatus(Guid orderId, string newStatus)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false; // Order not found
            }

            var bookings = await _dbContext.Bookings.Where(x => x.OrderId == orderId).ToListAsync();
            if (bookings == null || !bookings.Any())
            {
                return false; // No bookings found for this order
            }

            foreach (var booking in bookings)
            {
                booking.Status = newStatus;
            }

            await _dbContext.SaveChangesAsync(); // Save all updates in the database

            return true;
        }

        public async Task<Order?> GetOrder(Guid orderId)
        {
            return await _dbContext.Orders
                .Include(o => o.Bookings)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetOrdersByUser(Guid userId)
        {
            return await _dbContext.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Bookings)
                .ToListAsync();
        }

        public async Task<bool> CancelOrder(Guid orderId)
        {
            //var order = await _dbContext.Orders
            //    .Include(o => o.Bookings)
            //    .FirstOrDefaultAsync(o => o.Id == orderId);

            //if (order == null) return false;

            //_dbContext.Bookings.RemoveRange(order.Bookings); // Remove all related bookings
            //_dbContext.Orders.Remove(order);
            //await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

