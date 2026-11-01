using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     讨论中心消息视图模型
    /// </summary>
    public class TalkViewModel
    {
        /// <summary>
        ///     消息ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        ///     所属社团ID
        /// </summary>
        [JsonPropertyName("clubId")]
        public int ClubId { get; set; }

        /// <summary>
        ///     发送人ID
        /// </summary>
        [JsonPropertyName("senderId")]
        public int SenderId { get; set; }

        /// <summary>
        ///     发送人类型: 0=成员, 1=管理员, 2=超管
        /// </summary>
        [JsonPropertyName("senderType")]
        public byte SenderType { get; set; }

        /// <summary>
        ///     发送人姓名
        /// </summary>
        [JsonPropertyName("senderName")]
        public string SenderName { get; set; }

        /// <summary>
        ///     身份标签（如"超级管理员"、"管理员"、"xx部部长"等）
        /// </summary>
        [JsonPropertyName("senderIdentity")]
        public string SenderIdentity { get; set; }

        /// <summary>
        ///     消息内容
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        ///     发送时间
        /// </summary>
        [JsonPropertyName("sendTime")]
        public DateTime SendTime { get; set; }

        /// <summary>
        ///     状态: 0=正常, 1=已删除
        /// </summary>
        [JsonPropertyName("status")]
        public byte Status { get; set; }

        /// <summary>
        ///     是否可撤回（发送者本人且60秒内）
        /// </summary>
        [JsonPropertyName("canWithdraw")]
        public bool CanWithdraw { get; set; }

        /// <summary>
        ///     是否可删除（管理员/超管可删除）
        /// </summary>
        [JsonPropertyName("canDelete")]
        public bool CanDelete { get; set; }

        /// <summary>
        ///     是否为当前用户发送的消息
        /// </summary>
        [JsonPropertyName("isSelf")]
        public bool IsSelf { get; set; }
    }

    /// <summary>
    ///     讨论中心仓储接口
    /// </summary>
    public interface ITalkRepository : IBaseRepository<Talk>
    {
        /// <summary>
        ///     获取社团消息列表
        /// </summary>
        /// <param name="clubId">社团ID</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页数量</param>
        /// <returns>消息视图模型列表</returns>
        Task<IEnumerable<TalkViewModel>> GetTalkListAsync(int clubId, int page, int limit);

        /// <summary>
        ///     发送消息
        /// </summary>
        /// <param name="clubId">社团ID</param>
        /// <param name="senderId">发送人ID</param>
        /// <param name="senderType">发送人类型: 0=成员, 1=管理员, 2=超管</param>
        /// <param name="content">消息内容</param>
        /// <returns>发送成功返回消息实体，失败返回null</returns>
        Task<Talk> SendTalkAsync(int clubId, int senderId, byte senderType, string content);

        /// <summary>
        ///     撤回消息（仅发送者本人可撤回，且需在60秒内）
        /// </summary>
        /// <param name="talkId">消息ID</param>
        /// <param name="senderId">发送人ID</param>
        /// <param name="senderType">发送人类型</param>
        /// <returns>撤回成功返回true，失败返回false</returns>
        Task<bool> WithdrawAsync(int talkId, int senderId, byte senderType);

        /// <summary>
        ///     删除消息（管理员/超管可删除，无时间限制）
        /// </summary>
        /// <param name="talkId">消息ID</param>
        /// <returns>删除成功返回true，失败返回false</returns>
        Task<bool> DeleteAsync(int talkId);

        /// <summary>
        ///     获取发送人身份标签
        /// </summary>
        /// <param name="senderId">发送人ID</param>
        /// <param name="senderType">发送人类型: 0=成员, 1=管理员, 2=超管</param>
        /// <param name="clubId">社团ID</param>
        /// <returns>身份标签字符串</returns>
        Task<string> GetSenderIdentityAsync(int senderId, byte senderType, int clubId);
    }
}