using System.Threading.Tasks;
using AcroFormFiller;


namespace PdfFiller.API.Infrastructure
{
    public interface IPdfFormDefinitionStore
    {
        PdfFormDefinition GetFormDefinitionById(string id);
        bool FormDefinitionExists(string id);
        Task<byte[]> GetPdfByIdAsync(string id);

    }
}