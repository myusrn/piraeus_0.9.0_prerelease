

namespace Orleans.Storage.Redis
{
    using Orleans;
    using Orleans.Providers;
    using Orleans.Runtime;
    using Orleans.Storage;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RedisStorageProvider : IStorageProvider
    {
        public RedisStorageProvider()
        {
           
        }

        private string connectionString;
        private ConnectionMultiplexer connection;
        private IDatabase database;        
        
        public Logger Log
        {
            get;
            protected set;
        }

        public string Name { get; protected set; }
        
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {           
            Log = providerRuntime.GetLogger(this.GetType().FullName + "." + providerRuntime.ServiceId.ToString());

            if (string.IsNullOrWhiteSpace(config.Properties["DataConnectionString"]))
            {
                throw new ArgumentException("Redis DataConnectionString property not set");
            }

            connectionString = config.Properties["DataConnectionString"];
            await ConnectAsync();

            Name = name;
        }

        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if(!connection.IsConnected)
            {
                Task task = ConnectAsync();
                Task.WaitAll(task);
            }

            string key = grainReference.ToKeyString();
            Dictionary<string, object> dict = database.Get<Dictionary<string, object>>(key);

            if (dict == null)
            {
                dict = new Dictionary<string, object>();
            }
            else
            {
                grainState.State = dict;
            }

            return Task.CompletedTask;
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!connection.IsConnected)
            {
                Task task = ConnectAsync();
                Task.WaitAll(task);
            }

            var key = grainReference.ToKeyString();
            Dictionary<string, object> state = grainState.State as Dictionary<string, object>;

            if (state == null)
            {
                state = new Dictionary<string, object>();
            }
            else
            {
                database.Set(key, state);
            }

            database.Set(key, state);           

            return Task.CompletedTask;
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!connection.IsConnected)
            {
                Task task = ConnectAsync();
                Task.WaitAll(task);
            }

            string key = grainReference.ToKeyString();
            database.KeyDelete(key);

            return Task.CompletedTask;
        }

        public Task Close()
        {
            connection.Dispose();
            return Task.CompletedTask;
        }

        private async Task  ConnectAsync()
        {
            if(connection == null || !connection.IsConnected)
            {
                connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
                database = connection.GetDatabase();
            }
        }

        
    }
}
