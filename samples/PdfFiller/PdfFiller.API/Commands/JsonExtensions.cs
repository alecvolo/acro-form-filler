using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text.Json;

namespace PdfFiller.API.Commands
{
    public static class JsonExtensions
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
        {
            return (T)ToObject(element, typeof(T), options);
        }
        public static object ToObject(this JsonElement element, Type objectType, JsonSerializerOptions options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
                element.WriteTo(writer);
            return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, objectType, options);
        }
        public static object ToObject(this JsonElement jsonElement, string typeName, JsonSerializerOptions options = null)
        {
            //todo convert into TypeLoader? + jsonConvertors
            //https://stackoverflow.com/questions/37608298/mvc-6-rc2-controllers-in-another-assembly/37647605
            //https://shazwazza.com/post/custom-assembly-loading-with-aspnet-core/
            if (string.IsNullOrWhiteSpace(typeName))
            {
                var result = jsonElement.ToObject<Dictionary<string, object>>(options);
                //todo go deep or mock??
                return result;
            }
            var modelType = Type.GetType(typeName)?? Type.GetType(typeName, assemblyName =>
            {
                var assemblyLoadContext = AssemblyLoadContext.Default;
                var a = assemblyLoadContext.Assemblies.FirstOrDefault(t => t.GetName().Name == assemblyName.Name && (assemblyName.Version == null || assemblyName.Version == t.GetName().Version) );
                if (a == null)
                {
                    a = assemblyLoadContext.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        assemblyName.Name));
                    if (assemblyName.Version != null)
                    {
                        a = assemblyLoadContext.LoadFromAssemblyName(assemblyName);
                    }
                }

                return a;

            }, null, true);
            return jsonElement.ToObject(modelType, options);

        }

        public static T ToObject<T>(this JsonDocument document, JsonSerializerOptions options = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return document.RootElement.ToObject<T>(options);
        }
    }
}