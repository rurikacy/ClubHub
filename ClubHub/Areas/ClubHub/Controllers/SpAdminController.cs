using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClubHub.Areas.ClubHub.Models;
using ClubHub.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repository.DbEntity;
using Repository.IRepository;
using TalkViewModel = Repository.IRepository.TalkViewModel;

namespace ClubHub.Areas.ClubHub.Controllers;

/// <summary>
///     超级管理员控制器
/// </summary>
[Area("ClubHub")]
public class SpAdminController(IAdminRepository adminRepository, IClubRepository clubRepository, ITalkRepository talkRepository, IHubContext<ChatHub> hubContext) : Controller
{
    /// <summary>
    ///     超级管理员个人信息页面
    /// </summary>
    /// <returns>显示超级管理员个人信息页面</returns>
    public async Task<IActionResult> Info()
    {
        string studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
        {
            return Json(new { Msg = false, Message = "用户未登录" });
        }

        const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId AND AdminType = 0";
        IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
        Admin admin = admins.FirstOrDefault();

        if (admin == null)
        {
            return Json(new { Msg = false, Message = "用户不存在" });
        }

        return PartialView(admin);
    }

    /// <summary>
    ///     更新超级管理员信息（手机号和密码）
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateInfo([FromForm] string phoneNumber, [FromForm] string password)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new { Msg = false, Message = "用户未登录" });
            }

            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId AND AdminType = 0";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin admin = admins.FirstOrDefault();

            if (admin == null)
            {
                return Json(new { Msg = false, Message = "用户不存在" });
            }

            // 验证手机号格式（如果填写了）
            if (!string.IsNullOrEmpty(phoneNumber) && !Regex.IsMatch(phoneNumber, @"^1[3-9]\d{9}$"))
            {
                return Json(new { Msg = false, Message = "请输入正确的手机号格式" });
            }

            // 更新手机号
            admin.PhoneNumber = phoneNumber;

            // 如果填写了密码则更新密码
            if (!string.IsNullOrWhiteSpace(password))
            {
                admin.Password = password;
            }

            bool result = await adminRepository.UpdateAsync(admin);
            if (result)
            {
                return Json(new { Msg = true, Message = "更新成功" });
            }

            return Json(new { Msg = false, Message = "更新失败" });
        }
        catch (Exception ex)
        {
            return Json(new { Msg = false, Message = "更新失败：" + ex.Message });
        }
    }

    #region 讨论中心管理功能

    /// <summary>
    ///     讨论中心管理页面
    /// </summary>
    /// <returns>显示超级管理员视角的讨论中心管理页面</returns>
    public IActionResult Talk()
    {
        string studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
        {
            return Json(new { Msg = false, Message = "用户未登录" });
        }
        return PartialView();
    }

    /// <summary>
    ///     获取所有社团列表（用于社团选择器）
    /// </summary>
    /// <returns>社团列表</returns>
    [HttpGet]
    public async Task<IActionResult> GetClubListForTalk()
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            IEnumerable<T_Club> clubs = await clubRepository.FindAllAsync<T_Club>();
            return Json(new ResponseMessage
            {
                Code = 0,
                Msg = true,
                Message = "获取成功",
                Data = clubs.Select(c => new { id = c.Id, name = c.Name })
            });
        }
        catch (Exception ex)
        {
            return Json(new ResponseMessage { Code = 1, Msg = false, Message = "获取失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     获取指定社团的讨论中心消息列表
    /// </summary>
    /// <param name="clubId">社团ID</param>
    /// <param name="page">页码</param>
    /// <param name="limit">每页数量</param>
    /// <returns>消息列表</returns>
    [HttpGet]
    public async Task<IActionResult> GetTalkList(int clubId, int page = 1, int limit = 50)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            if (clubId <= 0)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "请选择社团" });
            }

            // 获取当前超管信息
            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId AND AdminType = 0";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin currentAdmin = admins.FirstOrDefault();

            if (currentAdmin == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到超级管理员信息" });
            }

            // 获取消息列表
            IEnumerable<TalkViewModel> talks = await talkRepository.GetTalkListAsync(clubId, page, limit);

            // 计算每条消息的属性
            DateTime now = DateTime.Now;
            foreach (TalkViewModel talk in talks)
            {
                // 判断是否为当前用户发送的消息
                bool isSelf = talk.SenderId == currentAdmin.Id && talk.SenderType == 2;
                talk.IsSelf = isSelf;
                // 超管自己的消息可以撤回（60秒内）
                talk.CanWithdraw = isSelf
                                   && talk.Status == 0
                                   && (now - talk.SendTime).TotalSeconds <= 60;
                // 所有未删除消息超管都可以删除
                talk.CanDelete = talk.Status == 0;
            }

            return Json(new ResponseMessage
            {
                Code = 0,
                Msg = true,
                Message = "获取成功",
                Data = talks.Reverse() // 按时间正序显示（最新的在底部）
            });
        }
        catch (Exception ex)
        {
            return Json(new ResponseMessage { Code = 1, Msg = false, Message = "获取失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     超管发送消息
    /// </summary>
    /// <param name="clubId">社团ID</param>
    /// <param name="content">消息内容</param>
    /// <returns>发送结果</returns>
    [HttpPost]
    public async Task<IActionResult> SendTalk([FromForm] int clubId, [FromForm] string content)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            if (clubId <= 0)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "请选择社团" });
            }

            // 验证消息内容非空
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息内容不能为空" });
            }

            // 获取当前超管信息
            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId AND AdminType = 0";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin currentAdmin = admins.FirstOrDefault();

            if (currentAdmin == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到超级管理员信息" });
            }

            // 发送消息（SenderType = 2 表示超管）
            Talk talk = await talkRepository.SendTalkAsync(clubId, currentAdmin.Id, 2, content);

            if (talk == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "发送失败" });
            }

            // 通过SignalR广播新消息到社团群组
            await hubContext.Clients.Group($"club_{clubId}").SendAsync("ReceiveMessage", new
            {
                id = talk.Id,
                clubId = talk.ClubId,
                senderId = talk.SenderId,
                senderType = talk.SenderType,
                senderName = currentAdmin.Name,
                senderIdentity = "管理员",
                content = talk.Content,
                sendTime = talk.SendTime,
                status = talk.Status
            });

            return Json(new ResponseMessage { Code = 0, Msg = true, Message = "发送成功", Data = talk });
        }
        catch (Exception ex)
        {
            return Json(new ResponseMessage { Code = 1, Msg = false, Message = "发送失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     超管撤回消息（只能撤回自己的消息，且在60秒内）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>撤回结果</returns>
    [HttpPost]
    public async Task<IActionResult> WithdrawTalk(int id)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            // 获取当前超管信息
            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId AND AdminType = 0";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin currentAdmin = admins.FirstOrDefault();

            if (currentAdmin == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到超级管理员信息" });
            }

            // 获取消息信息进行验证
            Talk talk = await talkRepository.SingleAsync(id);
            if (talk == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息不存在" });
            }

            // 验证消息是否已删除
            if (talk.Status == 1)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息已被删除" });
            }

            // 验证是否是自己的消息
            if (talk.SenderId != currentAdmin.Id || talk.SenderType != 2)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "只能撤回自己的消息" });
            }

            // 验证时间限制（60秒内）
            if ((DateTime.Now - talk.SendTime).TotalSeconds > 60)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息发送已超过1分钟，无法撤回" });
            }

            // 执行撤回
            bool result = await talkRepository.WithdrawAsync(id, currentAdmin.Id, 2);

            if (!result)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "撤回失败" });
            }

            return Json(new ResponseMessage { Code = 0, Msg = true, Message = "撤回成功" });
        }
        catch (Exception ex)
        {
            return Json(new ResponseMessage { Code = 1, Msg = false, Message = "撤回失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     超管删除消息（无时间限制）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>删除结果</returns>
    [HttpPost]
    public async Task<IActionResult> DeleteTalk(int id)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            // 获取消息信息进行验证
            Talk talk = await talkRepository.SingleAsync(id);
            if (talk == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息不存在" });
            }

            // 验证消息是否已删除
            if (talk.Status == 1)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息已被删除" });
            }

            // 执行删除（无时间限制）
            bool result = await talkRepository.DeleteAsync(id);

            if (!result)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "删除失败" });
            }

            return Json(new ResponseMessage { Code = 0, Msg = true, Message = "删除成功" });
        }
        catch (Exception ex)
        {
            return Json(new ResponseMessage { Code = 1, Msg = false, Message = "删除失败：" + ex.Message });
        }
    }

    #endregion
}