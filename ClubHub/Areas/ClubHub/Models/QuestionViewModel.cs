using System;
using System.Collections.Generic;

namespace ClubHub.Areas.ClubHub.Models;

/// <summary>
///     问卷视图模型
/// </summary>
public class QuestionViewModel
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public string ClubName { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime EnterData { get; set; }
    public byte Status { get; set; }
    public int AnswerCount { get; set; }
    public bool HasAnswered { get; set; }
}

/// <summary>
///     问卷题目视图模型
/// </summary>
public class QuestionItemViewModel
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Content { get; set; }
    public byte ItemType { get; set; }
    public string Options { get; set; }
    public int SortOrder { get; set; }
    public List<string> OptionList { get; set; }
    public string MyAnswer { get; set; }
}

/// <summary>
///     问卷统计视图模型
/// </summary>
public class QuestionStatViewModel
{
    public int ItemId { get; set; }
    public string Content { get; set; }
    public byte ItemType { get; set; }
    public List<OptionStatViewModel> OptionStats { get; set; }
}

/// <summary>
///     选项统计视图模型
/// </summary>
public class OptionStatViewModel
{
    public int OptionIndex { get; set; }
    public string OptionText { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
///     提交答案请求模型
/// </summary>
public class SubmitAnswerRequest
{
    public int QuestionId { get; set; }
    public List<ItemAnswer> Answers { get; set; }
}

/// <summary>
///     单题答案
/// </summary>
public class ItemAnswer
{
    public int ItemId { get; set; }
    public string Answer { get; set; }
}