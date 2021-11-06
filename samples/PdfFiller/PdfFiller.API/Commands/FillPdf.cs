using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AcroFormFiller;
using FluentValidation;
using iTextSharp.text.pdf;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PdfFiller.API.Infrastructure;

namespace PdfFiller.API.Commands
{
    public static class FillPdf
    {
        public class Command: IRequest<byte[]>
        {
            [FromRoute]
            public string FormId { get; set; }
            [FromBody]
            public JsonElement JsonData { get; set; }
            [FromQuery]
            public string ResultFileName { get; set; }
        }
        public class NotFoundException : ApplicationException
        {
            public NotFoundException(): base()
            {
                
            }
            public NotFoundException(string errorMessage) : base(errorMessage)
            {

            }
        }
        public class Validator: AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(t => t.JsonData).NotNull().Must((context, jsonData, propertyValidatorContext) =>
                {
                    //todo check that model is correct type??
                    return jsonData.ValueKind == JsonValueKind.Object;
                }).WithMessage("should be an object");
            }
            
        }

        public class Handler : IRequestHandler<Command, byte[]>
        {
            private readonly IPdfFormDefinitionStore _store;

            public Handler(IPdfFormDefinitionStore store)
            {
                _store = store;
            }

            public async Task<byte[]> Handle(Command request, CancellationToken cancellationToken)
            {
                var formDef = _store.GetFormDefinitionById(request.FormId) ??
                              throw new NotFoundException($"Form is not found");
                var model = request.JsonData.ToObject(formDef.ObjectTypeName, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                });
                var evaluator = new AcroFieldsEvaluator();
                var pdfFormFieldValues = evaluator.CalcPdfFields(formDef.PdfFielsdDef, model,
                    new {DateTime = (Func<int, int, int, DateTime>) ((y, m, d) => new DateTime(y, m, d))});
                var pdfReader = new PdfReader(await _store.GetPdfByIdAsync(formDef.PdfFileName));
                pdfReader.RemoveUsageRights();
                await using var result = new MemoryStream();
                var pdfStamper = new PdfStamper(pdfReader, result);
                var pdfFormFields = pdfStamper.AcroFields;

                foreach (var formField in pdfFormFieldValues)
                {
                    pdfFormFields.SetField(formField.Key, formField.Value);
                }

                pdfStamper.FormFlattening = true;
                pdfStamper.Close();

                return result.ToArray();
            }
        }
    }
}
