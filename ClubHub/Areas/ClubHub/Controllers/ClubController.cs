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
public class ClubController(IClubRepository clubRepository) : Controller
{
    // GET: ClubHub/Club
    public Task<IActionResult> Index()
    {
        return Task.FromResult<IActionResult>(View());
    }

    // GET: ClubHub/Club/GuestView (游客浏览页面)
    public Task<IActionResult> GuestView()
    {
        return Task.FromResult<IActionResult>(View());
    }

    // GET: ClubHub/Club/Admin/Index (管理员社团信息管理页面)
    [Route("ClubHub/Club/Admin/Index")]
    public async Task<IActionResult> AdminIndex()
    {
        string clubIdStr = HttpContext.Session.GetString("ClubId");
        if (int.TryParse(clubIdStr, out int clubId))
        {
            T_Club club = await clubRepository.SingleAsync(clubId);
            return PartialView("Admin/Index", club);
        }
        return PartialView("Admin/Index", new T_Club());
    }

    // POST: ClubHub/Club/UpdateDescription (更新社团介绍)
    [HttpPost]
    public async Task<IActionResult> UpdateDescription([FromForm] string description)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            string clubIdStr = HttpContext.Session.GetString("ClubId");
            if (!int.TryParse(clubIdStr, out int clubId) || clubId <= 0)
            {
                response.Message = "无法获取社团信息";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            T_Club club = await clubRepository.SingleAsync(clubId);
            if (club == null)
            {
                response.Message = "社团不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            club.Description = description;
            bool result = await clubRepository.UpdateAsync(club);

            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "更新成功" : "更新失败";
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "更新失败：" + ex.Message;
        }
        return Json(response);
    }

    // GET: ClubHub/Club/ClubManagement
    public async Task<IActionResult> ClubManagement()
    {
        string clubIdStr = HttpContext.Session.GetString("ClubId");
        if (int.TryParse(clubIdStr, out int clubId))
        {
            T_Club club = await clubRepository.SingleAsync(clubId);
            return View(club);
        }
        return View(new T_Club());
    }

