#if UNITY_EDITOR
using NF.UnityLibs.Managers.AssetBundleManagement.Serializables;
#else
using System.Collections.Concurrent;
#endif // UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace NF.UnityLibs.Managers.AssetBundleManagement.Impl
{
    [Serializable]
    internal partial class TaskQueueProcessor : IDisposable
    {
#if UNITY_EDITOR
        // For Inspector Debugging
        [SerializeField]
        private SerializableConcurrentQueue<TaskBundleLoad> _queue_TaskBundleLoad_Requested = new SerializableConcurrentQueue<TaskBundleLoad>();

        [SerializeField]
        private SerializableConcurrentQueue<TaskBundleUnload> _queue_TaskBundleUnload_Requested = new SerializableConcurrentQueue<TaskBundleUnload>();

        [SerializeField]
        private SerializableConcurrentQueue<AssetBundle> _queue_AssetBundle_LoadCompleted = new SerializableConcurrentQueue<AssetBundle>();

        [SerializeField]
        private SerializableDictionary<string, AssetBundleRef> _dic_AssetBundleRef_Loaded = new SerializableDictionary<string, AssetBundleRef>(100);
#else
        private readonly ConcurrentQueue<TaskBundleLoad> _queue_TaskBundleLoad_Requested = new ConcurrentQueue<TaskBundleLoad>();
        private readonly ConcurrentQueue<TaskBundleUnload> _queue_TaskBundleUnload_Requested = new ConcurrentQueue<TaskBundleUnload>();
        private readonly ConcurrentQueue<AssetBundle> _queue_AssetBundle_LoadCompleted = new ConcurrentQueue<AssetBundle>();
        private readonly Dictionary<string, AssetBundleRef> _dic_AssetBundleRef_Loaded = new Dictionary<string, AssetBundleRef>(100);
#endif // UNITY_EDITOR

        private readonly List<TaskBundleLoad> _list_TaskBundleLoad_Requested = new List<TaskBundleLoad>();
        private readonly List<TaskBundleUnload> _list_TaskBundleUnload_Requested = new List<TaskBundleUnload>();
        private readonly List<AssetBundle> _list_AssetBundle_LoadCompleted = new List<AssetBundle>();
        private readonly List<TaskBundleLoad> _list_TaskBundleLoad_Pending = new List<TaskBundleLoad>();
        private readonly List<TaskBundleLoad> _list_TaskBundleLoad_Loaded = new List<TaskBundleLoad>();

        [SerializeField]
        private AssetBundleManifest _assetBundleManifest = null!;

        [SerializeField]
        private string _deviceBaseFpath = string.Empty;

        public void Init(string deviceBaseFpath, AssetBundleManifest assetBundleManifest)
        {
            _assetBundleManifest = assetBundleManifest;
            _deviceBaseFpath = deviceBaseFpath.Replace("\\", "/");
        }

        public TaskBundleLoad EnqueueTaskBundleLoad(string name)
        {
            TaskBundleLoad t = new TaskBundleLoad(name);
            _queue_TaskBundleLoad_Requested.Enqueue(t);
            return t;
        }

        public TaskBundleUnload EnqueueTaskBundleUnload(Bundle bundle)
        {
            TaskBundleUnload t = new TaskBundleUnload(bundle);
            _queue_TaskBundleUnload_Requested.Enqueue(t);
            return t;
        }


        public void Update()
        {
            {
                while (_queue_TaskBundleUnload_Requested.TryDequeue(out TaskBundleUnload t))
                {
                    _list_TaskBundleUnload_Requested.Add(t);
                }

                foreach (TaskBundleUnload t in _list_TaskBundleUnload_Requested)
                {
                    _HandleTaskBundleUnload(t);
                }

                if (_list_TaskBundleUnload_Requested.Count > 0)
                {
                    _list_TaskBundleUnload_Requested.Clear();
                }
            }

            {
                while (_queue_TaskBundleLoad_Requested.TryDequeue(out TaskBundleLoad t))
                {
                    _list_TaskBundleLoad_Requested.Add(t);
                }

                foreach (TaskBundleLoad t in _list_TaskBundleLoad_Requested)
                {
                    if (!_HandleTaskBundleLoad(t))
                    {
                        _list_TaskBundleLoad_Pending.Add(t);
                    }
                }

                if (_list_TaskBundleLoad_Requested.Count > 0)
                {
                    _list_TaskBundleLoad_Requested.Clear();
                }
            }

            {
                while (_queue_AssetBundle_LoadCompleted.TryDequeue(out AssetBundle assetBundle))
                {
                    _list_AssetBundle_LoadCompleted.Add(assetBundle);
                }

                foreach (AssetBundle assetBundle in _list_AssetBundle_LoadCompleted)
                {
                    bool isSuccess = _dic_AssetBundleRef_Loaded.TryGetValue(assetBundle.name, out AssetBundleRef assetBundleRef);
                    Assert.IsTrue(isSuccess);
                    assetBundleRef!.SetAssetBundle(assetBundle);
                }

                if (_list_AssetBundle_LoadCompleted.Count > 0)
                {
                    _list_AssetBundle_LoadCompleted.Clear();
                }
            }

            {
                foreach (TaskBundleLoad t in _list_TaskBundleLoad_Pending)
                {
                    bool isSuccess0 = _dic_AssetBundleRef_Loaded.TryGetValue(t.AssetBundleName, out AssetBundleRef mainAssetBundleRef);
                    Assert.IsTrue(isSuccess0);
                    if (mainAssetBundleRef!.State != AssetBundleRef.E_ASSETBUNDLEREF_STATE.LOADED)
                    {
                        continue;
                    }

                    bool isAllDepdenciesLoaded = true;
                    foreach (string depName in mainAssetBundleRef.Dependencies)
                    {
                        bool isSuccess1 = _dic_AssetBundleRef_Loaded.TryGetValue(depName, out AssetBundleRef depAssetBundleRef);
                        Assert.IsTrue(isSuccess1);
                        if (depAssetBundleRef!.State != AssetBundleRef.E_ASSETBUNDLEREF_STATE.LOADED)
                        {
                            isAllDepdenciesLoaded = false;
                            break;
                        }
                    }

                    if (isAllDepdenciesLoaded)
                    {
                        t.SetAssetBundleRef(mainAssetBundleRef);
                        _list_TaskBundleLoad_Loaded.Add(t);
                    }
                }

                foreach (TaskBundleLoad t in _list_TaskBundleLoad_Loaded)
                {
                    _list_TaskBundleLoad_Pending.Remove(t);
                }

                if (_list_TaskBundleLoad_Loaded.Count > 0)
                {
                    _list_TaskBundleLoad_Loaded.Clear();
                }
            }
        }

        public void Dispose()
        {
            foreach (AssetBundleRef assetBundleRef in _dic_AssetBundleRef_Loaded.Values)
            {
                assetBundleRef.Unload();
            }

            _dic_AssetBundleRef_Loaded.Clear();
        }

        internal void _HandleTaskBundleUnload(TaskBundleUnload t)
        {
            AssetBundleRef mainAssetBundleRef = t.Bundle.AssetBundleRef;
            string mainAssetBundleRefName = mainAssetBundleRef.Name;
            foreach (string depName in mainAssetBundleRef.Dependencies)
            {
                bool isSuccess0 = _dic_AssetBundleRef_Loaded.TryGetValue(depName, out AssetBundleRef depAssetBundleRef);
                Assert.IsTrue(isSuccess0);

                int depRefCount = depAssetBundleRef!.ReferenceCount_Decrement();
                Assert.IsTrue(depRefCount >= 0, $"depRefCount >= 0 / {depRefCount} / {mainAssetBundleRefName}");
                if (depRefCount == 0)
                {
                    depAssetBundleRef.Unload();
                    bool isSuccess1 = _dic_AssetBundleRef_Loaded.Remove(depName);
                    Assert.IsTrue(isSuccess1, $"_dic_AssetBundleRef.Remove({depName});");
                }
            }

            int mainRefCount = mainAssetBundleRef.ReferenceCount_Decrement();
            Assert.IsTrue(mainRefCount >= 0, $"mainRefCount >= 0: {mainAssetBundleRefName} // {mainRefCount}");
            if (mainRefCount == 0)
            {
                mainAssetBundleRef.Unload();
                bool isSuccess = _dic_AssetBundleRef_Loaded.Remove(mainAssetBundleRefName);
                Assert.IsTrue(isSuccess, $"_dic_AssetBundleRef.Remove({mainAssetBundleRefName})");
            }

            t.SetComplete();
        }

        private bool _HandleTaskBundleLoad(TaskBundleLoad t)
        {
            string mainAssetBundleName = t.AssetBundleName;
            if (_dic_AssetBundleRef_Loaded.TryGetValue(mainAssetBundleName, out AssetBundleRef mainAssetBundleRef))
            {
                mainAssetBundleRef.ReferenceCount_Increment();
                foreach (string dep in mainAssetBundleRef.Dependencies)
                {
                    bool isSuccess = _dic_AssetBundleRef_Loaded.TryGetValue(dep, out AssetBundleRef depAssetBundleRef);
                    Assert.IsTrue(isSuccess);
                    depAssetBundleRef!.ReferenceCount_Increment();
                }

                if (mainAssetBundleRef.State == AssetBundleRef.E_ASSETBUNDLEREF_STATE.LOADED)
                {
                    t.SetAssetBundleRef(mainAssetBundleRef);
                    return true;
                }

                return false;
            }

            AssetBundleRef newAssetBundleRef = __RegisterAssetBundleRef(mainAssetBundleName);
            foreach (string dep in newAssetBundleRef.Dependencies)
            {
                if (_dic_AssetBundleRef_Loaded.TryGetValue(dep, out AssetBundleRef depAssetBundleRef))
                {
                    depAssetBundleRef.ReferenceCount_Increment();
                }
                else
                {
                    __RegisterAssetBundleRef(dep);
                }
            }

            return false;
        }

        private AssetBundleRef __RegisterAssetBundleRef(string assetBundleName)
        {
            string[] dependencies = _assetBundleManifest.GetAllDependencies(assetBundleName);
            AssetBundleRef assetBundleRef = new AssetBundleRef(assetBundleName, dependencies);
            _dic_AssetBundleRef_Loaded[assetBundleName] = assetBundleRef;

            string assetBundleFpath = $"{_deviceBaseFpath}/{assetBundleName}";
            Assert.IsTrue(File.Exists(assetBundleFpath), $"assetBundleFpath {assetBundleFpath} does not exist");
            
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(assetBundleFpath);
            abcr.completed += (AsyncOperation ao) =>
            {
                AssetBundle bundle = abcr.assetBundle;
                Assert.IsNotNull(bundle, $"abcr.assetBundle is null. {assetBundleName}");
                _queue_AssetBundle_LoadCompleted.Enqueue(bundle);
            };
            return assetBundleRef;
        }
    }
}