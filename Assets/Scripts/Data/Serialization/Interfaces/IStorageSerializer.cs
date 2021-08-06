
namespace Maze.Data.Serialization
{
    using System.Collections.Generic;

    using Maze.Data.LocalStorages;

    public interface IStorageSerializer
    {
        ILocalStorage Storage { get; }
        ISerializer Serializer { get; }
        void Serialize<T>(string key, T obj) where T : class;
        void SerializeSequence<T>(string key, IEnumerable<T> obj) where T : class;
        T Deserialize<T>(string key) where T : class;
        IEnumerable<T> DeserializeSequence<T>(string key) where T : class;
        bool IsEncryptor { get; }
    }
}
