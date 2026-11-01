using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClubHub.Areas.ClubHub.Models;
using Dapper;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DbEntity;
using Repository.IRepository;

namespace ClubHub.Areas.ClubHub.Controllers;

[Area("ClubHub")]
public class ActiveController(IActiveRepository activeRepository, IActiveSignRepository activeSignRepository, IMemberRepository memberRepository) : Controller
{
    // GET: ClubHub/Active
    public Task<IActionResult> Index()
    {
        ViewBag.CurrentClubId = HttpContext.Session.GetString("ClubId");
        ViewBag.CurrentUserId = HttpContext.Session.GetString("UserId");
        return Task.FromResult<IActionResult>(View());
    }

    // GET: ClubHub/Active/MemberIndex (成员端活动管理)
    public Task<IActionResult> MemberIndex()
    {
        return Task.FromResult<IActionResult>(View());
    }

    // GET: ClubHub/Active/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(string Title = "", int? ClubId = null, int page = 1, int limit = 10)
    {
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
        {
            ClubId = adminClubId;
        }

        string studentId = HttpContext.Session.GetString("StudentId");

        string sql = """
                     SELECT a.Id, a.ClubId, a.AdminId, a.Title, a.Content, a.EnterData, 
                            a.StartTime, a.EndTime, a.JoinNum, a.Place,
                            ISNULL(a.MinParticipants, 0) AS MinParticipants,
                            ISNULL(a.[Status], 0) AS [Status],
                            a.CancelReason,
                            c.Name AS ClubName, ad.Name AS AdminName, s.SignType,
                            CASE 
                                WHEN ISNULL(a.[Status], 0) = 1 THEN 3
                                WHEN a.EndTime <= GETDATE() THEN 2
                                WHEN a.StartTime <= GETDATE() THEN 1
                                ELSE 0
                            END AS StatusOrder
                     FROM T_Active a 
                     LEFT JOIN T_Club c ON a.ClubId = c.Id 
                     LEFT JOIN T_Admin ad ON a.AdminId = ad.Id 
                     LEFT JOIN T_Member m ON m.ClubId = a.ClubId AND m.StudentId = @StudentId
                     LEFT JOIN T_ActiveSign s ON s.ActiveId = a.Id AND s.MemberId = m.Id
                     WHERE 1=1
                     """;
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("StudentId", studentId);

        if (!string.IsNullOrEmpty(Title))
        {
            sql += " AND a.Title LIKE @Title";
            parameters.Add("Title", $"%{Title}%");
        }

        if (ClubId.HasValue)
        {
            sql += " AND a.ClubId = @ClubId";
            parameters.Add("ClubId", ClubId.Value);
        }

        PageData<ActiveViewModel> pageData = await activeRepository.PagedAsync<ActiveViewModel>(sql, parameters, "StatusOrder ASC, StartTime ASC", page, limit);

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

    // GET: ClubHub/Active/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        Active active = await activeRepository.SingleAsync(id);
        if (active == null)
        {
            return NotFound();
        }
        return Json(active);
    }

