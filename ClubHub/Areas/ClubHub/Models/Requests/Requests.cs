namespace ClubHub.Areas.ClubHub.Models.Requests;

/// <summary>
///     成绩管理--成绩查看页面搜索request
/// </summary>
public class ReqScoreViewList : PageBaseRequest
{
}

/// <summary>
///     成绩管理--成绩维护页面搜索request
/// </summary>
public class ReqScoreList : PageBaseRequest
{
    public string StudentName { get; set; }

    public string CourseName { get; set; }
}

/// <summary>
///     授课信息管理搜索request
/// </summary>
public class ReqTeachInfoList : PageBaseRequest
{
    /// <summary>
    ///     社团管理员姓名
    /// </summary>
    public string TeacherName { get; set; }

    /// <summary>
    ///     课程名称
    /// </summary>
    public string CourseName { get; set; }
}

/// <summary>
///     班级管理搜索request
/// </summary>
public class ReqClassList : PageBaseRequest
{
    /// <summary>
    ///     班级名称
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
///     学生管理搜索request
/// </summary>
public class ReqStudentList : PageBaseRequest
{
    /// <summary>
    ///     社团管理员用户名或真实姓名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     用户手机号
    /// </summary>
    public string Phone { get; set; }
}

/// <summary>
///     社团管理员管理搜索request
/// </summary>
public class ReqTeacherList : PageBaseRequest
{
    /// <summary>
    ///     社团管理员用户名或真实姓名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     用户手机号
    /// </summary>
    public string Phone { get; set; }
}

/// <summary>
///     保存角色模块request
/// </summary>
public class ReqSaveRoleModule
{
    /// <summary>
    ///     角色id
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    ///     模块ids 如1,2,3,4
    /// </summary>
    public string ModuleIds { get; set; }
}

/// <summary>
///     通过角色id获取模块list request
/// </summary>
public class ReqListByRoleId
{
    public int RoleId { get; set; } = 0;
}

/// <summary>
///     用户管理搜索request
/// </summary>
public class ReqUserList : PageBaseRequest
{
    /// <summary>
    ///     用户名或真实姓名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     用户手机号
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    ///     社团ID
    /// </summary>
    public int? ClubId { get; set; }
}

/// <summary>
///     用户管理搜索request
/// </summary>
public class ReqRoleList : PageBaseRequest
{
    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
///     分页基础request
/// </summary>
public class PageBaseRequest
{
    public int Page { get; set; }

    public int Limit { get; set; }
}