using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AcroFormFiller
{
    public class PdfDefFieldsGroup: PdfDefElement
    {
        public const string VarNameAttributeName = "var-name";
        public const string VarExprAttributeName = "var-expr";
        public const string ScopeNodeName = "{http://acro.form.filler}scope";
        private TrimedString _variableName;
        private TrimedString _variableExpression;

        public string VariableName
        {
            get => _variableName;
            set => _variableName = value;
        }

        public string VariableExpression
        {
            get => _variableExpression;
            set => _variableExpression = value;
        }


        public PdfDefFieldsGroup(string name = null):base(false)
        {
            _name = ValidatedName(name, true);
        }
        public PdfDefFieldsGroup Add(PdfDefElement element)
        {
            AddElement(element??throw new ArgumentNullException(nameof(element)));
            return this;
        }
        public PdfDefFieldsGroup Add(IEnumerable<PdfDefElement> elements)
        {
            foreach (var element in (elements ?? throw new ArgumentNullException(nameof(elements))).Where(t=>t!=null))
            {
                Add(element);
            }
            return this;
        }

        public PdfDefFieldsGroup Add(params PdfDefElement[] elements) => Add(elements.AsEnumerable());

        public IEnumerable<PdfDefElement> Elements()
        {
            return GetElements().Cast<PdfDefElement>();
        }

        public override XElement ToXElement()
        {
            var xElement = new XElement(Name ?? ScopeNodeName
                , Condition != null ? new XAttribute(ConditionAttributeName, Condition) : null
                , VariableName != null ? new XAttribute(VarNameAttributeName, VariableName) : null
                , VariableExpression != null ? new XAttribute(VarExprAttributeName, VariableExpression) : null
            );

            foreach (var entry in Elements())
            {
                xElement.Add(entry.ToXElement());
            }

            return xElement;
        }
        public static PdfDefFieldsGroup FromXElementSelfOnly(XElement xElement)
        {
            if (!xElement.HasElements)
            {
                throw new ArgumentOutOfRangeException(nameof(xElement));
            }

            return new PdfDefFieldsGroup(xElement.Name != ScopeNodeName ? xElement.Name.ToString():null)
            {
                Condition = (string)xElement.Attribute(ConditionAttributeName),
                VariableName = (string)xElement.Attribute(VarNameAttributeName),
                VariableExpression = (string)xElement.Attribute(VarExprAttributeName)
            };

        }

        public static PdfDefFieldsGroup LoadFrom(XElement xRootElement)
        {
            void Func(XElement parentNode, PdfDefFieldsGroup parent)
            {
                foreach (var xElement in parentNode.Elements())
                {
                    PdfDefElement bindingDef = null;
                    if (xElement.HasElements)
                    {
                        bindingDef = PdfDefFieldsGroup.FromXElementSelfOnly(xElement);
                        Func(xElement, (PdfDefFieldsGroup)bindingDef);
                    }
                    else
                    {
                        bindingDef = PdfDefField.FromXElement(xElement);
                    }
                    parent.Add(bindingDef);
                }

            }
            var rootGroup = PdfDefFieldsGroup.FromXElementSelfOnly(xRootElement);
            Func(xRootElement, rootGroup);
            return rootGroup;
        }

    }
}