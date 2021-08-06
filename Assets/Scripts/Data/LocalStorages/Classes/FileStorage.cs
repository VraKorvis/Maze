
namespace Maze.Data.LocalStorages
{
    using System.IO;
    using PathProvider;

    public class FileStorage : ILocalStorage
    {
        public bool IsEncrypted { get { return false; } }
        public IPathProvider PathProvider { get; }

        public FileStorage(IPathProvider path)
        {
            PathProvider = path;
        }

        public void Delete(string path)
        {
            path = PathProvider.GetPath(path);
            if (File.Exists(path))
                File.Delete(path);
        }

        public void Load(string path, out string data)
        {
            path = PathProvider.GetPath(path);
            data = File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void Load(string path, out byte[] data)
        {
            path = PathProvider.GetPath(path);
            data = File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public void Save(string path, string data)
        {
            path = PathProvider.GetPath(path);
            File.WriteAllText(path, data);
        }

        public void Save(string path, byte[] data)
        {
            path = PathProvider.GetPath(path);
            File.WriteAllBytes(path, data);
        }
    }
}
