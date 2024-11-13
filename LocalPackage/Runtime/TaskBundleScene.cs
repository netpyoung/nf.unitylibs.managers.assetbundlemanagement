using NF.UnityLibs.Managers.AssetBundleManagement.Impl;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    public class TaskBundleScene
    {
        private Task<BundleScene> _task;
        private float _progress;

        public bool IsCompleted => _task.IsCompleted;
        public float Progress => _progress;

        internal TaskBundleScene(TaskQueueProcessor taskQueueProcessor, BundleFactory bundleFactory, string assetBundleName, CancellationToken ct)
        {
            _task = _LoadTask(taskQueueProcessor, bundleFactory, assetBundleName, ct);
        }

        internal TaskBundleScene(BundleScene bundleScene)
        {
            _task = Task.FromResult(bundleScene);
        }

        private async Task<BundleScene> _LoadTask(TaskQueueProcessor taskQueueProcessor, BundleFactory bundleFactory, string assetBundleName, CancellationToken ct)
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
            BundleScene bundle = bundleFactory.GetBundleSceneFromAssetBundleRef(assetBundleRef);
            return bundle;
        }

        public TaskAwaiter<BundleScene> GetAwaiter()
        {
            return _task.GetAwaiter();
        }
    }
}
