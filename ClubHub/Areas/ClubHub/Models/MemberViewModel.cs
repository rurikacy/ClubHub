using Repository.DbEntity;

namespace ClubHub.Areas.ClubHub.Models;

public class MemberViewModel : Member
{
    /// <summary>
    ///     社团名称
    /// </summary>
    public string ClubName { get; set; }

    /// <summary>
    ///     部门名称
    /// </summary>
    public string DutyName { get; set; }

    /// <summary>
    ///     部门职位 0：成员 1：部长
    /// </summary>
    public byte? DutyRole { get; set; }
}