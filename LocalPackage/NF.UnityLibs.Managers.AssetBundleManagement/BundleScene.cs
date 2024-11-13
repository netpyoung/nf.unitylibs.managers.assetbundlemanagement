using System;
using UnityEngine;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    [Serializable]
    public sealed class BundleScene : Bundle
    {
        [SerializeField]
        private string[] _scenePaths;

        public string[] ScenePaths => _scenePaths;

        internal BundleScene(uint bundleUid, AssetBundleRef assetBundleRef) : base(bundleUid, assetBundleRef)
        {
            _scenePaths = assetBundleRef.AssetBundleOrNull!.GetAllScenePaths();
        }
    }
}