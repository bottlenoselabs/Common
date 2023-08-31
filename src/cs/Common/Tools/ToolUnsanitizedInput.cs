// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace bottlenoselabs.Common.Tools;

/// <summary>
///     Represents un-sanitized input for execution of some sort of computation.
/// </summary>
[PublicAPI]
public class ToolUnsanitizedInput
{
    /// <summary>
    ///     Gets or sets the working directory to use.
    /// </summary>
    /// <remarks>
    ///     <para>Default is <c>null</c>. If <c>null</c>, the current directory is used.</para>
    /// </remarks>
    [JsonIgnore]
    public string? WorkingFileDirectory { get; set; }
}
