using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
public class QuestionController(IQuestionRepository questionRepository, IMemberRepository memberRepository) : Controller
{
    #region 管理员功能

    // GET: ClubHub/Admin/Question (社团管理员问卷管理)
    [Route("ClubHub/Admin/Question")]
    public IActionResult AdminIndex()
    {
        ViewBag.IsSuperAdmin = false;
        ViewBag.CurrentClubId = HttpContext.Session.GetString("ClubId");
        return PartialView("AdminIndex");
    }

    // GET: ClubHub/SpAdmin/Question (超级管理员问卷管理)
    [Route("ClubHub/SpAdmin/Question")]
    public IActionResult SpAdminIndex()
    {
        ViewBag.IsSuperAdmin = true;
        ViewBag.CurrentClubId = null;
        return PartialView("AdminIndex");
    }

    // GET: 获取问卷列表（管理员）
    [HttpGet]
    public async Task<IActionResult> GetAdminList(string Title = "", int? ClubId = null, int page = 1, int limit = 10)
    {
        try
        {
            string userTypeStr = HttpContext.Session.GetString("UserType");
            bool isSuperAdmin = userTypeStr == "0";

            string sql = """
                         SELECT q.*, c.Name AS ClubName, a.Name AS AdminName,
                                (SELECT COUNT(DISTINCT MemberId) FROM T_QuestionAnswer WHERE QuestionId = q.Id) AS AnswerCount
                         FROM T_Question q
                         LEFT JOIN T_Club c ON q.ClubId = c.Id
                         LEFT JOIN T_Admin a ON q.AdminId = a.Id
                         WHERE 1=1
                         """;
            DynamicParameters parameters = new DynamicParameters();

            if (!isSuperAdmin)
            {
                string clubIdStr = HttpContext.Session.GetString("ClubId");
                if (!string.IsNullOrEmpty(clubIdStr) && int.TryParse(clubIdStr, out int adminClubId))
                {
                    sql += " AND q.ClubId = @ClubId";
                    parameters.Add("ClubId", adminClubId);
                }
            }
            else if (ClubId.HasValue && ClubId.Value > 0)
            {
                sql += " AND q.ClubId = @ClubId";
                parameters.Add("ClubId", ClubId.Value);
            }

            if (!string.IsNullOrEmpty(Title))
            {
                sql += " AND q.Title LIKE @Title";
                parameters.Add("Title", $"%{Title}%");
            }

            PageData<QuestionViewModel> pageData = await questionRepository.PagedAsync<QuestionViewModel>(
                sql, parameters, "Id DESC", page, limit);

            return Json(new { code = 0, msg = true, message = "获取成功", count = pageData.TotalCount, data = pageData.Items });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "获取失败：" + ex.Message });
        }
    }

    // POST: 创建问卷
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
    {
        try
        {
            string clubIdStr = HttpContext.Session.GetString("ClubId");
            string userIdStr = HttpContext.Session.GetString("UserId");
            string userTypeStr = HttpContext.Session.GetString("UserType");
            bool isSuperAdmin = userTypeStr == "0";

            int clubId = request.ClubId;
            if (!isSuperAdmin && !string.IsNullOrEmpty(clubIdStr))
            {
                clubId = int.Parse(clubIdStr);
            }

            if (clubId <= 0)
            {
                return Json(new { code = 1, msg = false, message = "请选择社团" });
            }

            // 创建问卷
            Question question = new Question
            {
                ClubId = clubId,
                AdminId = int.TryParse(userIdStr, out int adminId) ? adminId : 0,
                Title = request.Title,
                Description = request.Description,
                EnterData = DateTime.Now,
                Status = 1 // 直接发布
            };

            int questionId = await questionRepository.AddAsync(question);
            if (questionId <= 0)
            {
                return Json(new { code = 1, msg = false, message = "创建问卷失败" });
            }

            // 创建题目
            int sortOrder = 1;
            foreach (var item in request.Items)
            {
                string optionsJson = JsonSerializer.Serialize(item.Options);
                await questionRepository.ExecuteAsync(
                    "INSERT INTO T_QuestionItem (QuestionId, Content, ItemType, Options, SortOrder) VALUES (@QuestionId, @Content, @ItemType, @Options, @SortOrder)",
                    new { QuestionId = questionId, item.Content, item.ItemType, Options = optionsJson, SortOrder = sortOrder++ });
            }

            return Json(new { code = 0, msg = true, message = "创建成功" });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "创建失败：" + ex.Message });
        }
    }

    // POST: 删除问卷
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // 删除答案
            await questionRepository.ExecuteAsync("DELETE FROM T_QuestionAnswer WHERE QuestionId = @Id", new { Id = id });
            // 删除题目
            await questionRepository.ExecuteAsync("DELETE FROM T_QuestionItem WHERE QuestionId = @Id", new { Id = id });
            // 删除问卷
            Question question = await questionRepository.SingleAsync(id);
            if (question != null)
            {
                await questionRepository.DeleteAsync(question);
            }

            return Json(new { code = 0, msg = true, message = "删除成功" });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "删除失败：" + ex.Message });
        }
    }

    // GET: 获取问卷统计
    [HttpGet]
    public async Task<IActionResult> GetStats(int questionId)
    {
        try
        {
            // 获取题目
            IEnumerable<QuestionItem> items = await questionRepository.FindAllAsync<QuestionItem>(
                "SELECT * FROM T_QuestionItem WHERE QuestionId = @QuestionId ORDER BY SortOrder",
                new { QuestionId = questionId });

            // 获取所有答案
            IEnumerable<QuestionAnswer> answers = await questionRepository.FindAllAsync<QuestionAnswer>(
                "SELECT * FROM T_QuestionAnswer WHERE QuestionId = @QuestionId",
                new { QuestionId = questionId });

            List<QuestionStatViewModel> stats = new List<QuestionStatViewModel>();

            foreach (var item in items)
            {
                List<string> options = JsonSerializer.Deserialize<List<string>>(item.Options) ?? new List<string>();
                var itemAnswers = answers.Where(a => a.ItemId == item.Id).ToList();
                int totalAnswers = itemAnswers.Count;

                List<OptionStatViewModel> optionStats = new List<OptionStatViewModel>();
                for (int i = 0; i < options.Count; i++)
                {
                    int count = itemAnswers.Count(a => a.Answer.Split(',').Contains(i.ToString()));
                    optionStats.Add(new OptionStatViewModel
                    {
                        OptionIndex = i,
                        OptionText = options[i],
                        Count = count,
                        Percentage = totalAnswers > 0 ? Math.Round((double)count / totalAnswers * 100, 1) : 0
                    });
                }

                stats.Add(new QuestionStatViewModel
                {
                    ItemId = item.Id,
                    Content = item.Content,
                    ItemType = item.ItemType,
                    OptionStats = optionStats
                });
            }

            // 获取回答人数
            int answerCount = await questionRepository.ExecuteScalarAsync<int>(
                "SELECT COUNT(DISTINCT MemberId) FROM T_QuestionAnswer WHERE QuestionId = @QuestionId",
                new { QuestionId = questionId });

            return Json(new { code = 0, msg = true, data = stats, answerCount });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "获取统计失败：" + ex.Message });
        }
    }

    // GET: 获取社团列表
    [HttpGet]
    public async Task<IActionResult> GetClubList()
    {
        IEnumerable<T_Club> clubs = await questionRepository.FindAllAsync<T_Club>("SELECT * FROM T_Club ORDER BY Id");
        return Json(clubs.Select(c => new { c.Id, c.Name }));
    }

    #endregion

    #region 成员功能

    // GET: ClubHub/Member/Question (成员问卷中心)
    [Route("ClubHub/Member/Question")]
    public IActionResult MemberIndex()
    {
        return PartialView("MemberIndex");
    }

    // GET: 获取问卷列表（成员）
    [HttpGet]
    public async Task<IActionResult> GetMemberList(int page = 1, int limit = 10)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            string clubIdStr = HttpContext.Session.GetString("ClubId");

            if (string.IsNullOrEmpty(clubIdStr) || !int.TryParse(clubIdStr, out int clubId) || clubId <= 0)
            {
                return Json(new { code = 0, msg = true, message = "您尚未加入社团", count = 0, data = new List<object>() });
            }

            // 获取成员ID
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId", new { StudentId = studentId });
            Member member = members.FirstOrDefault();
            int memberId = member?.Id ?? 0;

            // 先获取问卷列表
            string sql = """
                         SELECT q.*, c.Name AS ClubName
                         FROM T_Question q
                         LEFT JOIN T_Club c ON q.ClubId = c.Id
                         WHERE q.ClubId = @ClubId AND q.Status = 1
                         """;

            PageData<Question> pageData = await questionRepository.PagedAsync<Question>(
                sql, new { ClubId = clubId }, "Id DESC", page, limit);

            // 手动构建返回数据，检查每个问卷是否已回答
            List<object> resultList = new List<object>();
            foreach (Question q in pageData.Items)
            {
                // 获取社团名称
                IEnumerable<T_Club> clubs = await questionRepository.FindAllAsync<T_Club>(
                    "SELECT Name FROM T_Club WHERE Id = @ClubId", new { q.ClubId });
                string clubName = clubs.FirstOrDefault()?.Name ?? "";

                // 检查是否已回答
                int answerCount = await questionRepository.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM T_QuestionAnswer WHERE QuestionId = @QuestionId AND MemberId = @MemberId",
                    new { QuestionId = q.Id, MemberId = memberId });

                resultList.Add(new
                {
                    id = q.Id,
                    clubId = q.ClubId,
                    clubName,
                    adminId = q.AdminId,
                    title = q.Title,
                    description = q.Description,
                    enterData = q.EnterData,
                    status = q.Status,
                    hasAnswered = answerCount > 0
                });
            }

            return Json(new { code = 0, msg = true, message = "获取成功", count = pageData.TotalCount, data = resultList });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "获取失败：" + ex.Message });
        }
    }

    // GET: 获取问卷详情（用于填写）
    [HttpGet]
    public async Task<IActionResult> GetDetail(int questionId)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId", new { StudentId = studentId });
            Member member = members.FirstOrDefault();
            int memberId = member?.Id ?? 0;

            // 获取问卷信息
            Question question = await questionRepository.SingleAsync(questionId);
            if (question == null)
            {
                return Json(new { code = 1, msg = false, message = "问卷不存在" });
            }

            // 获取题目
            IEnumerable<QuestionItem> items = await questionRepository.FindAllAsync<QuestionItem>(
                "SELECT * FROM T_QuestionItem WHERE QuestionId = @QuestionId ORDER BY SortOrder",
                new { QuestionId = questionId });

            // 获取已有答案
            IEnumerable<QuestionAnswer> myAnswers = await questionRepository.FindAllAsync<QuestionAnswer>(
                "SELECT * FROM T_QuestionAnswer WHERE QuestionId = @QuestionId AND MemberId = @MemberId",
                new { QuestionId = questionId, MemberId = memberId });

            // 手动构建返回数据，确保字段名小写
            List<object> itemList = new List<object>();
            foreach (QuestionItem item in items)
            {
                List<string> optList = new List<string>();
                try
                {
                    if (!string.IsNullOrEmpty(item.Options))
                    {
                        optList = JsonSerializer.Deserialize<List<string>>(item.Options) ?? new List<string>();
                    }
                }
                catch
                {
                    optList = new List<string>();
                }

                itemList.Add(new
                {
                    id = item.Id,
                    questionId = item.QuestionId,
                    content = item.Content,
                    itemType = item.ItemType,
                    options = item.Options,
                    sortOrder = item.SortOrder,
                    optionList = optList,
                    myAnswer = myAnswers.FirstOrDefault(a => a.ItemId == item.Id)?.Answer
                });
            }

            return Json(new
            {
                code = 0,
                msg = true,
                data = new
                {
                    id = question.Id,
                    title = question.Title,
                    description = question.Description,
                    items = itemList,
                    hasAnswered = myAnswers.Any()
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "获取失败：" + ex.Message });
        }
    }

    // POST: 提交答案
    [HttpPost]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        try
        {
            string studentId = HttpContext.Session.GetString("StudentId");
            IEnumerable<Member> members = await memberRepository.FindAllAsync<Member>(
                "SELECT * FROM T_Member WHERE StudentId = @StudentId", new { StudentId = studentId });
            Member member = members.FirstOrDefault();

            if (member == null)
            {
                return Json(new { code = 1, msg = false, message = "用户信息不存在" });
            }

            // 检查是否已回答
            int existingCount = await questionRepository.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM T_QuestionAnswer WHERE QuestionId = @QuestionId AND MemberId = @MemberId",
                new { request.QuestionId, MemberId = member.Id });

            if (existingCount > 0)
            {
                return Json(new { code = 1, msg = false, message = "您已提交过此问卷" });
            }

            // 保存答案
            foreach (var answer in request.Answers)
            {
                await questionRepository.ExecuteAsync(
                    "INSERT INTO T_QuestionAnswer (QuestionId, ItemId, MemberId, Answer, EnterData) VALUES (@QuestionId, @ItemId, @MemberId, @Answer, @EnterData)",
                    new
                    {
                        request.QuestionId,
                        answer.ItemId,
                        MemberId = member.Id,
                        answer.Answer,
                        EnterData = DateTime.Now
                    });
            }

            return Json(new { code = 0, msg = true, message = "提交成功" });
        }
        catch (Exception ex)
        {
            return Json(new { code = 1, msg = false, message = "提交失败：" + ex.Message });
        }
    }

    #endregion
}

/// <summary>
///     创建问卷请求模型
/// </summary>
public class CreateQuestionRequest
{
    public int ClubId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<CreateQuestionItemRequest> Items { get; set; }
}

/// <summary>
///     创建题目请求模型
/// </summary>
public class CreateQuestionItemRequest
{
    public string Content { get; set; }
    public byte ItemType { get; set; }
    public List<string> Options { get; set; }
}