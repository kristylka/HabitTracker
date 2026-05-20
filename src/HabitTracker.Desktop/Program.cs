using System;
using System.IO;
using Avalonia;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Interfaces;
using HabitTracker.Data;
using HabitTracker.Data.Repositories;
using HabitTracker.Desktop.ViewModels;
using HabitTracker.Desktop.Views;
using HabitTracker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        App.Services = serviceProvider;

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HabitTracker", "habits.db");

        var dir = Path.GetDirectoryName(dbPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IHabitEntryRepository, HabitEntryRepository>();
        services.AddScoped<IHabitScheduleRepository, HabitScheduleRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<MainWindow>();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<global::HabitTracker.Desktop.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}