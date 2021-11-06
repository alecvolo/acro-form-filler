using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AcroFormFiller;
using iTextSharp.text.pdf;

namespace FieldDefExtractor
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {

            var rootCommand = new RootCommand(
                description: "Generate form definition for a pdf file."){TreatUnmatchedTokensAsErrors = true};
            Option inputOption = new Option<string>(
                 new [] { "--input", "-i" }
                , "The path to the pdf file.")
            {
                IsRequired = true,
            };
            inputOption.AddValidator(o =>
            {
                var fileName = o.Tokens.FirstOrDefault()?.Value;
                if (!File.Exists(fileName))
                {
                    return $"File name doesn't exist: {fileName}";
                }

                return null;

            });
            rootCommand.AddOption(inputOption);
            Option useFieldNameAsFieldValueOption = new Option<bool>(
                aliases: new string[] { "--highlight", "-h" }
                , description: "Set field's value to its name.");
            rootCommand.AddOption(useFieldNameAsFieldValueOption);
            rootCommand.Handler =
                CommandHandler.Create<string, bool>((input, highlight)=> CreateFormDef(input, highlight));
            return await rootCommand.InvokeAsync(args);
        }

        public static void CreateFormDef(string pdfFileName, bool useFieldNameAsFieldValue = false)
        {
            var pdfFields = GetPdfFieldsTree(pdfFileName, useFieldNameAsFieldValue);
            var xDoc = new PdfFormDefinition() {PdfFileName = pdfFileName, PdfFielsdDef = pdfFields.Elements()}
                .ToXDocument();

            using (var writer = new XmlTextWriter(Path.ChangeExtension(pdfFileName, ".xml"), System.Text.Encoding.UTF8)
            {
                QuoteChar = '\'',
                Formatting = Formatting.Indented
            })
            {
                xDoc.WriteTo(writer);
            }
        }

        public static PdfDefFieldsGroup GetPdfFieldsTree(string pdfFileName, bool setFieldNameAsValue = false)
        {
            var memStream = new MemoryStream();
            var pdfReader = new PdfReader(pdfFileName);
            var pdfStamper = new PdfStamper(pdfReader, memStream);
            var rootScope = new PdfDefFieldsGroup();
            var xfa = pdfReader.AcroForm;
            //foreach (var paths in pdfReader.AcroFields.Fields.Keys.OfType<string>().Select(t => t.Split('.')))
            //todo sort by numbers
            foreach (var path in pdfReader.AcroForm.Fields.Cast<PrAcroForm.FieldInformation>()
                .Select(t => new string(t.Name.Where(c => !char.IsControl(c) && c < 128).ToArray())))
//            foreach (var path in pdfReader.AcroForm.Fields.Cast<string>())
            {
                var parentScope = rootScope;
                var paths = path.Split('.');
                for (var i = 0; i < paths.Length - 1; i++)
                {
                    var lastName = ExtractFieldName(paths[i]);
                    var scope = parentScope.Elements().LastOrDefault(t => t.Name == lastName) as PdfDefFieldsGroup;
                    if (scope == null)
                    {
                        scope = new PdfDefFieldsGroup(lastName);
                        parentScope.Add(scope);
                    }

                    parentScope = scope;
                }

                var field = new PdfDefField(ExtractFieldName(paths[paths.Length - 1]));
                var checkBox = pdfStamper.AcroFields.GetAppearanceStates(path);
                if (checkBox.Length > 0)
                {
                    field.OnValue = checkBox[0];
                }

                var fieldValue = pdfStamper.AcroFields.GetField(path).TrimToNull();
                field.ValueExpression = fieldValue != null ? $"\"{fieldValue}\"" :
                    field.OnValue == null && setFieldNameAsValue ? $"\"{field.Name}\"" : null;
                parentScope.Add(field);
            }

            return rootScope;
        }
        public static string ExtractFieldName(string value)
        {
            value = value.TrimToNull();
            if (value == null)
            {
                return null;
            }

            var paths = value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length == 0)
            {
                return null;
            }

            return new string(paths[paths.Length - 1].TakeWhile(t => t != '[').ToArray()).TrimToNull();
        }

    }
}
