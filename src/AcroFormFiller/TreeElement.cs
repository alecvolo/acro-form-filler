using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AcroFormFiller
{
    public class TreeElement<T>
    {
        private readonly List<TreeElement<T>> _elements;
        public bool HasElements => _elements == null || _elements.Count == 0;
        public TreeElement(bool isLeaf = false)
        {
            if (!isLeaf)
            {
                _elements = new List<TreeElement<T>>();
            }
        }

        public void SortChildren(Comparison<TreeElement<T>> comparison)
        {
            if (_elements == null || _elements.Count <= 1)
            {
                return;
            }
            _elements.Sort(comparison);
        }

        protected void AddElement(TreeElement<T> element)
        {
            //todo check on circle
            if (_elements == null)
            {
                throw new InvalidOperationException("Can't add element to the leaf");
            }
            _elements.Add(element??throw new ArgumentNullException(nameof(element)));
        }
        protected void AddElement(IEnumerable<TreeElement<T>> elements)
        {
            if (_elements == null)
            {
                throw new InvalidOperationException("Can't add element to the leaf");
            }

            foreach (var element in (elements ?? throw new ArgumentNullException(nameof(elements))).Where(t=>t!=null))
            {
                _elements.Add(element);
            }
        }
        protected IEnumerable<TreeElement<T>> Descendants()
        {
            IEnumerable<TreeElement<T>> Func(TreeElement<T> parent)
            {
                if (_elements != null)
                {
                    foreach (var entry in parent._elements)
                    {
                        yield return entry;
                        foreach (var formField in Func(entry))
                        {
                            yield return formField;
                        }
                    }
                }
            }
            return Func(this);
        }
        protected IEnumerable<TreeElement<T>> GetElements()
        {
            if (_elements != null)
            {
                foreach (var entry in _elements)
                {
                    yield return entry;
                }
            }
        }
        protected IEnumerable<TreeElement<T>> DescendantsAndSelf()
        {
            yield return this;
            foreach (var treeElement in Descendants())
            {
                yield return treeElement;
            }
        }

        IEnumerator GetEnumerator()
        {
            return DescendantsAndSelf().GetEnumerator();
        }

    }
}