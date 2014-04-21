using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace JohnsWorkshop.XSharp
{
    internal static class XMember
    {
        /// <summary>
        /// Used for generating random numbers.
        /// </summary>
        private static Random _random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Dynamically invokes a specified method on a sequence of nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static object Invoke(IEnumerable<XmlNode> nodes, string methodName, object[] args)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("methodName");

            methodName = methodName.ToLowerInvariant();
            switch (methodName)
            {
                case "invoke":
                    {
                        if (args != null && args.Any())
                        {
                            string func = args[0] as string;
                            return Invoke(nodes, func, args.Skip(1).ToArray());
                        }

                        break;
                    }

                case "equals":
                case "eq":
                    {
                        if (args != null && args.Length == 1)
                        {
                            object objB = args[0];
                            if (objB == null || !(objB is XNodeList))
                                return false;

                            return new XNodeList(nodes) == (objB as XNodeList);
                        }

                        break;
                    }

                // Retrieves the index of the first item in the sequence
                // within the list of it's parents child nodes.
                case "index":
                    {
                        if (nodes != null && nodes.Any())
                        {
                            XmlNode theNode = nodes.ElementAt(0);
                            if (theNode.ParentNode != null)
                            {
                                if (theNode.ParentNode == theNode.OwnerDocument)
                                    return -1;

                                var siblingsAndSelf = theNode.ParentNode.ChildNodes.OfType<XmlNode>();
                                return siblingsAndSelf.IndexOf(p => p == theNode);
                            }
                        }

                        return -1;
                    }

                // Retrieve the number of elements in the input sequence.
                case "length":
                case "count":
                    {
                        return nodes.Count();
                    }

                // Retrieve the name of the first element in the sequence.
                case "name":
                case "n":
                    {
                        if (nodes.Any())
                            return nodes.ElementAt(0).LocalName;
                        else
                            return string.Empty;
                    }

                // Retrieve the patch of the first element in the sequence.
                case "path":
                    {
                        if (nodes.Any())
                            return GetNodePath(nodes.ElementAt(0));
                        else
                            return string.Empty;
                    }

                // Returns true if the input sequence contains elements.
                case "any":
                    {
                        return nodes.Any();
                    }

                // Returns true if the input sequence does not contain any elements.
                case "empty":
                    {
                        return !nodes.Any();
                    }

                // Retrieves a list of elements from the input sequence on a specified axis.
                case "first":
                case "last":
                case "nth":
                case "prev":
                case "next":
                case "parent":
                case "root":
                case "children":
                case "random":
                case "siblings":
                    {
                        int argCount = (args != null) ? args.Length : 0;

                        if (methodName.Equals("nth") && argCount == 1 ||
                            !methodName.Equals("nth") && argCount == 0)
                        {
                            return GetAxis(nodes, methodName, args);
                        }

                        break;
                    }

                // Either gets the value of a specified attribute for the first element in the
                // input sequence, or sets the value of a specified attribute for all of the
                // elements in the input sequence.
                case "attr":
                    {
                        if (args != null)
                        {
                            if (args.Length == 1)
                            {
                                if (args[0] is string)
                                    return GetAttributeValue(nodes, args[0] as string);
                                else if (args[0] == null)
                                    throw new ArgumentNullException("attribute name");

                                throw new ArgumentException("attribute name");
                            }
                            else if (args.Length == 2)
                            {
                                if (args[0] is string)
                                {
                                    SetAttributeValue(nodes, args[0] as string, args[1]);
                                    return nodes;
                                }
                                else if (args[0] == null)
                                {
                                    throw new ArgumentNullException("attribute name");
                                }

                                throw new ArgumentException("attribute name");
                            }
                        }

                        break;
                    }

                // Iterates over all of the elements in the input sequence and
                // invokes a specified action on each of them.
                case "each":
                    {
                        if (args != null && args.Length == 1 && args[0] is XAction)
                        {
                            var func = args[0] as XAction;
                            foreach (XmlNode n in nodes)
                                func(new XNodeList(n));
                        }

                        break;
                    }

                // Filters the underlying element sequence based on a specified predicate or selector.
                case "where":
                case "filter":
                    {
                        if (args != null && args.Length == 1 && args[0] is XPred)
                        {
                            var pred = args[0] as XPred;
                            var filteredNodes = nodes.Where(n => pred(new XNodeList(n)));
                            return new XNodeList(filteredNodes);
                        }
                        else if (args != null && args.Length == 1 && args[0] is string)
                        {
                            string xpath = args[0] as string;
                            if (!string.IsNullOrEmpty(xpath))
                            {
                                var results = new List<XmlNode>();
                                FindNodes(nodes, xpath, results);

                                if (results.Any())
                                    return new XNodeList(results);
                            }
                        }

                        break;
                    }

                // Either gets the text value of the first element in the input sequence or
                // sets the text value to a specified value for all the elements.
                case "val":
                case "value":
                case "text":
                case "v":
                    {
                        if (args == null || args.Length == 0)
                        {
                            return GetNodeValue(nodes, n => n.InnerText);
                        }
                        else if (args != null && args.Length == 1)
                        {
                            string strValue = args[0] != null ? args[0].ToString() : string.Empty;

                            foreach (XmlNode n in nodes)
                                n.InnerText = strValue;
                        }

                        break;
                    }

                // Projects the elements from the input sequence into a dictionary.
                case "map":
                case "toDictionary":
                case "toDict":
                    {
                        if (args != null)
                        {
                            if (args.Length == 1 && args[0] is XAction<XNodeList, string>)
                            {
                                var dict = new Dictionary<string, XNodeList>();
                                var func = args[0] as XAction<XNodeList, string>;

                                foreach (XmlNode node in nodes)
                                {
                                    dynamic value = new XNodeList(node);
                                    string key = func(value);
                                    dict.Add(key, value);
                                }

                                return dict;
                            }
                            else
                            {
                                throw new NotImplementedException(string.Format("{0} not implemented for {1} parameter(s)", methodName, args.Length));
                            }
                        }

                        break;
                    }

                // Projects the elements of the input sequence using a specified delegate.
                case "select":
                case "project":
                    {
                        if (args != null && args.Length == 1 && args[0] != null && args[0] is XFunc)
                        {
                            var results = new List<object>();
                            var func = args[0] as XFunc;

                            foreach (XmlNode node in nodes)
                            {
                                var res = func(new XNodeList(node));
                                if (res != null)
                                    results.Add(res);
                            }

                            return results;
                        }

                        break;
                    }

                // 
                case "find":
                case "lookup":
                    {
                        if (args != null && args.Length == 1 && args[0] != null && args[0] is XPred)
                        {
                            var pred = args[0] as XPred;
                            var results = new List<XmlNode>();

                            FindNodes(nodes, pred, results);
                            if (results.Any())
                                return new XNodeList(results);
                        }
                        else if (args != null && args.Length == 1 && args[0] != null && args[0] is string)
                        {
                            string strSelector = args[0] as string;
                            var results = new List<XmlNode>();
                            FindNodes(nodes, args[0] as string, results);

                            if (results.Any())
                                return new XNodeList(results);
                        }

                        break;
                    }
            }

            // Method/Function call was not handled. Format an exception message.
            string msg;

            if (args != null && args.Any())
                msg = string.Format("Function '{0}' with {1} argument(s) is not supported.", methodName, args.Length);
            else
                msg = string.Format("Unsupported function: '{0}'.", methodName);

            throw new NotSupportedException(string.Format(msg));
        }

        static string GetNodePath(XmlNode node)
        {
            var sbPath = new System.Text.StringBuilder();


            while (node.ParentNode != null)
            {
                sbPath.Insert(0, string.Concat("\\", node.Name));
                node = node.ParentNode;
            }

            return sbPath.ToString().Trim('\\');
        }

        internal static void FindNodes(IEnumerable<XmlNode> nodes, string selector, List<XmlNode> results)
        {
            var selectors = selector.Split("\t ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();

            XPred p = delegate(dynamic d)
            {
                dynamic n = d;
                int pos = 0;
                bool exactMatchNeeded = true;

                while (n.count() > 0)
                {
                    string currentSelector = selectors[pos];
                    string id = n.attr("id");

                    if (currentSelector.StartsWith("#") && (string.IsNullOrEmpty(id) || !id.Equals(currentSelector.Substring(1))))
                    {
                        if (exactMatchNeeded)
                            break;
                    }
                    else if (!n.name().Equals(currentSelector, StringComparison.OrdinalIgnoreCase) && !currentSelector.StartsWith("#"))
                    {
                        if (exactMatchNeeded)
                            break;
                    }
                    else
                    {
                        // found it!
                        pos++;

                        if (pos >= selectors.Length)
                            return true;

                        if (pos < selectors.Length && selectors[pos] == ">")
                        {
                            pos++;
                            exactMatchNeeded = true;
                        }
                        else
                        {
                            exactMatchNeeded = false;
                        }
                    }

                    n = n.parent();
                }

                return false;
            };

            FindNodes(nodes, p, results);
        }

        static void FindNodes(IEnumerable<XmlNode> nodes, XPred func, List<XmlNode> results)
        {
            if (nodes == null || !nodes.Any() || func == null || results == null)
                return;

            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (func(new XNodeList(node)))
                        results.Add(node);

                    if (node.ChildNodes != null && node.ChildNodes.Count > 0)
                        FindNodes(node.ChildNodes.OfType<XmlNode>(), func, results);
                }
            }
        }

        static string GetNodeValue(IEnumerable<XmlNode> nodes, Func<XmlNode, string> func)
        {
            if (nodes != null && nodes.Any() && func != null)
                return func(nodes.ElementAt(0));

            return string.Empty;
        }

        private static string GetAttributeValue(IEnumerable<XmlNode> nodes, string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentException("attribute name");

            if (nodes != null && nodes.Any())
            {
                var theNode = nodes.ElementAt(0);
                if (theNode != null && theNode.Attributes != null && theNode.Attributes.OfType<XmlAttribute>().Any())
                {
                    XmlAttribute attr = theNode.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
                    if (attr != null)
                        return attr.Value;
                }
            }

            return string.Empty;
        }

        private static void SetAttributeValue(IEnumerable<XmlNode> nodes, string attributeName, object attributeValue)
        {
            if (nodes != null && nodes.Any())
            {
                string strValue = attributeValue as string;
                foreach (XmlNode node in nodes)
                {
                    var attributes = node.Attributes.OfType<XmlAttribute>();

                    if (attributes.Any())
                    {
                        XmlAttribute attr = attributes.FirstOrDefault(a => a.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
                        if (attr != null)
                        {
                            if (strValue != null)
                            {
                                // update existing value;
                                attr.Value = strValue;
                            }
                            else
                            {
                                // remove the existing attribute
                                node.Attributes.RemoveNamedItem(attributeName);
                            }
                        }
                        else
                        {
                            if (strValue != null)
                            {
                                // create the new attribute and assign the value
                                attr = node.OwnerDocument.CreateAttribute(attributeName);
                                attr.Value = strValue;
                                node.Attributes.Append(attr);
                            }
                        }
                    }
                }
            }
        }

        private static object GetAxis(IEnumerable<XmlNode> nodes, string axisName, object[] args)
        {
            if (string.IsNullOrEmpty(axisName))
                throw new ArgumentException("axisName");

            if (nodes == null)
                throw new ArgumentNullException("nodes");

            if (nodes.Any())
            {
                if (axisName == "first")
                {
                    return new XNodeList(nodes.ElementAt(0));
                }
                else if (axisName == "last")
                {
                    return new XNodeList(nodes.ElementAt(nodes.Count() - 1));
                }
                else if (axisName == "nth" && args != null && args.Count() == 1 && args[0] is Int32)
                {
                    return GetElementAtPosition(nodes, (Int32)args[0]);
                }
                else if (axisName == "parent")
                {
                    return new XNodeList(nodes.ElementAt(0).ParentNode);
                }
                else if (axisName == "root")
                {
                    XmlNode n = nodes.ElementAt(0);
                    while (n.ParentNode != null)
                    {
                        n = n.ParentNode;
                    }

                    return new XNodeList(n.LastChild);
                }
                else if (axisName == "next")
                {
                    return new XNodeList(nodes.ElementAt(0).NextSibling);
                }
                else if (axisName == "prev")
                {
                    return new XNodeList(nodes.ElementAt(0).PreviousSibling);
                }
                else if (axisName == "children")
                {
                    return GetAllChildNodes(nodes, args);
                }
                else if (axisName == "random")
                {
                    return GetRandomChildNode(nodes);
                }
                else if (axisName == "siblings")
                {
                    return GetSiblingNodes(nodes);
                }

                throw new NotSupportedException(string.Format("Unknown axis '{0}'.", axisName));
            }

            return XNodeList.Empty;
        }

        private static XNodeList GetSiblingNodes(IEnumerable<XmlNode> nodes)
        {
            if (nodes != null && nodes.Any())
            {
                var results = new List<XmlNode>();

                foreach (XmlNode n in nodes)
                {
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        // Find all the siblings of the current node.
                        var siblings = n.ParentNode.ChildNodes.OfType<XmlNode>().Where(n2 => n2 != n);

                        // Filter out any duplicates.
                        siblings = siblings.Where(s => !results.Contains(s));

                        if (siblings.Any())
                            results.AddRange(siblings);
                    }
                }

                if (results.Any())
                    return new XNodeList(results);
            }

            return XNodeList.Empty;
        }

        private static XNodeList GetRandomChildNode(IEnumerable<XmlNode> nodes)
        {
            if (nodes == null || !nodes.Any())
                return XNodeList.Empty;

            int count = nodes.Count() - 1;
            int r = _random.Next(4, 20);
            int pos = 0;

            while (r >= 0)
            {
                pos = _random.Next(0, count);
                r--;
            }

            return GetElementAtPosition(nodes, pos);
        }

        private static XNodeList GetAllChildNodes(IEnumerable<XmlNode> nodes, object[] args)
        {
            if (nodes == null || !nodes.Any())
                return XNodeList.Empty;

            var childNodes = new List<XmlNode>();

            if (args == null || args.Length == 0)
            {
                foreach (XmlNode node in nodes)
                    childNodes.AddRange(node.ChildNodes.OfType<XmlNode>());


            }
            else if (args != null && args.Length == 1)
            {
                if (args[0] is Int32)
                {
                    int pos = (int)args[0];
                    var children = nodes.ElementAt(0).ChildNodes.OfType<XmlNode>();
                    if (pos >= 0 && pos < children.Count())
                        childNodes.Add(children.ElementAt(pos));
                }
            }

            if (childNodes.Any())
                return new XNodeList(childNodes);

            return XNodeList.Empty;
        }

        internal static XNodeList GetElementAtPosition(IEnumerable<XmlNode> nodes, int pos)
        {
            if (nodes == null || !nodes.Any())
                return XNodeList.Empty;

            return new XNodeList(nodes.ElementAt(pos));
        }
    }
}
