using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NF.UnityLibs.Managers.AssetBundleManagement.Editors
{
    public class EditorAssetBundleHelper
    {
        public const string ASSETBUNDLE_DIR_ASSETPATH = "Assets/@AssetBundle";
        public const string OUT_BASE_DIR_NAME = "__ASSETBUNDLE";
        public const string MANIFEST_NAME = OUT_BASE_DIR_NAME;

        [MenuItem("@NFV2/AssetBundles/TaggingAssetName")]
        public static void TaggingAssetName()
        {
            string inAssetBundleDirAssetPath = ASSETBUNDLE_DIR_ASSETPATH;
            try
            {
                AssetDatabase.StartAssetEditing();

                string[] assetGuids = AssetDatabase.FindAssets("", new string[] { inAssetBundleDirAssetPath });
                foreach (string guid in assetGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        continue;
                    }

                    string assetBundleName = GetAssetBundleNameFromAssetPath(inAssetBundleDirAssetPath, assetPath);
                    AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
                    assetImporter.assetBundleName = assetBundleName;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("@NFV2/AssetBundles/BuildVersion0")]
        public static void BuildVersion0()
        {
            // EditorBuildSettings.UseParallelAssetBundleBuilding
            //EditorUtility.DisplayProgressBar("Hello", "world", 0.5f);
            //EditorUtility.ClearProgressBar();

            string inAssetBundleDirAssetPath = ASSETBUNDLE_DIR_ASSETPATH;
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            string outBaseDirName = OUT_BASE_DIR_NAME;
            int version = 0;
            string outBaseBuildDirFPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", outBaseDirName));
            Exception? exOrNull = BuildAssetBundle(inAssetBundleDirAssetPath, outBaseBuildDirFPath, buildTarget, version);
            if (exOrNull is Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return;
            }
        }

        [MenuItem("@NFV2/AssetBundles/BuildVersion0 - OpenDirectory")]
        public static void BuildVersion0_OpenDirectory()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            string outBaseDirName = OUT_BASE_DIR_NAME;
            int version = 0;
            string outBaseBuildDirFPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", outBaseDirName));
            string outBuildDirFPath = Path.Combine(outBaseBuildDirFPath, buildTarget.ToString(), version.ToString(), MANIFEST_NAME).Replace('\\', '/');
            
            Debug.Log(outBuildDirFPath);
            EditorUtility.RevealInFinder(outBuildDirFPath);
        }

        public static string GetAssetBundleNameFromAssetPath(string inAssetBundleDirAssetPath, string assetPath)
        {
            string lowerCaseAssetPath = assetPath.ToLower();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lowerCaseAssetPath);
            string lowerCaseAssetPathWithoutExtension = Path.Combine(Path.GetDirectoryName(lowerCaseAssetPath), fileNameWithoutExtension).Replace('\\', '/');
            string lowerCaseBundlePath = lowerCaseAssetPathWithoutExtension.Substring(inAssetBundleDirAssetPath.Length + 1);
            string lowerCaseBundlePathWithSeperator = lowerCaseBundlePath.Replace('/', '$');

            // lowerCaseAssetPath                  "assets/_bundle/ui/ui_01.mat"
            // fileNameWithoutExtension            "ui_01"
            // lowerCaseAssetPathWithoutExtension  "assets/_bundle/ui/ui_01"
            // lowerCaseBundlePath                 "ui/ui_01"
            // lowerCaseBundlePathWithSeperator    "ui$ui_01"
            string assetBundleName = lowerCaseBundlePathWithSeperator;
            return assetBundleName;
        }

        private static Exception? BuildAssetBundle(string inAssetBundleDirAssetPath, string outBaseBuildDirFPath, BuildTarget buildTarget, int version)
        {
            string outBuildDirFPath = Path.Combine(outBaseBuildDirFPath, buildTarget.ToString(), version.ToString()).Replace('\\', '/');

            BuildAssetBundleOptions buildOpt = BuildAssetBundleOptions.None;
            buildOpt |= BuildAssetBundleOptions.ChunkBasedCompression; // Use chunk-based LZ4
            buildOpt |= BuildAssetBundleOptions.StrictMode;

            AssetBundleManifest manifest;
            {
                List<AssetBundleBuild> abbList = _GetAssetBundleBuildList(inAssetBundleDirAssetPath);
                AssetBundleBuild[] abbArr = abbList.ToArray();
                BuildAssetBundlesParameters buildParams = new BuildAssetBundlesParameters
                {
                    options = buildOpt,
                    targetPlatform = buildTarget,
                    outputPath = outBuildDirFPath,
                    bundleDefinitions = abbArr,
                };

                try
                {
                    Directory.CreateDirectory(outBuildDirFPath);
                    manifest = BuildPipeline.BuildAssetBundles(buildParams);


                    string manifestFpath_Version = Path.Combine(outBuildDirFPath, $"{version}");
                    string manifestFpath_Version_manifest = Path.Combine(outBuildDirFPath, $"{version}.manifest");
                    string manifestFpath_MANIFEST_NAME = Path.Combine(outBuildDirFPath, $"{MANIFEST_NAME}");
                    string manifestFpath_MANIFEST_NAME_manifest = Path.Combine(outBuildDirFPath, $"{MANIFEST_NAME}.manifest");
                    File.Copy(manifestFpath_Version, manifestFpath_MANIFEST_NAME, overwrite: true);
                    File.Copy(manifestFpath_Version_manifest, manifestFpath_MANIFEST_NAME_manifest, overwrite: true);
                    File.Delete(manifestFpath_Version);
                    File.Delete(manifestFpath_Version_manifest);
                }
                catch (Exception ex)
                {
                    return ex;
                }

                BuildReport report = BuildReport.GetLatestReport();
                if (report != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Build result   : {report.summary.result}");
                    sb.AppendLine($"Build size     : {report.summary.totalSize} bytes");
                    sb.AppendLine($"Build time     : {report.summary.totalTime}");
                    sb.AppendLine($"Error summary  : {report.SummarizeErrors()}");
                    sb.Append(LogBuildReportSteps(report));
                    sb.AppendLine(LogBuildMessages(report));
                    Debug.Log(sb.ToString());
                }

                if (manifest == null)
                {
                    return new Exception("WTF");
                }
            }

            return null;
        }

        private static List<AssetBundleBuild> _GetAssetBundleBuildList(string inAssetBundleDirAssetPath)
        {
            string[] assetGuids = AssetDatabase.FindAssets("", new string[] { inAssetBundleDirAssetPath });
            if (assetGuids.Length == 0)
            {
                return new List<AssetBundleBuild>();
            }

            List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild>(inAssetBundleDirAssetPath.Length);
            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    continue;
                }

                string assetBundleName = GetAssetBundleNameFromAssetPath(inAssetBundleDirAssetPath, assetPath);
                AssetBundleBuild abb = new AssetBundleBuild
                {
                    assetBundleName = assetBundleName,
                    assetBundleVariant = string.Empty,
                    assetNames = new string[] { assetPath },
                    addressableNames = new string[] { assetBundleName }
                };
                assetBundleBuildList.Add(abb);
            }

            return assetBundleBuildList;
        }

        private static string LogBuildReportSteps(BuildReport buildReport)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Build steps: {buildReport.steps.Length}");
            int maxWidth = buildReport.steps.Max(s => s.name.Length + s.depth) + 3;
            foreach (BuildStep step in buildReport.steps)
            {
                string rawStepOutput = new string('-', step.depth + 1) + ' ' + step.name;
                sb.AppendLine($"{rawStepOutput.PadRight(maxWidth)}: {step.duration:g}");
            }
            return sb.ToString();
        }

        private static string LogBuildMessages(BuildReport buildReport)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BuildStep step in buildReport.steps)
            {
                foreach (BuildStepMessage message in step.messages)
                {
                    sb.AppendLine($"[{message.type}] {message.content}");
                }
            }

            if (sb.Length == 0)
            {
                return string.Empty;
            }

            sb.Insert(0, "Messages logged during Build:\n");
            string buildMessage = sb.ToString();
            return buildMessage;
        }
    }
}
