using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.Json;
using Windows.Win32;

namespace ProcessLogger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var procs = Process.GetProcesses();

                foreach (var proc in procs)
                {
                    var si = proc.StartInfo;
                    var st = proc.StartTime;
                    var id = proc.Id;
                    var hasExited = proc.HasExited;
                    var tc = proc.Threads.Count;
                    var et = proc.ExitTime;
                    var ec = proc.ExitCode;
                    var f = proc.ProcessName;

                    Console.WriteLine(JsonSerializer.Serialize());
                }

            }
        }

        // https://stackoverflow.com/a/38676215/534812
        private static string GetProcessUser(Process process)
        {
            SafeFileHandle? processHandle = null;
            try
            {
                PInvoke.OpenProcessToken(process.SafeHandle, Windows.Win32.Security.TOKEN_ACCESS_MASK.TOKEN_QUERY, out processHandle);
                string identity = string.Empty;
                unsafe
                {
                    const int bufferSize = 4096;
                    fixed (char* buffer = new char[bufferSize])
                    {
                        uint numberOfBytesRead;
                        NativeOverlapped nativeOverlapped;
                        PInvoke.ReadFile(processHandle, buffer, bufferSize, &numberOfBytesRead, &nativeOverlapped);
                    }
                }
                WindowsIdentity wi = new WindowsIdentity(identity);
                string user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (processHandle != null)
                {
                    processHandle.Close();
                }
            }
        }
    }
}