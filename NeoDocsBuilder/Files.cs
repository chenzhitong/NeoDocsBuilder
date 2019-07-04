using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NeoDocsBuilder
{
    public static class Files
    {
        public static void CopyDirectory(string sourceDirPath, string saveDirPath, bool includeMarkDown = false)
        {
            if (!Directory.Exists(saveDirPath))
                Directory.CreateDirectory(saveDirPath);
            Directory.GetFiles(sourceDirPath).ToList().ForEach(
                p => { if (Path.GetExtension(p) != ".md" && Path.GetFileName(p) != "folder.json")
                        File.Copy(p, Path.Combine(saveDirPath, Path.GetFileName(p)), true);
                }
            );
            Directory.GetDirectories(sourceDirPath).ToList().ForEach(
                p => CopyDirectory(p, Path.Combine(saveDirPath, Path.GetFileName(p)))
            );
        }

        public static void CopyDirectoryOnly(string sourceDirPath, string saveDirPath)
        {
            if (!Directory.Exists(saveDirPath))
                Directory.CreateDirectory(saveDirPath);
            Directory.GetDirectories(sourceDirPath).ToList().ForEach(
                p => CopyDirectoryOnly(p, Path.Combine(saveDirPath, Path.GetFileName(p)))
            );
        }
    }
}
