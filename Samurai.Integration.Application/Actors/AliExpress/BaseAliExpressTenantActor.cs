using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Samurai.Integration.Domain.Messages;
using System;

namespace Samurai.Integration.Application.Actors.AliExpress
{
    public class BaseAliExpressTenantActor : ReceiveActor
    {
        public readonly string _actorName;
        protected readonly ILoggingAdapter _log;
        protected TenantDataMessage _tenantData;

        public BaseAliExpressTenantActor(string actorName)
        {
            _actorName = actorName;
            _log = Context.GetLogger<SerilogLoggingAdapter>();
        }

        public void LogDebug(string format, params object[] args)
        {
            GetLog().Debug(GetMessage(format), args);
        }

        public void LogInfo(string format, params object[] args)
        {
            GetLog().Info(GetMessage(format), args);
        }

        public void LogWarning(string format, params object[] args)
        {
            GetLog().Warning(GetMessage(format), args);
        }

        public void LogError(string format, params object[] args)
        {
            GetLog().Error(GetMessage(format), args);
        }

        public void LogError(Exception cause, string format, params object[] args)
        {
            GetLog().Error(cause, GetMessage(format), args);
        }

        private (string StoreHandle, ILoggingAdapter Log)? _tenantLogger = null;
        protected ILoggingAdapter GetLog()
        {
            if (_tenantLogger == null || _tenantLogger.Value.StoreHandle != _tenantData.StoreHandle)
                _tenantLogger = (_tenantData.StoreHandle, _log.ForContext("StoreHandle", _tenantData.StoreHandle));
            return _tenantLogger.Value.Log;
        }

        private string GetMessage(string message)
        {
            return $"{_actorName} - {message}";
        }
    }
}
