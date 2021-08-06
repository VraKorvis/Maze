
namespace Maze.Data.Serialization
{
    using System.Collections.Generic;
    using Data.LocalStorages;

    public class LocalStorageSerializer : IStorageSerializer
    {
        public ILocalStorage Storage { get; }
        public ISerializer Serializer { get; }

        public bool IsEncryptor { get { return Storage == null ? false : Storage.IsEncrypted; } }

        public LocalStorageSerializer(ISerializer serializer, ILocalStorage storage)
        {
            Serializer = serializer;
            Storage = storage;
        }

        public void Serialize<T>(string key, T value) where T : class
        {
            var data = Serializer.Serialize(value);
            Storage.Save(key, data);
        }

        public void SerializeSequence<T>(string key, IEnumerable<T> obj) where T : class
        {
            var data = Serializer.SerializeSequence(obj);
            Storage.Save(key, data);
        }

        public void Serialize<K, V>(string key, Dictionary<K, V> obj)
        {
            var data = Serializer.Serialize(obj);
            Storage.Save(key, data);
        }

        public T Deserialize<T>(string key) where T : class
        {
            string data;
            Storage.Load(key, out data);
            var obj = string.IsNullOrEmpty(data) ? default : Serializer.Deserialize<T>(data);
            return obj;
        }

        public IEnumerable<T> DeserializeSequence<T>(string key) where T : class
        {
            string data;
            Storage.Load(key, out data);
            var obj = string.IsNullOrEmpty(data) ? default : Serializer.DeserializeSequence<T>(data);
            return obj;
        }
    }
}
