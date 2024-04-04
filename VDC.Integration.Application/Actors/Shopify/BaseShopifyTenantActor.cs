using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using System;
using VDC.Integration.Domain.Messages.Shopify;

namespace VDC.Integration.Application.Actors.Shopify
{
    public class BaseShopifyTenantActor : ReceiveActor
    {
        public readonly string _actorName;
        protected readonly ILoggingAdapter _log;
        protected ShopifyDataMessage _shopifyData;

        public BaseShopifyTenantActor(string actorName)
        {
            _actorName = actorName;
            _log = Context.GetLogger<SerilogLoggingAdapter>();
        }

        public void LogDebug(String format, params Object[] args)
        {
            GetLog().Debug(GetMessage(format), args);
        }

        public void LogInfo(String format, params Object[] args)
        {
            GetLog().Info(GetMessage(format), args);
        }

        public void LogWarning(String format, params Object[] args)
        {
            GetLog().Warning(GetMessage(format), args);
        }

        public void LogError(String format, params Object[] args)
        {
            GetLog().Error(GetMessage(format), args);
        }

        public void LogError(Exception cause, String format, params Object[] args)
        {
            GetLog().Error(cause, GetMessage(format), args);
        }

        private (string StoreHandle, ILoggingAdapter Log)? _tenantLogger = null;
        protected ILoggingAdapter GetLog()
        {
            if (_tenantLogger == null || _tenantLogger.Value.StoreHandle != _shopifyData.StoreHandle)
                _tenantLogger = (_shopifyData.StoreHandle, _log.ForContext("StoreHandle", _shopifyData.StoreHandle));
            return _tenantLogger.Value.Log;
        }

        private string GetMessage(string message)
        {
            return $"{_actorName}-{message}";
        }
    }
}
