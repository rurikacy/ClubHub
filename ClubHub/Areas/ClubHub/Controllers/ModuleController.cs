using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using ClubHub.Areas.ClubHub.Models;
using ClubHub.Areas.ClubHub.Models.Module;
using ClubHub.Areas.ClubHub.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.DbEntity;
using Repository.IRepository;
using Repository.Model;

namespace ClubHub.Areas.ClubHub.Controllers;

[Area("ClubHub")]
public class ModuleController(ILogger<ModuleController> logger, IModuleRepository moduleRepository) : Controller
{
    /// <summary>
    ///     获取模块
    /// </summary>
    public async Task<IActionResult> Modules(string token)
    {
        try
        {
            logger.LogInformation("开始获取菜单，token: {Token}", token);

            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Token为空");
                return Json(new { status = "error", msg = "获取菜单失败：Token为空" });
            }

            CurrentUser user = HttpContext.Session.GetObjectFromJson<CurrentUser>(token);
            if (user == null)
            {
                logger.LogWarning("用户未登录，token: {Token}", token);
                return Json(new { status = "error", msg = "获取菜单失败：用户未登录" });
            }

            logger.LogInformation("用户信息：Role={Role}, UserId={UserId}, RoleIds={RoleIds}, ClubId={ClubId}",
                user.Role, user.UserId, string.Join(",", user.RoleIds ?? []), user.ClubId);

            // 此处的 Role 实际上等同于 T_User.UserType：0=超管，1=社管，2=成员，3=游客
            int userType = user.Role;

            if (userType is < 0 or > 3)
            {
                logger.LogWarning("未知用户类型：{Role}", userType);
                return Json(new { status = "error", msg = "获取菜单失败：未知用户类型" });
            }

            List<Module> treeModel;
            
            // 游客（Role=3）只返回社团浏览模块
            if (userType == 3)
            {
                treeModel = await moduleRepository.GetModulesForGuest();
            }
            else
            {
                // 根据用户类型和ClubId状态获取菜单
                // UserType=2且ClubId为null：只返回"社团报名"模块
                // UserType=2且ClubId不为null：返回除"社团报名"外的成员模块
                // UserType=0或1：返回完整管理员模块，不受ClubId影响
                bool hasClub = user.ClubId is > 0;
                treeModel = await moduleRepository.GetModulesByUserTypeAndClubStatus(userType, hasClub);
            }

            logger.LogInformation("获取到 {Count} 个菜单项", treeModel.Count);

            // 获取父级菜单（ParentId为0的菜单）
            List<TreeModel> model = treeModel
                .Where(n => n.ParentId == 0)
                .Select(n => new TreeModel(n, treeModel))
                .ToList();

            logger.LogInformation("返回 {Count} 个父级菜单", model.Count);

            return Json(new { status = "success", msg = "获取成功", info = model });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取菜单时发生异常");
            return Json(new { status = "error", msg = "获取菜单失败：" + ex.Message });
        }
    }

    /// <summary>
    ///     通过角色获取模块
    /// </summary>
    /// <param name="req">角色id，如果角色id为0，获取所有的菜单</param>
    [HttpGet]
    public async Task<IActionResult> ListByRoleId(ReqListByRoleId req)
    {
        ResponseMessage result = new ResponseMessage();
        try
        {
            //获取全部菜单
            IEnumerable<Module> allModules = await moduleRepository.FindAllAsync<Module>();
            List<Module> manageModules = allModules.Where(n => n.Type == 0).ToList();
            List<int> roleModuleIds = [];
            //已经拥有的菜单权限（基于Module.Type字段）
            if (req.RoleId > 0)
            {
                // 根据角色类型获取对应的模块
                // RoleId: 1=超管(Type=0), 2=社管(Type=1), 3=成员(Type=2)
                int moduleType = req.RoleId == 1 ? 0 : req.RoleId == 2 ? 1 : 2;
                List<Module> modules = await moduleRepository.GetModulesByUserType(moduleType);
                roleModuleIds = modules.Select(n => n.Id).ToList();
            }

            //获取一级菜单
            List<LayTreeModel> parent = manageModules.Where(n => n.ParentId == 0).Select(n => new LayTreeModel
            {
                Id = n.Id.ToString(),
                Title = n.Name,
                Checked = roleModuleIds.Contains(n.Id)
            }).ToList();

            //获取子菜单
            parent.ForEach(n => { n.Children.AddRange(GetChildrenModule(manageModules, n, roleModuleIds)); });

            result.Message = "获取成功";
            result.Msg = true;
            result.Data = parent;
        }
        catch (Exception e)
        {
            result.Message = e.Message;
        }
        return Json(result);
    }

    /// <summary>
    ///     获取子节点
    /// </summary>
    private List<LayTreeModel> GetChildrenModule(List<Module> allModules, LayTreeModel parent, List<int> roleModuleIds)
    {
        List<LayTreeModel> children = allModules.Where(n => n.ParentId.ToString().Equals(parent.Id)).Select(n => new LayTreeModel
        {
            Id = n.Id.ToString(),
            Title = n.Name,
            Checked = roleModuleIds.Contains(n.Id)
        }).ToList();

        //获取子菜单
        children.ForEach(n => { n.Children.AddRange(GetChildrenModule(allModules, n, roleModuleIds)); });

        return children;
    }
}