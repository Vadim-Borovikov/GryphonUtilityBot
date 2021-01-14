using System.IO;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models.Save
{
    internal sealed class Manager<T> where T : class, new()
    {
        public T Data { get; private set; }

        public Manager(string path)
        {
            _path = path;
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
                Data = new T();
            }
        }

        private readonly string _path;
        private readonly object _locker;
    }
}