namespace Maze.Data.LocalStorages.PathProvider
{
    using UnityEngine;

    public class StreamingAssetsFolderPathProvider : PathProvider
    {
        public StreamingAssetsFolderPathProvider() : base(Application.streamingAssetsPath, "LocalData") { }
    }
}