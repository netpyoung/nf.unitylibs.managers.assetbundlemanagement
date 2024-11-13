#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    public class AssetBundleManager_ForEditorBuild0 : AssetBundleManager
    {
        public const string OUT_BASE_DIR_NAME = "__ASSETBUNDLE";
        public const string MANIFEST_NAME = OUT_BASE_DIR_NAME;

        public static string GetDeviceBaseDirFpath_ForEditorBuild0()
        {
            string outBaseDirName = OUT_BASE_DIR_NAME;
            string outBaseBuildDirFPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", outBaseDirName));
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            int version = 0;
            string deviceBaseDirFpath = Path.Combine(outBaseBuildDirFPath, buildTarget.ToString(), version.ToString()).Replace('\\', '/');
            return deviceBaseDirFpath;
        }

        public override async Task<Exception?> InitAsync(string deviceBaseDirFpath, string manifestName)
        {
            deviceBaseDirFpath = GetDeviceBaseDirFpath_ForEditorBuild0();
            manifestName = MANIFEST_NAME;
            Exception? ret = await base.InitAsync(deviceBaseDirFpath, manifestName);
            return ret;
        }
    }
}
#endif // UNITY_EDITOR