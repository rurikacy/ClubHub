using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClubHub.Filters
{
    /// <summary>
    /// 管理员权限验证特性
    /// 确保只有管理员可以访问管理员端功能
    /// </summary>
    public class AdminAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            
            // 检查用户是否已登录
            string userId = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new JsonResult(new { status = "error", msg = "请先登录" });
                return;
            }

            // 检查用户类型是否为管理员（0=超级管理员，1=社团管理员）
            string userType = httpContext.Session.GetString("UserType");
            if (string.IsNullOrEmpty(userType) || (userType != "0" && userType != "1"))
            {
                context.Result = new JsonResult(new { status = "error", msg = "您没有管理员权限" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// 超级管理员权限验证特性
    /// 确保只有超级管理员可以访问特定功能
    /// </summary>
    public class SuperAdminAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            
            // 检查用户是否已登录
            string userId = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new JsonResult(new { status = "error", msg = "请先登录" });
                return;
            }

            // 检查用户类型是否为超级管理员（0=超级管理员）
            string userType = httpContext.Session.GetString("UserType");
            if (string.IsNullOrEmpty(userType) || userType != "0")
            {
                context.Result = new JsonResult(new { status = "error", msg = "您没有超级管理员权限" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// 角色权限验证特性
    /// 基于RBAC的权限验证
    /// </summary>
    public class RoleAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly string _requiredPermission;

        public RoleAuthorizationAttribute(string requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            
            // 检查用户是否已登录
            string userId = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new JsonResult(new { status = "error", msg = "请先登录" });
                return;
            }

            // 检查用户类型是否为管理员
            string userType = httpContext.Session.GetString("UserType");
            if (string.IsNullOrEmpty(userType) || (userType != "0" && userType != "1"))
            {
                context.Result = new JsonResult(new { status = "error", msg = "您没有管理员权限" });
                return;
            }

            // 超级管理员拥有所有权限
            if (userType == "0")
            {
                base.OnActionExecuting(context);
                return;
            }

            // TODO: 实现基于RBAC的细粒度权限验证
            // 这里可以查询数据库验证用户是否有_requiredPermission权限

            base.OnActionExecuting(context);
        }
    }
}
