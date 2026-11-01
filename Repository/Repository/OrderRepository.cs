using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    /// 订单管理实现
    /// </summary>
    public class OrderRepository : AppBaseService<Order>, IOrderRepository
    {
        public OrderRepository(IConfiguration config)
        {
            Config = config;
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM T_Order WHERE Id = @id";
            var orders = await FindAllAsync<Order>(sql, new { id });
            return orders.FirstOrDefault();
        }

        public async Task<Order> GetByOrderNoAsync(string orderNo)
        {
            const string sql = "SELECT * FROM T_Order WHERE OrderNo = @orderNo";
            var orders = await FindAllAsync<Order>(sql, new { orderNo });
            return orders.FirstOrDefault();
        }

        public async Task<PageData<Order>> GetPagedAsync(int page, int limit, byte? orderType = null, byte? status = null,
            string keyword = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            string sql = "SELECT * FROM T_Order WHERE 1=1";
            var parameters = new DynamicParameters();

            if (orderType.HasValue)
            {
                sql += " AND OrderType = @orderType";
                parameters.Add("orderType", orderType.Value);
            }

            if (status.HasValue)
            {
                sql += " AND Status = @status";
                parameters.Add("status", status.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " AND (OrderNo LIKE @keyword OR ReceiverName LIKE @keyword OR ReceiverPhone LIKE @keyword)";
                parameters.Add("keyword", $"%{keyword}%");
            }

            if (startTime.HasValue)
            {
                sql += " AND OrderTime >= @startTime";
                parameters.Add("startTime", startTime.Value);
            }

            if (endTime.HasValue)
            {
                sql += " AND OrderTime <= @endTime";
                parameters.Add("endTime", endTime.Value);
            }

            sql += " ORDER BY OrderTime DESC";
            return await PagedAsync<Order>(sql, parameters, "Id DESC", page, limit);
        }

        public async Task<int> InsertAsync(Order order)
        {
            order.CreateTime = DateTime.Now;
            order.OrderTime = DateTime.Now;
            return await AddAsync(order);
        }

        public async Task<bool> ModifyAsync(Order order)
        {
            order.UpdateTime = DateTime.Now;
            return await UpdateAsync(order);
        }

        public async Task<bool> UpdateStatusAsync(int orderId, byte newStatus, string remark = null)
        {
            const string getOldStatusSql = "SELECT Status FROM T_Order WHERE Id = @orderId";
            var oldStatus = await ExecuteScalarAsync<byte>(getOldStatusSql, new { orderId });

            const string updateSql = @"
                UPDATE T_Order 
                SET Status = @newStatus, UpdateTime = GETDATE(),
                    CompleteTime = CASE WHEN @newStatus = 3 THEN GETDATE() ELSE CompleteTime END
                WHERE Id = @orderId";

            int result = await ExecuteAsync(updateSql, new { orderId, newStatus });

            if (result > 0)
            {
                // 记录状态变更日志
                await AddStatusLogAsync(new OrderStatusLog
                {
                    OrderId = orderId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Remark = remark,
                    CreateTime = DateTime.Now
                });
            }

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM T_Order WHERE Id = @id";
            int result = await ExecuteAsync(sql, new { id });
            return result > 0;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            const string sql = "SELECT * FROM T_OrderItem WHERE OrderId = @orderId ORDER BY Id";
            return await FindAllAsync<OrderItem>(sql, new { orderId });
        }

        public async Task<bool> AddOrderItemsAsync(List<OrderItem> items)
        {
            if (items == null || !items.Any())
            {
                return false;
            }

            await using var connection = new SqlConnection(Config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            const string sql = @"
                INSERT INTO T_OrderItem (OrderId, ProductId, ProductName, ProductImage, UnitPrice, Quantity, SubTotal, CreateTime)
                VALUES (@OrderId, @ProductId, @ProductName, @ProductImage, @UnitPrice, @Quantity, @SubTotal, GETDATE())";

            int result = await connection.ExecuteAsync(sql, items);
            return result > 0;
        }

        public async Task<IEnumerable<OrderStatusLog>> GetStatusLogsAsync(int orderId)
        {
            const string sql = @"
                SELECT l.*, a.Name as OperatorName 
                FROM T_OrderStatusLog l
                LEFT JOIN T_Admin a ON l.OperatorId = a.Id
                WHERE l.OrderId = @orderId 
                ORDER BY l.CreateTime DESC";
            return await FindAllAsync<OrderStatusLog>(sql, new { orderId });
        }

        public async Task<bool> AddStatusLogAsync(OrderStatusLog log)
        {
            log.CreateTime = DateTime.Now;
            await using var connection = new SqlConnection(Config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            return await connection.InsertAsync(log) > 0;
        }

        public async Task<OrderStatistics> GetStatisticsAsync(DateTime? startTime = null, DateTime? endTime = null)
        {
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (startTime.HasValue)
            {
                whereClause += " AND OrderTime >= @startTime";
                parameters.Add("startTime", startTime.Value);
            }

            if (endTime.HasValue)
            {
                whereClause += " AND OrderTime <= @endTime";
                parameters.Add("endTime", endTime.Value);
            }

            var sql = $@"
                SELECT 
                    COUNT(*) as TotalCount,
                    SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) as PendingCount,
                    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) as ProcessingCount,
                    SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) as DeliveringCount,
                    SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) as CompletedCount,
                    SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) as CancelledCount,
                    ISNULL(SUM(TotalAmount), 0) as TotalAmount,
                    SUM(CASE WHEN OrderType = 0 THEN 1 ELSE 0 END) as NormalOrderCount,
                    SUM(CASE WHEN OrderType = 1 THEN 1 ELSE 0 END) as DeliveryOrderCount
                FROM T_Order {whereClause}";

            var result = await FindAllAsync<OrderStatistics>(sql, parameters);
            return result.FirstOrDefault() ?? new OrderStatistics();
        }
    }
}
