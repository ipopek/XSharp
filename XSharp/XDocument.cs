using System;
using System.Linq;
using System.Xml;

namespace JohnsWorkshop.XSharp
{
    public class XDocument : XNodeList
    {
        /// <summary>
        /// This object encapsulates the entire underlying XML structure.
        /// </summary>
        protected XmlDocument _xmlDocument = null;

        #region Class constructors and initializers

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="xmlDoc"></param>
        private XDocument(XmlDocument xmlDoc)
        {
            _xmlDocument = xmlDoc;

            if (xmlDoc != null && xmlDoc.LastChild != null)
                DefaultObject = xmlDoc.LastChild;
            else
                DefaultObject = xmlDoc;
        }
        
        /// <summary>
        /// Creates an instance of a <see cref="XDocument"/> object from an XML file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static dynamic FromFile(string filename)
        {            
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);

            return new XDocument(xmlDoc);
        }

        /// <summary>
        /// Creates an instance of a <see cref="XDocument"/> object from an XML fragment.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static dynamic FromXml(string xml)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            return new XDocument(xmlDoc);
        }

        /// <summary>
        /// Creates an instance of a <see cref="XDocument"/> object from an <see cref="System.Xml.XmlDocument"/> object.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static dynamic FromDocument(XmlDocument xmlDoc)
        {
            return new XDocument(xmlDoc);
        }

        #endregion

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            result = XNodeList.Empty;
            if (AllObjects != null)
            {
                var nodes = AllObjects.Where(n => n.LocalName.Equals(binder.Name, StringComparison.OrdinalIgnoreCase));
                
                if (nodes.Any())                
                    result = new XNodeList(nodes);
            }

            return true;
        }

    }
}
