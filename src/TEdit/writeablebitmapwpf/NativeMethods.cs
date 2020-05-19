using System.Runtime.InteropServices;

namespace System.Windows.Media.Imaging
{
    public static class NativeMethods
    {
        public static void CopyUnmanagedMemory(IntPtr src, int srcOffset, IntPtr dst, int dstOffset, int count)
        {
            unsafe
            {
                var srcPtr = (byte*) src.ToPointer();
                srcPtr += srcOffset;
                var dstPtr = (byte*) dst.ToPointer();
                dstPtr += dstOffset;

                memcpy(dstPtr, srcPtr, count);
            }
        }

        public static void SetUnmanagedMemory(IntPtr dst, int filler, int count)
        {
            memset(dst, filler, count);
        }

        // Win32 memory copy function
        //[DllImport("ntdll.dll")]
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl,
            SetLastError = false)]
        private static extern unsafe byte* memcpy(
            byte* dst,
            byte* src,
            int count);

        // Win32 memory set function
        //[DllImport("ntdll.dll")]
        //[DllImport("coredll.dll", EntryPoint = "memset", SetLastError = false)]
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl,
            SetLastError = false)]
        private static extern void memset(
            IntPtr dst,
            int filler,
            int count);
    }
}