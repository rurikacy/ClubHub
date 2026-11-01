using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClubHub.Areas.ClubHub.Models;
using ClubHub.Hubs;
using Dapper;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repository.DbEntity;
using Repository.IRepository;
using TalkViewModel = Repository.IRepository.TalkViewModel;

namespace ClubHub.Areas.ClubHub.Controllers;

[Area("ClubHub")]
public class AdminController(IAdminRepository adminRepository, IClubRepository clubRepository, IMemberRepository memberRepository, ITalkRepository talkRepository, IHubContext<ChatHub> hubContext) : Controller
{
    // GET: ClubHub/Admin/GetClubList
    [HttpGet]
    public async Task<IActionResult> GetClubList()
    {
        IEnumerable<T_Club> clubs = await clubRepository.FindAllAsync<T_Club>();
        return Json(clubs.Select(c => new { c.Id, c.Name }));
    }

    // GET: ClubHub/Admin
    public IActionResult Index()
    {
        return View();
    }

    // GET: ClubHub/Admin/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(string Name = "", int? ClubId = null, int? AdminType = null, int page = 1, int limit = 10)
    {
        string sql = """
                     SELECT a.*, c.Name as ClubName,
                            (SELECT COUNT(*) FROM T_Member WHERE ClubId = a.ClubId) as MemberCount
                     FROM T_Admin a
                     LEFT JOIN T_Club c ON a.ClubId = c.Id
                     WHERE 1=1
                     """;
        DynamicParameters parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(Name))
        {
            sql += " AND a.Name LIKE @Name";
            parameters.Add("Name", $"%{Name}%");
        }

        if (ClubId.HasValue)
        {
            sql += " AND a.ClubId = @ClubId";
            parameters.Add("ClubId", ClubId.Value);
        }

        if (AdminType.HasValue)
        {
            sql += " AND a.AdminType = @AdminType";
            parameters.Add("AdminType", AdminType.Value);
        }

        PageData<AdminViewModel> pageData = await adminRepository.PagedAsync<AdminViewModel>(sql, parameters, "Id DESC", page, limit);

