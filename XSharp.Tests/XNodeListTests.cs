using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JohnsWorkshop.XSharp;

namespace XSharp.Tests
{
    [TestClass]
    public class XNodeListTests
    {
        dynamic doc = null;
        const int bookCount = 12;

        public XNodeListTests()
        {
            doc = XDocument.FromXml(Properties.Resources.Books);
        }

        #region XNodeList - node query mechanism

        [TestMethod]
        public void XNodeList_ValidChainedNodeQueries_NonEmptyObject()
        {
            var prices = doc.catalog.book.price;

            Assert.IsNotNull(prices, "The result is null");
            Assert.IsTrue(prices is XNodeList, "The result is not a valid XNodeList object");

            // Sequence must contain exact number of elements (number of books in input file).
            Assert.IsTrue(prices.count() == bookCount, string.Format("Element count should be {0}", bookCount));
        }

        [TestMethod]
        public void XNodeList_InvalidChainedNodeQueries_EmptyObject()
        {
            var prices = doc.cat___alog.book.price;

            Assert.IsNotNull(prices, "Result is null");
            Assert.IsTrue(prices is XNodeList, "The result is not a valid XNodeList object");

            // Sequence must contain no elements (invalid path).
            Assert.IsTrue(prices.count() == 0, "Result should contain no elements");
        }

        #endregion

        #region XNodeList - single node selectors

        [TestMethod]
        public void XNodeList_SingleValidNodeSelector_ValidObjects()
        {
            var results = doc["price"];

            Assert.IsNotNull(results, "Result is null");
            Assert.IsTrue(results is XNodeList, "The result is not a valid XNodeList object");

            Assert.IsTrue(results.count() == bookCount, string.Format("Element count should be {0}", bookCount));
        }

        [TestMethod]
        public void XNodeList_SingleInvalidNodeSelector_EmptyObject()
        {
            var results = doc["pr_ice"];

            Assert.IsNotNull(results, "Result is null");
            Assert.IsTrue(results is XNodeList, "The result is not a valid XNodeList object");

            Assert.IsTrue(results.count() == 0, "Result should contain no elements");
        }

        #endregion

        #region XNodeList - multiple node selectors

        [TestMethod]
        public void XNodeList_MultipleValidNodeSelector_ValidObjects()
        {
            string[] queries = 
            {
                "catalog price",
                "catalog > book > price",
                "catalog book > price",
                "catalog > book price",
                "book > price",
                "book price"
            };

            foreach (string q in queries)
            {
                var results = doc[q];
                string msg = string.Format(" ({0})", q);

                Assert.IsNotNull(results, "Result is null" + msg);
                Assert.IsTrue(results is XNodeList, "Result is not a valid XNodeList object" + msg);

                Assert.IsTrue(results.count() == bookCount,
                    string.Format("Element count should be {0}", bookCount) + msg);
            }
        }

        [TestMethod]
        public void XNodeList_MultipleInvalidNodeSelector_EmptyObject()
        {
            string[] queries = 
            {
                "cat_alog price",   // invalid + valid node
                "catalog pri_ce",   // valid + invalid node
                "price catalog",    // valid nodes in reverse order
                "catalog > price",  // incorrect relationship
                "price > catalog"   // incorrect order and relationship
            };

            foreach (string q in queries)
            {
                var results = doc[q];
                string msg = string.Format(" ({0})", q);

                Assert.IsNotNull(results, "Result is null" + msg);
                Assert.IsTrue(results is XNodeList, "Result is not a valid XNodeList object" + msg);
                Assert.IsTrue(results.count() == 0, "Element count should be zero" + msg);
            }
        }

        #endregion

        #region Testing foreach loop

        [TestMethod]
        public void XNodeList_ForEachLoop_ValidItems()
        {
            var books = doc.catalog.book;
            Assert.IsTrue(books.count() == bookCount, "Book count changed (XML modified?)");

            int nBooks = books.count();
            int nCurrent = 0;

            foreach (var b in books)                
                Assert.IsTrue(b == books[nCurrent++], "Book at position {0} does not match", nCurrent);

            Assert.IsTrue(nCurrent == nBooks, "Did not iterate through all the elements");
        }

        [TestMethod]
        public void XNodeList_ForEachLoop_EmptyObject()
        {
            var books = doc.catalog.book.does_not_exist;

            foreach (var b in books)
            {
                // we should never get here, since there are no elements.
                Assert.Fail("Enumerator returned results; empty sequence expected");
            }
        }

        #endregion

        #region Testing while loop

        [TestMethod]
        public void XNodeList_ForwardWhileLoop_ValidItems()
        {
            var books = doc.catalog.book;
            int nBooks = books.count();
            Assert.IsTrue(nBooks == bookCount, "Book count changed (XML modified?)");

            int nCurrent = 0;
            var currentBook = books.first();

            while (currentBook)
            {
                Assert.IsTrue(currentBook == books[nCurrent], "Item at index {0} does not match", nCurrent);

                nCurrent++;
                currentBook = currentBook.next();
            }

            Assert.IsTrue(nCurrent == nBooks, "Item count does not match");
        }

        [TestMethod]
        public void XNodeList_ReverseWhileLoop_ValidItems()
        {
            var books = doc.catalog.book;

            int nBooks = books.count();
            Assert.IsTrue(nBooks == bookCount, "Book count changed (XML modified?)");
            int nCurrent = 0;

            var currentBook = books.last();
            while (currentBook)
            {
                nCurrent++;
                int index = nBooks - nCurrent;

                Assert.IsTrue(currentBook == books[index], "Item at index {0} does not match", index);
                currentBook = currentBook.prev();
            }

            Assert.IsTrue(nCurrent == nBooks, "Item count does not match");
        }

        [TestMethod]
        public void XNodeList_ForwardWhileLoop_EmptyObject()
        {
            var books = doc.catalog.book.does_not_exist;

            var theBook = books.first();
            while (theBook)
            {
                Assert.Fail("Entered while loop for empty object; expected skip");
            }
        }

        [TestMethod]
        public void XNodeList_ReverseWhileLoop_EmptyObject()
        {
            var books = doc.catalog.book.does_not_exist;

            var theBook = books.last();
            while (theBook)
            {
                Assert.Fail("Entered while loop for empty object; expected skip");
            }
        }

        #endregion
    }
}
