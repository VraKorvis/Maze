
namespace Maze.Data.Storages
{
    using Maze.Data.Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class AbstractListStorage<TItem> : IEnumerable<TItem>, IStorage
        where TItem : class
    {
        public readonly IStorageSerializer _serializer;
        protected readonly List<TItem> _storageData;

        public string StorageKey { get; protected set; }

        public AbstractListStorage(IStorageSerializer serializer)
        {
            _serializer = serializer;
            _storageData = new List<TItem>();
        }

        public void Load()
        {
            _storageData.Clear();
            var loaded = _serializer.DeserializeSequence<TItem>(StorageKey);

            if (loaded != null)
                _storageData.AddRange(loaded);
        }

        public void Save()
        {
            _serializer.SerializeSequence(StorageKey, _storageData);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _storageData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}