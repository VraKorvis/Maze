
namespace Maze.Data.Serialization
{
    using System.Collections.Generic;

    public interface ISerializer
    {
        string Serialize<T>(T obj) where T : class;
        string SerializeSequence<T>(IEnumerable<T> obj) where T : class;
        T Deserialize<T>(string data) where T : class;
        IEnumerable<T> DeserializeSequence<T>(string data) where T : class;
    }
}
