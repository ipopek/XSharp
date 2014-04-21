using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace JohnsWorkshop.XSharp
{
    public abstract class XElement<T> : DynamicObject, IEnumerable<XElement<T>>
    {
        private IEnumerable<T> _allObjects;
        private T _defaultObject;

        /// <summary>
        /// Gets or sets a sequence of underlying objects.
        /// </summary>
        protected IEnumerable<T> AllObjects
        {
            get { return _allObjects; }
            set 
            {
                _allObjects = value;
                if (_allObjects != null && _allObjects.Any())
                    _defaultObject = _allObjects.ElementAt(0);
                else
                    _defaultObject = default(T);
            }
        }

        /// <summary>
        /// Gets or sets an instance of the detault underlying object.
        /// </summary>
        protected T DefaultObject
        {
            get { return _defaultObject; }
            set
            {
                _defaultObject = value;
                _allObjects = new List<T> { value };
            }
        }

        IEnumerator<XElement<T>> IEnumerable<XElement<T>>.GetEnumerator()
        {
            return AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return AsEnumerable().GetEnumerator();
        }

        /// <summary>
        /// Retrieves the underlying element sequence that will be used when enumerating the object.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<XElement<T>> AsEnumerable();
    }
}
