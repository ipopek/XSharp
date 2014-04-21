
namespace JohnsWorkshop.XSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public delegate void XAction(dynamic d);
    public delegate void XAction<T1, T2>(dynamic d);

    public delegate object XFunc(dynamic d);

    public delegate bool XPred(dynamic d);

    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the index of the first element in the sequence that matches the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> pred)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            for (int nItem = 0; nItem < source.Count(); nItem++)
            {
                if (pred(source.ElementAt(nItem)))
                    return nItem;
            }

            return -1;
        }
    }
}
