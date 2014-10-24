using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableWizard.Toolkit
{
    class WinProcessRestarter
    {
        public static void RestartProcess(string processName){
            foreach (Process p in Process.GetProcesses())
	        {
                try
                {
                    if (processName.StartsWith(p.ProcessName))
                    {
                        p.Kill();
                        Process proc = new Process();
                        proc.StartInfo.FileName = processName;
                        proc.StartInfo.UseShellExecute = true;
                        proc.Start();
                        break;
                    }
                }
                catch { }
	        }
        }
        public static void KillProcess(string processName)
        {
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (processName.StartsWith(p.ProcessName))
                    {
                        p.Kill();
                        break;
                    }
                }
                catch { }
            }
        }
    }
}
