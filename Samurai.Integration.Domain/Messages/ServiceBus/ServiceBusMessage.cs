using Microsoft.Azure.ServiceBus;
using Samurai.Integration.Domain.Messages.Helpers;
using Samurai.Integration.Domain.Messages.ServiceBus;
using Samurai.Integration.Domain.Messages.Shopify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Samurai.Integration.Domain.Messages
{
    public class ServiceBusMessage
    {
        private EncodedMessage _message;
        private Type _type;

        public ServiceBusMessage(object value)
        {
            _type = value.GetType();
            _message = new EncodedMessage
            {
                TypeName = _type.Name,
                AssemblyQualifiedName = _type.AssemblyQualifiedName,
                MessageValue = JsonSerializer.Serialize(value)
            };
        }

        public ServiceBusMessage(byte[] json, bool compressed = false)
        {
            if (compressed)
                _message = JsonSerializer.Deserialize<EncodedMessage>(EncryptedMessage.Decompress(Encoding.UTF8.GetString(json)));
            else
                _message = JsonSerializer.Deserialize<EncodedMessage>(Encoding.UTF8.GetString(json));
        }

        public Message GetMessage(object id, int timeToAdd = 0)
        {
            var timeTo = 120000;

            Action<int> getSchadule = (valor) =>
            {
                timeTo = valor > 0 ? valor : 0;
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_message)));

            if (_type == typeof(ShopifyUpdateFullProductMessage))
            {
                message.MessageId = GetMessageId(id, false);
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(timeTo);
            }
            else if (_type == typeof(ShopifyUpdateProductImagesMessage))
            {
                message.MessageId = GetMessageId(id, false);
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(timeTo);
            }
            else if (_type == typeof(ShopifyUpdateProductGroupingMessage))
            {
                message.MessageId = GetMessageId(id, true);
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(timeTo);
            }
            else if (_type == typeof(ShopifyListOrderMessage))
            {
                getSchadule(timeToAdd);
                message.MessageId = GetMessageId(id, true);
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(timeTo);
            }
            else
            {
                getSchadule(timeToAdd);
                message.MessageId = GetMessageId(id, true);
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(timeTo);
            }
            return message;
        }

        private string GetMessageId(object id, bool hashContent)
        {
            if (hashContent)
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(_message.MessageValue));

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return $"{_message.TypeName}-{id}-{builder.ToString()}";
                }
            }
            else
            {
                return $"{_message.TypeName}-{id}";
            }
        }

        public object GetValue()
        {
            if (IntegrationSettings.Instancia != null)
            {
                var assemblyName = _message?.AssemblyQualifiedName?.Split(",").FirstOrDefault();
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    var messageDefinition = IntegrationSettings.Instancia.Messages.FirstOrDefault(x => x.OriginMessageTypeFullName == assemblyName);

                    if (messageDefinition != null)
                        _type = Type.GetType(messageDefinition.DestinyMessageTypeFullName);
                }
            }

            if (_type == null)
                _type = Type.GetType(_message.AssemblyQualifiedName);

            return JsonSerializer.Deserialize(_message.MessageValue, _type);
        }

        public T GetValue<T>()
        {
            return JsonSerializer.Deserialize<T>(_message.MessageValue);
        }

        private class EncodedMessage
        {
            public string TypeName { get; set; }
            public string AssemblyQualifiedName { get; set; }
            public string MessageValue { get; set; }
        }
    }
}
