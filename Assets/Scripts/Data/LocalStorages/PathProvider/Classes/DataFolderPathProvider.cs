
namespace Maze.Data.LocalStorages.PathProvider
{
    using UnityEngine;

    public class DataFolderPathProvider : PathProvider
    {
        public DataFolderPathProvider() : base(Application.persistentDataPath, "LocalData") { }
    }
}