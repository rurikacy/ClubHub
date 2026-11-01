using Repository.DbEntity;

namespace ClubHub.Areas.ClubHub.Models;

public class ActiveViewModel : Active
{
    public string ClubName { get; set; }
    public string AdminName { get; set; }

    /// <summary>
    ///     当前用户的报名状态 null:未报名 0:已撤销 1:已报名 2:已参加 3:未参加
    /// </summary>
    public int? SignType { get; set; }

    /// <summary>
    ///     活动状态排序 3:已取消 2:已结束 1:已开始 0:未开始
    /// </summary>
    public int StatusOrder { get; set; }
}