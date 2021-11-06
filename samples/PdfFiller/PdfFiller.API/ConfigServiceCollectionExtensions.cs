using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfFiller.API.Infrastructure;

namespace PdfFiller.API
{
    public static class ConfigServiceCollectionExtensions
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            //services.Configure<PositionOptions>(
            //    config.GetSection(PositionOptions.Position));
            //services.Configure<ColorOptions>(
            //    config.GetSection(ColorOptions.Color));

            return services;
        }
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IPdfFormDefinitionStore>(new FilePdfFormDefinitionStore(CheckExistsFolder(config.GetRequiredKeyValue("pdfFormFolder"))));
            //services.AddScoped<IValidator<Commands.FillPdf.Command>, Commands.FillPdf.Validator>();

            return services;
        }
        public static string GetFullPath(string path)
        {

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }

        public static string GetRequiredKeyValue(this IConfiguration config, string key)
        {
            var value = config[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ApplicationException($"\"{key}\" parameter is not specified");
            }
            return value;
        }
        public static string ExistedFileName(string fileName)
        {
            fileName = GetFullPath(fileName);
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"\"{fileName}\" doesn't exist");
            }
            return fileName;
        }
        public static string CheckExistsFolder(string folderName)
        {
            folderName = GetFullPath(folderName);
            if (!Directory.Exists(folderName))
            {
                throw new DirectoryNotFoundException($"\"{folderName}\" doesn't exist");
            }

            return folderName;
        }

    }
}
