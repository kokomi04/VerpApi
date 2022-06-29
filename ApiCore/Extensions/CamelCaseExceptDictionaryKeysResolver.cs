﻿using Newtonsoft.Json.Serialization;
using System;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ApiCore.Extensions
{
    class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);
            if (objectType == typeof(NonCamelCaseDictionary) || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(NonCamelCaseDictionary<>)))
            {
                contract.DictionaryKeyResolver = propertyName => propertyName;
            }

            return contract;
        }
    }
}
