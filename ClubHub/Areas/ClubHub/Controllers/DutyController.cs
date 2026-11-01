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
public class DutyController(IDutyRepository dutyRepository, IDutyMemberRepository dutyMemberRepository, IMemberRepository memberRepository) : Controller
{
    // GET: ClubHub/Duty
    public IActionResult Index()
    {
        ViewBag.CurrentClubId = HttpContext.Session.GetString("ClubId");
        return View();
    }

    // GET: ClubHub/Duty/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(string Name = "", int? ClubId = null, int page = 1, int limit = 10)
    {
        // 如果是社团管理员，只显示自己社团的部门
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
        {
            ClubId = adminClubId;
        }

        string sql = """
                     SELECT d.Id, d.ClubId, d.Name, d.Description, d.MemberNum, c.Name AS ClubName,
                                                    (SELECT TOP 1 m.Name 
                                                     FROM T_DutyMember dm 
                                                     INNER JOIN T_Member m ON dm.MemberId = m.Id 
                                                     WHERE dm.DutyId = d.Id AND dm.Role = 1) AS LeaderName
                                            FROM T_Duty d 
                                            LEFT JOIN T_Club c ON d.ClubId = c.Id 
                                            WHERE 1=1
                     """;
        DynamicParameters parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(Name))
        {
            sql += " AND d.Name LIKE @Name";
            parameters.Add("Name", $"%{Name}%");
        }

        if (ClubId.HasValue)
        {
            sql += " AND d.ClubId = @ClubId";
            parameters.Add("ClubId", ClubId.Value);
        }

        PageData<DutyViewModel> pageData = await dutyRepository.PagedAsync<DutyViewModel>(sql, parameters, "Id DESC", page, limit);

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

    // GET: ClubHub/Duty/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        Duty duty = await dutyRepository.SingleAsync(id);
        if (duty == null)
        {
            return NotFound();
        }
        return Json(duty);
    }

    // POST: ClubHub/Duty/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Duty duty)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (duty == null)
            {
                response.Message = "部门信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 如果是社团管理员，自动设置社团ID
            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
            {
                duty.ClubId = adminClubId;
            }

            duty.Id = 0;
            duty.MemberNum = 0;

            if (string.IsNullOrWhiteSpace(duty.Name))
            {
                response.Message = "部门名称不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证社团ID是否有效
            if (duty.ClubId <= 0)
            {
                response.Message = "请选择所属社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证社团是否存在
            IEnumerable<T_Club> clubs = await dutyRepository.FindAllAsync<T_Club>("SELECT * FROM T_Club WHERE Id = @Id", new { Id = duty.ClubId });
            if (!clubs.Any())
            {
                response.Message = "所选社团不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            int result = await dutyRepository.AddAsync(duty);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? duty : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Duty/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Duty duty)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (duty is not { Id: > 0 })
            {
                response.Message = "部门信息无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(duty.Name))
            {
                response.Message = "部门名称不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await dutyRepository.UpdateAsync(duty);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "编辑成功" : "编辑失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? duty : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "编辑失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Duty/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Duty duty = await dutyRepository.SingleAsync(id);
            if (duty == null)
            {
                response.Message = "部门不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 检查部门是否还有成员
            if (duty.MemberNum > 0)
            {
                response.Message = "该部门还有成员，无法删除";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await dutyRepository.DeleteAsync(duty);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "删除成功" : "删除失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? duty : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "删除失败：" + ex.Message;
        }
        return Json(response);
    }

    // GET: ClubHub/Duty/GetDutyMembers (获取部门成员列表)
    [HttpGet]
    public async Task<IActionResult> GetDutyMembers(int dutyId, int page = 1, int limit = 10)
    {
        string sql = """
                     SELECT m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber, m.ClubId, m.EnterData,
                                                    dm.Id AS DutyMemberId, dm.Role AS DutyRole, dm.DutyId
                                             FROM T_DutyMember dm
                                             INNER JOIN T_Member m ON dm.MemberId = m.Id
                                             WHERE dm.DutyId = @DutyId
                     """;

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("DutyId", dutyId);

        PageData<DutyMemberViewModel> pageData = await memberRepository.PagedAsync<DutyMemberViewModel>(sql, parameters, "StudentId ASC", page, limit);

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

    // POST: ClubHub/Duty/RemoveMemberFromDuty (从部门中删除成员)
    [HttpPost]
    public async Task<IActionResult> RemoveMemberFromDuty(int dutyMemberId, int dutyId)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            int result = await dutyMemberRepository.ExecuteAsync(
                "DELETE FROM T_DutyMember WHERE Id = @Id",
                new { Id = dutyMemberId });

            // 更新部门人数
            if (result > 0)
            {
                await dutyRepository.ExecuteAsync(
                    "UPDATE T_Duty SET MemberNum = CASE WHEN MemberNum > 0 THEN MemberNum - 1 ELSE 0 END WHERE Id = @DutyId",
                    new { DutyId = dutyId });
            }

            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "删除成功" : "删除失败";
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "删除失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Duty/UpdateMemberRole (修改部门成员职位)
    [HttpPost]
    public async Task<IActionResult> UpdateMemberRole(int dutyMemberId, int role)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            int result = await dutyMemberRepository.ExecuteAsync(
                "UPDATE T_DutyMember SET Role = @Role WHERE Id = @Id",
                new { Id = dutyMemberId, Role = role });

            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "更新成功" : "更新失败";
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "更新失败：" + ex.Message;
        }
        return Json(response);
    }

    // GET: ClubHub/Duty/GetClubList (获取社团列表用于筛选)
    [HttpGet]
    public async Task<IActionResult> GetClubList()
    {
        IEnumerable<T_Club> clubs = await dutyRepository.FindAllAsync<T_Club>("SELECT * FROM T_Club");
        return Json(clubs.Select(c => new { c.Id, c.Name }));
    }

    // POST: ClubHub/Duty/AddMember (添加部门成员)
    [HttpPost]
    public async Task<IActionResult> AddMember([FromForm] DutyMember dutyMember)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (dutyMember == null)
            {
                response.Message = "成员信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            dutyMember.Id = 0;

            int result = await dutyMemberRepository.AddAsync(dutyMember);

            // 更新部门人数
            if (result > 0)
            {
                Duty duty = await dutyRepository.SingleAsync(dutyMember.DutyId);
                if (duty != null)
                {
                    duty.MemberNum += 1;
                    await dutyRepository.UpdateAsync(duty);
                }
            }

            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? dutyMember : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
        }
        return Json(response);
    }
}

/// <summary>
///     部门视图模型（用于列表显示）
/// </summary>
public class DutyViewModel
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MemberNum { get; set; }
    public string ClubName { get; set; }
    public string LeaderName { get; set; }
}

/// <summary>
///     部门成员视图模型（用于列表显示）
/// </summary>
public class DutyMemberViewModel
{
    public int Id { get; set; }
    public string StudentId { get; set; }
    public string Name { get; set; }
    public byte Sex { get; set; }
    public string PhoneNumber { get; set; }
    public int DutyMemberId { get; set; }
    public byte DutyRole { get; set; }
    public int DutyId { get; set; }
}