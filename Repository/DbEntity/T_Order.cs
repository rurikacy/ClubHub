using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 订单表
    /// </summary>
    [Table("T_Order")]
    public class Order : BaseModel
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string OrderNo { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 用户类型 0：管理员 1：成员
        /// </summary>
        [Required]
        public byte UserType { get; set; }

        /// <summary>
        /// 订单类型 0：普通订单 1：外卖订单
        /// </summary>
        [Required]
        public byte OrderType { get; set; }

        /// <summary>
        /// 订单状态 0：待处理 1：处理中 2：配送中 3：已完成 4：已取消
        /// </summary>
        [Required]
        public byte Status { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [MaxLength(50)]
        public string ReceiverName { get; set; }

        /// <summary>
        /// 收货人电话
        /// </summary>
        [MaxLength(20)]
        public string ReceiverPhone { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        [MaxLength(500)]
        public string ReceiverAddress { get; set; }

        /// <summary>
        /// 配送方式 0：自提 1：配送
        /// </summary>
        public byte? DeliveryType { get; set; }

        /// <summary>
        /// 配送费用
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DeliveryFee { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        public string Remark { get; set; }

        /// <summary>
        /// 取消原因
        /// </summary>
        [MaxLength(500)]
        public string CancelReason { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        [Required]
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
