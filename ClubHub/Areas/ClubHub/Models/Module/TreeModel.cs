using System.Collections.Generic;
using System.Linq;

namespace ClubHub.Areas.ClubHub.Models.Module;

public class TreeModel
{
    public TreeModel(Repository.DbEntity.Module parent, List<Repository.DbEntity.Module> modules)
    {
        Id = parent.Id;
        Name = parent.Name;
        Icon = parent.Icon;
        Url = parent.Url;
        ParentId = parent.ParentId;
        List<Repository.DbEntity.Module> children = modules.Where(n => n.ParentId == parent.Id).ToList();
        if (children.Any())
        {
            Children.AddRange(children.Select(n => new TreeModel(n, modules)).ToList());
        }
    }

    /// <summary>
    ///     id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     模块名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     模块图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     模块url
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     父级id
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    ///     子级
    /// </summary>
    public List<TreeModel> Children { get; set; } = [];
}