    // POST: ClubHub/Active/Revoke (撤销报名)
    [HttpPost]
    public async Task<IActionResult> Revoke(int activeId)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                response.Message = "请先登录";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            Active active = await activeRepository.SingleAsync(activeId);
            if (active == null)
            {
                response.Message = "活动不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 查找该用户在该社团的成员信息
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>("SELECT * FROM T_Member WHERE StudentId = @StudentId AND ClubId = @ClubId", new { StudentId = studentId, active.ClubId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                response.Message = "您不是该社团成员";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 查找报名记录
            IEnumerable<ActiveSign> signs = await activeSignRepository.FindAllAsync<ActiveSign>("SELECT * FROM T_ActiveSign WHERE MemberId = @MemberId AND ActiveId = @ActiveId", new { MemberId = member.Id, ActiveId = active.Id });
            ActiveSign sign = signs.FirstOrDefault();

            if (sign == null)
            {
                response.Message = "您尚未报名该活动";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (sign.SignType == 0)
            {
                response.Message = "您已撤销报名，无需重复操作";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 更新为已撤销
            sign.SignType = 0;
            bool result = await activeSignRepository.UpdateAsync(sign);

            // 更新活动的参与人数 (减少)
            if (result)
            {
                if (active.JoinNum > 0)
                {
                    active.JoinNum -= 1;
                    await activeRepository.UpdateAsync(active);
                }
            }

            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "撤销成功" : "撤销失败";
            response.Count = result ? 1 : 0;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "撤销失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Active/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Active active)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (active == null)
            {
                response.Message = "活动信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
            {
                active.ClubId = adminClubId;
            }

            string sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int adminId))
            {
                active.AdminId = adminId;
            }

            active.Id = 0;
            active.EnterData = DateTime.Now;
            active.JoinNum = 0;
            active.Status = 0; // 默认状态为正常

            if (string.IsNullOrWhiteSpace(active.Title))
            {
                response.Message = "活动标题不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(active.Place))
            {
                response.Message = "活动地点不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            int result = await activeRepository.AddAsync(active);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? active : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Active/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Active active)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (active is not { Id: > 0 })
            {
                response.Message = "活动信息无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 先获取原有活动信息，保留不可修改的字段
            Active existingActive = await activeRepository.SingleAsync(active.Id);
            if (existingActive == null)
            {
                response.Message = "活动不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
            {
                active.ClubId = adminClubId;
            }
            else
            {
                active.ClubId = existingActive.ClubId;
            }

            string sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int adminId))
            {
                active.AdminId = adminId;
            }
            else
            {
                active.AdminId = existingActive.AdminId;
            }

            if (string.IsNullOrWhiteSpace(active.Title))
            {
                response.Message = "活动标题不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(active.Place))
            {
                response.Message = "活动地点不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 保留原有的 EnterData、JoinNum 和 Status
            active.EnterData = existingActive.EnterData;
            active.JoinNum = existingActive.JoinNum;
            active.Status = existingActive.Status;

            bool result = await activeRepository.UpdateAsync(active);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "编辑成功" : "编辑失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? active : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "编辑失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Active/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Active active = await activeRepository.SingleAsync(id);
            if (active == null)
            {
                response.Message = "活动不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await activeRepository.DeleteAsync(active);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "删除成功" : "删除失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? active : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "删除失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Active/Sign (活动报名)
    [HttpPost]
    public async Task<IActionResult> Sign([FromForm] ActiveSign activeSign)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (activeSign == null)
            {
                response.Message = "报名信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                response.Message = "请先登录";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            Active active = await activeRepository.SingleAsync(activeSign.ActiveId);
            if (active == null)
            {
                response.Message = "活动不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 检查活动是否已取消
            if (active.Status == 1)
            {
                response.Message = "该活动已取消，无法报名";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 检查活动是否已开始或已结束
            if (active.StartTime <= DateTime.Now)
            {
                response.Message = "该活动已开始或已结束，无法报名";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 查找该用户在该社团的成员信息
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>("SELECT * FROM T_Member WHERE StudentId = @StudentId AND ClubId = @ClubId", new { StudentId = studentId, active.ClubId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                response.Message = "您不是该社团成员，无法报名";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 检查是否已经报名
            IEnumerable<ActiveSign> signs = await activeSignRepository.FindAllAsync<ActiveSign>("SELECT * FROM T_ActiveSign WHERE MemberId = @MemberId AND ActiveId = @ActiveId", new { MemberId = member.Id, ActiveId = active.Id });
            ActiveSign existingSign = signs.FirstOrDefault();
            if (existingSign != null)
            {
                response.Message = existingSign.SignType == 0 ? "您已撤销报名，无法再次报名" : "您已报名参加该活动";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            activeSign.MemberId = member.Id; // 设置正确的MemberId
            activeSign.Id = 0;
            activeSign.EnterData = DateTime.Now;
            activeSign.SignType = 1; // 已报名

            int result = await activeSignRepository.AddAsync(activeSign);

            // 更新活动的参与人数
            if (result > 0)
            {
                active.JoinNum += 1;
                await activeRepository.UpdateAsync(active);
            }

            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "报名成功" : "报名失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? activeSign : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "报名失败：" + ex.Message;
        }
        return Json(response);
    }

    // GET: ClubHub/Active/GetActiveMembers
    [HttpGet]
    public async Task<IActionResult> GetActiveMembers(int activeId, int page = 1, int limit = 10)
    {
        const string sql = """
                           SELECT m.*, s.Id AS SignId, s.SignType 
                                                   FROM T_ActiveSign s
                                                   INNER JOIN T_Member m ON s.MemberId = m.Id
                                                   WHERE s.ActiveId = @ActiveId AND s.SignType IN (1, 2, 3)
                           """;

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("ActiveId", activeId);

        PageData<ActivityMemberViewModel> pageData = await memberRepository.PagedAsync<ActivityMemberViewModel>(sql, parameters, "StudentId ASC", page, limit);

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

    // POST: ClubHub/Active/UpdateSignStatus
    [HttpPost]
    public async Task<IActionResult> UpdateSignStatus(int signId, int status)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            ActiveSign sign = await activeSignRepository.SingleAsync(signId);
            if (sign == null)
            {
                response.Message = "报名记录不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            sign.SignType = (byte)status;
            bool result = await activeSignRepository.UpdateAsync(sign);

            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "操作成功" : "操作失败";
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "操作失败：" + ex.Message;
        }
        return Json(response);
    }

    // GET: ClubHub/Active/GetClubList (获取社团列表用于筛选)
    [HttpGet]
    public async Task<IActionResult> GetClubList()
    {
        IEnumerable<T_Club> clubs = await activeRepository.FindAllAsync<T_Club>("SELECT * FROM T_Club");
        return Json(clubs.Select(c => new { c.Id, c.Name }));
    }
}