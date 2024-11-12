using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace NF.UnityLibs.Managers.AssetBundleManagement.Impl
{
    internal partial class TaskQueueProcessor
    {
        [Serializable]
        internal sealed class TaskBundleLoad
        {
            private TaskCompletionSource<AssetBundleRef> _tcs = new TaskCompletionSource<AssetBundleRef>();

            [SerializeField]
            public string _assetBundleName;

            public string AssetBundleName => _assetBundleName;

            public TaskBundleLoad(string assetBundleName)
            {
                _assetBundleName = assetBundleName;
            }

            public TaskAwaiter<AssetBundleRef> GetAwaiter()
            {
                return _tcs.Task.GetAwaiter();
            }

            public void SetAssetBundleRef(AssetBundleRef assetBundleRef)
            {
                bool isSuccess = _tcs.TrySetResult(assetBundleRef);
                Assert.IsTrue(isSuccess);
            }
        }
    }
}