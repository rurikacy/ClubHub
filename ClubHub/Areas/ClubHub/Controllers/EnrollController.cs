using System;
using System.Collections.Generic;
using System.Diagnostics;
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
public class EnrollController(IEnrollRepository enrollRepository, IMemberRepository memberRepository) : Controller
{
    /// <summary>
    ///     从字典中获取值，支持大小写不敏感
    /// </summary>
    private object GetValue(IDictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out object value))
            return value;

        // 尝试小写
        string lowerKey = key.ToLower();
        foreach (KeyValuePair<string, object> kvp in dict)
        {
            if (kvp.Key.ToLower() == lowerKey)
                return kvp.Value;
        }

        return null;
    }

    /// <summary>
    ///     验证当前用户是否有权限访问社团报名功能
    ///     只有ClubId为null的成员可以访问
    /// </summary>
    private Task<(bool hasPermission, string errorMessage, int? clubId)> CheckEnrollPermission()
    {
        // 直接从Session获取用户信息（与其他Controller保持一致）
        string userIdStr = HttpContext.Session.GetString("UserId");
        string clubIdStr = HttpContext.Session.GetString("ClubId");

        if (string.IsNullOrEmpty(userIdStr))
        {
            return Task.FromResult<(bool, string, int?)>((false, "用户未登录", null));
        }

        // 解析ClubId
        int? clubId = null;
        if (!string.IsNullOrEmpty(clubIdStr) && int.TryParse(clubIdStr, out int parsedClubId))
        {
            clubId = parsedClubId;
        }

        // 注意：这里我们无法直接从Session获取UserType
        // 但是根据登录逻辑，只有成员才会有ClubId的概念
        // 管理员通常都有ClubId（社团管理员）或者不需要访问社团报名
        // 所以我们简化逻辑：只检查ClubId是否为null

        // 对于成员，检查ClubId
        if (clubId is > 0)
        {
            return Task.FromResult<(bool, string, int?)>((false, "您已加入社团，无法访问社团报名功能", clubId));
        }

        // ClubId为null或0，允许访问
        return Task.FromResult<(bool, string, int?)>((true, null, clubId));
    }

    // GET: ClubHub/Enroll
    public async Task<IActionResult> Index()
    {
        (bool hasPermission, string errorMessage, int? clubId) = await CheckEnrollPermission();

        if (!hasPermission)
        {
            // 如果没有权限，返回错误视图而不是重定向
            // 重定向会导致整个页面刷新，破坏AJAX加载的体验
            ViewBag.ErrorMessage = errorMessage;
            return PartialView("_Error");
        }

        // 对于无社团成员，显示可报名的社团列表
        return PartialView();
    }

    // GET: ClubHub/Enroll/Approve/Index (管理员审批报名申请)
    [Route("ClubHub/Enroll/Approve/Index")]
    public IActionResult ApproveIndex()
    {
        // 从Session获取用户类型
        string userTypeStr = HttpContext.Session.GetString("UserType");
        bool isSuperAdmin = userTypeStr == "0"; // 0表示超级管理员

        ViewBag.IsSuperAdmin = isSuperAdmin;
        return PartialView("Approve");
    }

    // GET: ClubHub/Enroll/GetClubListForFilter (获取社团列表用于筛选)
    [HttpGet]
    public async Task<IActionResult> GetClubListForFilter()
    {
        try
        {
            IEnumerable<T_Club> clubs = await enrollRepository.FindAllAsync<T_Club>("SELECT Id, Name FROM T_Club ORDER BY Id");
            return Json(clubs.Select(c => new { id = c.Id, name = c.Name }));
        }
        catch (Exception ex)
        {
            return Json(new List<object>());
        }
    }

    // GET: ClubHub/Enroll/GetClubList (获取所有可报名的社团)
    [HttpGet]
    public async Task<IActionResult> GetClubList(string Name = "", int page = 1, int limit = 10)
    {
        try
        {
            // 获取当前成员ID
            string studentId = HttpContext.Session.GetString("StudentId");
            int? currentMemberId = null;

            if (!string.IsNullOrEmpty(studentId))
            {
                IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                    "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                    new { StudentId = studentId });
                Member member = members.FirstOrDefault();
                if (member != null)
                {
                    currentMemberId = member.Id;
                }
            }

            // 获取社团列表
            string sql = @"SELECT * FROM T_Club WHERE 1=1";
            DynamicParameters parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(Name))
            {
                sql += " AND Name LIKE @Name";
                parameters.Add("Name", $"%{Name}%");
            }

            PageData<T_Club> pageData = await enrollRepository.PagedAsync<T_Club>(sql, parameters, "Id ASC", page, limit);

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

                // 检查申请状态
                if (currentMemberId.HasValue)
                {
                    IEnumerable<Enroll> enrolls = await enrollRepository.FindAllAsync<Enroll>(
                        "SELECT * FROM T_Enroll WHERE MemberId = @MemberId AND ClubId = @ClubId",
                        new { MemberId = currentMemberId.Value, ClubId = club.Id });
                    Enroll enroll = enrolls.FirstOrDefault();

                    if (enroll != null)
                    {
                        viewModel.HasApplied = true;
                        viewModel.ApplyStatus = enroll.HandType;
                    }
                }

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

    // GET: ClubHub/Enroll/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(int? ClubId = null, int? MemberId = null, byte? HandType = null, int page = 1, int limit = 10)
    {
        try
        {
            string sql = """
                         SELECT e.*, 
                                                    c.Name AS ClubName, 
                                                    m.Name AS MemberName,
                                                    m.StudentId AS MemberStudentId,
                                                    m.PhoneNumber AS MemberPhoneNumber,
                                                    m.Sex AS MemberSex
                                                    FROM T_Enroll e 
                                                    LEFT JOIN T_Club c ON e.ClubId = c.Id 
                                                    LEFT JOIN T_Member m ON e.MemberId = m.Id 
                                                    WHERE 1=1
                         """;
            DynamicParameters parameters = new DynamicParameters();

            // 检查用户类型
            string userTypeStr = HttpContext.Session.GetString("UserType");
            bool isSuperAdmin = userTypeStr == "0"; // 0表示超级管理员

            // 如果没有指定ClubId，且不是超级管理员，则从Session获取当前管理员的ClubId
            if (!ClubId.HasValue && !isSuperAdmin)
            {
                string clubIdStr = HttpContext.Session.GetString("ClubId");
                if (!string.IsNullOrEmpty(clubIdStr) && int.TryParse(clubIdStr, out int sessionClubId))
                {
                    ClubId = sessionClubId;
                }
            }

            // 如果指定了ClubId，则按社团筛选（超管可以筛选特定社团）
            if (ClubId is > 0)
            {
                sql += " AND e.ClubId = @ClubId";
                parameters.Add("ClubId", ClubId.Value);
            }

            if (MemberId.HasValue)
            {
                sql += " AND e.MemberId = @MemberId";
                parameters.Add("MemberId", MemberId.Value);
            }

            if (HandType.HasValue)
            {
                sql += " AND e.HandType = @HandType";
                parameters.Add("HandType", HandType.Value);
            }
            else
            {
                // 如果HandType为null，则只查询待审核的申请
                // 注意：这里需要特殊处理，因为前端可能传递null来表示"待审核"
                // 我们通过检查是否明确传递了HandType参数来判断
                string handTypeParam = HttpContext.Request.Query["HandType"].ToString();
                if (string.IsNullOrEmpty(handTypeParam))
                {
                    // 没有传递HandType参数，不做过滤
                }
                else if (handTypeParam.ToLower() == "null")
                {
                    // 明确传递了null，查询待审核的
                    sql += " AND e.HandType IS NULL";
                }
            }

            // 使用dynamic类型来接收包含扩展字段的查询结果
            PageData<dynamic> pageData = await enrollRepository.PagedAsync<dynamic>(sql, parameters, "Id DESC", page, limit);

            // 手动映射数据，确保字段名正确
            List<object> mappedData = [];
            foreach (IDictionary<string, object> item in pageData.Items)
            {
                // 使用字典方式访问，处理大小写不敏感
                mappedData.Add(new
                {
                    id = GetValue(item, "Id"),
                    clubId = GetValue(item, "ClubId"),
                    memberId = GetValue(item, "MemberId"),
                    enterData = GetValue(item, "EnterData"),
                    handType = GetValue(item, "HandType"),
                    handInfo = GetValue(item, "HandInfo"),
                    handDate = GetValue(item, "HandDate"),
                    clubName = GetValue(item, "ClubName"),
                    memberName = GetValue(item, "MemberName"),
                    memberStudentId = GetValue(item, "MemberStudentId"),
                    memberPhoneNumber = GetValue(item, "MemberPhoneNumber"),
                    memberSex = GetValue(item, "MemberSex")
                });
            }

            // 直接返回匿名对象，确保字段名为小写
            return Json(new
            {
                code = 0,
                msg = true,
                message = "获取成功",
                count = pageData.TotalCount,
                data = mappedData
            });
        }
        catch (Exception ex)
        {
            // 直接返回匿名对象，确保字段名为小写
            return Json(new
            {
                code = 1,
                msg = false,
                message = "获取失败：" + ex.Message
            });
        }
    }

    // POST: ClubHub/Enroll/Create (申请加入社团)
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] int clubId)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            // 权限验证：只有ClubId为null的成员可以提交报名申请
            (bool hasPermission, string errorMessage, int? currentClubId) = await CheckEnrollPermission();

            if (!hasPermission)
            {
                response.Message = errorMessage ?? "您没有权限访问社团报名功能";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 从Session获取当前成员的StudentId
            string studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                response.Message = "无法获取用户信息";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 根据StudentId查找成员ID
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId",
                new { StudentId = studentId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                response.Message = "找不到成员信息";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 检查是否已经申请过该社团
            IEnumerable<Enroll> existingEnrolls = await enrollRepository.FindAllAsync<Enroll>(
                "SELECT * FROM T_Enroll WHERE MemberId = @MemberId AND ClubId = @ClubId",
                new { MemberId = member.Id, ClubId = clubId });

            if (existingEnrolls.Any())
            {
                response.Message = "您已经申请过该社团，请勿重复申请";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            // 创建报名申请
            Enroll enroll = new Enroll
            {
                Id = 0,
                MemberId = member.Id,
                ClubId = clubId,
                EnterData = DateTime.Now,
                HandType = null // 未处理
            };

            int result = await enrollRepository.AddAsync(enroll);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "申请成功，请等待审核" : "申请失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? enroll : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "申请失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Enroll/Handle (处理申请)
    [HttpPost]
    public async Task<IActionResult> Handle([FromForm] int id, [FromForm] byte handType, [FromForm] string handInfo)
    {
        try
        {
            // 添加日志以便调试
            Debug.WriteLine($"Handle called: id={id}, handType={handType}, handInfo={handInfo}");

            if (id <= 0)
            {
                return Json(new
                {
                    code = 1,
                    msg = false,
                    message = $"申请信息无效：id={id}"
                });
            }

            Enroll existingEnroll = await enrollRepository.SingleAsync(id);
            if (existingEnroll == null)
            {
                return Json(new
                {
                    code = 1,
                    msg = false,
                    message = "申请不存在"
                });
            }

            existingEnroll.HandType = handType;
            existingEnroll.HandInfo = handInfo;
            existingEnroll.HandDate = DateTime.Now;

            bool result = await enrollRepository.UpdateAsync(existingEnroll);

            // 如果批准（HandType=0），自动退回该成员的其他申请
            if (result && handType == 0)
            {
                // 获取该成员的所有其他申请
                IEnumerable<Enroll> otherEnrolls = await enrollRepository.FindAllAsync<Enroll>(
                    """
                    SELECT * FROM T_Enroll 
                                              WHERE MemberId = @MemberId 
                                              AND Id != @CurrentId 
                                              AND (HandType IS NULL OR HandType != 1)
                    """,
                    new { existingEnroll.MemberId, CurrentId = existingEnroll.Id });

                // 将其他申请的状态改为退回（HandType=1）
                foreach (Enroll otherEnroll in otherEnrolls)
                {
                    otherEnroll.HandType = 1; // 退回
                    otherEnroll.HandInfo = "已有其他社团批准";
                    otherEnroll.HandDate = DateTime.Now;
                    await enrollRepository.UpdateAsync(otherEnroll);
                }

                // 更新成员的ClubId
                Member member = await memberRepository.SingleAsync(existingEnroll.MemberId);
                if (member != null)
                {
                    member.ClubId = existingEnroll.ClubId;
                    await memberRepository.UpdateAsync(member);
                }
            }

            return Json(new
            {
                code = result ? 0 : 1,
                msg = result,
                message = result ? "处理成功" : "处理失败",
                count = result ? 1 : 0,
                data = result ? existingEnroll : null
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                code = 1,
                msg = false,
                message = "处理失败：" + ex.Message
            });
        }
    }

    // POST: ClubHub/Enroll/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Enroll enroll = await enrollRepository.SingleAsync(id);
            if (enroll == null)
            {
                response.Message = "申请不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await enrollRepository.DeleteAsync(enroll);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "删除成功" : "删除失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? enroll : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "删除失败：" + ex.Message;
        }
        return Json(response);
    }
}