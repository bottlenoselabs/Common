// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;

namespace bottlenoselabs.Common;

/// <summary>
///     The output of executing a command from command line.
/// </summary>
[PublicAPI]
public class ShellOutput
{
    /// <summary>
    ///     Gets the exit code.
    /// </summary>
    public int ExitCode { get; internal set; }

    /// <summary>
    ///     Gets the output of the command.
    /// </summary>
    public string Output { get; internal set; } = string.Empty;
}
