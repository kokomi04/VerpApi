using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings;

namespace Verp.Cache.RedisCache
{
    public static class RedisCacheFactory
    {
        static readonly object _objLock = new object();
        static ConnectionMultiplexer _connection = null;
        static ConfigurationOptions _configOptions = null;

      
        public static ConfigurationOptions GetConfigurationOptions()
        {
            lock (_objLock)
            {
                if (_configOptions == null)
                {
                    var appConfig = AppConfigSetting.Config().AppSetting;

                    if (appConfig.Redis == null)
                        return null;

                    var options = new ConfigurationOptions();

                    options.EndPoints.Add(appConfig.Redis.Endpoint, appConfig.Redis.Port ?? 6379);

                    if (!string.IsNullOrEmpty(appConfig.Redis.AuthKey))
                    {
                        options.Password = appConfig.Redis.AuthKey;
                    }
                    options.Ssl = appConfig.Redis.Ssl;

                    options.AbortOnConnectFail = true;
                    options.ConnectTimeout = 10 * 1000;
                    options.KeepAlive = 180;

                    _configOptions = options;
                }

                return _configOptions;
            }
        }

      

        public static void InitRedisCacheFactory()
        {
            var _ = GetConfigurationOptions();
        }

        public static ConnectionMultiplexer GetConnectionMultiplexer()
        {
            lock (_objLock)
            {
                if (_connection == null || !_connection.IsConnected)
                {
                    if (_connection != null)
                    {
                        _connection.Close(false);
                        _connection.Dispose();
                    }
                    _connection = _GetNewConnection();
                }
                return _connection;
            }
        }

        private static ConnectionMultiplexer _GetNewConnection()
        {
            var options = GetConfigurationOptions();
            if (options == null) return null;
            return ConnectionMultiplexer.Connect(options);
        }

        public static IDatabase GetDataBase(int db)
        {
            var _connection =  GetConnectionMultiplexer();
            if (_connection == null) return null;
            return _connection.GetDatabase(db);
        }

    }

  
}
