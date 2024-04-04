using System.Collections.Generic;
using System.Linq;
using VDC.Integration.Domain.ValueObjects;

namespace VDC.Integration.Domain.Entities.Database
{
    public class Param
    {
        public string Key { get; private set; }
        public List<ParamValue> Values { get; private set; }

        public Param(string key)
        {
            Key = key;
        }

        public Param Add(string key, object value)
        {
            CreateValuesIfNull();

            Values.Add(new ParamValue()
            {
                Key = key,
                Value = value
            });

            return this;
        }

        public Param Update(string key, object value)
        {
            CreateValuesIfNull();

            var paramValue = Values.FirstOrDefault(x => x.Key == key);

            if (paramValue == null)
            {
                Add(key, value);
            }
            else
            {
                paramValue.Value = value;
            }

            return this;
        }

        public ParamValue GetValueBykey(string key)
        {
            CreateValuesIfNull();

            return Values.FirstOrDefault(x => x.Key == key);
        }

        public bool ExistsByKeyValue(string keyValue)
        {
            CreateValuesIfNull();

            return Values.Any(x => x.Key == keyValue);
        }

        public void RemoveByKeyValue(string keyValue)
        {
            CreateValuesIfNull();

            var valueToRemove = Values.FirstOrDefault(x => x.Key == keyValue);

            if (valueToRemove == null)
            {
                return;
            }

            Values.Remove(valueToRemove);
        }

        private void CreateValuesIfNull()
        {
            if (Values == null)
                Values = new List<ParamValue>();
        }

    }
}
