using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NReco.Linq;

namespace AcroFormFiller
{
    public class AcroFieldsEvaluator
    {

        private readonly LambdaParser _lambdaParser = new LambdaParser();
        private readonly bool _ignoreErrors;

        private readonly Stack<Dictionary<string, object>> _scopeVariables = new Stack<Dictionary<string, object>>();
        private readonly Stack<string> _fieldPaths = new Stack<string>();
        private readonly List<KeyValuePair<string, string>> _notEmptyFields = new List<KeyValuePair<string, string>>();
        private readonly UniqueNameGenerator _uniqueIdentifierGenerator =
            new UniqueNameGenerator(attempt => new StringBuilder().Append('[').Append(attempt).Append(']').ToString());

        public AcroFieldsEvaluator()
        {
            
        }

        public AcroFieldsEvaluator(bool ignoreEvaluationErrors)
        {
            _ignoreErrors = ignoreEvaluationErrors;
        }

        public IEnumerable<KeyValuePair<string, string>> CalcPdfFields(IEnumerable<PdfDefElement> elements,
            object bindObject, object variables = null)
        {
            Reset();
            var rootContext = ToVariables(bindObject, variables);
            _scopeVariables.Push(rootContext);
            try
            {
                Visit(elements);
                return _notEmptyFields.ToList();
            }
            finally
            {
                Reset();
            }
        }

        protected void Reset()
        {
            _scopeVariables.Clear();
            _notEmptyFields.Clear();
            _fieldPaths.Clear();
            _uniqueIdentifierGenerator.Reset();

        }
        protected void Visit(IEnumerable<PdfDefElement> entries)
        {
            foreach (var entry in entries)
            {
                _fieldPaths.Push(entry.Name != null ? _uniqueIdentifierGenerator.GetUnique(entry.Name) : null);
                if (IsConditionTrue(entry.Condition))
                {
                    switch (entry)
                    {
                        case PdfDefFieldsGroup @group:
                            Visit(@group);
                            break;
                        case PdfDefField field:
                            Visit(field);
                            break;
                    }
                }
                _fieldPaths.Pop();
            }
        }
        protected void Visit(PdfDefFieldsGroup scope)
        {
            var scopeHasVariables = false;
            if (scope.VariableExpression != null && scope.VariableName != null)
            {
                var scopeVariableValue = Eval(scope.VariableExpression);
                if (scopeVariableValue == null)
                {
                    return;
                }

                _scopeVariables.Push(ToDictionary(_scopeVariables.Peek(),
                    new[] { new KeyValuePair<string, object>(scope.VariableName, scopeVariableValue) }));
                scopeHasVariables = true;
            }

            Visit(scope.Elements());
            if (scopeHasVariables)
            {
                _scopeVariables.Pop();
            }
        }

        protected void Visit(PdfDefField field)
        {
            var stringValue = field.ToString(Eval(field.ValueExpression)).TrimToNull();
            if (stringValue != null)
            {
                var fullName = string.Join(".", _fieldPaths.Reverse().Where(t=>t!=null));
                _notEmptyFields.Add(new KeyValuePair<string, string>(fullName, stringValue));
            }
        }

        public static Dictionary<string, object> ToDictionary(object obj, StringComparer stringComparer = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj is IDictionary<string, object> asDictionary)
            {
                return new Dictionary<string, object>(asDictionary, stringComparer??StringComparer.CurrentCulture);
            }
            return obj.GetType().GetProperties().ToDictionary(p => p.Name,
                p => p.GetValue(obj),
                stringComparer??StringComparer.CurrentCulture);
        }
        public static Dictionary<string, object> ToVariables(object contextObject,
            object variables = null)
        {
            var result = contextObject == null
                ? new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
                : ToDictionary(contextObject, StringComparer.CurrentCultureIgnoreCase);
            if (variables != null)
            {

                foreach (var variable in ToDictionary(variables, StringComparer.CurrentCultureIgnoreCase))
                {
                    result[variable.Key] = variable.Value;
                }
            }

            return result;
        }

        public static Dictionary<string, object> ToDictionary(Dictionary<string, object> dictionary,
             IEnumerable<KeyValuePair<string, object>> newItems)
        {
            var result = dictionary.ToDictionary(t => t.Key, t => t.Value,
                dictionary.Comparer);
            if (newItems != null)
            {
                foreach (var keyValuePair in newItems)
                {
                    result[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            return result;
        }
        protected object Eval(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return null;
            }
            try
            {
                return _lambdaParser.Eval(expression, _scopeVariables.Peek());
            }
            catch (Exception e)
            {
                if (_ignoreErrors)
                {
                    return null;
                }

                throw new Exception($"Error in \"{expression}\" expression", e);
            }
        }

        protected bool IsConditionTrue(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return true;
            }

            return Convert.ToBoolean(Eval(condition));
        }

    }

}
