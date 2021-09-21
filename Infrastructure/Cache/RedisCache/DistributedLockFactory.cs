using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.AppSettings;

namespace Verp.Cache.RedisCache
{
    public static class DistributedLockFactory
    {
        static readonly TimeSpan DefaultExpiryTime = TimeSpan.FromSeconds(120);
        static readonly TimeSpan DefaultWaitTime = TimeSpan.FromSeconds(60);
        static readonly TimeSpan DefaultRetryTime = TimeSpan.FromMilliseconds(300);

        private static List<RedLockEndPoint> _redlockEndpoint = null;
        public static List<RedLockEndPoint> GetRedLockEndpoints()
        {
            if (_redlockEndpoint == null)
            {
                var appConfig = AppConfigSetting.Config().AppSetting;

                if (appConfig.Redis == null) return null;

                var endpoint = new RedLockEndPoint();

                endpoint.EndPoints.Add(new DnsEndPoint(appConfig.Redis.Endpoint, appConfig.Redis.Port ?? 6379));

                if (!string.IsNullOrEmpty(appConfig.Redis.AuthKey))
                {
                    endpoint.Password = appConfig.Redis.AuthKey;
                }
                endpoint.Ssl = appConfig.Redis.Ssl;

                //endpoint.AbortOnConnectFail = true;
                //endpoint.ConnectTimeout = 10 * 1000;
                //endpoint.KeepAlive = 180;
                _redlockEndpoint = new List<RedLockEndPoint> { endpoint };
            }
            return _redlockEndpoint;
        }

        private class RedLockFactoryInstance
        {
            public static readonly RedLockFactory RedisLockFactory = RedLockFactory.Create(GetRedLockEndpoints());
        }

        public static async Task<IRedLock> GetLockAsync(string resource,
           TimeSpan? expiryTime = null, TimeSpan? waitTime = null, TimeSpan? retryTime = null)
        {
            try
            {
                waitTime = waitTime ?? DefaultWaitTime;
                expiryTime = expiryTime ?? DefaultExpiryTime;
                retryTime = retryTime ?? DefaultRetryTime;

                var redLockEndpoints = GetRedLockEndpoints();
                if (redLockEndpoints == null)
                {
                    return await MemLockLock.CreateLockAsync(resource, expiryTime.Value, waitTime.Value, retryTime.Value);
                }

                var @lock = await RedLockFactoryInstance.RedisLockFactory.CreateLockAsync(resource, expiryTime.Value, waitTime.Value, retryTime.Value);
                if (!@lock.IsAcquired)
                {
                    throw new DistributedLockExeption(resource);
                }
                return @lock;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        public static string GetLockStockKeyKey(long stockTakePeriodId)
        {
            return $"STOCK_TAKE_PERIOD_LOCK_{stockTakePeriodId}";
        }

        public static string GetLockReportKey(int reportTypeId)
        {
            return $"REPORT_LOCK_{reportTypeId}";
        }


        public static string GetLockCategoryKey(int categoryId)
        {
            return $"CATEGORY_LOCK_{categoryId}";
        }

        public static string GetLockProductionOrderKey(long productOrderId)
        {
            return $"PRODUCTION_ORDER_LOCK_{productOrderId}";
        }
        public static string GetLockCuttingWorkSheet(long propertyCalcId)
        {
            return $"CUTTING_WORK_SHEET_LOCK_{propertyCalcId}";
        }

        public static string GetLockInputTypeKey(int inputTypeId)
        {
            return $"INPUTTYPE_LOCK_{inputTypeId}";
        }

        public static string GetLockVoucherTypeKey(int voucherTypeId)
        {
            return $"VOUCHERTYPE_LOCK_{voucherTypeId}";
        }

        public static string GetLockHrTypeKey(int hrTypeId)
        {
            return $"HRTYPE_LOCK_{hrTypeId}";
        }

        public static string GetLockStockResourceKey(int stockId)
        {
            return $"STOCK_LOCK_{stockId}";
        }

        public static string GetLockGenerateCodeKey(int customGenCodeId)
        {
            return $"GENERATECODE_LOCK_{customGenCodeId}";
        }

        public static string GetLockGenerateCodeCustomKey(int customGenCodeId)
        {
            return $"GENERATECODE_CUSTOM_LOCK_{customGenCodeId}";
        }
        public static string GetLockPoRequest()
        {
            return $"PO_REQUEST_LOCK";
        }

        public static string GetLockPoSuggest()
        {
            return $"PO_SUGGUEST_LOCK";
        }
    }

    public class MemLockLock : IRedLock
    {
        private static readonly IDictionary<string, DateTime> _resources = new Dictionary<string, DateTime>();
        private static readonly object _objLock = new object();
        private string _resource;
        private Guid _lockId;
        public MemLockLock(string resource)
        {
            _resources.Add(resource, DateTime.UtcNow);
            _resource = resource;
            _lockId = Guid.NewGuid();
        }

        public static async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime)
        {
            for (var i = 0; i < waitTime.TotalMilliseconds; i += (int)retryTime.TotalMilliseconds)
            {
                lock (_objLock)
                {
                    if (!_resources.ContainsKey(resource))
                    {
                        return new MemLockLock(resource);
                    }
                    else
                    {
                        if (DateTime.UtcNow.Subtract(_resources[resource]) > expiryTime)
                        {
                            _resources.Remove(resource);
                            return new MemLockLock(resource);
                        }
                    }
                }

                await Task.Delay((int)retryTime.TotalMilliseconds);
            }

            throw new DistributedLockExeption(resource);
        }

        public string Resource => _resource;

        public string LockId => _lockId.ToString();

        public bool IsAcquired => true;

        public RedLockStatus Status => RedLockStatus.Acquired;

        public RedLockInstanceSummary InstanceSummary => new RedLockInstanceSummary(1, 0, 0);

        public int ExtendCount => _resources.Count;

        public void Dispose()
        {
            lock (_objLock)
            {
                _resources.Remove(_resource);
            }
        }
    }
}
