using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Samurai.Integration.APIClient.Tray.Extension
{
    public static class Extensions
    {
        public static NameValueCollection ToNameValueCollection<T>(this T dynamicObject)
        {
            var nameValueCollection = new NameValueCollection();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynamicObject))
            {
                string value = propertyDescriptor.GetValue(dynamicObject).ToString();
                if (value != null)
                {
                    nameValueCollection.Add(propertyDescriptor.Name, value);
                }
            }
            return nameValueCollection;
        }

        public static IDictionary<string, string> ToDictionary<T>(this T dynamicObject)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynamicObject))
            {
                var value = propertyDescriptor.GetValue(dynamicObject);

                if (propertyDescriptor.GetType() == typeof(DateTime?))
                {
                    if (value != null)
                    {
                        string date = ((DateTime)value).ToString("yyyy-MM-dd");

                        dictionary.Add(propertyDescriptor.Name, date);
                    }
                }
                else if (propertyDescriptor.GetType() == typeof(DateTime))
                {
                    if (value != null && (DateTime)value != DateTime.MinValue)
                    {
                        string date = ((DateTime)value).ToString("yyyy-MM-dd");

                        dictionary.Add(propertyDescriptor.Name, date);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        dictionary.Add(propertyDescriptor.Name, value.ToString());
                    }
                }
            }

            return dictionary;
        }
    }
}
