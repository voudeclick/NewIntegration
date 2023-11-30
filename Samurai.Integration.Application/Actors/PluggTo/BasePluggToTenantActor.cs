using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Samurai.Integration.Domain.Messages.PluggTo;
using System;

namespace Samurai.Integration.Application.Actors.PluggTo
{
    public class BasePluggToTenantActor : ReceiveActor
    {
        public readonly string _actorName;
        protected readonly ILoggingAdapter _log;        
        protected PluggToData _pluggToData;

        public BasePluggToTenantActor(string actorName)
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
            if (_tenantLogger == null || _tenantLogger.Value.StoreHandle != _pluggToData.StoreHandle)
                _tenantLogger = (_pluggToData.StoreHandle, _log.ForContext("StoreHandle", _pluggToData.StoreHandle));
            return _tenantLogger.Value.Log;
        }

        private string GetMessage(string message)
        {
            return $"{_actorName} - {message}";
        }
    }
}
