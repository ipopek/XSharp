using JohnsWorkshop.XSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using XSharp.Tests.Properties;

namespace XSharp.Tests
{
    [TestClass]
    public class XDocumentTests
    {
        #region XDocument.FromXml() tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FromXml_NullInput_ExceptionThrown()
        {
            XDocument.FromXml(null);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FromXml_EmptyString_ExceptionThrown()
        {
            XDocument.FromXml(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FromXml_InvalidXml_ExceptionThrown()
        {
            var xml = "<doc><insert invalid xml here></doc>";
            XDocument.FromXml(xml);
        }

        [TestMethod]
        public void FromXml_ValidXml_NoExceptionThrown()
        {
            var xml = "<doc></doc>";
            var d = XDocument.FromXml(xml);

            Assert.IsNotNull(d, "The XDocument object is null");
            Assert.IsTrue(d is XNodeList, "The XDocument object is not an XNodeList object");
            Assert.IsTrue(d.count() == 1, "The XDocument object does not contain a single root node");
        }

        #endregion

        #region XDocument - node query tests

        [TestMethod]
        public void XDocument_ValidNodeQuery_ValidObject()
        {
            var doc = XDocument.FromXml(Resources.Books);
            var c = doc.catalog;

            // Must be valid XNodeList object containing a single element.
            Assert.IsNotNull(c, "The catalog object is null");
            Assert.IsTrue(c is XNodeList, "The catalog object is not an XNodeList object");
            Assert.IsTrue(c != XNodeList.Empty, "The catalog is not an empty object");
            Assert.IsTrue(c.count() == 1, "The number of elements is incorrect");
        }

        [TestMethod]
        public void XDocument_NodeQueryCase_SameObject()
        {
            var doc = XDocument.FromXml(Resources.Books);

            dynamic[] objects = 
            {
                doc.catalog, doc.CATALOG, doc.Catalog, doc.CATaLOg
            };

            // All of these queries should return the same object,
            // regardless of the case.
            for (int i = 0; i < objects.Length - 1; i++)
                Assert.IsTrue(objects[i] == objects[i + 1], "Not all queries return the same object");                
        }

        [TestMethod]
        public void XDocument_InvalidNodeQuery_EmptyObject()
        {
            var doc = XDocument.FromXml(Resources.Books);
            var node = doc.non_existent_node;

            // Must be valid and empty XNodeList object.
            Assert.IsNotNull(node, "The node is null");
            Assert.IsTrue(node is XNodeList, "The node object is not a valid XNodeList object");
                        
            Assert.IsTrue(node.count() == 0, "The node should not contain any elements");
            Assert.IsTrue(node == XNodeList.Empty, "The node should be equal to the Empty XNostList object");

            // Implicit boolean conversion test
            Assert.IsFalse(node, "Implicit boolean conversion test failed");

            // Implicit string conversion test
            Assert.IsTrue(string.IsNullOrEmpty(node), "Implicit string conversion test failed");
        }

        #endregion
    }
}
