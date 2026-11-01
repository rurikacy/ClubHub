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
public class MemberController(IMemberRepository memberRepository, IClubRepository clubRepository, IEnrollRepository enrollRepository, ITalkRepository talkRepository, IHubContext<ChatHub> hubContext) : Controller
{
    // GET: ClubHub/Member/GetClubList
    [HttpGet]
    public async Task<IActionResult> GetClubList()
    {
        IEnumerable<T_Club> clubs = await clubRepository.FindAllAsync<T_Club>();
        var clubList = clubs.Select(c => new { Id = c.Id.ToString(), c.Name }).ToList();

        // 添加"无"选项，Id为空字符串
        clubList.Insert(0, new { Id = "", Name = "无" });

        return Json(clubList);
    }

    // GET: ClubHub/Member/GetDutyList
    [HttpGet]
    public async Task<IActionResult> GetDutyList(int? clubId = null)
    {
        string sql = "SELECT * FROM T_Duty WHERE 1=1";
        DynamicParameters parameters = new DynamicParameters();

        // 如果是社团管理员，只显示自己社团的部门
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
        {
            clubId = adminClubId;
        }

        if (clubId.HasValue)
        {
            sql += " AND ClubId = @ClubId";
            parameters.Add("ClubId", clubId.Value);
        }

        IEnumerable<Duty> duties = await memberRepository.FindAllAsync<Duty>(sql, parameters);
        return Json(duties.Select(d => new { d.Id, d.Name }));
    }

    // POST: ClubHub/Member/AssignDuty (分配部门)
    [HttpPost]
    public async Task<IActionResult> AssignDuty(int memberId, int dutyId, int role = 0)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            // 检查成员是否已经在该部门
            IEnumerable<DutyMember> existingAssignments = await memberRepository.FindAllAsync<DutyMember>(
                "SELECT * FROM T_DutyMember WHERE MemberId = @MemberId AND DutyId = @DutyId",
                new { MemberId = memberId, DutyId = dutyId });