    // GET: ClubHub/Club/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(string Name = "", int page = 1, int limit = 10)
    {
        // 构建查询SQL，添加社团人数统计
        string sql = """
                     SELECT c.Id, c.Name, c.Description, c.EnterData,
                                                    (SELECT COUNT(*) FROM T_Member m WHERE m.ClubId = c.Id) AS MemberCount
                                             FROM T_Club c 
                                             WHERE 1=1
                     """;
        DynamicParameters parameters = new DynamicParameters();

        // 添加搜索条件
        if (!string.IsNullOrEmpty(Name))
        {
            sql += " AND c.Name LIKE @Name";
            parameters.Add("Name", $"%{Name}%");
        }

        // 使用分页查询
        PageData<ClubViewModel> pageData = await clubRepository.PagedAsync<ClubViewModel>(sql, parameters, "Id DESC", page, limit);

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

    // GET: ClubHub/Club/GetClubMembers (获取社团成员列表)
    [HttpGet]
    public async Task<IActionResult> GetClubMembers(int clubId, int page = 1, int limit = 10)
    {
        string sql = """
                     SELECT m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber, m.ClubId, m.EnterData,
                                                    d.Name as DutyName, dm.Role as DutyRole, dm.Id as DutyMemberId
                                             FROM T_Member m 
                                             LEFT JOIN T_DutyMember dm ON dm.MemberId = m.Id
                                             LEFT JOIN T_Duty d ON d.Id = dm.DutyId
                                             WHERE m.ClubId = @ClubId
                     """;

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("ClubId", clubId);

        PageData<ClubMemberViewModel> pageData = await clubRepository.PagedAsync<ClubMemberViewModel>(sql, parameters, "StudentId ASC", page, limit);

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

    // POST: ClubHub/Club/UpdateMember (更新社团成员信息)
    [HttpPost]
    public async Task<IActionResult> UpdateMember([FromForm] ClubMemberUpdateModel model)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (model is not { Id: > 0 })
            {
                response.Message = "成员信息无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 更新成员基本信息
            string updateSql = """
                               UPDATE T_Member 
                                                                SET Name = @Name, 
                                                                    Sex = @Sex, 
                                                                    PhoneNumber = @PhoneNumber 
                                                                WHERE Id = @Id
                               """;

            int result = await clubRepository.ExecuteAsync(updateSql, new
            {
                model.Id,
                model.Name,
                model.Sex,
                model.PhoneNumber
            });

            // 如果指定了部门，更新部门信息
            if (model.DutyId is > 0)
            {
                // 先删除旧的部门关联
                await clubRepository.ExecuteAsync(
                    "DELETE FROM T_DutyMember WHERE MemberId = @MemberId",
                    new { MemberId = model.Id });

                // 添加新的部门关联
                await clubRepository.ExecuteAsync(
                    "INSERT INTO T_DutyMember (DutyId, MemberId, Role) VALUES (@DutyId, @MemberId, @Role)",
                    new { DutyId = model.DutyId.Value, MemberId = model.Id, Role = model.DutyRole });

                // 更新部门人数
                await clubRepository.ExecuteAsync(
                    """
                    UPDATE T_Duty 
                                              SET MemberNum = (SELECT COUNT(*) FROM T_DutyMember WHERE DutyId = @DutyId) 
                                              WHERE Id = @DutyId
                    """,
                    new { DutyId = model.DutyId.Value });
            }

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

    // GET: ClubHub/Club/GetDutyListByClub (获取社团的部门列表)
    [HttpGet]
    public async Task<IActionResult> GetDutyListByClub(int clubId)
    {
        string sql = "SELECT * FROM T_Duty WHERE ClubId = @ClubId";
        IEnumerable<Duty> duties = await clubRepository.FindAllAsync<Duty>(sql, new { ClubId = clubId });
        return Json(duties.Select(d => new { d.Id, d.Name }));
    }

    // GET: ClubHub/Club/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        T_Club club = await clubRepository.SingleAsync(id);
        if (club == null)
        {
            return NotFound();
        }
        return Json(club);
    }

    // POST: ClubHub/Club/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] T_Club club)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            // 检查模型绑定状态
            if (!ModelState.IsValid)
            {
                string errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                response.Message = "数据验证失败：" + (string.IsNullOrEmpty(errors) ? "模型绑定失败" : errors);
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (club == null)
            {
                response.Message = "社团信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 新增时确保Id为0
            club.Id = 0;
            club.EnterData = DateTime.Now;

            // 验证必填字段
            if (string.IsNullOrWhiteSpace(club.Name))
            {
                response.Message = "社团名称不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            int result = await clubRepository.AddAsync(club);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? club : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
            // 记录内部异常信息
            if (ex.InnerException != null)
            {
                response.Message += " (" + ex.InnerException.Message + ")";
            }
        }
        return Json(response);
    }

    // POST: ClubHub/Club/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] T_Club club)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            // 检查模型绑定状态
            if (!ModelState.IsValid)
            {
                string errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                response.Message = "数据验证失败：" + (string.IsNullOrEmpty(errors) ? "模型绑定失败" : errors);
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (club == null)
            {
                response.Message = "社团信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证Id是否存在
            if (club.Id <= 0)
            {
                response.Message = "社团ID无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证必填字段
            if (string.IsNullOrWhiteSpace(club.Name))
            {
                response.Message = "社团名称不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 获取现有社团信息
            T_Club existingClub = await clubRepository.SingleAsync(club.Id);
            if (existingClub == null)
            {
                response.Message = "未找到该社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 更新字段
            existingClub.Name = club.Name;
            existingClub.Description = club.Description;
            // EnterData 保持不变

            bool result = await clubRepository.UpdateAsync(existingClub);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "编辑成功" : "编辑失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? existingClub : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "编辑失败：" + ex.Message;
            // 记录内部异常信息
            if (ex.InnerException != null)
            {
                response.Message += " (" + ex.InnerException.Message + ")";
            }
        }
        return Json(response);
    }

    // POST: ClubHub/Club/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        T_Club club = await clubRepository.SingleAsync(id);
        if (club == null)
        {
            return NotFound();
        }

        // 检查社团是否还有成员
        int memberCount = await clubRepository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM T_Member WHERE ClubId = @ClubId",
            new { ClubId = id });

        if (memberCount > 0)
        {
            ResponseMessage errorResponse = new ResponseMessage
            {
                Code = 1,
                Msg = false,
                Message = $"该社团还有 {memberCount} 名成员，无法删除。请先移除所有成员后再删除社团。",
                Count = 0,
                Data = null
            };
            return Json(errorResponse);
        }

        bool result = await clubRepository.DeleteAsync(club);
        ResponseMessage response = new ResponseMessage
        {
            Code = result ? 0 : 1,
            Msg = result,
            Message = result ? "删除成功" : "删除失败",
            Count = result ? 1 : 0,
            Data = result ? club : null
        };
        return Json(response);
    }
}

/// <summary>
///     社团视图模型（用于列表显示）
/// </summary>
public class ClubViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime EnterData { get; set; }
    public int MemberCount { get; set; }
}

/// <summary>
///     社团成员视图模型（用于列表显示）
/// </summary>
public class ClubMemberViewModel
{
    public int Id { get; set; }
    public string StudentId { get; set; }
    public string Name { get; set; }
    public byte Sex { get; set; }
    public string PhoneNumber { get; set; }
    public int ClubId { get; set; }
    public DateTime EnterData { get; set; }
    public string DutyName { get; set; }
    public byte? DutyRole { get; set; }
    public int? DutyMemberId { get; set; }
}

/// <summary>
///     社团成员更新模型
/// </summary>
public class ClubMemberUpdateModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public byte Sex { get; set; }
    public string PhoneNumber { get; set; }
    public int? DutyId { get; set; }
    public byte DutyRole { get; set; }
}