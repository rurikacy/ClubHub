using System.Collections.Generic;

namespace Repository.Model
{
    public class CurrentUser
    {
        /// <summary>
        ///     用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///     角色 0：超级管理员 1：社团管理员 2：成员
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        ///     超级管理员角色ids
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();

        /// <summary>
        ///     所属社团ID，可为null表示未加入任何社团
        /// </summary>
        public int? ClubId { get; set; }
    }
}