using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.IRepository;

namespace ClubHub.Services;

/// <summary>
///     活动自动取消服务 - 当活动开始时间到达但报名人数不足时自动取消活动
/// </summary>
public class ActivityAutoCancelService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ActivityAutoCancelService> _logger;

    public ActivityAutoCancelService(IServiceProvider serviceProvider, ILogger<ActivityAutoCancelService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("活动自动取消服务已启动");

        // 等待10秒让应用程序完全启动
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndCancelActivitiesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查并取消活动时发生错误");
            }

            // 每分钟检查一次
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("活动自动取消服务已停止");
    }

    /// <summary>
    ///     检查并取消不满足最低人数要求的活动
    /// </summary>
    private async Task CheckAndCancelActivitiesAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var activeRepository = scope.ServiceProvider.GetRequiredService<IActiveRepository>();

        try
        {
            // 获取所有即将开始且未取消的活动
            // 条件是：开始时间 <= 当前时间，状态为正常，且设置了最低人数要求
            // 使用动态查询避免字段映射问题
            string sql = """
                         SELECT Id, ClubId, AdminId, Title, Content, EnterData, 
                                StartTime, EndTime, JoinNum, Place,
                                ISNULL(MinParticipants, 0) AS MinParticipants,
                                ISNULL([Status], 0) AS [Status],
                                CancelReason
                         FROM T_Active 
                         WHERE StartTime <= GETDATE() 
                         AND ISNULL([Status], 0) = 0 
                         AND ISNULL(MinParticipants, 0) > 0
                         """;

            // 使用动态类型查询
            var activitiesToCheck = await activeRepository.FindAllAsync<dynamic>(sql);

            foreach (var activity in activitiesToCheck)
            {
                try
                {
                    int activityId = (int)activity.Id;
                    int minParticipants = (int)activity.MinParticipants;
                    string title = activity.Title?.ToString() ?? "";

                    // 获取实际报名人数（SignType = 1 表示已报名）
                    string countSql = """
                                      SELECT COUNT(*) FROM T_ActiveSign 
                                      WHERE ActiveId = @ActiveId AND SignType = 1
                                      """;
                    int actualParticipants = await activeRepository.ExecuteScalarAsync<int>(countSql, new { ActiveId = activityId });

                    // 如果报名人数少于最低要求，则取消活动
                    if (actualParticipants < minParticipants)
                    {
                        string cancelReason = $"报名人数不足，实际报名{actualParticipants}人，最低要求{minParticipants}人";
                        
                        // 使用SQL直接更新，避免实体映射问题
                        string updateSql = """
                                          UPDATE T_Active 
                                          SET [Status] = 1, 
                                              CancelReason = @CancelReason 
                                          WHERE Id = @Id
                                          """;
                        
                        int rowsAffected = await activeRepository.ExecuteAsync(updateSql, new { Id = activityId, CancelReason = cancelReason });

                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation("活动已自动取消 - 活动ID: {ActivityId}, 标题: {Title}, 实际报名: {Actual}, 最低要求: {Min}",
                                activityId, title, actualParticipants, minParticipants);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理活动取消时发生错误 - 活动ID: {ActivityId}", (int)activity.Id);
                }
            }
        }
        catch (Exception ex)
        {
            // 如果是因为字段不存在导致的错误，记录警告而不是错误
            if (ex.Message.Contains("列名") && (ex.Message.Contains("Status") || ex.Message.Contains("MinParticipants")))
            {
                _logger.LogWarning("数据库缺少必要的字段（Status/MinParticipants），请先执行数据库迁移脚本。活动自动取消功能暂时不可用。");
            }
            else
            {
                throw;
            }
        }
    }
}
