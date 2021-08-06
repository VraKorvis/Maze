
namespace Maze.Data.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public class JsonSerializer : ISerializer
    {
        private string json;
        private readonly string wrapperChars = "{\"l\":";

        public string Serialize<T>(T obj) where T : class
        {
            var serializable = obj as ISerializable;
            if (serializable != null)
                serializable.OnBeforeSerialization();

            json = JsonUtility.ToJson(obj, false);

            if (serializable != null)
                serializable.OnAfterSerialization();

            return json;
        }

        public string SerializeSequence<T>(IEnumerable<T> obj) where T : class
        {
            var wrapper = new JsonWrapper<T>(obj.ToList());
            json = JsonUtility.ToJson(wrapper);

            //Remove first {"l": - 5 chars and last } char
            //empty wrapper {"l":[]}
            var jsonLength = json.Length;
            json = json.Substring(5, jsonLength - 6);

            return json;
        }

        public T Deserialize<T>(string data) where T : class
        {
            if (string.IsNullOrEmpty(data)) return default(T);

            var obj = JsonUtility.FromJson<T>(data);

            if (obj == null)
            {
                Debug.LogErrorFormat(" JSON Deserialization error {0} JSON data: {1}", typeof(T), data);
                return null;
            }

            var serializable = obj as ISerializable;
            if (serializable != null)
                serializable.OnAfterDeserialization();

            return obj;
        }

        public IEnumerable<T> DeserializeSequence<T>(string data) where T : class
        {
            if (string.IsNullOrEmpty(data)) return new List<T>();

            var sb = new StringBuilder(wrapperChars.Length + data.Length + 1);
            sb.Append(wrapperChars);
            sb.Append(data);
            sb.Append("}");

            var wrapper = JsonUtility.FromJson<JsonWrapper<T>>(sb.ToString());

            return wrapper.List;
        }

        [Serializable]
        private class JsonWrapper<T>
        {
            //Do not modify the filed char count
            [SerializeField] private List<T> l;

            public JsonWrapper(List<T> list)
            {
                l = list;
            }

            public List<T> List
            {
                get { return l; }
            }
        }
    }
}