
namespace Maze.Data.Storages
{
    using System;

    public interface IUpdatableItem<TItem, in TItemData>
    {
        event EventHandler UpdatedEvent;
        void Update(TItemData data);
    }
}
