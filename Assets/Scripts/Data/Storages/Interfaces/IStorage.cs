namespace Maze.Data.Storages
{
    public interface IStorage
    {
        void Load();
        void Save();
    }
}