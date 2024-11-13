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

        public override async Task<Exception?> InitAsync(string deviceBaseDirFpath, string manifestName)
        {
            string outBaseDirName = OUT_BASE_DIR_NAME;
            string outBaseBuildDirFPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", outBaseDirName));
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            int version = 0;
            string outBuildDirFPath = Path.Combine(outBaseBuildDirFPath, buildTarget.ToString(), version.ToString()).Replace('\\', '/');
            Exception? ret = await base.InitAsync(outBuildDirFPath, outBaseDirName);
            return ret;
        }
    }
}
#endif // UNITY_EDITOR