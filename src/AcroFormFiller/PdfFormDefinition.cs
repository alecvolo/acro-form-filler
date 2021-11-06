using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace AcroFormFiller
{
    public class PdfFormDefinition
    {
        public const string DatafieldsNodeName = "dataFields";
        public const string FormNodeName = "form";
        public const string PdfFileAttributeName = "pdfFile";
        public const string ObjectTypeNameAttributeName = "objectTypeName";
        public string PdfFileName { get; set; }
        public string ObjectTypeName { get; set; }

        public IEnumerable<PdfDefElement> PdfFielsdDef
        {
            get => _dataFields.Elements();
            set
            {
                _dataFields = new PdfDefFieldsGroup();
                if (value != null)
                {
                    _dataFields.Add(value);
                }
            }
        }

        private PdfDefFieldsGroup _dataFields = new PdfDefFieldsGroup();

        public static PdfFormDefinition LoadFrom(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadFrom(XDocument.Load(stream));
            }
        }

        public static PdfFormDefinition LoadFrom(XDocument xDocument)
        {
            if (xDocument.Root == null || xDocument.Root.Name != FormNodeName)
            {
                throw new InvalidDataException("missed <form> element");
            }

            var result = new PdfFormDefinition
            {
                PdfFileName = (string)xDocument.Root.Attribute(PdfFileAttributeName),
                ObjectTypeName = (string)xDocument.Root.Attribute(ObjectTypeNameAttributeName),
            };


            var dataFieldsElement = xDocument.Root.Element(DatafieldsNodeName);
            if (dataFieldsElement != null)
            {
                result.PdfFielsdDef = PdfDefFieldsGroup.LoadFrom(dataFieldsElement).Elements();
            }

            return result;
        }
        public  XDocument ToXDocument()
        {
            var dataFieldsXElement = new XElement(DatafieldsNodeName);
            foreach (var entry in PdfFielsdDef)
            {
                dataFieldsXElement.Add(entry.ToXElement());
            }

            return new XDocument(new XElement(FormNodeName
                , !string.IsNullOrWhiteSpace(PdfFileName) ? new XAttribute(PdfFileAttributeName, PdfFileName) : null
                , !string.IsNullOrWhiteSpace(ObjectTypeName) ? new XAttribute(ObjectTypeNameAttributeName, ObjectTypeName) : null
                , new XAttribute(XNamespace.Xmlns + "h", @"http://acro.form.filler")
                , dataFieldsXElement));


        }

    }
}