
namespace Maze.Data.LocalStorages.PathProvider
{
    using System.IO;

    public class PathProvider : IPathProvider
    {
        protected string prefix;

        public PathProvider(string path, string folder)
        {
            prefix = Path.Combine(path, folder + "/");

            if (!Directory.Exists(prefix))
                Directory.CreateDirectory(prefix);
        }

        public string GetPath(string path)
        {
            var result = Path.Combine(prefix, path);

            var directory = Path.GetDirectoryName(result);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return result;
        }
    }
}