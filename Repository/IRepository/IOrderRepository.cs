using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Data;
using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    /// 订单管理接口
    /// </summary>
    public interface IOrderRepository : IDependencyRepository
    {
        /// <summary>
        /// 根据ID获取订单
        /// </summary>
        Task<Order> GetByIdAsync(int id);

        /// <summary>
        /// 根据订单编号获取订单
        /// </summary>
        Task<Order> GetByOrderNoAsync(string orderNo);

        /// <summary>
        /// 分页获取订单列表
        /// </summary>
        Task<PageData<Order>> GetPagedAsync(int page, int limit, byte? orderType = null, byte? status = null, 
            string keyword = null, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// 添加订单
        /// </summary>
        Task<int> InsertAsync(Order order);

        /// <summary>
        /// 更新订单
        /// </summary>
        Task<bool> ModifyAsync(Order order);

        /// <summary>
        /// 更新订单状态
        /// </summary>
        Task<bool> UpdateStatusAsync(int orderId, byte newStatus, string remark = null);

        /// <summary>
        /// 添加订单状态日志
        /// </summary>
        Task<bool> AddStatusLogAsync(OrderStatusLog log);

        /// <summary>
        /// 删除订单
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 获取订单商品明细
        /// </summary>
        Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId);

        /// <summary>
        /// 添加订单商品明细
        /// </summary>
        Task<bool> AddOrderItemsAsync(List<OrderItem> items);

        /// <summary>
        /// 获取订单状态日志
        /// </summary>
        Task<IEnumerable<OrderStatusLog>> GetStatusLogsAsync(int orderId);

        /// <summary>
        /// 获取订单统计信息
        /// </summary>
        Task<OrderStatistics> GetStatisticsAsync(DateTime? startTime = null, DateTime? endTime = null);
    }

    /// <summary>
    /// 订单统计信息
    /// </summary>
    public class OrderStatistics
    {
        /// <summary>
        /// 订单总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 待处理订单数
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 处理中订单数
        /// </summary>
        public int ProcessingCount { get; set; }

        /// <summary>
        /// 配送中订单数
        /// </summary>
        public int DeliveringCount { get; set; }

        /// <summary>
        /// 已完成订单数
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// 已取消订单数
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 普通订单数
        /// </summary>
        public int NormalOrderCount { get; set; }

        /// <summary>
        /// 外卖订单数
        /// </summary>
        public int DeliveryOrderCount { get; set; }
    }
}
