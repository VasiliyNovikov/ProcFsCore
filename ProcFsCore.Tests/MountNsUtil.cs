using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProcFsCore.Tests;

public class MountNsUtil
{
    public static void Scope(Action<Context> scopeAction)
    {
        ExceptionDispatchInfo? edi = null;
        var thread = new Thread(() =>
        {
            try
            {
                //CreateNewMountNamespaceForCurrentThread();
                scopeAction(new Context());
            }
            catch (Exception e)
            {
                edi = ExceptionDispatchInfo.Capture(e);
            }
        });
        thread.Start();
        thread.Join();
        edi?.Throw();
    }

    public struct Context
    {
        public void MountTemp(string target) => MountTmpFs(target);
    }

    private static void CreateNewMountNamespaceForCurrentThread() => UnShare(0x00020000); // CLONE_NEWNS

    private static void MountTmpFs(string target) => Mount("tmpfs", target, "tmpfs", 0, "");
    
    private const string LibC = "libc.so.6";

    [DllImport(LibC, EntryPoint = "unshare", SetLastError = true)]
    private static extern int UnShareRaw(int flags);
    private static void UnShare(int flags)
    {
        if (UnShareRaw(flags) == -1)
            throw new Win32Exception();
    }

    [DllImport(LibC, EntryPoint = "mount", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int MountRaw(string source, string target, string fileSystemType, ulong mountFlags, string data);
    private static void Mount(string source, string target, string fileSystemType, ulong mountFlags, string data)
    {
        if (MountRaw(source, target, fileSystemType, mountFlags, data) == -1)
            throw new Win32Exception();
    }
    
    [DllImport(LibC, EntryPoint = "umount2", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int UnmountRaw(string target, int flags);
    private static void Unmount(string target)
    {
        if (UnmountRaw(target, 0) == -1)
            throw new Win32Exception();
    }
}