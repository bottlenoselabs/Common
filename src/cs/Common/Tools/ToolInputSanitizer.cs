// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace bottlenoselabs.Common.Tools;

public abstract class ToolInputSanitizer<TUnsanitizedInput, TInput>
{
    public abstract TInput Sanitize(TUnsanitizedInput unsanitizedInput);
}
