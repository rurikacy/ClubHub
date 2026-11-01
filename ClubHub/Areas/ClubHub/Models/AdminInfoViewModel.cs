using System;

namespace ClubHub.Areas.ClubHub.Models;

/// <summary>
///     管理员个人信息视图模型
/// </summary>
public class AdminInfoViewModel
{
    /// <summary>
    ///     管理员ID
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
    ///     管理员类别 0：超级管理员 1：社团管理员
    /// </summary>
    public byte AdminType { get; set; }

    /// <summary>
    ///     社团ID
    /// </summary>
    public int? ClubId { get; set; }

    /// <summary>
    ///     社团名称
    /// </summary>
    public string ClubName { get; set; }

    /// <summary>
    ///     注册日期
    /// </summary>
    public DateTime EnterData { get; set; }
}