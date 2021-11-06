namespace Models
{
    public class Dependent
    {
        public Person Person { get; set; }
        public string Relation { get; set; }
        public bool ChildTaxCredit { get; set; }
        public bool CreditForOtherDependents { get; set; }
    }
}