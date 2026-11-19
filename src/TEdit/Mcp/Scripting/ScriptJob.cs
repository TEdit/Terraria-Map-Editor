using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TEdit.Mcp.Scripting;

// Hold the job in the editor. A worker receives no source until assignment succeeds.
internal static class ScriptJob
{
    private const uint ProcessMemory = 0x100, KillOnClose = 0x2000, ActiveProcess = 0x8;
    private const int ExtendedLimitInformation = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct BasicLimits
    {
        public long ProcessTime, JobTime;
        public uint Flags;
        public nuint MinWorkingSet, MaxWorkingSet;
        public uint ActiveProcesses;
        public nuint Affinity;
        public uint Priority, Scheduling;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExtendedLimits
    {
        public BasicLimits Basic;
        public ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes;
        public nuint ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemory, PeakJobMemory;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateJobObjectW(IntPtr attributes, string? name);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(SafeFileHandle job, int kind, ref ExtendedLimits limits, uint size);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(SafeFileHandle job, SafeProcessHandle process);

    internal static SafeFileHandle Attach(Process worker)
    {
        var job = CreateJobObjectW(IntPtr.Zero, null);
        var limits = new ExtendedLimits
        {
            Basic = new() { Flags = ProcessMemory | KillOnClose | ActiveProcess, ActiveProcesses = 1 },
            ProcessMemoryLimit = ScriptProtocol.ProcessMemoryBytes
        };
        if (!job.IsInvalid &&
            SetInformationJobObject(job, ExtendedLimitInformation, ref limits, (uint)Marshal.SizeOf<ExtendedLimits>()) &&
            AssignProcessToJobObject(job, worker.SafeHandle)) return job;
        var error = new Win32Exception(Marshal.GetLastWin32Error());
        job.Dispose();
        throw error;
    }
}
