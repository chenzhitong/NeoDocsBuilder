using System;
using System.Diagnostics;
using System.IO;

namespace NeoDocsBuilder
{

    internal class Git
    {
        public static string GetLastEditDateTime(string gitRepoPath, string filePath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log -1 --format=%cd --date=format:\"%Y-%m-%d\" -- \"{filePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = gitRepoPath
            };

            using Process process = Process.Start(psi);
            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            return result;
        }
    }

}
