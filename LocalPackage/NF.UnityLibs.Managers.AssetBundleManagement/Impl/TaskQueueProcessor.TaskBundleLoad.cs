using System;
using System.Collections.Generic;
using System.Linq;
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
            private Dictionary<string, AssetBundleRef> _dic = new Dictionary<string, AssetBundleRef>();

            [SerializeField]
            public string _assetBundleName;

            private AssetBundleRef? _mainAssetBundleRefOrNull;

            public string AssetBundleName => _assetBundleName;

            public float Progress
            {
                get
                {
                    if (_mainAssetBundleRefOrNull is null)
                    {
                        return 0;
                    }

                    if (_dic.Count == 0)
                    {
                        return _mainAssetBundleRefOrNull.Progress;
                    }

                    float average = _dic.Values.Average(x => x.Progress);
                    return average;
                }
            }

            public bool IsCompleted => _tcs.Task.IsCompleted;

            public TaskBundleLoad(string assetBundleName)
            {
                _assetBundleName = assetBundleName;
            }

            public TaskAwaiter<AssetBundleRef> GetAwaiter()
            {
                return _tcs.Task.GetAwaiter();
            }

            public void MarkAsComplete(AssetBundleRef mainAssetBundleRef)
            {
                _mainAssetBundleRefOrNull = mainAssetBundleRef;
                bool isSuccess = _tcs.TrySetResult(mainAssetBundleRef);
                Assert.IsTrue(isSuccess);
            }

            public void SetAssetBundleRef(string assetBundleName, AssetBundleRef assetBundleRef, bool isMain)
            {
                if (isMain)
                {
                    _mainAssetBundleRefOrNull = assetBundleRef;
                    _dic = new Dictionary<string, AssetBundleRef>(assetBundleRef.Dependencies.Length + 1);
                }

                _dic[assetBundleName] = assetBundleRef;
            }

            public void MarkAsDispose()
            {
                _tcs.SetCanceled();
            }
        }
    }
}