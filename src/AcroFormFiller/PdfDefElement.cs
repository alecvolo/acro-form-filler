using System;
using System.Xml;
using System.Xml.Linq;

namespace AcroFormFiller
{
    public abstract class PdfDefElement : TreeElement<PdfDefElement>
    {
        public const string ConditionAttributeName = "condition";
        private TrimedString _condition;
        protected TrimedString _name;

        public string Condition
        {
            get => _condition;
            set => _condition = value;
        }

        public string Name => _name;

        public PdfDefElement(bool isLeaf = false) :base(isLeaf)
        {
            
        }

        public abstract XElement ToXElement();

        public static string ValidatedName(string name, bool canBeNull = false)
        {
            name = name.TrimToNull();
            if (name == null)
            {
                if (!canBeNull)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                return null;
            }

            try
            {
                var xName = XName.Get(name);
                if (xName.NamespaceName != "")
                {
                    throw new ArgumentOutOfRangeException(nameof(name));
                }

                return xName.LocalName;

            }
            catch (XmlException e)
            {
                throw new ArgumentOutOfRangeException(nameof(name), e.Message);
            }
        }

    }
}