namespace Repository.DbEntity
{
    /// <summary>
    ///     用户视图模型（包含所属社团信息）
    /// </summary>
    public class UserViewModel : User
    {
        /// <summary>
        ///     社团ID
        /// </summary>
        public int? ClubId { get; set; }

        /// <summary>
        ///     社团名称
        /// </summary>
        public string ClubName { get; set; }
    }
}