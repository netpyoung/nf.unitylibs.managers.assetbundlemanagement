using System;
using System.Collections.Generic;
using UnityEngine;

namespace NF.UnityLibs.Managers.AssetBundleManagement.Serializables
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();
        [SerializeField]
        private List<TValue> _values = new List<TValue>();

        public SerializableDictionary(int capacity) : base(capacity)
        {
        }

        public void OnBeforeSerialize()
        {
            if (_keys != null)
            {
                _keys.Clear();
            }

            if (_values != null)
            {
                _values.Clear();
            }

            foreach (KeyValuePair<TKey, TValue> keyPair in this)
            {
                _keys!.Add(keyPair.Key);
                _values!.Add(keyPair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < _keys.Count; ++i)
            {
                Add(_keys[i], _values[i]);
            }
        }
    }
}
