using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyDonHang.Models;
//using QuanLyDonHang.Models;

namespace QuanLyDonHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderManagementDBContext _dbContext;

        public OrdersController(OrderManagementDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("")]
        public async Task<ActionResult> CreateOrder(CreateOrderDTO createOrderDTO)
        {
            Order order = new Order
            {
                CustomerId = createOrderDTO.CustomerId,
                OrderDate = DateTime.Now,
                TotalAmount = 0
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();


            return Ok(new
            {
                id = order.Id,
                customerId = order.CustomerId,
                orderDate = order.OrderDate,
                totalAmount = order.TotalAmount
            });
        }

        [HttpPost("{orderId}/items")]
        public async Task<ActionResult> AddOrderItem(int orderId, AddOrderItemDTO dto)
        {
            try
            {
                await _dbContext.Database.BeginTransactionAsync();
                SqlParameter paramOrderId = new SqlParameter("@OrderId", System.Data.SqlDbType.Int) { Value = orderId };
                SqlParameter paramProductId = new SqlParameter("@ProductId", System.Data.SqlDbType.Int) { Value = dto.ProductId };
                SqlParameter paramQuantity = new SqlParameter("@Quantity", System.Data.SqlDbType.Int) { Value = dto.Quantity };

                string sqlRaw = @$"EXEC AddProductToOrder @OrderId , @ProductId , @Quantity ";

                await _dbContext.Database.ExecuteSqlRawAsync(sqlRaw, paramOrderId, paramProductId, paramQuantity);

                await _dbContext.Database.CommitTransactionAsync();

                return StatusCode(200, new { message = "Thêm sản phẩm vào đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                await _dbContext.Database.RollbackTransactionAsync();
                return StatusCode(500, new { message = "Lỗi khi thực hiện store procedure", error = ex.Message });
            }

        }

        [HttpGet("{orderId}/total")]
        public async Task<IActionResult> GetOrderTotal(int orderId)
        {
            try
            {
                await _dbContext.Database.BeginTransactionAsync();

                SqlParameter paramOrderId = new SqlParameter("@OrderId", System.Data.SqlDbType.Int) { Value = orderId };

                string sqlRaw = @"EXEC GetOrderTotal @OrderId";

                var res = await _dbContext.Database.SqlQueryRaw<OrderTotalDTO>(sqlRaw, paramOrderId).ToListAsync();
                Console.WriteLine(res.Count);
                await _dbContext.Database.CommitTransactionAsync();

                if (res.Count == 0)
                {
                    return StatusCode(404, new { message = "Order không tồn tại" });
                }

                return StatusCode(200, new{ message = "Thành công",data = res });
            }
            catch (Exception ex)
            {
                await _dbContext.Database.RollbackTransactionAsync();

                return StatusCode(500, new
                {
                    message = "Lỗi server",
                    error = ex.Message
                });
            }
        }


    }
}