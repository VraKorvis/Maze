using System;
using Maze.Data.Storages;

namespace Maze.Data.Storages {

    using System;
    using UnityEngine;
    using Maze.Data.Serialization;

    [Serializable]
    public class Ability : IUpdatableItem<Ability, AbilityData> {
        [SerializeField]
        private AbilityData _data;

        public event EventHandler UpdatedEvent;

        public void Update(AbilityData data) {
            _data = data;

            InvokeUpdatedEvent();
        }

        private void InvokeUpdatedEvent() {
            var handler = UpdatedEvent;
            if (handler != null) handler.Invoke(this, EventArgs.Empty);
        }
    }

    [Serializable]
    public class AbilityData {
        public string  description;
    }

    public class AbilitiesStorage : AbstractListStorage<Ability> {
        public AbilitiesStorage(IStorageSerializer storageSerializer) : base(storageSerializer) { StorageKey = "abilities"; }
    }

}