            if (existingAssignments.Any())
            {
                response.Message = "该成员已在此部门";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            DutyMember dutyMember = new DutyMember
            {
                MemberId = memberId,
                DutyId = dutyId,
                Role = (byte)role
            };

            int result = await memberRepository.ExecuteAsync(
                "INSERT INTO T_DutyMember (DutyId, MemberId, Role) VALUES (@DutyId, @MemberId, @Role)",
                dutyMember);

            // 更新部门人数
            if (result > 0)
            {
                await memberRepository.ExecuteAsync(
                    "UPDATE T_Duty SET MemberNum = MemberNum + 1 WHERE Id = @DutyId",
                    new { DutyId = dutyId });
            }

            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "分配成功" : "分配失败";
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "分配失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Member/RemoveDuty (移除部门)
    [HttpPost]
    public async Task<IActionResult> RemoveDuty(int memberId, int dutyId)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            int result = await memberRepository.ExecuteAsync(
                "DELETE FROM T_DutyMember WHERE MemberId = @MemberId AND DutyId = @DutyId",
                new { MemberId = memberId, DutyId = dutyId });

            // 更新部门人数
            if (result > 0)
            {
                await memberRepository.ExecuteAsync(
                    "UPDATE T_Duty SET MemberNum = CASE WHEN MemberNum > 0 THEN MemberNum - 1 ELSE 0 END WHERE Id = @DutyId",
                    new { DutyId = dutyId });
            }

            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "移除成功" : "移除失败";
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "移除失败：" + ex.Message;
        }
        return Json(response);
    }

    /// <summary>
    ///     成员个人信息页面
    /// </summary>
    /// <returns>显示成员个人信息页面</returns>
    public async Task<IActionResult> Info()
    {
        string studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
        {
            return Json(new { Msg = false, Message = "用户未登录" });
        }

        const string sql = @"
            SELECT m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber, m.ClubId, m.EnterData,
                   c.Name AS ClubName, d.Name AS DutyName, dm.Role AS DutyRole
            FROM T_Member m
            LEFT JOIN T_Club c ON m.ClubId = c.Id
            LEFT JOIN T_DutyMember dm ON dm.MemberId = m.Id
            LEFT JOIN T_Duty d ON d.Id = dm.DutyId
            WHERE m.StudentId = @studentId";

        IEnumerable<MemberInfoViewModel> members = await memberRepository.FindAllAsync<MemberInfoViewModel>(sql, new { studentId });
        MemberInfoViewModel member = members.FirstOrDefault();

        if (member == null)
        {
            return Json(new { Msg = false, Message = "用户不存在" });
        }

        return PartialView(member);
    }

    /// <summary>
    ///     更新成员信息（手机号和密码）
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

            const string sql = @"SELECT * FROM T_Member WHERE StudentId = @studentId";
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(sql, new { studentId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                return Json(new { Msg = false, Message = "用户不存在" });
            }

            // 验证手机号格式（如果填写了）
            if (!string.IsNullOrEmpty(phoneNumber) && !Regex.IsMatch(phoneNumber, @"^1[3-9]\d{9}$"))
            {
                return Json(new { Msg = false, Message = "请输入正确的手机号格式" });
            }

            // 更新手机号
            member.PhoneNumber = phoneNumber;

            // 如果填写了密码则更新密码
            if (!string.IsNullOrWhiteSpace(password))
            {
                member.Password = password;
            }

            bool result = await memberRepository.UpdateAsync(member);
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

    // GET: ClubHub/Member
    public Task<IActionResult> Index()
    {
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        ViewBag.CurrentClubId = sessionClubId;
        return Task.FromResult<IActionResult>(View());
    }

    /// <summary>
    ///     社团广场页面 - 查看所有社团的活动和通知（只读，无报名功能）
    /// </summary>
    public IActionResult ClubInfo()
    {
        return PartialView();
    }

    /// <summary>
    ///     我的活动页面 - 查看已报名的活动（列表视图和日历视图）
    /// </summary>
    public IActionResult MyActive()
    {
        return PartialView();
    }

    /// <summary>
    ///     获取我的活动列表（已报名的活动）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyActiveList(string Title = "", int? status = null, int page = 1, int limit = 10)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            // 查找当前成员
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                new { StudentId = studentId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到成员信息" });
            }

            // 查询已报名的活动（SignType = 1, 2, 3 表示已报名、已参加、未参加）
            string sql = """
                         SELECT a.*, c.Name AS ClubName, s.SignType,
                                CASE 
                                    WHEN a.EndTime <= GETDATE() THEN 2
                                    WHEN a.StartTime <= GETDATE() THEN 1
                                    ELSE 0
                                END AS StatusOrder
                         FROM T_Active a 
                         INNER JOIN T_ActiveSign s ON s.ActiveId = a.Id
                         LEFT JOIN T_Club c ON a.ClubId = c.Id 
                         WHERE s.MemberId = @MemberId AND s.SignType IN (1, 2, 3)
                         """;
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("MemberId", member.Id);

            if (!string.IsNullOrEmpty(Title))
            {
                sql += " AND a.Title LIKE @Title";
                parameters.Add("Title", $"%{Title}%");
            }

            // 根据状态筛选：0=未开始, 1=进行中, 2=已结束
            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case 0: // 未开始
                        sql += " AND a.StartTime > GETDATE()";
                        break;
                    case 1: // 进行中
                        sql += " AND a.StartTime <= GETDATE() AND a.EndTime > GETDATE()";
                        break;
                    case 2: // 已结束
                        sql += " AND a.EndTime <= GETDATE()";
                        break;
                }
            }

            PageData<ActiveViewModel> pageData = await memberRepository.PagedAsync<ActiveViewModel>(
                sql, parameters, "StatusOrder ASC, StartTime ASC", page, limit);

            return Json(new ResponseMessage
            {
                Code = 0,
                Msg = true,
                Message = "获取成功",
                Count = pageData.TotalCount,
                Data = pageData.Items
            });
        }
        catch (Exception ex)
        {
            return Json(new ResponseMessage { Code = 1, Msg = false, Message = "获取失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     获取我的活动日历数据（用于日历视图）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyActiveCalendar(int year, int month)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new { code = 1, msg = false, message = "用户未登录" });
            }

            // 查找当前成员
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                new { StudentId = studentId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                return Json(new { code = 1, msg = false, message = "找不到成员信息" });
            }

            // 计算月份的开始和结束日期
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // 查询该月份内的已报名活动
            string sql = """
                         SELECT a.Id, a.Title, a.Content, a.Place, a.StartTime, a.EndTime, 
                                c.Name AS ClubName, s.SignType
                         FROM T_Active a 
                         INNER JOIN T_ActiveSign s ON s.ActiveId = a.Id
                         LEFT JOIN T_Club c ON a.ClubId = c.Id 
                         WHERE s.MemberId = @MemberId AND s.SignType IN (1, 2, 3)
                           AND ((a.StartTime >= @StartDate AND a.StartTime <= @EndDate)
                                OR (a.EndTime >= @StartDate AND a.EndTime <= @EndDate)
                                OR (a.StartTime <= @StartDate AND a.EndTime >= @EndDate))
                         ORDER BY a.StartTime ASC
                         """;

            IEnumerable<dynamic> activities = await memberRepository.FindAllAsync<dynamic>(sql, new
            {
                MemberId = member.Id,
                StartDate = startDate,
                EndDate = endDate.AddDays(1) // 包含最后一天
            });

            // 转换为日历事件格式
            var events = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                content = a.Content,
                place = a.Place,
                start = ((DateTime)a.StartTime).ToString("yyyy-MM-dd HH:mm"),
                end = ((DateTime)a.EndTime).ToString("yyyy-MM-dd HH:mm"),
                clubName = a.ClubName,
                signType = a.SignType,
                // 根据活动状态设置颜色
                color = DateTime.Now > (DateTime)a.EndTime ? "#999" :
                    DateTime.Now >= (DateTime)a.StartTime ? "#5FB878" : "#1E9FFF"
            });

            return Json(new { code = 0, msg = true, data = events });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "获取失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     获取社团广场列表（所有社团的活动和通知，无申请状态）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetClubInfoList(string Name = "", int page = 1, int limit = 10)
    {
        try
        {
            // 获取社团列表
            string sql = @"SELECT * FROM T_Club WHERE 1=1";
            DynamicParameters parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(Name))
            {
                sql += " AND Name LIKE @Name";
                parameters.Add("Name", $"%{Name}%");
            }

            PageData<T_Club> pageData = await memberRepository.PagedAsync<T_Club>(sql, parameters, "Id ASC", page, limit);

            // 构建视图模型
            List<ClubEnrollViewModel> viewModels = [];

            foreach (T_Club club in pageData.Items)
            {
                // 计算成员人数
                int memberCount = await memberRepository.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM T_Member WHERE ClubId = @ClubId",
                    new { ClubId = club.Id });

                ClubEnrollViewModel viewModel = new ClubEnrollViewModel
                {
                    Id = club.Id,
                    Name = club.Name,
                    Description = club.Description,
                    MemberNum = memberCount,
                    EnterData = club.EnterData,
                    HasApplied = false,
                    ApplyStatus = null,
                    RecentActivities = [],
                    RecentMessages = []
                };

                // 获取近期活动（最近10条）
                IEnumerable<Active> activities = await enrollRepository.FindAllAsync<Active>(
                    """
                    SELECT TOP 10 * FROM T_Active 
                    WHERE ClubId = @ClubId 
                    ORDER BY EnterData DESC
                    """,
                    new { ClubId = club.Id });

                foreach (Active activity in activities)
                {
                    viewModel.RecentActivities.Add(new RecentActivity
                    {
                        Title = activity.Title,
                        Content = activity.Content,
                        StartTime = activity.StartTime,
                        EndTime = activity.EndTime,
                        JoinNum = activity.JoinNum
                    });
                }

                // 获取近期通知（最近10条）
                IEnumerable<Message> messages = await enrollRepository.FindAllAsync<Message>(
                    """
                    SELECT TOP 10 * FROM T_Message 
                    WHERE ClubId = @ClubId 
                    ORDER BY EnterData DESC
                    """,
                    new { ClubId = club.Id });

                foreach (Message message in messages)
                {
                    viewModel.RecentMessages.Add(new RecentMessage
                    {
                        Title = message.Title,
                        Content = message.Content,
                        EnterData = message.EnterData
                    });
                }

                viewModels.Add(viewModel);
            }

            ResponseMessage response = new ResponseMessage
            {
                Code = 0,
                Msg = true,
                Message = "获取成功",
                Count = pageData.TotalCount,
                Data = viewModels
            };
            return Json(response);
        }
        catch (Exception ex)
        {
            ResponseMessage response = new ResponseMessage
            {
                Code = 1,
                Msg = false,
                Message = "获取失败：" + ex.Message
            };
            return Json(response);
        }
    }

    // GET: ClubHub/Member/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(string Name = "", int? ClubId = null, int page = 1, int limit = 10)
    {
        string sql = """
                     SELECT m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber, m.Password, m.ClubId, m.EnterData, 
                                                    c.Name as ClubName, d.Name as DutyName, dm.Role as DutyRole
                                             FROM T_Member m 
                                             LEFT JOIN T_Club c ON m.ClubId = c.Id 
                                             LEFT JOIN T_DutyMember dm ON dm.MemberId = m.Id
                                             LEFT JOIN T_Duty d ON d.Id = dm.DutyId
                                             WHERE 1=1
                     """;
        DynamicParameters parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(Name))
        {
            sql += " AND m.Name LIKE @Name";
            parameters.Add("Name", $"%{Name}%");
        }

        // Check if logged in user has a specific ClubId (Club Admin or Member)
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int currentClubId))
        {
            sql += " AND m.ClubId = @CurrentClubId";
            parameters.Add("CurrentClubId", currentClubId);
        }
        else if (ClubId.HasValue)
        {
            sql += " AND m.ClubId = @ClubId";
            parameters.Add("ClubId", ClubId.Value);
        }

        PageData<MemberViewModel> pageData = await memberRepository.PagedAsync<MemberViewModel>(sql, parameters, "Id DESC", page, limit);

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

    // GET: ClubHub/Member/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        Member member = await memberRepository.SingleAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return Json(member);
    }

    // POST: ClubHub/Member/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Member member)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (member == null)
            {
                response.Message = "成员信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            member.Id = 0;
            member.EnterData = DateTime.Now;

            if (string.IsNullOrWhiteSpace(member.Name))
            {
                response.Message = "成员姓名不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(member.Password))
            {
                response.Message = "密码不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证ClubId：如果为0或负数，设置为null；如果为正数，验证社团是否存在
            if (member.ClubId is <= 0)
            {
                member.ClubId = null;
            }
            else if (member.ClubId is > 0)
            {
                T_Club club = await clubRepository.SingleAsync(member.ClubId.Value);
                if (club == null)
                {
                    response.Message = "所属社团无效";
                    response.Msg = false;
                    response.Code = 1;
                    return Json(response);
                }
            }

            int result = await memberRepository.AddAsync(member);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? member : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Member/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Member member)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (member is not { Id: > 0 })
            {
                response.Message = "成员信息无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(member.Name))
            {
                response.Message = "成员姓名不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 获取现有成员信息
            Member existingMember = await memberRepository.SingleAsync(member.Id);
            if (existingMember == null)
            {
                response.Message = "未找到该成员";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 验证ClubId：如果为0或负数，设置为null；如果为正数，验证社团是否存在
            if (member.ClubId is <= 0)
            {
                member.ClubId = null;
            }
            else if (member.ClubId is > 0)
            {
                T_Club club = await clubRepository.SingleAsync(member.ClubId.Value);
                if (club == null)
                {
                    response.Message = "所属社团无效";
                    response.Msg = false;
                    response.Code = 1;
                    return Json(response);
                }
            }

            // 更新字段
            existingMember.StudentId = member.StudentId;
            existingMember.Name = member.Name;
            existingMember.Sex = member.Sex;
            existingMember.PhoneNumber = member.PhoneNumber;
            existingMember.ClubId = member.ClubId;

            // 如果密码不为空，则更新密码
            if (!string.IsNullOrWhiteSpace(member.Password))
            {
                existingMember.Password = member.Password;
            }

            bool result = await memberRepository.UpdateAsync(existingMember);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "编辑成功" : "编辑失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? existingMember : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "编辑失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Member/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Member member = await memberRepository.SingleAsync(id);
            if (member == null)
            {
                response.Message = "成员不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await memberRepository.DeleteAsync(member);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "删除成功" : "删除失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? member : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "删除失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Member/RemoveFromClub
    [HttpPost]
    public async Task<IActionResult> RemoveFromClub(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Member member = await memberRepository.SingleAsync(id);
            if (member == null)
            {
                response.Message = "成员不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            if (member.ClubId == null)
            {
                response.Message = "该成员不属于任何社团";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 将成员社团设置为null（无社团）
            member.ClubId = null;
            bool result = await memberRepository.UpdateAsync(member);

            if (result)
            {
                // 同时移除该成员的所有部门分配
                await memberRepository.ExecuteAsync(
                    "DELETE FROM T_DutyMember WHERE MemberId = @MemberId",
                    new { MemberId = id });
            }

            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "移出社团成功" : "移出社团失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? member : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "移出社团失败：" + ex.Message;
        }
        return Json(response);
    }

    #region 讨论中心功能

    /// <summary>
    ///     讨论中心页面
    /// </summary>
    /// <returns>显示成员视角的讨论中心页面</returns>
    public IActionResult Talk()
    {
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (string.IsNullOrEmpty(sessionClubId))
        {
            return Json(new { Msg = false, Message = "您尚未加入任何社团" });
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
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "您尚未加入任何社团" });
            }

            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "用户未登录" });
            }

            // 获取当前成员信息
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                new { StudentId = studentId });
            Member currentMember = members.FirstOrDefault();

            if (currentMember == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到成员信息" });
            }

            // 获取消息列表
            IEnumerable<TalkViewModel> talks = await talkRepository.GetTalkListAsync(clubId, page, limit);

            // 计算每条消息的属性
            DateTime now = DateTime.Now;
            foreach (TalkViewModel talk in talks)
            {
                // 判断是否为当前用户发送的消息
                bool isSelf = talk.SenderId == currentMember.Id && talk.SenderType == 0;
                talk.IsSelf = isSelf;
                // 只有发送者本人且消息在60秒内才能撤回
                talk.CanWithdraw = isSelf
                                   && talk.Status == 0
                                   && (now - talk.SendTime).TotalSeconds <= 60;
                talk.CanDelete = false; // 普通成员不能删除消息
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
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "您尚未加入任何社团" });
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

            // 获取当前成员信息
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                new { StudentId = studentId });
            Member currentMember = members.FirstOrDefault();

            if (currentMember == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到成员信息" });
            }

            // 发送消息（SenderType = 0 表示成员）
            Talk talk = await talkRepository.SendTalkAsync(clubId, currentMember.Id, 0, content);

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
                senderName = currentMember.Name,
                senderIdentity = "成员",
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
    ///     撤回消息
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

            // 获取当前成员信息
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                new { StudentId = studentId });
            Member currentMember = members.FirstOrDefault();

            if (currentMember == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "找不到成员信息" });
            }

            // 获取消息信息进行验证
            Talk talk = await talkRepository.SingleAsync(id);
            if (talk == null)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息不存在" });
            }

            // 验证是否为发送者本人
            if (talk.SenderId != currentMember.Id || talk.SenderType != 0)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "只能撤回自己的消息" });
            }

            // 验证消息是否已删除
            if (talk.Status == 1)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息已被删除" });
            }

            // 验证是否在60秒内
            if ((DateTime.Now - talk.SendTime).TotalSeconds > 60)
            {
                return Json(new ResponseMessage { Code = 1, Msg = false, Message = "消息发送已超过1分钟，无法撤回" });
            }

            // 执行撤回
            bool result = await talkRepository.WithdrawAsync(id, currentMember.Id, 0);

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

    #endregion
}