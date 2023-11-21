// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace bottlenoselabs.Common;

[PublicAPI]
public static class Shell
{
    /// <summary>
    ///     Runs a command-line command using a new process and shell.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="workingDirectory">The current directory the command will be running from.</param>
    /// <param name="fileName">
    ///     Optional. The name of program to run. Default is <c>null</c>. If <c>null</c>, a shell is used as the file
    ///     name and the <paramref name="command" /> becomes the arguments to run from the shell. If the operating
    ///     system is not Windows, then the shell is <c>bash</c>. If the operating system is Windows, PowerShell is used
    ///     if <paramref name="windowsUsePowerShell" /> is <c>true</c>; otherwise Git Bash is used.
    /// </param>
    /// <param name="windowsUsePowerShell">
    ///     Determines whether to use PowerShell or Git Bash when <paramref name="fileName" />
    ///     is <c>null</c> and the operating system is Windows. Has no effect when <paramref name="fileName" /> is not
    ///     <c>null</c> or the operating system is not Windows.
    /// </param>
    /// <returns>A <see cref="ShellOutput" /> instance.</returns>
    public static ShellOutput ExecuteShellCommand(
        this string command,
        string? workingDirectory = null,
        string? fileName = null,
        bool windowsUsePowerShell = true)
    {
        using var process = CreateShellProcess(command, workingDirectory, fileName, windowsUsePowerShell);
        var stringBuilder = new StringBuilder();
        using var stringWriter = new StringWriter(stringBuilder);
        var spinLock = default(SpinLock);

        process.OutputDataReceived += OnProcessOnDataReceived;
        process.ErrorDataReceived += OnProcessOnDataReceived;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();

        var outputString = stringBuilder.ToString();

        var result = new ShellOutput
        {
            ExitCode = process.ExitCode,
            Output = outputString
        };
        return result;

        void OnProcessOnDataReceived(
            object sender,
            DataReceivedEventArgs args)
        {
            var gotLock = false;
            spinLock.Enter(ref gotLock);
            // ReSharper disable once AccessToDisposedClosure
            stringWriter.WriteLine(args.Data);
            if (gotLock)
            {
                spinLock.Exit();
            }
        }
    }

    private static Process CreateShellProcess(
        string command,
        string? workingDirectory,
        string? fileName,
        bool windowsUsePowerShell = true)
    {
        if (workingDirectory != null && !Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException(workingDirectory);
        }

        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(fileName))
        {
            processStartInfo.FileName = fileName;
            processStartInfo.Arguments = command;
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (windowsUsePowerShell)
                {
                    processStartInfo.FileName = "powershell.exe";
                }
                else
                {
                    var bashFilePath = WindowsBashFilePath();
                    if (string.IsNullOrEmpty(bashFilePath))
                    {
                        throw new FileNotFoundException(
                            "Failed to find a `git-bash.exe` or `bash.exe` on Windows. Did you forget to install Git Bash and/or add it to your PATH?");
                    }

                    processStartInfo.FileName = bashFilePath;
                }
            }
            else
            {
                processStartInfo.FileName = "bash";
            }

            var escapedArgs = command.Replace("\"", "\\\"", StringComparison.InvariantCulture);
            processStartInfo.Arguments = $"-c \"{escapedArgs}\"";
        }

        var process = new Process
        {
            StartInfo = processStartInfo
        };
        return process;
    }

    private static string WindowsBashFilePath()
    {
        var candidateBashFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "bin", "bash.exe");
        if (File.Exists(candidateBashFilePath))
        {
            return candidateBashFilePath;
        }

        candidateBashFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "bin", "bash.exe");
        if (File.Exists(candidateBashFilePath))
        {
            return candidateBashFilePath;
        }

        var environmentVariablePath = Environment.GetEnvironmentVariable("PATH");
        var searchDirectories = environmentVariablePath?.Split(';') ?? Array.Empty<string>();

        foreach (var searchDirectory in searchDirectories)
        {
            candidateBashFilePath = Path.Combine(searchDirectory, "bash.exe");
            if (File.Exists(candidateBashFilePath))
            {
                return candidateBashFilePath;
            }
        }

        return string.Empty;
    }
}
