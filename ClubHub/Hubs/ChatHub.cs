using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ClubHub.Hubs;

/// <summary>
/// 讨论中心 SignalR Hub
/// 用于实现社团内的实时消息推送
/// </summary>
public class ChatHub : Hub
{
    // 存储用户连接信息：ConnectionId -> ClubId
    private static readonly ConcurrentDictionary<string, int> UserClubMapping = new();

    /// <summary>
    /// 加入社团聊天室
    /// </summary>
    /// <param name="clubId">社团ID</param>
    public async Task JoinClub(int clubId)
    {
        if (clubId <= 0) return;
        
        // 记录用户所属社团
        UserClubMapping[Context.ConnectionId] = clubId;
        
        // 将用户加入对应社团的组
        await Groups.AddToGroupAsync(Context.ConnectionId, $"club_{clubId}");
    }

    /// <summary>
    /// 离开社团聊天室
    /// </summary>
    /// <param name="clubId">社团ID</param>
    public async Task LeaveClub(int clubId)
    {
        if (clubId <= 0) return;
        
        // 移除用户社团记录
        UserClubMapping.TryRemove(Context.ConnectionId, out _);
        
        // 将用户从社团组中移除
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"club_{clubId}");
    }

    /// <summary>
    /// 连接断开时清理
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // 移除用户社团记录
        if (UserClubMapping.TryRemove(Context.ConnectionId, out int clubId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"club_{clubId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
