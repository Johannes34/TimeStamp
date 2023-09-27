using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeStamp
{
    internal static class ProcessWatcher
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        internal static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        internal static Process GetActiveWindowProcess()
        {
            uint processID;
            IntPtr hWnd = GetForegroundWindow(); // Get foreground window handle
            uint threadID = GetWindowThreadProcessId(hWnd, out processID); // Get PID from window handle
            Process fgProc = Process.GetProcessById(Convert.ToInt32(processID)); // Get it as a C# obj.
            // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
            return fgProc;
        }

        private static Task m_watcher;
        private static CancellationTokenSource m_token;

        internal static HashSet<string> PreviousWindowTitles { get; } = new HashSet<string>();

        internal static string CurrentProcessWindowTitle { get; private set; }
        internal static event Action<string> OnCurrentProcessWindowTitleChanged;

        internal static string CurrentProcess { get; private set; }
        internal static event Action<string> OnCurrentProcessChanged;

        internal static List<ProcessTime> ProcessTimes { get; } = new List<ProcessTime>();

        internal static void Start(TimeSettings settings)
        {
            m_token = new CancellationTokenSource();
            m_watcher = Task.Run(() =>
            {
                while (!m_token.IsCancellationRequested)
                {
                    if (settings.EnableAutoTrackingApplications)
                    {
                        var currentTitle = GetActiveWindowTitle();

                        if (CurrentProcessWindowTitle != currentTitle)
                        {
                            CurrentProcessWindowTitle = currentTitle;
                            PreviousWindowTitles.Add(currentTitle);

                            foreach (var mapping in settings.AutoTrackingApplications)
                            {
                                if (currentTitle.IndexOf(mapping.Key, StringComparison.OrdinalIgnoreCase) != -1)
                                {
                                    if (CurrentProcess != mapping.Value)
                                    {
                                        CurrentProcess = mapping.Value;
                                        if (ProcessTimes.Any())
                                            ProcessTimes.Last().End = DateTime.Now;
                                        ProcessTimes.Add(new ProcessTime() { Process = CurrentProcess, Start = DateTime.Now });
                                        OnCurrentProcessChanged?.Invoke(CurrentProcess);
                                    }
                                    break;
                                }
                            }

                            OnCurrentProcessWindowTitleChanged?.Invoke(CurrentProcessWindowTitle);
                        }
                    }
                    Thread.Sleep(5000);
                }
            });
        }

        internal static void Stop()
        {
            m_token?.Cancel();
        }

    }

    internal class ProcessTime
    {
        public string Process { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
