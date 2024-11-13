using NF.UnityLibs.Managers.AssetBundleManagement.Impl;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    public class TaskBundle<T> where T : UnityEngine.Object
    {
        private Task<Bundle<T>> _task;
        private float _progress;

        public bool IsCompleted => _task.IsCompleted;
        public float Progress => _progress;

        internal TaskBundle(TaskQueueProcessor taskQueueProcessor, BundleFactory bundleFactory, string assetBundleName, CancellationToken ct)
        {
            _task = _LoadTask(taskQueueProcessor, bundleFactory, assetBundleName, ct);
        }

        private async Task<Bundle<T>> _LoadTask(TaskQueueProcessor taskQueueProcessor, BundleFactory bundleFactory, string assetBundleName, CancellationToken ct)
        {
            TaskQueueProcessor.TaskBundleLoad x = taskQueueProcessor.EnqueueTaskBundleLoad(assetBundleName);
            while (!x.IsCompleted)
            {
                if (ct.IsCancellationRequested)
                {
                    x.MarkAsDispose();
                    break;
                }

                _progress = x.Progress;
                await Task.Yield();
            }

            AssetBundleRef assetBundleRef = await x;
            Bundle<T> bundle = bundleFactory.GetBundleFromAssetBundleRef<T>(assetBundleRef);
            return bundle;
        }

        public TaskAwaiter<Bundle<T>> GetAwaiter()
        {
            return _task.GetAwaiter();
        }
    }
}
