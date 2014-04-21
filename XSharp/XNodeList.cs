using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace JohnsWorkshop.XSharp
{
    public class XNodeList : XElement<XmlNode>, IEquatable<XNodeList>
    {        
        #region Constructors / initializers

        public XNodeList()
        {
            Initialize(new XmlNode[0]);
        }

        public XNodeList(XmlNode node)
        {
            if (node != null)
                Initialize(new XmlNode[] { node });
            else
                AllObjects = new XmlNode[0];
        }

        public XNodeList(IEnumerable<XmlNode> nodes)
        {
            Initialize(nodes);
        }

        public XNodeList(XmlNodeList nodes)
        {
            if (nodes != null)
                Initialize(nodes.OfType<XmlNode>());
        }

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="nodes"></param>
        private void Initialize(IEnumerable<XmlNode> nodes)
        {
            AllObjects = nodes;
        }

        #endregion

        #region Interfaces / Overrides

        private static XNodeList _empty = null;

        /// <summary>
        /// Retrieves an empty <see cref="XNodeList"/> object.
        /// </summary>
        public static dynamic Empty
        {
            get 
            {
                if (_empty == null)
                    _empty = new XNodeList(new XmlNode[0]);

                return _empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return AllObjects.GetHashCode();
        }

        protected override IEnumerable<XElement<XmlNode>> AsEnumerable()
        {
            if (AllObjects != null && AllObjects.Any())
            {
                foreach (XmlNode node in AllObjects)
                    yield return new XNodeList(node);
            }
            else
            {
                yield break;
            }
        }

        #endregion

        /// <summary>
        /// Retrieves an element from the underlying sequence at a specified index (if any).
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public XNodeList this[int pos]
        {
            get { return XMember.GetElementAtPosition(AllObjects, pos); }
        }

        /// <summary>
        /// Retrieves any elements from the underlying sequence that match the specified selector.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public XNodeList this[string selector]
        {
            get
            {
                if (AllObjects != null && AllObjects.Any() && !string.IsNullOrEmpty(selector))
                {
                    var results = new List<XmlNode>();
                    XMember.FindNodes(AllObjects, selector, results);

                    if (results.Any())
                        return new XNodeList(results);
                }

                return Empty;
            }
        }

        /// <summary>
        /// Performs an implicit conversion to a <see cref="System.String"/> object.
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static implicit operator string(XNodeList nodeList)
        {
            if (nodeList != null && nodeList.AllObjects != null && nodeList.AllObjects.Any())
                return nodeList.AllObjects.ElementAt(0).InnerXml;

            return string.Empty;
        }

        /// <summary>
        /// Performs an implicit conversion to a <see cref="System.Boolean"/> object.
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static implicit operator bool(XNodeList nodeList)
        {
            return nodeList != null && nodeList.AllObjects != null && nodeList.AllObjects.Any();
        }

        /// <summary>
        /// Performs an implicit conversion to a <see cref="System.XmlNode[]"/> object.
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static implicit operator XmlNode[](XNodeList nodeList)
        {
            if (nodeList != null && nodeList.AllObjects != null)
                return nodeList.AllObjects.ToArray();

            return new XmlNode[0];
        }

        /// <summary>
        /// Performs an implicit conversion to a <see cref="System.XmlNode"/> object.
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static implicit operator XmlNode(XNodeList nodeList)
        {
            if (nodeList != null && nodeList.AllObjects != null && nodeList.AllObjects.Any())
                return nodeList.AllObjects.ElementAt(0).Clone();
            
            return null;
        }

        /// <summary>
        /// Attempts to dynamicall retrieve a property (member) of the object.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            if (!base.TryGetMember(binder, out result))
            {
                List<XmlNode> nodes = new List<XmlNode>();

                foreach (XmlNode node in AllObjects)
                {
                    var childNodes = node.ChildNodes.OfType<XmlNode>().Where(n => n.LocalName.Equals(binder.Name, StringComparison.OrdinalIgnoreCase));
                    nodes.AddRange(childNodes);
                }

                result = new XNodeList(nodes);
            }

            return true;
        }

        /// <summary>
        /// Attempts to dynamically invoke a method on the object.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;

            if (!base.TryInvokeMember(binder, args, out result))
                result = XMember.Invoke(AllObjects, binder.Name, args);

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is XNodeList))
                return false;

            return this == (XNodeList)obj;
        }

        public static bool operator !=(XNodeList first, XNodeList second)
        {
            return !(first == second);
        }

        public static bool operator ==(XNodeList first, XNodeList second)
        {
            // Same object or both null?
            if (object.ReferenceEquals(first, second))
                return true;

            // Only one null?
            if (((object)first == null || ((object)second == null)))
                return false;

            // Element count must match.
            if (first.Count() != second.Count())
                return false;

            XmlNode[] nodeListA = first;
            XmlNode[] nodeListB = second;

            for (int nNode = 0; nNode < nodeListA.Length; nNode++)
            {
                if (!nodeListA[nNode].Equals(nodeListB[nNode]))
                    return false;
            }

            return true;
        }

        public bool Equals(XNodeList other)
        {
            return (this == other);
        }
    }
}
