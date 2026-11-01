using System.Collections.Generic;

namespace ClubHub.Areas.ClubHub.Models.Module;

/// <summary>
///     layui tree
/// </summary>
public class LayTreeModel
{
    /// <summary>
    ///     节点标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     节点唯一索引值
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     节点字段名
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    ///     节点是否初始为选中状态（如果开启复选框的话），默认 false
    /// </summary>
    public bool Checked { get; set; }

    /// <summary>
    ///     点击节点弹出新窗口对应的 url。需开启 isJump 参数
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    ///     节点是否为禁用状态。默认 false
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    ///     节点是否初始展开，默认 false
    /// </summary>
    public bool Spread { get; set; }

    /// <summary>
    ///     子节点
    /// </summary>
    public List<LayTreeModel> Children { get; set; } = [];
}