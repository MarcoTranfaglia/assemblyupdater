using AssemblyUpdater.Models;
using Microsoft.Build.Evaluation;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


//NOT WORKING BECAUSE  Microsoft.Build.Evaluation does not run correctly in NET Core
//https://github.com/dotnet/msbuild/issues/3434
//Ugly workaround to call SetMsBuildExePath 
//https://blog.rsuter.com/missing-sdk-when-using-the-microsoft-build-package-in-net-core/

//<WcfServiceModelTargetPath>$(VSToolsPath)\WCF</WcfServiceModelTargetPath>

namespace AssemblyUpdater.Utility
{
    public static class FrameworkManager
    {
        /*MT Findings
        When loading NET FRAMEWORK projects 
        it fails <WcfServiceModelTargetPath>$(VSToolsPath)\WCF</WcfServiceModelTargetPath>
       <Import Project="$(WcfServiceModelTargetPath)\Microsoft.VisualStudio.ServiceModel.targets" />
        so you need to use  ProjectLoadSettings.IgnoreMissingImports
        */

        private static void SetMsBuildExePath()
        {
            try
            {
                var startInfo = new ProcessStartInfo("dotnet", "--list-sdks")
                {
                    RedirectStandardOutput = true
                };
                var process = Process.Start(startInfo);
                process.WaitForExit(1000);

                var output = process.StandardOutput.ReadToEnd();
                var sdkPaths = Regex.Matches(output, "([0-9]+.[0-9]+.[0-9]+) \\[(.*)\\]")
                    .OfType<Match>()
                    .Select(m => System.IO.Path.Combine(m.Groups[2].Value, m.Groups[1].Value, "MSBuild.dll"));

                var sdkPath = sdkPaths.Last();
                Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", sdkPath);

            }
            catch (Exception exception)
            {
            }
        }

        public static FrameworkProjectItem ReadFramework(string absoluteFilePath)
        {
            FrameworkProjectItem frameworkItem = new FrameworkProjectItem();
            SetMsBuildExePath();
            string targetFramework = string.Empty;
            string frameworkVersion = string.Empty;
            var collection = new ProjectCollection();

            var project = new Project(absoluteFilePath, null, null, collection, ProjectLoadSettings.IgnoreMissingImports);

            targetFramework = project.GetProperty("TargetFramework") != null ? project.GetProperty("TargetFramework").EvaluatedValue : "";

            if (targetFramework.StartsWith("netcoreapp"))
            {
                frameworkItem.FrameworkType = MicrosoftFrameworkType.NETCore;
            }
            else if (targetFramework.StartsWith("netstandard"))
            {
                frameworkItem.FrameworkType = MicrosoftFrameworkType.NETStandard;
            }
            else
            {
                frameworkItem.FrameworkType = MicrosoftFrameworkType.NETFramework;
            }

            frameworkItem.FrameworkVersion = project.GetProperty("TargetFrameworkVersion").EvaluatedValue;
            frameworkItem.Project = Path.GetFileName(absoluteFilePath);
            return frameworkItem;
        }

        public static string ReadFrameworkVersion(string absoluteFilePath)
        {
            SetMsBuildExePath();
            if (!string.IsNullOrEmpty(absoluteFilePath))
            {
                string frameworkVersion = string.Empty;
                var collection = new ProjectCollection();

                var project = new Project(absoluteFilePath, null, null, collection, ProjectLoadSettings.IgnoreMissingImports);

                frameworkVersion = project.GetProperty("TargetFrameworkVersion").EvaluatedValue;

                return frameworkVersion;
            }

            return "0.0";
        }

        public static string ReadVersionFromProjectFile(string absoluteFilePath)
        {
            SetMsBuildExePath();
            if (!string.IsNullOrEmpty(absoluteFilePath))
            {
                string frameworkVersion = string.Empty;
                var collection = new ProjectCollection();

                var project = new Project(absoluteFilePath, null, null, collection, ProjectLoadSettings.IgnoreMissingImports);

                frameworkVersion = project.GetProperty("Version").EvaluatedValue;

                return frameworkVersion;
            }

            return "0.0";
        }

        public static void WriteFrameworkVersion(string absoluteFilePath, string newFrameworkVersion)
        {
            SetMsBuildExePath();

            if (!string.IsNullOrEmpty(absoluteFilePath))
            {
                string frameworkVersion = string.Empty;
                var collection = new ProjectCollection();
              // var project = collection.LoadProject(absoluteFilePath);
                var project = new Project(absoluteFilePath, null, null, collection, ProjectLoadSettings.IgnoreMissingImports);

                project.SetProperty("TargetFrameworkVersion", newFrameworkVersion);
                project.Save();

            }

        }
    }
}
