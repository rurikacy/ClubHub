using System;
using System.Diagnostics;
using System.Threading.Tasks;
using App;
using ClubHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.DbEntity;
using Repository.IRepository;
using Repository.Model;

namespace ClubHub.Controllers;

public class HomeController(ILogger<HomeController> logger, IAuthRepository authRepository, IClubRepository clubRepository) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    /// <summary>
    ///     登录页面
    /// </summary>
    /// <returns>显示登陆页面</returns>
    public IActionResult Login()
    {
        return PartialView();
    }

    /// <summary>
    ///     退出页面
    /// </summary>
    /// <returns>显示登陆页面</returns>
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return PartialView("Login");
    }

    /// <summary>
    ///     校验登录（自动识别用户角色）
    /// </summary>
    /// <param name="userName">用户名（StudentId）</param>
    /// <param name="password">密码</param>
    /// <returns>返回成功失败信息</returns>
    [HttpPost]
    public async Task<IActionResult> CheckLogin(string userName, string password)
    {
        try
        {
            // 自动识别用户角色并登录
            var (userId, role) = await authRepository.LoginAutoRole(userName, password);

            if (userId > 0)
            {
                // 获取用户详细信息
                object userObj = await authRepository.GetUserByIdAsync(userId, role);
                if (userObj == null)
                {
                    return Json(new { status = "error", msg = "用户信息不存在" });
                }

                string userDisplayName;
                string userStudentId;
                int? clubId;

                if (role is 0 or 1)
                {
                    // 管理员
                    if (userObj is not Admin admin)
                    {
                        return Json(new { status = "error", msg = "管理员信息获取失败" });
                    }

                    userDisplayName = admin.Name;
                    userStudentId = admin.StudentId;
                    clubId = admin.ClubId;
                }
                else
                {
                    // 成员
                    if (userObj is not Member member)
                    {
                        return Json(new { status = "error", msg = "成员信息获取失败" });
                    }

                    userDisplayName = member.Name;
                    userStudentId = member.StudentId;
                    clubId = member.ClubId;
                }

                string token = Guid.NewGuid().ToString().GetHashCode().ToString("x");
                
                // 从 T_User 中读取真实的 UserType，作为后续菜单渲染依据
                User userEntity = await authRepository.GetUserEntityByStudentIdAsync(userStudentId);
                byte userType = (byte)role;
                if (userEntity != null)
                {
                    userType = userEntity.UserType;
                }

                CurrentUser currentUser = new CurrentUser
                {
                    Role = userType,
                    UserId = userId,
                    ClubId = clubId
                };

                // 存储用户信息到Session
                HttpContext.Session.SetString("UserId", userId.ToString());
                HttpContext.Session.SetString("UserName", userDisplayName);
                HttpContext.Session.SetString("StudentId", userStudentId);
                HttpContext.Session.SetString("UserType", userType.ToString());
                if (clubId.HasValue)
                {
                    HttpContext.Session.SetString("ClubId", clubId.Value.ToString());

                    // 获取并存储社团名称
                    T_Club club = await clubRepository.SingleAsync(clubId.Value);
                    if (club != null)
                    {
                        HttpContext.Session.SetString("ClubName", club.Name);
                    }
                }
                else
                {
                    // ClubId为null时，确保Session中没有ClubId
                    HttpContext.Session.Remove("ClubId");
                    HttpContext.Session.Remove("ClubName");
                }

                HttpContext.Session.SetObjectAsJson(token, currentUser);
                return Json(new { status = "success", msg = "登录成功", token });
            }

            return Json(new { status = "error", msg = "登录失败：用户名或密码错误" });
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", msg = "登录失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     游客登录
    /// </summary>
    /// <returns>返回成功失败信息</returns>
    [HttpPost]
    public IActionResult GuestLogin()
    {
        try
        {
            string token = Guid.NewGuid().ToString().GetHashCode().ToString("x");
            
            CurrentUser currentUser = new CurrentUser
            {
                Role = 3, // 3表示游客
                UserId = 0,
                ClubId = null
            };

            // 存储游客信息到Session
            HttpContext.Session.SetString("UserId", "0");
            HttpContext.Session.SetString("UserName", "游客");
            HttpContext.Session.SetString("StudentId", "guest");
            HttpContext.Session.SetString("UserType", "3");
            HttpContext.Session.Remove("ClubId");
            HttpContext.Session.Remove("ClubName");

            HttpContext.Session.SetObjectAsJson(token, currentUser);
            return Json(new { status = "success", msg = "游客登录成功", token });
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", msg = "登录失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     主页面
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        string name = HttpContext.Session.GetString("UserName");
        ViewBag.RealName = name;
        return View();
    }

    /// <summary>
    ///     欢迎内容（Partial）
    /// </summary>
    public IActionResult Welcome()
    {
        return PartialView("_HomeContent");
    }

    /// <summary>
    ///     错误页
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}