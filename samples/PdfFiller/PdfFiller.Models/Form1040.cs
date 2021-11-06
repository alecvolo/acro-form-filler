using System.Collections.Generic;

namespace Models
{
    public class Form1040
    {
        public Person TaxPayer { get; set; }

        public FillingStatus FillingStatus { get; set; }
        public Person Spouse { get; set; }
        public List<Dependent> Dependents { get; set; } = new List<Dependent>();
    }

}
