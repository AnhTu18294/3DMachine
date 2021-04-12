using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CT3DMachine.Tools
{
    class PathTool
    {
        public static string bingPathFromAppDir(string localPath)
        {
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDir, @"..\..\" + localPath)));
            return directory.ToString();
        }
    }
}
