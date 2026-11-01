using System;
using System.Collections.Generic;

namespace ClubHub.Areas.ClubHub.Models;

/// <summary>
///     社团报名视图模型
/// </summary>
public class ClubEnrollViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MemberNum { get; set; }
    public DateTime EnterData { get; set; }

    // 申请状态
    public bool HasApplied { get; set; }
    public byte? ApplyStatus { get; set; } // null=未申请, 0=已批准, 1=已退回, 其他=待审核

    // 近期活动
    public List<RecentActivity> RecentActivities { get; set; }

    // 近期通知
    public List<RecentMessage> RecentMessages { get; set; }
}

/// <summary>
///     近期活动
/// </summary>
public class RecentActivity
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int JoinNum { get; set; }
}

/// <summary>
///     近期通知
/// </summary>
public class RecentMessage
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime EnterData { get; set; }
}