using ByteSizeLib;
using System;

namespace VDC.Integration.Domain
{
    public class AppDomainMonitor
    {
        public AppDomainMonitor(AppDomain? targetAppDomain = null)
        {
            AppDomain.MonitoringIsEnabled = true;
            TargetAppDomain = targetAppDomain ?? AppDomain.CurrentDomain;
            Reset();
        }


        public AppDomain TargetAppDomain { get; private set; }
        private TimeSpan InitialProcessorTimeField;
        private long InitialAllocatedMemorySizeField;
        private long InitialSurvivedMemorySize;

        public void Reset()
        {
            InitialProcessorTimeField = TargetAppDomain.MonitoringTotalProcessorTime;
            InitialAllocatedMemorySizeField = TargetAppDomain.MonitoringTotalAllocatedMemorySize;
            InitialSurvivedMemorySize = TargetAppDomain.MonitoringSurvivedMemorySize;
        }

        public AppDomainMonitorSnapshot TakeSnapshot()
        {
            return new AppDomainMonitorSnapshot(this);
        }

        public struct AppDomainMonitorSnapshot
        {
            public AppDomainMonitorSnapshot(AppDomainMonitor appDomainMonitor) : this()
            {
                if (appDomainMonitor == null)
                    throw new ArgumentNullException(nameof(appDomainMonitor));

                GC.Collect();
                AppDomainFriendlyName = appDomainMonitor.TargetAppDomain.FriendlyName;
                ProcessorTimeMs = (appDomainMonitor.TargetAppDomain.MonitoringTotalProcessorTime - appDomainMonitor.InitialProcessorTimeField).TotalMilliseconds;
                AllocatedMemorySize = appDomainMonitor.TargetAppDomain.MonitoringTotalAllocatedMemorySize - appDomainMonitor.InitialAllocatedMemorySizeField;
                SurvivedMemorySize = appDomainMonitor.TargetAppDomain.MonitoringSurvivedMemorySize - appDomainMonitor.InitialSurvivedMemorySize;
            }

            public string AppDomainFriendlyName { get; private set; }
            public double ProcessorTimeMs { get; private set; }

            //Tamanho total em bytes de uso de memoria
            public long AllocatedMemorySize { get; private set; }
            public long SurvivedMemorySize { get; private set; }

            public override string ToString()
            {
                var memory = ByteSize.FromBytes(AllocatedMemorySize);
                var memorySurvived = ByteSize.FromBytes(SurvivedMemorySize);

                return string.Format(
                    "Webjob: {0}, Uso de CPU:{1}, \nTamanho da Memória Alocada em MB: {2:N0}, Tamanho da memória sobrevivente = {3:N0}",
                    AppDomainFriendlyName,
                    TimeSpan.FromMinutes(ProcessorTimeMs),
                    memory.MegaBytes,
                    memorySurvived.MegaBytes
                    );
            }
        }
    }

    public class AppDomainMonitorDelta : IDisposable
    {
        readonly AppDomainMonitor _monitorField;

        public AppDomainMonitorDelta(AppDomain? targetAppDomain = null)
        {
            _monitorField = new AppDomainMonitor(targetAppDomain);
        }

        public void Dispose()
        {
            Console.WriteLine(_monitorField.TakeSnapshot());
            _monitorField.Reset();
            GC.SuppressFinalize(this);
        }
    }
}
