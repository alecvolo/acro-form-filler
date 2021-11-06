using Xunit;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;


namespace AcroFormFiller.Tests
{
    public class ScopeTests
    {
        [Theory]
        [InlineData("name", "condition", "variable-name", "variable-expression")]
        public void Should_Create_Scope_With_Correct_Values(string name, string condition, string variableName, string variableExpression)
        {
            var scope = new PdfDefFieldsGroup(name)
            {
                Condition = condition,
                VariableName = variableName,
                VariableExpression = variableExpression
            };
            scope.Name.Should().Be(name);
            scope.Condition.Should().Be(condition);
            scope.VariableName.Should().Be(variableName);
            scope.VariableExpression.Should().Be(variableExpression);
            scope.Elements().Should().BeEmpty();
        }


        [Theory]
        [InlineData("name1", "name2", "name1.name2")]
        [InlineData("name1", null, "name1")]
        [InlineData(null, "name2", "name2")]
        [InlineData(null, null, null)]
        public void Should_Create_Scope_Without_Elements1(string name, string name2, string result)
        {
            string.Join('.', new [] {name, name2}.Where(t=>!string.IsNullOrWhiteSpace(t))).TrimToNull().Should().Be(result);
        }

        [Theory]
        [InlineData("name1", true)]
        [InlineData("name1[1232]", false)]
        [InlineData("name&sd", false)]
        [InlineData("<name>", false)]
        [InlineData("{http://acro.form.filler}scope", false)]
        public void TestName(string name, bool isValid=true)
        {
            try
            {
                (XName.Get(name).NamespaceName == "").Should().Be(isValid) ; // new XName();ExtractFieldName(name);

            }
            catch (XmlException e)
            {
                if (isValid)
                {
                    throw new ArgumentOutOfRangeException(nameof(name), e.Message);
                }
            }

        }

        [Fact()]

        public void Should_Create_Scope()
        {
            var scope = new PdfDefFieldsGroup() {Condition = "1=1"}.Add(new PdfDefField("field1") { Condition = "1=2"}
                , new PdfDefFieldsGroup("sub").Add(new PdfDefField("f-3"){ ValueExpression = "\"Test\""}));
            var xml = scope.ToXElement().ToString();
        }
    }
}