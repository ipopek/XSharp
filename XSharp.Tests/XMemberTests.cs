using JohnsWorkshop.XSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XSharp.Tests.XmlFiles
{
    [TestClass]
    public class XMemberTests
    {
        dynamic doc = null;
        const int bookCount = 12;

        public XMemberTests()
        {
            doc = XDocument.FromXml(Properties.Resources.Books);
        }

        #region Tests for count(), length(), any(), empty()

        [TestMethod]
        public void XMember_InvokeMethodCaseTest_SameResults()
        {
            dynamic d = doc.catalog.book;
            Assert.IsTrue(d.count() == bookCount, "Book count changed");

            // additionally, "length" and "count" should be equivalent
            string[] methods = { "count", "COUnT", "COUNT", "cOUNT", "Count",
                                 "length", "LENGTH", "lENgth"};
            foreach (string m in methods)
            {
                int result = d.invoke(m);
                Assert.IsTrue(result == bookCount);
            }

        }

        [TestMethod]
        public void XMember_EmptyObject_ZeroCount()
        {
            dynamic d = XNodeList.Empty;
            
            Assert.IsTrue(d.count() == 0, "Element count is not zero");
            Assert.IsTrue(d.empty(), "The returned object is not empty");
            Assert.IsTrue(!d.Any(), "The returned object contains elements");
        }

        [TestMethod]
        public void XMember_ValidObjects_ValidCount()
        {
            var map = new[]
            {
                 new { Query = "book", Count = bookCount},
                 new { Query = "catalog",  Count = 1},
                 new { Query = "price",  Count = bookCount},
                 new { Query = "does_not_exist",  Count = 0}
            };

            foreach (var m in map)
            {
                // Check all selectors.
                var result = doc[m.Query];
                string msg = string.Format(" ({0})", m.Query);

                Assert.IsTrue(result.count() == m.Count, "Count value does not match" + msg);

                // Check empty() and any() functions
                Assert.IsTrue(result.any() == (m.Count > 0), "any() test failed" + msg);
                Assert.IsTrue(result.empty() == (m.Count == 0), "empty() test failed" + msg);
            }
        }

        #endregion

        #region Sanity tests

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void XMember_InvalidMethod_ExceptionThrown()
        {
            doc.invalidFunctionCall();
        }

        #endregion

        #region Tests for name(), n(), text(), val(), v(), text(), path()

        [TestMethod]
        public void XMember_ValidObjects_CorrectName()
        {
            string[] queries = { "catalog", "book", "price", "genre", "title" };

            foreach (string q in queries)
            {
                string n1 = doc[q].name();
                string n2 = doc[q].n();

                string m = string.Format(" ({0})", q);

                Assert.IsNotNull(n1, "First object is null" + m);
                Assert.IsNotNull(n2, "Second object is null" + m);

                Assert.IsTrue(n1 == n2, "Different objects returned" + m);
                Assert.IsTrue(n1 == n2 && n2 == q, "Node name does not equal selector text");
            }
        }

        [TestMethod]
        public void XMember_EmptyObject_EmptyNameString()
        {
            var result = doc.does_not_exist;

            string n1 = result.name();
            string n2 = result.n();

            Assert.IsNotNull(n1, "First object is null");
            Assert.IsNotNull(n2, "Second object is null");

            Assert.IsTrue(n1 == n2, "Different objects returned");
            Assert.IsTrue(n1 == string.Empty && n2 == string.Empty, "Result is not tempty string");
        }

        [TestMethod]
        public void XMember_ValidObjects_CorrectValue()
        {
            var book = doc.catalog.book.first();
            var map = new[]
            {
                new { Name = "author", Value = "Gambardella, Matthew" },
                new { Name = "title", Value = "XML Developer's Guide" },
                new { Name = "genre", Value = "Computer" },
                new { Name = "price", Value = "44.95" },
                new { Name = "publish_date", Value = "2000-10-01" },
            };

            foreach (var m in map)
            {
                var node = book[m.Name];
                string[] results = { m.Value, node.value(), node.val(), node.v(), node.text() };

                for (int n = 0; n < results.Length - 1; n++)
                {
                    string msg = string.Format(" ({0}, '{1}')", m.Name, m.Value);
                    Assert.IsTrue(results[n] == results[n + 1], "Value does not match" + msg);
                }
            }
        }

        [TestMethod]
        public void XMember_EmptyObject_EmptyValueString()
        {
            var node = doc.does_not_exist;
            string[] results = { node.value(), node.val(), node.v(), node.text() };

            foreach (string r in results)
                Assert.IsTrue(r == string.Empty, string.Format("{0} is not empty", r != null ? r : "<null>"));
        }

        [TestMethod]
        public void XMember_ValidObjects_CorrectPath()
        {
            var map = new[]
            {
                new { Name = "catalog", Value = "catalog" },
                new { Name = "book", Value = "catalog\\book" },
                new { Name = "price", Value = "catalog\\book\\price" },
                new { Name = "invalid_name", Value = "" },
                new { Name = "", Value = "" },
            };

            foreach (var item in map)
            {
                string p = doc[item.Name].path();
                Assert.AreEqual(item.Value, p);
            }
        }

        #endregion

        #region XMethod axis selector tests

        [TestMethod]
        public void XMethod_AxisSelectorForEmptyObject_EmptyObject()
        {
            string[] selectors = { "first", "last", "nth", "prev", "next", "parent", 
                                     "root", "random", "children", "siblings" };

            var target = doc.does_not_exist;

            foreach (var s in selectors)
            {
                dynamic result = null;

                if (s == "nth")
                {
                    // must invoke 'nth' with a single parameter,
                    // otherwise we get an exception (not supported Ex).
                    result = target.invoke(s, 0);
                }
                else
                {
                    result = target.invoke(s);
                }

                string msg = string.Format(" ({0})", s);
                Assert.IsNotNull(result, "Result is null" + msg);
                Assert.IsTrue(result is XNodeList, "Result is not XNodeList object" + msg);
            }
        }

        [TestMethod]
        public void XMethod_RandomAxisSelector_RandomResults()
        {
            var books = doc.catalog.book;
            int nBooks = books.count();

            // The number of times the random node will be retrieved.
            int nIterations = 500;

            // Check that the input file has not been modified
            // to skew the test results.
            Assert.IsTrue(books.count() == bookCount);

            var randomNodes = new List<XNodeList>();
            while (nIterations > 0)
            {
                var rnd = books.random();

                // The random node must be a 
                // child of the 'books' object.                
                Assert.IsTrue(rnd.parent() == books.parent());

                if (!randomNodes.Contains(rnd))
                    randomNodes.Add(rnd);

                nIterations--;
            }

            int nUnique = randomNodes.Count();
            int nMinUniquePercent = 85;

            Assert.IsTrue((nUnique >= ((float)nMinUniquePercent / 100) * nBooks) && nUnique <= nBooks,
                "Less than {0}% of range covered (actual: {1}/{2})", nMinUniquePercent, nUnique, nBooks);
        }

        [TestMethod]
        public void XMethod_FirstAxisSelector_FirstObject()
        {
            System.Xml.XmlNode[] allXmlNodes = doc.catalog.book;
            System.Xml.XmlNode firstXmlNode = doc.catalog.book.first();

            // %%TODO: fix this!
            //Assert.IsTrue(firstXmlNode.Equals(allXmlNodes[0]), "Xml node is first element of sequence");
        }

        [TestMethod]
        public void XMethod_LastAxisSelector_LastObject()
        {
            System.Xml.XmlNode[] allXmlNodes = doc.catalog.book;
            System.Xml.XmlNode lastXmlNode = doc.catalog.book.last();

            // %%TODO: fix this!
            //Assert.IsTrue(lastXmlNode.Equals(allXmlNodes[allXmlNodes.Length - 1]), "Xml node is last element of sequence");
        }

        [TestMethod]
        public void XMethod_NthAxisSelectorInvalidIndex_ExceptionThrown()
        {
            var books = doc.catalog.book;
            int nBooks = books.count();

            int[] indices = { -17, -42, -69, Int32.MinValue, -1, nBooks, nBooks + 1, Int32.MaxValue };
            int exceptions = 0;

            foreach (int index in indices)
            {
                try
                {
                    books.nth(index);
                }
                catch (ArgumentOutOfRangeException)
                {
                    exceptions++;
                }
            }

            Assert.IsTrue(exceptions == indices.Length, "Number of exceptions does not match");
        }

        [TestMethod]
        public void XMethod_NthAxisSelector_NthObject()
        {
            var books = doc.catalog.book;
            System.Xml.XmlNode[] allXmlNodes = books;

            for (int nPos = 0; nPos < allXmlNodes.Length; nPos++)
            {
                System.Xml.XmlNode nthNode = doc.catalog.book.nth(nPos);

                // %%TODO: fix this!
                //Assert.IsTrue(nthNode.Equals(allXmlNodes[nPos]), "Xml node is {0}. element of sequence", nPos + 1);
            }
        }

        [TestMethod]
        public void XMethod_RootAxisSelector_SameRootObject()
        {
            string[] queries = { "book", "title", "catalog", "price", "genre" };
            var root = doc.catalog;

            foreach (string q in queries)
            {
                string msg = string.Format(" ({0})", q);
                Assert.IsTrue(root == doc[q].root(), "Root node does not match" + msg);
            }
        }

        #endregion

        #region attr() get and set

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void XMethod_AttrWithNoParams_ExceptionThrown()
        {
            doc.attr();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void XMethod_AttrWithSingleEmptyParam1_ValidAttribute()
        {
            var theBook = doc["book"].first();
            theBook.attr(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XMethod_AttrWithSingleEmptyParam2_ValidAttribute()
        {
            var theBook = doc["book"].first();
            theBook.attr(null);
        }

        [TestMethod]
        public void XMethod_AttrWithSingleValidParam_ValidAttribute()
        {
            var allBooks = doc["book"];
            var theBook = allBooks.first();

            Assert.IsTrue(theBook.attr("doesNotExist") == string.Empty, "Non-existing attribute returns non-empty value");

            int nPosition = 0;
            foreach (var book in allBooks)
            {
                string id = string.Format("bk1{0:00}", ++nPosition);
                string attrValue = book.attr("id");
                Assert.AreEqual(id, attrValue);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void XMethod_AttrWithTwoParams1_ExceptionThrown()
        {
            var allBooks = doc["book"];
            Assert.IsTrue(allBooks);

            allBooks.attr(string.Empty, "true");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XMethod_AttrWithTwoParams2_ExceptionThrown()
        {
            var allBooks = doc["book"];
            Assert.IsTrue(allBooks);

            allBooks.attr(null, "true");
        }

        [TestMethod]
        public void XMethod_AttrWithTwoParams3_ValidAttribute()
        {
            var allBooks = doc["book"];
            Assert.IsTrue(allBooks);

            string attrName = "new";
            string[] attrValues = { "true", "false", string.Empty , null };

            foreach (string a in attrValues)
            {
                // create / update attribute
                allBooks.attr(attrName, a);

                if (a != null)
                {
                    // check it
                    foreach (System.Xml.XmlNode node in allBooks)
                        Assert.AreEqual(node.Attributes[attrName].Value as string, a);
                }
                else
                {
                    // the attribute should have been removed
                    foreach (System.Xml.XmlNode node in allBooks)
                    {
                        var attributes = node.Attributes.OfType<System.Xml.XmlAttribute>().Select(attr => attr.LocalName);
                        Assert.IsTrue(!attributes.Contains(attrName), "Should not contain attribute '{0}'", attrName);
                    }
                }
            }
        }

        #endregion

        #region index() tests

        [TestMethod]
        public void XMember_IndexMethod_AllTests()
        {
            var books = doc["book"];
            Random r = new Random();
            int pos = r.Next(0, books.count());

            var map = new[]
            {
                /* Run 0 */ new { Actual = books.first().index(), Expected = 0 },
                /* Run 1 */ new { Actual = books.index(), Expected = (int)books.first().index() },
                /* Run 2 */ new { Actual = books.last().index(), Expected = (int)(books.count() - 1) },
                /* Run 3 */ new { Actual = books[pos].index(), Expected = pos },
                /* Run 4 */ new { Actual = XNodeList.Empty.index(), Expected = -1 },
                /* Run 5 */ new { Actual = books.root().index(), Expected = -1 },
                /* Run 6 */ new { Actual = books.does_not_exist.index(), Expected = -1 }
            };

            for (int nMap = 0; nMap < map.Length; nMap++)
            {                
                var m = map[nMap];
                Assert.AreEqual(m.Expected, m.Actual, string.Concat("Run: ", nMap));
            }
        }

        #endregion
    }
}
