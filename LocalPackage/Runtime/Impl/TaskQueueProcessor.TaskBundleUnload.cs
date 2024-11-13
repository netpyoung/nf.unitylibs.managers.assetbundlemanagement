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
        public sealed class TaskBundleUnload
        {
            private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

            [SerializeField]
            public Bundle _bundle;

            public Bundle Bundle => _bundle;

            public TaskBundleUnload(Bundle bundle)
            {
                _bundle = bundle;
                _bundle.SetState_RETURNING();
            }

            public TaskAwaiter<bool> GetAwaiter()
            {
                return _tcs.Task.GetAwaiter();
            }

            public void SetComplete()
            {
                _bundle.SetState_RETURNED();
                bool isSuccess = _tcs.TrySetResult(true);
                Assert.IsTrue(isSuccess);
            }
        }
    }
}