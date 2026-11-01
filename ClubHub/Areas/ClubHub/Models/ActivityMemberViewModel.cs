using Repository.DbEntity;

namespace ClubHub.Areas.ClubHub.Models;

public class ActivityMemberViewModel : Member
{
    /// <summary>
    ///     报名记录ID
    /// </summary>
    public int SignId { get; set; }

    /// <summary>
    ///     报名状态 0已撤销 1已报名 2已参加 3未参加
    /// </summary>
    public byte SignType { get; set; }
}