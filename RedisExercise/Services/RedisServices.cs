using StackExchange.Redis;

namespace RedisExercise.Services
{
    public static class RedisServices
    {
        static ConfigurationOptions sentinelOptions => new()
        {
            EndPoints =
            {
                {"localhost", 6383 },
                {"localhost", 6384 },
                {"localhost", 6385 }
            },
            CommandMap = CommandMap.Sentinel,
            AbortOnConnectFail = false
        };


        static ConfigurationOptions masterOptions => new()
        {
            AbortOnConnectFail = false
        };

        public async static Task<IDatabase> RedisMasterDatabase()
        {
            ConnectionMultiplexer sentinelConnection = await ConnectionMultiplexer.SentinelConnectAsync(sentinelOptions);
            System.Net.EndPoint masterEndpoint = null;

            foreach (System.Net.EndPoint endpoint in sentinelConnection.GetEndPoints())
            {
                IServer server = sentinelConnection.GetServer(endpoint);

                if (!server.IsConnected)
                    continue;

                masterEndpoint = await server.SentinelGetMasterAddressByNameAsync("mymaster");
                break;
            }

            string localMasterIP = masterEndpoint.ToString() switch
            {
                "172.18.0.2:6379" => "localhost:6379",
                "172.18.0.3:6379" => "localhost:6380",
                "172.18.0.4:6379" => "localhost:6381",
                "172.18.0.5:6379" => "localhost:6382"
            };

            ConnectionMultiplexer masterConnection = await ConnectionMultiplexer.ConnectAsync(localMasterIP);

            IDatabase database = masterConnection.GetDatabase();
            return database;
        }
    }
}
