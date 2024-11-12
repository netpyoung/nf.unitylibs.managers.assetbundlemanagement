#if UNITY_EDITOR
using NF.UnityLibs.Managers.AssetBundleManagement.Serializables;
#else
using System.Collections.Generic;
#endif // UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace NF.UnityLibs.Managers.AssetBundleManagement.Impl
{
    [Serializable]
    internal sealed class BundleFactory : IDisposable
    {
#if UNITY_EDITOR
        // For Inspector Debugging
        [SerializeField]
        private SerializableDictionary<uint, Bundle> _dic_RentedBundle = new SerializableDictionary<uint, Bundle>(100);
#else
        private readonly Dictionary<uint, Bundle> _dic_RentedBundle = new Dictionary<uint, Bundle>();
#endif // UNITY_EDITOR
        [SerializeField]
        private uint _accUid = 0;

        private uint _GetBundleUid()
        {
            do
            {
                unchecked
                {
                    _accUid++;
                }
            } while (_dic_RentedBundle.ContainsKey(_accUid));

            return _accUid;
        }

        public Bundle<T> GetBundleFromAssetBundleRef<T>(AssetBundleRef assetBundleRef) where T : UnityEngine.Object
        {
            uint bundleUid = _GetBundleUid();
            Bundle<T> bundle = new Bundle<T>(bundleUid, assetBundleRef);
            bool isSuccess = _dic_RentedBundle.TryAdd(bundle.BundleUID, bundle);
            Assert.IsTrue(isSuccess, $"_dic_RentedBundle.TryAdd({bundle.BundleUID}, bundle)");
            return bundle;
        }

        public void PrepareReturn(Bundle bundle)
        {
            bool isSuccess = _dic_RentedBundle.Remove(bundle.BundleUID, out Bundle outBundle);
            Assert.IsTrue(isSuccess, $"_dic_RentedBundle.TryRemove{bundle.BundleUID}");
            Assert.AreEqual(bundle.AssetBundleRef, outBundle.AssetBundleRef, $"bundle.AssetBundleRef == outBundle.AssetBundleRef / {bundle.AssetBundleRef} / {outBundle.AssetBundleRef}");
        }

        public void Dispose()
        {
            _dic_RentedBundle.Clear();
        }
    }
}