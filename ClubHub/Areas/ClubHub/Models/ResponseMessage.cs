using System.Text.Json.Serialization;

namespace ClubHub.Areas.ClubHub.Models;

/// <summary>
///     返回参数
/// </summary>
public class ResponseMessage
{
    /// <summary>
    ///     返回 code 标志
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; } = 0;

    /// <summary>
    ///     消息
    /// </summary>
    [JsonPropertyName("msg")]
    public bool Msg { get; set; } = false;

    /// <summary>
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     数量
    /// </summary>
    [JsonPropertyName("count")]
    public long Count { get; set; } = 0;

    /// <summary>
    ///     具体数据
    /// </summary>
    [JsonPropertyName("data")]
    public object Data { get; set; } = null;
}