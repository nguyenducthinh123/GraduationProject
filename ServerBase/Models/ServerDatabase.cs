using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public class ServerDatabase : BsonData.MainDatabase
    {
        public Config Config { get; protected set; }
        public ServerDatabase() : base("MainDB") { }

        public Config LoadConfig(string name)
        {
            var e = new Config();

            if (name == null) name = "Config";
            e.Name = name;

            e.Copy(Config.Load(ConnectionString, name));
            return e;
        }
        public override void Connect(string connectionString)
        {
            base.Connect(connectionString);
            Config = LoadConfig(null);
        }
    }
}
