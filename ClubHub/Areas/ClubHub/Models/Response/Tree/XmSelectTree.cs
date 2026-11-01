using System.Collections.Generic;

namespace ClubHub.Areas.ClubHub.Models.Response.Tree;

public class XmSelectTree
{
    /// <summary>
    ///     唯一标识
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    public bool Disabled { get; set; } = false;

    public bool Selected { get; set; } = false;

    /// <summary>
    ///     子集
    /// </summary>
    public List<XmSelectTree> Children { get; set; } = [];
}