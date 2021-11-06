using System;
using System.IO;
using System.Threading.Tasks;
using AcroFormFiller;

namespace PdfFiller.API.Infrastructure
{
    public class FilePdfFormDefinitionStore : IPdfFormDefinitionStore
    {
        private readonly string _folder;
        public FilePdfFormDefinitionStore(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException(folder);
            }
            _folder = folder;
        }
        public PdfFormDefinition GetFormDefinitionById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var fileInfo = new FileInfo(Path.Combine(_folder, id + ".xml"));
            return fileInfo.Exists ? PdfFormDefinition.LoadFrom(fileInfo.FullName) : null;
        }

        public bool FormDefinitionExists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return File.Exists(Path.Combine(_folder, id + ".xml"));
        }

        public Task<byte[]> GetPdfByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return File.ReadAllBytesAsync(Path.Combine(_folder, id + (string.IsNullOrWhiteSpace(Path.GetExtension(id)) ? ".pdf" : "")));
        }

    }
}
