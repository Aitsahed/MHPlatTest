
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace MHPlatTest.Utility
{


    // Source: https://khalidabuhakmeh.com/serialize-interface-instances-system-text-json
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class JsonInterfaceConverterAttribute : JsonConverterAttribute
    {
        public JsonInterfaceConverterAttribute(Type converterType) : base(converterType)
        {
        }
    }


    public class InterfaceConverter<T> : JsonConverter<T>
        where T : class
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;
            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = readerClone.GetString();
            if (propertyName != "$type")
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            string typeValue = readerClone.GetString();
            var instance = Activator.CreateInstance(Assembly.GetExecutingAssembly().FullName, typeValue).Unwrap();
            var entityType = instance.GetType();

            if (entityType.Name == "MHPlatTest.Models.OptimizationResultModel")
            {
                int ii = 1;
            }

            var deserialized = JsonSerializer.Deserialize(ref reader, entityType, options);
            return (T)deserialized;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (T)null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
                        writer.WriteStartObject();
                        writer.WriteString("$type", type.FullName);

                        foreach (var element in jsonDocument.RootElement.EnumerateObject())
                        {
                            element.WriteTo(writer);
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }



    public class CustomConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                //throw new JsonException();
                return default;
            }

            string typeName = null;
            T value = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "type")
                {
                    reader.Read();

                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException();
                    }

                    typeName = reader.GetString();
                }
                else
                {
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
                }
            }

            if (typeName == null)
            {
                throw new JsonException();
            }

            switch (typeName)
            {
                case "type1":
                    return value;//JsonSerializer.Deserialize<object>(ref reader, options);
                case "type2":
                    return value ; // JsonSerializer.Deserialize<Int32>(ref reader, options);
                default:
                    throw new NotSupportedException($"Unknown type {typeName}");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }



}
