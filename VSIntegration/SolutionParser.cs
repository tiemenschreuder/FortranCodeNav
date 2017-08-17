using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VSIntegration
{
    public class SolutionParser
    {
        public IList<CodeFile> ParseSln(string slnPath, Predicate<string> fileFilter)
        {
            slnPath = Path.GetFullPath(slnPath);

            var files = new List<CodeFile>();
            foreach (var projPath in GetProjectPaths(slnPath))
            {
                var projectName = Path.GetFileNameWithoutExtension(projPath);
                VSLogger.Write(string.Format(" parsing project {0}", projectName));
                var filePaths = GetFilePaths(projPath, fileFilter).ToList();
                VSLogger.Write(string.Format(" - {0} files", filePaths.Count));
                foreach (var file in filePaths)
                {
                    files.Add(new CodeFile { FilePath = file, ProjectName = projectName });
                }
            }
            VSLogger.Write(string.Format("- total {0} files", files.Count));
            return files;
        }

        private IEnumerable<string> GetFilePaths(string projPath, Predicate<string> fileFilter)
        {
            //only works for vfproj..
            if (!File.Exists(projPath))
                return new string[0];

            var dir = Path.GetDirectoryName(projPath);
            var regex = new Regex(@"<File.*?=\""(.*?)\"".*?");
            var projContent = File.ReadAllText(projPath);
            return regex.Matches(projContent)
                        .Cast<Match>()
                        .Select(match => match.Groups[1].Value)
                        .Where(f => fileFilter(f))
                        .Select(f => Path.GetFullPath(Path.Combine(dir, f)));
        }

        private IEnumerable<string> GetProjectPaths(string slnPath)
        {
            var dir = Path.GetDirectoryName(slnPath);
            var regex = new Regex("Project.*?=.*?\".*?\", \"(.*?)\".*?");
            var slnContent = File.ReadAllText(slnPath);
            return regex.Matches(slnContent)
                        .Cast<Match>()
                        .Select(match => Path.Combine(dir, match.Groups[1].Value));
        } 
    }
}