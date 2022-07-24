using System.Reflection;
using Microsoft.Extensions.Configuration;
using NeerCore.DependencyInjection.Extensions;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace NeerCore.Logging;

/// <summary>
///   NLog logger installer. Configures and creates logger.
/// </summary>
public static class LoggerInstaller
{
    private const string AppSettingsName = "appsettings.json";

    /// <summary>
    ///   Configures logger with predefined settings and sets it to
    ///   <see cref="LogManager.Configuration">LogManager.Configuration</see>.
    /// </summary>
    /// <param name="configureOptions">Logger options configuration <see cref="Action{T}"/>.</param>
    /// <param name="defaultLoggerName">Returned <see cref="ILogger"/> instance name (<b>{RootNamespace}.Program</b> by default).</param>
    /// <returns>Configured <see cref="ILogger"/> instance.</returns>
    public static ILogger InitDefault(Action<LoggingSettings>? configureOptions = null, string? defaultLoggerName = null)
    {
        DefaultLoggerInstaller.Configure(configureOptions);
        return CreateDefaultLogger(defaultLoggerName);
    }

    /// <summary>
    ///   Configures logger with predefined settings based on <b>appsettings.{Environment}.json</b>
    ///   and sets it to <see cref="LogManager.Configuration">LogManager.Configuration</see>.
    /// </summary>
    /// <param name="sectionName">Logger configuration section name in appsettings file.</param>
    /// <param name="environmentVariableName">Environment variable name where is set application environment (Development/Production/Staging).</param>
    /// <param name="defaultLoggerName">Returned <see cref="ILogger"/> instance name (<b>{RootNamespace}.Program</b> by default).</param>
    /// <returns>Configured <see cref="ILogger"/> instance.</returns>
    public static ILogger InitFromCurrentEnvironment(string sectionName = "Logging", string environmentVariableName = "ASPNETCORE_ENVIRONMENT", string? defaultLoggerName = null)
    {
        return InitFromEnvironment(Environment.GetEnvironmentVariable(environmentVariableName), sectionName, defaultLoggerName);
    }

    /// <summary>
    ///   Configures logger with predefined settings based on <b>appsettings.{Environment}.json</b>
    ///   and sets it to <see cref="LogManager.Configuration">LogManager.Configuration</see>.
    /// </summary>
    /// <param name="environment">Application environment (Development/Production/Staging).</param>
    /// <param name="sectionName">Logger configuration section name in appsettings file.</param>
    /// <param name="defaultLoggerName">Returned <see cref="ILogger"/> instance name (<b>{RootNamespace}.Program</b> by default).</param>
    /// <returns>Configured <see cref="ILogger"/> instance.</returns>
    public static ILogger InitFromEnvironment(string? environment, string sectionName = "Logging", string? defaultLoggerName = null)
    {
        string configurationPath = string.IsNullOrEmpty(environment) ? AppSettingsName : $"appsettings.{environment}.json";
        var loggingConfigSection = BuildConfiguration(configurationPath, AppSettingsName).GetSection(sectionName);
        DefaultLoggerInstaller.Configure(loggingConfigSection.Bind);
        return CreateDefaultLogger(defaultLoggerName);
    }

    /// <summary>
    ///   Configures logger with predefined settings based on file targeted in <paramref name="configurationPath"/>
    ///   and sets it to <see cref="LogManager.Configuration">LogManager.Configuration</see>.
    /// </summary>
    /// <remarks>
    ///   This method does not provide predefined configuration, you should set all configuration in
    ///   <paramref name="configurationPath"/> file with root section <paramref name="rootSectionName"/>.
    /// </remarks>
    /// <param name="rootSectionName">NLog configuration file path.</param>
    /// <param name="configurationPath">Logger configuration section name in <paramref name="rootSectionName"/> file.</param>
    /// <param name="defaultLoggerName">Returned <see cref="ILogger"/> instance name (<b>{RootNamespace}.Program</b> by default).</param>
    /// <returns>Configured <see cref="ILogger"/> instance.</returns>
    public static ILogger InitFromFile(string configurationPath = "NLog.json", string rootSectionName = "NLog", string? defaultLoggerName = null)
    {
        var config = BuildConfiguration(configurationPath);

        LogManager.Configuration = new NLogLoggingConfiguration(config.GetRequiredSection(rootSectionName));
        return NLogBuilder.ConfigureNLog(LogManager.Configuration).GetLogger(defaultLoggerName);
    }

    private static ILogger CreateDefaultLogger(string? loggerName)
    {
        loggerName ??= Assembly.GetEntryAssembly()!.GetBaseNamespace() + ".Program";
        return NLogBuilder.ConfigureNLog(LogManager.Configuration).GetLogger(loggerName);
    }

    private static IConfigurationRoot BuildConfiguration(string configurationPath, string? altConfigurationPath = null)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configurationPath, optional: true);
        if (!string.IsNullOrEmpty(altConfigurationPath))
            configurationBuilder = configurationBuilder
                .AddJsonFile(altConfigurationPath, optional: false);

        return configurationBuilder.Build();
    }
}