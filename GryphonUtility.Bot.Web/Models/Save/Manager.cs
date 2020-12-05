using System;
using System.IO;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Manager<T>
    {
        public T Data { get; private set; }

        public Manager(string path, Func<T> create)
        {
            _path = path;
            _create = create;
            _locker = new object();
        }

        public void Save()
        {
            lock (_locker)
            {
                string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
                File.WriteAllText(_path, json);
            }
        }

        public void Load()
        {
            lock (_locker)
            {
                if (File.Exists(_path))
                {
                    string json = File.ReadAllText(_path);
                    Data = JsonConvert.DeserializeObject<T>(json);
                }
            }

            if (Data == null)
            {
                Data = _create();
            }
        }

        private readonly string _path;
        private readonly Func<T> _create;
        private readonly object _locker;
    }
}