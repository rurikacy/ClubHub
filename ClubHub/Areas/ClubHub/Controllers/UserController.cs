using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClubHub.Areas.ClubHub.Models;
using ClubHub.Areas.ClubHub.Models.Requests;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.DbEntity;
using UserViewModel = Repository.DbEntity.UserViewModel;
using Repository.IRepository;

namespace ClubHub.Areas.ClubHub.Controllers;

/// <summary>
///     用户管理
/// </summary>
[Area("ClubHub")]
public class UserController(ILogger<UserController> logger, IUserRepository userRepository) : Controller
{
    private readonly ILogger<UserController> _logger = logger;

    /// <summary>
    ///     用户管理页面
    /// </summary>
    public IActionResult Index()
    {
        return PartialView();
    }

    /// <summary>
    ///     获取列表数据
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(ReqUserList req)
    {
        ResponseMessage result = new ResponseMessage();
        try
        {
            PageData<UserViewModel> users = await userRepository.GetPageDataAsync(req.UserName, req.Phone, req.ClubId, req.Page, req.Limit);
            result.Message = "获取成功";
            result.Msg = true;
            result.Data = users.Items;
            result.Count = users.TotalCount;
        }
        catch (Exception e)
        {
            result.Message = e.Message;
        }

        return Json(result);
    }

    /// <summary>
    ///     编辑用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] User user)
    {
        ResponseMessage result = new ResponseMessage();
        try
        {
            if (user is not { Id: > 0 })
            {
                result.Message = "用户信息无效";
                return Json(result);
            }

            User existingUser = await userRepository.SingleAsync(user.Id);
            if (existingUser == null)
            {
                result.Message = "用户不存在";
                return Json(result);
            }

            // 更新字段（StudentId不可修改）
            existingUser.Name = user.Name;
            existingUser.Sex = user.Sex;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.UserType = user.UserType;

            // 如果提供了新密码则更新
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.Password = user.Password;
            }

            if (await userRepository.UpdateAsync(existingUser))
            {
                result.Message = "更新成功";
                result.Msg = true;
            }
        }
        catch (Exception e)
        {
            result.Message = e.Message;
        }

        return Json(result);
    }

    /// <summary>
    ///     删除
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        ResponseMessage result = new ResponseMessage();
        try
        {
            User user = await userRepository.SingleAsync(id);
            if (user == null) return Json(result);

            if (await userRepository.DeleteAsync(user))
            {
                result.Message = "删除成功";
                result.Msg = true;
            }
        }
        catch (Exception e)
        {
            result.Message = e.Message;
        }

        return Json(result);
    }

    /// <summary>
    ///     获取社团列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetClubList()
    {
        IEnumerable<T_Club> clubs = await userRepository.FindAllAsync<T_Club>("SELECT * FROM T_Club");
        return Json(clubs.Select(c => new { c.Id, c.Name }));
    }
}