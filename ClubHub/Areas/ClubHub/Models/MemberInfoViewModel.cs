using System;

namespace ClubHub.Areas.ClubHub.Models;

/// <summary>
///     成员个人信息视图模型
/// </summary>
public class MemberInfoViewModel
{
    /// <summary>
    ///     成员ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     学号
    /// </summary>
    public string StudentId { get; set; }

    /// <summary>
    ///     姓名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     性别 0：未知 1：男 2：女
    /// </summary>
    public byte Sex { get; set; }

    /// <summary>
    ///     手机号
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    ///     社团ID
    /// </summary>
    public int? ClubId { get; set; }

    /// <summary>
    ///     社团名称
    /// </summary>
    public string ClubName { get; set; }

    /// <summary>
    ///     部门名称
    /// </summary>
    public string DutyName { get; set; }

    /// <summary>
    ///     部门职位 0：成员 1：部长
    /// </summary>
    public byte? DutyRole { get; set; }

    /// <summary>
    ///     注册日期
    /// </summary>
    public DateTime EnterData { get; set; }
}