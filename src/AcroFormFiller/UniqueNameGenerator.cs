using System;
using System.Collections.Generic;

namespace AcroFormFiller
{
        public class UniqueNameGenerator
        {
            private readonly Func<int, string> _postfixFunc;
            private readonly IDictionary<string, string> _usedNames = new Dictionary<string, string>();

            public UniqueNameGenerator(Func<int, string> postfixFunc)
            {
                _postfixFunc = postfixFunc;
            }

            public void Reset()
            {
                _usedNames.Clear();
            }
            public string GetUnique(string identifier)
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    throw new ArgumentNullException(nameof(identifier));
                }
                var attempt = 0;
                while (true)
                {
                    var tryIdentifier = string.Concat(identifier, _postfixFunc(attempt++));
                    if (!_usedNames.ContainsKey(tryIdentifier))
                    {
                        _usedNames.Add(tryIdentifier, null);
                        return tryIdentifier;
                    }
                }
            }
        }
}