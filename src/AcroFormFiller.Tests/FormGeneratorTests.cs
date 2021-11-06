using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AcroFormFiller;
using iTextSharp.text.pdf;

namespace AcroFormFiller.Tests
{
    public class FormGeneratorTests
    {

        public enum FillingStatus
        {
            Unknown,
            Single,
            MarriedFillingJointly,
            MarriedFillingSeparately,
            HeadOfHouseHold,
            QualifyingWidow
        }
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string Ssn { get; set; }
            public DateTime BirthDay { get; set; }
            public bool IsBlind { get; set; }

        }

        public class Dependent
        {
            public Person Person { get; set; }
            public string Relation { get; set; }
            public bool ChildTaxCredit { get; set; }
            public bool CreditForOtherDependents { get; set; }
        }

        public class Form1040
        {
            public Person TaxPayer { get; set; }

            public FillingStatus FillingStatus { get; set; }
            public Person Spouse { get; set; }
            public List<Dependent> Dependents { get; set; } = new List<Dependent>();
        }

        [Fact]
        public void Should_Read_FormDef()
        {
            var formDef = PdfFormDefinition.LoadFrom(@"../../../Data/f1040.xml");

 //           var pdfFormFields = formDef.AcroFieldsBinding.Select(t => new {Name = t.Name, Value = t.Bind})
 //               .Where(t => t.Value != null).ToList();
 //           pdfFormFields.Count.Should().Be(42);
        }


        [Fact()]
        public void BuildTest()
        {
            var fileName = @"../../../Data/f1040.xml";
            //var fileName = @"../../../Data/fw4.xml";
            var formDef = PdfFormDefinition.LoadFrom(fileName);

            var entries = formDef.PdfFielsdDef;
        }

        [Fact()]
        public void Test_Check_Boxes()
        {
            var xmlText = @"
      <topmostSubform>
         <c1_01 expr ='1 == 1' />
         <c1_01 expr='true' onValue='2'/>
         <c1_01 expr='true' onValue='3'/>
         <c1_01 expr='true' onValue='4'/>
         <c1_01 expr='true' onValue='5'/>
      </topmostSubform>
";
            var scope = PdfDefFieldsGroup.LoadFrom(XDocument.Parse(xmlText).Root);

            scope.Elements().Should().HaveCount(5);
             
            var i = 0;
            foreach (var entry in new AcroFieldsEvaluator().CalcPdfFields(Enumerable.Repeat(scope, 1), new { }))
            {
                entry.Key.Should().Be($"topmostSubform[0].c1_01[{i++}]");
            }

        }


        [Fact]
        public void Should_GenerateForm()
        {
            var form = new Form1040()
            {
                FillingStatus = FillingStatus.MarriedFillingJointly,
                TaxPayer = new Person()
                {
                    FirstName = "Tax Payer's First Name",
                    LastName = "Tax Payer's Last Name",
                    MiddleName = "Mld",
                    Ssn = "123456789",
                    BirthDay = new DateTime(1955, 1, 1)
                },
                Spouse = new Person()
                {
                    FirstName = "Spouse's First Name",
                    LastName = "Spouse's  Last Name",
                    Ssn = "987654321",
                    BirthDay = new DateTime(1965, 1, 1),
                    IsBlind = true
                },
                Dependents = new List<Dependent>()
                {
                    new Dependent()
                    {
                        Person = new Person
                            {FirstName = "Dep1's First Name", LastName = "Dep1's Last Name", Ssn = "111223333"},
                        CreditForOtherDependents = true, Relation = "Child"
                    },
                    new Dependent()
                    {
                        Person = new Person
                            {FirstName = "Dep2's First Name", LastName = "Dep2's Last Name", Ssn = "222334444"},
                        ChildTaxCredit = true, Relation = "Child"
                    }
                }
            };
            var formDef = PdfFormDefinition.LoadFrom(@"../../../Data/f1040.xml");
            var entries = formDef.PdfFielsdDef;
            var variables = new {DateTime = (Func<int, int, int, DateTime>) ((y, m, d) => new DateTime(y, m, d))};

            var evaluator = new AcroFieldsEvaluator();
            var acroFieldsValues = evaluator.CalcPdfFields(entries, form, variables).ToArray();
            var pdf = GenerateForm(@"../../../Data/f1040.pdf",acroFieldsValues);
            Directory.CreateDirectory(@"../../../TestResult");
            File.WriteAllBytes(@"../../../TestResult/f1040-new.pdf", pdf);

        }

        public static byte[] GenerateForm(string pdfFile, IEnumerable<KeyValuePair<string, string>> fieldValues)
        {
            var pdfReader = new PdfReader(pdfFile);
            pdfReader.RemoveUsageRights();

            using var result = new MemoryStream();
            var pdfStamper = new PdfStamper(pdfReader, result);
            var pdfFormFields = pdfStamper.AcroFields;

            foreach (var formField in fieldValues)
            {
                pdfFormFields.SetField(formField.Key, formField.Value);
            }

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            return result.ToArray();

        }


    }
}