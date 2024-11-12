using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    [Serializable]
    public sealed class Bundle<T> : Bundle where T : UnityEngine.Object
    {
        [SerializeField]
        private T? _assetOrNull = null;

        public T? AssetOrNull
        {
            get
            {
                if (_assetOrNull is not null)
                {
                    return _assetOrNull;
                }

                T? assetOrNull = GetAssetOrNull<T>();
                Assert.IsNotNull(assetOrNull, $"{nameof(AssetOrNull)}");

                _assetOrNull = assetOrNull;
                return assetOrNull;
            }
        }

        internal Bundle(uint bundleUid, AssetBundleRef assetBundleRef) : base(bundleUid, assetBundleRef)
        {
        }
    }
}