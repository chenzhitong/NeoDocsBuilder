using System;
using System.IO;
using System.Linq;

namespace NeoDocsBuilder
{
    public static class Files
    {
        public static void CopyDirectory(string sourceDirPath, string saveDirPath)
        {
            if (!Directory.Exists(saveDirPath))
                Directory.CreateDirectory(saveDirPath);
            Directory.GetFiles(sourceDirPath).ToList().ForEach(p =>
            {
                var extension = Path.GetExtension(p);
                if (extension != ".md" && extension != ".json" && extension != ".yml")
                    try
                    {
                        File.Copy(p, Path.Combine(saveDirPath, Path.GetFileName(p)), true);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
            }
            );
            Directory.GetDirectories(sourceDirPath).ToList().ForEach(
                p => CopyDirectory(p, Path.Combine(saveDirPath, Path.GetFileName(p)))
            );
        }
    }
}
