using Maze.Data.LocalStorages.PathProvider;

namespace Maze.Data.LocalStorages
{
    public interface ILocalStorage
    {
        bool IsEncrypted { get; }
        IPathProvider PathProvider { get; }

        void Delete(string path);
        void Load(string path, out string data);
        void Load(string path, out byte[] data);
        void Save(string path, string data);
        void Save(string path, byte[] data);
    }
}
