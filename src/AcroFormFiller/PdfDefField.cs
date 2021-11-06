using System;
using System.Xml.Linq;

namespace AcroFormFiller
{
    public class PdfDefField : PdfDefElement
    {
        public const string ValueExpressionAttributeName = "expr";
        public const string OnValueAttributeName = "onValue";
        public const string FormatAttributeName = "format";
        private TrimedString _valueExpression;
        private TrimedString _onValue;
        private TrimedString _displayFormat;

        public string ValueExpression
        {
            get => _valueExpression;
            set => _valueExpression = value;
        }

        public string OnValue
        {
            get => _onValue;
            set => _onValue = value;
        }

        public string DisplayFormat
        {
            get => _displayFormat;
            set => _displayFormat = value;
        }

        public PdfDefField(string name):base(false)
        {
            _name = ValidatedName(name);
        }

        public static PdfDefField FromXElement(XElement xElement)
        {
            return new PdfDefField(xElement.Name.ToString())
            {
                Condition = (string)xElement.Attribute(ConditionAttributeName),
                ValueExpression = (string)xElement.Attribute(ValueExpressionAttributeName),
                DisplayFormat = (string)xElement.Attribute(FormatAttributeName),
                OnValue = (string)xElement.Attribute(OnValueAttributeName)
            };
        }

        public string ToString(object value)
        {
            if (value == null) return null;
            if (value is bool boolObj) return boolObj ? OnValue ?? "1" : null;
            if (DisplayFormat != null)
            {
                try
                {
                    return string.Format(DisplayFormat, value);
                }
                catch
                {
                }
            }

            return value is DateTime dateTime ? dateTime.ToShortDateString() : Convert.ToString(value);
        }

        public override XElement ToXElement()
        {
            return new XElement(Name
                , ValueExpression != null ? new XAttribute(ValueExpressionAttributeName, ValueExpression) : null
                , OnValue != null ? new XAttribute(OnValueAttributeName, OnValue) : null
                , DisplayFormat != null ? new XAttribute(FormatAttributeName, DisplayFormat) : null
                , Condition != null ? new XAttribute(ConditionAttributeName, Condition) : null);
        }
    }
}