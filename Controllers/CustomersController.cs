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
    public class CustomersController : ControllerBase
    {
        private readonly OrderManagementDBContext _dbContext;

        public CustomersController(OrderManagementDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{customerId}/orders")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            try
            {
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var customerIdParam = new SqlParameter("@CustomerId", customerId);
                        var orders = await _dbContext.Orders
                            .FromSqlRaw("SELECT * FROM Orders WHERE CustomerId = @CustomerId", customerIdParam)
                            .ToListAsync();

                        await transaction.CommitAsync();
                        return Ok(orders ?? new List<Order>());
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the orders.");
            }
        }
    }
}