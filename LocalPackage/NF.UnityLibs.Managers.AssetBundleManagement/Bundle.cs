using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    [Serializable]
    public class Bundle
    {
        public enum E_BUNDLE_STATE
        {
            RENTED,
            RETURNING,
            RETURNED,
        }

        [SerializeField]
        private uint _bundleUid;

        [SerializeField]
        private AssetBundleRef _assetBundleRef;

        [SerializeField]
        private Object[] _unityObjects;

        [SerializeField]
        private E_BUNDLE_STATE _state;

        public uint BundleUID => _bundleUid;
        public AssetBundleRef AssetBundleRef => _assetBundleRef;
        public E_BUNDLE_STATE State => _state;
        public string Name => _assetBundleRef.Name;

        internal Bundle(uint bundleUid, AssetBundleRef assetBundleRef)
        {
            _bundleUid = bundleUid;
            _assetBundleRef = assetBundleRef;
            _unityObjects = assetBundleRef.UnityObjects;
            _state = E_BUNDLE_STATE.RENTED;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetState_RETURNING()
        {
            Assert.AreEqual(E_BUNDLE_STATE.RENTED, _state);
            _state = E_BUNDLE_STATE.RETURNING;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetState_RETURNED()
        {
            Assert.AreEqual(E_BUNDLE_STATE.RETURNING, _state);
            _state = E_BUNDLE_STATE.RETURNED;
        }

        public T? GetAssetOrNull<T>() where T : Object
        {
            foreach (Object obj in _unityObjects)
            {
                if (obj is T t)
                {
                    return t;
                }
            }

            Assert.IsTrue(false, $"_unityObjects: {string.Join(", ", _unityObjects.Select(x => x.ToString()))}");
            return null;
        }
    }
}