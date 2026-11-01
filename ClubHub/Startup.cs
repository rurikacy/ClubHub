using System;
using System.Reflection;
using System.Text.Json;
using Autofac;
using ClubHub.Hubs;
using ClubHub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository;

namespace ClubHub;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                // 配置JSON序列化选项，使用小写属性名
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        //添加cookie服务
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => { });

        // 添加session服务
        services.AddSession(options =>
        {
            // 设置1天Session过期
            options.IdleTimeout = TimeSpan.FromDays(1);
            options.Cookie.HttpOnly = true;
        });

        // 添加SignalR服务
        services.AddSignalR();

        // 添加活动自动取消后台服务
        services.AddHostedService<ActivityAutoCancelService>();
    }

    /// <summary>
    ///     autoFac使用了UseServiceProviderFactory
    ///     必须写这个方法，不然不走
    ///     在ConfigureServices方法之后调用
    /// </summary>
    /// <param name="builder"></param>
    public void ConfigureContainer(ContainerBuilder builder)
    {
        //添加任何Autofac模块或注册。
        //这是在ConfigureServices之后调用的，所以
        //在此处注册将覆盖在ConfigureServices中注册的内容。
        //在构建主机时必须调用“UseServiceProviderFactory（new AutofacServiceProviderFactory（））”`否则将不会调用此方法
        //var basePath = ApplicationEnvironment.ApplicationBasePath;
        try
        {
            //Service是继承接口的实现方法类库名称
            Assembly assemblys = Assembly.Load("Repository");
            //IDependency 是一个接口（所有要实现依赖注入的借口都要继承该接口）
            Type baseType = typeof(IDependencyRepository);
            builder.RegisterAssemblyTypes(assemblys)
                .Where(m => baseType.IsAssignableFrom(m) && m != baseType)
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            // 这俩需要单独注册
            //builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            //builder.RegisterType<JwtAppService>().As<IJwtAppService>();
        }
        catch (Exception e)
        {
            Console.WriteLine("注入失败：" + e);
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            //endpoints.MapControllerRoute(
            //    name: "default",
            //    pattern: "{controller=Home}/{action=Login}/{id?}");

            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Login}/{id?}");

            endpoints.MapAreaControllerRoute(
                "areas", "ClubHub",
                "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            
            // 映射SignalR Hub路由
            endpoints.MapHub<ChatHub>("/chatHub");
            //endpoints.MapRazorPages();
        });
    }
}