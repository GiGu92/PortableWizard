using PortableWizard.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableWizard
{
    class ApplicationManager
    {
        private string AppsPath;
        public ObservableCollection<Application> AppList { get; set; }


        public ApplicationManager()
        {
            AppList = new ObservableCollection<Application>();
        }
        public void Init(string AppsPath)
        {

            this.AppsPath = AppsPath;
            searchPortableApps();
        }

        private void searchPortableApps()
        {
            if (AppsPath.EndsWith("\\"))
            {
                AppsPath = AppsPath.Substring(0, AppsPath.Length - 2);
            }
            DirectoryInfo directory = new DirectoryInfo(AppsPath);
            if (directory.Exists)
            {
                DirectoryInfo[] subDirs = directory.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    string iniPath = dirInfo.FullName + @"\App\AppInfo\appinfo.ini";
                    FileInfo iniFile = new FileInfo(iniPath);
                    if (iniFile.Exists)
                    {
                        Application app = new Application(iniFile);
                        AppList.Add(app);
                    }
                }
            }
        }



    }
}
