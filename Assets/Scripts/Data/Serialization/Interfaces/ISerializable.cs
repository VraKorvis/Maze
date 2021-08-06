namespace Maze.Data.Serialization
{
    public interface ISerializable
    {
        void OnBeforeSerialization();
        void OnAfterSerialization();
        void OnAfterDeserialization();
    }
}
