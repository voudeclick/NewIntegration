using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VDC.Integration.Domain.Messages.ServiceBus
{
    public class IntegrationSettings
    {
        private static IntegrationSettings _instancia;
        public static IntegrationSettings Instancia
        {
            get
            {
                return _instancia ?? (_instancia = CarregarConfiguracoes());
            }
        }

        private static IntegrationSettings CarregarConfiguracoes()
        {
            try
            {
                string filePath = GetFilePath();

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var jsonSerializer = new JsonSerializer();

                    return jsonSerializer.Deserialize<IntegrationSettings>(jsonReader);
                }
            }
            catch (Exception)
            {
                return default;
            }
        }

        private static string GetFilePath()
        {
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

            var filePath = Path.Combine(appPath, "integrationsettings.json");

            return filePath;
        }

        public IEnumerable<IntegrationSettingsMessage> Messages { get; set; }
    }
}
