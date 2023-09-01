// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace bottlenoselabs.Common.CommandLine;

/// <summary>
///     The application host for a command-line application.
/// </summary>
[PublicAPI]
public sealed class CommandLineHost : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly RootCommand _rootCommand;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLineHost"/> class.
    /// </summary>
    /// <param name="applicationLifetime">The application lifetime.</param>
    /// <param name="command">The command.</param>
    public CommandLineHost(
        IHostApplicationLifetime applicationLifetime,
        RootCommand command)
    {
        _applicationLifetime = applicationLifetime;
        _rootCommand = command;
    }

    /// <inheritdoc cref="IHostedService" />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() => Task.Run(Main, cancellationToken));
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IHostedService" />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void Main()
    {
        var commandLineArguments = Environment.GetCommandLineArgs();
        Environment.ExitCode = _rootCommand.Invoke(commandLineArguments);
        _applicationLifetime.StopApplication();
    }
}
