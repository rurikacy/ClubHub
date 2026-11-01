using Repository.DbEntity;

namespace ClubHub.Areas.ClubHub.Models;

/// <summary>
///     讨论中心消息视图模型
/// </summary>
public class TalkViewModel : Talk
{
    /// <summary>
    ///     发送人姓名
    /// </summary>
    public string SenderName { get; set; }

    /// <summary>
    ///     身份标签（如"超级管理员"、"管理员"、"xx部部长"、"xx部"、"成员"）
    /// </summary>
    public string SenderIdentity { get; set; }

    /// <summary>
    ///     是否可撤回（发送者本人且60秒内）
    /// </summary>
    public bool CanWithdraw { get; set; }

    /// <summary>
    ///     是否可删除（管理员/超管可删除任何消息）
    /// </summary>
    public bool CanDelete { get; set; }
}

/// <summary>
///     发送消息请求模型
/// </summary>
public class SendTalkRequest
{
    /// <summary>
    ///     社团ID
    /// </summary>
    public int ClubId { get; set; }

    /// <summary>
    ///     消息内容
    /// </summary>
    public string Content { get; set; }
}