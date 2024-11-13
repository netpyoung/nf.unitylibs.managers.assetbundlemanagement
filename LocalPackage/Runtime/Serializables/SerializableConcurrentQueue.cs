using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace NF.UnityLibs.Managers.AssetBundleManagement.Serializables
{
    [Serializable]
    internal sealed class SerializableConcurrentQueue<T> : ConcurrentQueue<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<T> serializedItems = new List<T>();

        public void OnBeforeSerialize()
        {
            serializedItems.Clear();
            foreach (T item in this)
            {
                serializedItems.Add(item);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (T item in serializedItems)
            {
                Enqueue(item);
            }
        }
    }
}
