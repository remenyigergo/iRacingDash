using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConsoleApp1
{
    class ValueConverter : JsonConverter
    {
        private Type _internalType;

        public ValueConverter(Type typeToDeserialize)
        {
            _internalType = typeToDeserialize;
        }

        static Type GetValueType(Type objectType)
        {
            var a = objectType                
                .BaseTypesAndSelf();

            var b = a
                .Where(t => t.IsGenericType && ( t.GetGenericTypeDefinition() == typeof(ITelemetryValue<>) || t.GetInterfaces().Any(x => x.GetGenericTypeDefinition() == typeof(ITelemetryValue<>))))
                .Select(t => t.GetGenericArguments()[0])
                .FirstOrDefault();

            return b;
        }

        public override bool CanConvert(Type objectType)
        {
            //return objectType == typeof(ITelemetryValue<>);
            //return true;
            var i= GetValueType(objectType) != null;
            Console.WriteLine(i);
            return i;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // You need to decide whether a null JSON token results in a null ValueObject<T> or 
            // an allocated ValueObject<T> with a null Value.
            if (reader.SkipComments().TokenType == JsonToken.Null)
                return null;
            var valueType = GetValueType(objectType);
            var value = serializer.Deserialize(reader, valueType);


            // Here we assume that every subclass of ValueObject<T> has a constructor with a single argument, of type T.
            //return Activator.CreateInstance(objectType, value);
            return Activator.CreateInstance(_internalType.MakeGenericType(valueType), value);
        }

        const string ValuePropertyName = nameof(ITelemetryValue<object>.Value);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());
            var valueProperty = contract.Properties.Where(p => p.UnderlyingName == ValuePropertyName).Single();
            // You can simplify this to .Single() if ValueObject<T> has no other properties:
            // var valueProperty = contract.Properties.Single();
            serializer.Serialize(writer, valueProperty.ValueProvider.GetValue(value));
        }
    }

    public static partial class JsonExtensions
    {
        public static JsonReader SkipComments(this JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment && reader.Read())
                ;
            return reader;
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<Type> BaseTypesAndSelf(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}