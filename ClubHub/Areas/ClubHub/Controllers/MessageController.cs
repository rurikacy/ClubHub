using System;
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
public class MessageController(IMessageRepository messageRepository) : Controller
{
    // GET: ClubHub/Message
    public Task<IActionResult> Index()
    {
        ViewBag.CurrentClubId = HttpContext.Session.GetString("ClubId");
        ViewBag.CurrentUserId = HttpContext.Session.GetString("UserId");
        return Task.FromResult<IActionResult>(View());
    }

    // GET: ClubHub/Message/MembertIndex (成员端公告查阅)
    public Task<IActionResult> MembertIndex()
    {
        return Task.FromResult<IActionResult>(View());
    }

    // GET: ClubHub/Message/GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll(string Title = "", int? ClubId = null, int page = 1, int limit = 10)
    {
        string sessionClubId = HttpContext.Session.GetString("ClubId");
        if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
        {
            ClubId = adminClubId;
        }

        string sql = """
                     SELECT m.*, c.Name AS ClubName, a.Name AS AdminName 
                                            FROM T_Message m 
                                            LEFT JOIN T_Club c ON m.ClubId = c.Id 
                                            LEFT JOIN T_Admin a ON m.AdminId = a.Id 
                                            WHERE 1=1
                     """;
        DynamicParameters parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(Title))
        {
            sql += " AND m.Title LIKE @Title";
            parameters.Add("Title", $"%{Title}%");
        }

        if (ClubId.HasValue)
        {
            sql += " AND m.ClubId = @ClubId";
            parameters.Add("ClubId", ClubId.Value);
        }

        PageData<MessageViewModel> pageData = await messageRepository.PagedAsync<MessageViewModel>(sql, parameters, "Id DESC", page, limit);

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

    // GET: ClubHub/Message/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        Message message = await messageRepository.SingleAsync(id);
        if (message == null)
        {
            return NotFound();
        }
        return Json(message);
    }

    // POST: ClubHub/Message/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Message message)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (message == null)
            {
                response.Message = "通知信息不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
            {
                message.ClubId = adminClubId;
            }

            string sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int adminId))
            {
                message.AdminId = adminId;
            }

            message.Id = 0;
            message.EnterData = DateTime.Now;

            if (string.IsNullOrWhiteSpace(message.Title))
            {
                response.Message = "通知标题不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            int result = await messageRepository.AddAsync(message);
            response.Code = result > 0 ? 0 : 1;
            response.Msg = result > 0;
            response.Message = result > 0 ? "添加成功" : "添加失败";
            response.Count = result > 0 ? 1 : 0;
            response.Data = result > 0 ? message : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "添加失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Message/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] Message message)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            if (message is not { Id: > 0 })
            {
                response.Message = "通知信息无效";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            string sessionClubId = HttpContext.Session.GetString("ClubId");
            if (!string.IsNullOrEmpty(sessionClubId) && int.TryParse(sessionClubId, out int adminClubId))
            {
                message.ClubId = adminClubId;
            }

            string sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int adminId))
            {
                message.AdminId = adminId;
            }

            if (string.IsNullOrWhiteSpace(message.Title))
            {
                response.Message = "通知标题不能为空";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await messageRepository.UpdateAsync(message);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "编辑成功" : "编辑失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? message : null;
        }
        catch (Exception ex)
        {
            response.Code = 1;
            response.Msg = false;
            response.Message = "编辑失败：" + ex.Message;
        }
        return Json(response);
    }

    // POST: ClubHub/Message/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage response = new ResponseMessage();
        try
        {
            Message message = await messageRepository.SingleAsync(id);
            if (message == null)
            {
                response.Message = "通知不存在";
                response.Msg = false;
                response.Code = 1;
                return Json(response);
            }

            bool result = await messageRepository.DeleteAsync(message);
            response.Code = result ? 0 : 1;
            response.Msg = result;
            response.Message = result ? "删除成功" : "删除失败";
            response.Count = result ? 1 : 0;
            response.Data = result ? message : null;
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