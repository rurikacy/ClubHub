using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     讨论中心仓储实现
    /// </summary>
    public class TalkRepository : BaseRepository<Talk>, ITalkRepository
    {
        public TalkRepository(IConfiguration config) : base(config)
        {
        }

        /// <summary>
        ///     获取社团消息列表
        /// </summary>
        public async Task<IEnumerable<TalkViewModel>> GetTalkListAsync(int clubId, int page, int limit)
        {
            var sql = @"
                SELECT 
                    t.Id,
                    t.ClubId,
                    t.SenderId,
                    t.SenderType,
                    t.Content,
                    t.SendTime,
                    t.Status,
                    CASE 
                        WHEN t.SenderType = 2 THEN (SELECT Name FROM T_Admin WHERE Id = t.SenderId)
                        WHEN t.SenderType = 1 THEN (SELECT Name FROM T_Admin WHERE Id = t.SenderId)
                        ELSE (SELECT Name FROM T_Member WHERE Id = t.SenderId)
                    END AS SenderName
                FROM T_Talk t
                WHERE t.ClubId = @ClubId
                ORDER BY t.SendTime DESC
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

            var talks = await FindAllAsync<TalkViewModel>(sql, new { ClubId = clubId, Offset = (page - 1) * limit, Limit = limit });

            // 计算每条消息的身份标签
            foreach (var talk in talks)
            {
                talk.SenderIdentity = await GetSenderIdentityAsync(talk.SenderId, talk.SenderType, clubId);
            }

            return talks;
        }

        /// <summary>
        ///     发送消息
        /// </summary>
        public async Task<Talk> SendTalkAsync(int clubId, int senderId, byte senderType, string content)
        {
            // 验证消息内容非空
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var talk = new Talk
            {
                ClubId = clubId,
                SenderId = senderId,
                SenderType = senderType,
                Content = content.Trim(),
                SendTime = DateTime.Now,
                Status = 0 // 正常状态
            };

            var id = await AddAsync(talk);
            talk.Id = id;
            return talk;
        }

        /// <summary>
        ///     撤回消息（仅发送者本人可撤回，且需在60秒内）
        /// </summary>
        public async Task<bool> WithdrawAsync(int talkId, int senderId, byte senderType)
        {
            var talk = await SingleAsync(talkId);
            if (talk == null)
            {
                return false;
            }

            // 验证是否为发送者本人
            if (talk.SenderId != senderId || talk.SenderType != senderType)
            {
                return false;
            }

            // 验证消息是否已删除
            if (talk.Status == 1)
            {
                return false;
            }

            // 验证是否在60秒内
            if ((DateTime.Now - talk.SendTime).TotalSeconds > 60)
            {
                return false;
            }

            // 更新状态为已删除
            talk.Status = 1;
            return await UpdateAsync(talk);
        }

        /// <summary>
        ///     删除消息（管理员/超管可删除，无时间限制）
        /// </summary>
        public async Task<bool> DeleteAsync(int talkId)
        {
            var talk = await SingleAsync(talkId);
            if (talk == null)
            {
                return false;
            }

            // 验证消息是否已删除
            if (talk.Status == 1)
            {
                return false;
            }

            // 更新状态为已删除（软删除，保留记录）
            talk.Status = 1;
            return await UpdateAsync(talk);
        }

        /// <summary>
        ///     获取发送人身份标签
        /// </summary>
        public async Task<string> GetSenderIdentityAsync(int senderId, byte senderType, int clubId)
        {
            // 超级管理员
            if (senderType == 2)
            {
                return "管理员";
            }

            // 社团管理员
            if (senderType == 1)
            {
                return "社长";
            }

            // 成员 - 查询部门信息
            var sql = @"
                SELECT 
                    d.Name AS DutyName,
                    dm.Role
                FROM T_DutyMember dm
                INNER JOIN T_Duty d ON dm.DutyId = d.Id
                WHERE dm.MemberId = @MemberId AND d.ClubId = @ClubId";

            var dutyInfo = await FindAllAsync<DutyMemberInfo>(sql, new { MemberId = senderId, ClubId = clubId });

            foreach (var info in dutyInfo)
            {
                if (info.Role == 1)
                {
                    return $"{info.DutyName}部长";
                }
                return info.DutyName;
            }

            // 普通成员（未加入任何部门）
            return "成员";
        }

        /// <summary>
        ///     部门成员信息辅助类
        /// </summary>
        private class DutyMemberInfo
        {
            public string DutyName { get; set; }
            public byte Role { get; set; }
        }
    }
}