        ResponseMessage response = new ResponseMessage
        {
            Code = 0,
            Msg = true,
            Message = "获取成功",
            Count = pageData.TotalCount,
            Data = pageData.Items
        };
        return Json(response);
    }

    // GET: ClubHub/Admin/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        Admin admin = await adminRepository.SingleAsync(id);
        if (admin == null)
        {
            return NotFound();
        }
        return Json(admin);
    }

    // POST: ClubHub/Admin/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Admin admin)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (admin == null)
            {
                response.Message = "管理员信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            admin.Id = 0;
            admin.EnterData = DateTime.Now;

            if (string.IsNullOrWhiteSpace(admin.Name))
            {
                response.Message = "管理员姓名不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(admin.Password))
            {
                response.Message = "密码不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证管理员类型和社团ID的关系
            if (admin.AdminType == 0 && admin.ClubId.HasValue)
            {
                response.Message = "超级管理员不能关联社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (admin.AdminType == 1 && !admin.ClubId.HasValue)
            {
                response.Message = "社长必须关联社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            int result = await adminRepository.AddAsync(admin);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? admin : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Admin/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Admin admin)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (admin is not { Id: > 0 })
            {
                response.Message = "管理员信息无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(admin.Name))
            {
                response.Message = "管理员姓名不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 获取现有管理员信息
            Admin existingAdmin = await adminRepository.SingleAsync(admin.Id);
            if (existingAdmin == null)
            {
                response.Message = "未找到该管理员";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证管理员类型和社团ID的关系
            if (admin.AdminType == 0 && admin.ClubId.HasValue)
            {
                response.Message = "超级管理员不能关联社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (admin.AdminType == 1 && !admin.ClubId.HasValue)
            {
                response.Message = "社长必须关联社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 更新字段
            existingAdmin.StudentId = admin.StudentId;
            existingAdmin.Name = admin.Name;
            existingAdmin.Sex = admin.Sex;
            existingAdmin.PhoneNumber = admin.PhoneNumber;
            existingAdmin.AdminType = admin.AdminType;
            existingAdmin.ClubId = admin.ClubId;

            // 如果密码不为空，则更新密码
            if (!string.IsNullOrWhiteSpace(admin.Password))
            {
                existingAdmin.Password = admin.Password;
            }

            bool result = await adminRepository.UpdateAsync(existingAdmin);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "编辑成功" : "编辑失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? existingAdmin : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "编辑失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Admin/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Admin admin = await adminRepository.SingleAsync(id);
            if (admin == null)
            {
                response.Message = "管理员不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 如果是社长(AdminType=1)，检查其社团是否还有成员
            if (admin.AdminType == 1 && admin.ClubId.HasValue)
            {
                const string countSql = "SELECT COUNT(*) FROM T_Member WHERE ClubId = @ClubId";
                IEnumerable<int> countResult = await memberRepository.FindAllAsync<int>(countSql, new { ClubId = admin.ClubId.Value });
                int memberCount = countResult.FirstOrDefault();
                if (memberCount > 0)
                {
                    response.Message = $"该社长对应的社团还有 {memberCount} 名成员，无法删除社长";
                    response.Msg = false;
                    response.Code = 1;
                    return Json(response);
                }
            }

            bool result = await adminRepository.DeleteAsync(admin);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "删除成功" : "删除失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? admin : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "删除失败：" + ex.Message;
        }
        return Json(response);
    }

    /// <summary>
    ///     管理员个人信息页面
    /// </summary>
    /// <returns>显示管理员个人信息页面</returns>
    public async Task<IActionResult> Info()
    {
        string studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
        {
            return Json(new { Msg = false, Message = "用户未登录" });
        }

        const string sql = @"
            SELECT a.Id, a.StudentId, a.Name, a.Sex, a.PhoneNumber, a.AdminType, a.ClubId, c.Name AS ClubName, a.EnterData
            FROM dbo.T_Admin a
            LEFT JOIN dbo.T_Club c ON a.ClubId = c.Id
            WHERE a.StudentId = @studentId";

        IEnumerable<AdminInfoViewModel> admins = await adminRepository.FindAllAsync<AdminInfoViewModel>(sql, new { studentId });
        AdminInfoViewModel adminInfo = admins.FirstOrDefault();

        if (adminInfo == null)
        {
            return Json(new { Msg = false, Message = "用户不存在" });
        }

        return PartialView(adminInfo);
    }

    /// <summary>
    ///     更新管理员信息（手机号和密码）
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

            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId";
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

    #region 讨论中心功能

    /// <summary>
    ///     讨论中心页面
    /// </summary>
    /// <returns>显示管理员视角的讨论中心页面</returns>
    public IActionResult Talk()
    {
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (string.IsNullOrEmpty(sessionClubId))
        {
            return Json(new { Msg = false, Message = "您尚未关联任何社团" });
        }
        ViewBag.ClubId = sessionClubId;
        return PartialView();
    }

    /// <summary>
    ///     获取讨论中心消息列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="limit">每页数量</param>
    /// <returns>消息列表</returns>
    [HttpGet]
    public async Task<IActionResult> GetTalkList(int page = 1, int limit = 50)
    {
        try
        {
            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (string.IsNullOrEmpty(sessionClubId) || !int.TryParse(sessionClubId, out int clubId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "您尚未关联任何社团" });
            }

            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            // 获取当前管理员信息
            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin currentAdmin = admins.FirstOrDefault();

            if (currentAdmin == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到管理员信息" });
            }

            // 获取消息列表
            IEnumerable<TalkViewModel> talks = await talkRepository.GetTalkListAsync(clubId, page, limit);

            // 计算每条消息的属性
            DateTime now = DateTime.Now;
            foreach (TalkViewModel talk in talks)
            {
                // 判断是否为当前用户发送的消息
                bool isSelf = talk.SenderId == currentAdmin.Id && talk.SenderType == 1;
                talk.IsSelf = isSelf;
                // 管理员自己的消息可以撤回（60秒内）
                talk.CanWithdraw = isSelf
                                   && talk.Status == 0
                                   && (now - talk.SendTime).TotalSeconds <= 60;
                // 所有未删除消息管理员都可以删除
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
    ///     发送消息
    /// </summary>
    /// <param name="content">消息内容</param>
    /// <returns>发送结果</returns>
    [HttpPost]
    public async Task<IActionResult> SendTalk([FromForm] string content)
    {
        try
        {
            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (string.IsNullOrEmpty(sessionClubId) || !int.TryParse(sessionClubId, out int clubId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "您尚未关联任何社团" });
            }

            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            // 验证消息内容非空
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息内容不能为空" });
            }

            // 获取当前管理员信息
            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin currentAdmin = admins.FirstOrDefault();

            if (currentAdmin == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到管理员信息" });
            }

            // 发送消息（SenderType = 1 表示管理员）
            Talk talk = await talkRepository.SendTalkAsync(clubId, currentAdmin.Id, 1, content);

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
                senderIdentity = "社长",
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
    ///     撤回消息（管理员只能撤回自己的消息，且在60秒内）
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

            // 获取当前管理员信息
            const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId";
            IEnumerable<Admin> admins = await adminRepository.FindAllAsync<Admin>(sql, new { studentId });
            Admin currentAdmin = admins.FirstOrDefault();

            if (currentAdmin == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到管理员信息" });
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
            if (talk.SenderId != currentAdmin.Id || talk.SenderType != 1)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "只能撤回自己的消息" });
            }

            // 验证时间限制（60秒内）
            if ((DateTime.Now - talk.SendTime).TotalSeconds > 60)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息发送已超过1分钟，无法撤回" });
            }

            // 执行撤回
            bool result = await talkRepository.WithdrawAsync(id, currentAdmin.Id, 1);

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
    ///     删除消息（管理员可删除任何消息，无时间限制）
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

/// <summary>
///     管理员视图模型（用于列表显示）
/// </summary>
public class AdminViewModel
{
    public int Id { get; set; }
    public string StudentId { get; set; }
    public string Name { get; set; }
    public byte Sex { get; set; }
    public string PhoneNumber { get; set; }
    public byte AdminType { get; set; }
    public int? ClubId { get; set; }
    public string ClubName { get; set; }
    public DateTime EnterData { get; set; }
    /// <summary>
    /// 社团成员数量（仅社长有效）
    /// </summary>
    public int MemberCount { get; set; }
}