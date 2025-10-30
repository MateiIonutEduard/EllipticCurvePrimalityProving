using System;
using Eduard;
using System.Xml;

namespace Elliptic_Curve_Primality_Proving
{
    class Certificate
    {
        private XmlDocument xmlDoc;
        private XmlNodeList nodes;

        private XmlNode rootNode;
        private int index;

        public Certificate()
        {
            xmlDoc = new XmlDocument();
            rootNode = xmlDoc.CreateElement("curves");
            xmlDoc.AppendChild(rootNode);
        }

        public Certificate(string path)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            nodes = xmlDoc.DocumentElement.SelectNodes("/curves/curve");
            index = 0;
        }

        public BigInteger[] Read()
        {
            if (index == nodes.Count) return null;
            BigInteger[] args;

            XmlNode node = nodes.Item(index);
            string atrib = node.Attributes[0].Value;
            BigInteger order = new BigInteger(atrib);

            string str = node.SelectSingleNode("a").InnerText;
            BigInteger a = new BigInteger(str);

            str = node.SelectSingleNode("b").InnerText;
            BigInteger b = new BigInteger(str);

            str = node.SelectSingleNode("field").InnerText;
            BigInteger field = new BigInteger(str);

            str = node.SelectSingleNode("factor").InnerText;
            BigInteger factor = new BigInteger(str);

            XmlNode point = node.SelectSingleNode("point");
            str = point.SelectSingleNode("x").InnerText;
            BigInteger x = new BigInteger(str);

            str = point.SelectSingleNode("y").InnerText;
            BigInteger y = new BigInteger(str);

            args = new BigInteger[7] { a, b, field, order, factor, x, y };
            index++;

            return args;
        }

        public void Write(params BigInteger[] args)
        {
            XmlNode userNode = xmlDoc.CreateElement("curve");
            XmlAttribute attribute = xmlDoc.CreateAttribute("order");
            attribute.Value = args[3].ToString();
            userNode.Attributes.Append(attribute);

            XmlNode node = xmlDoc.CreateElement("a");
            node.InnerText = args[0].ToString();
            userNode.AppendChild(node);

            node = xmlDoc.CreateElement("b");
            node.InnerText = args[1].ToString();
            userNode.AppendChild(node);

            node = xmlDoc.CreateElement("field");
            node.InnerText = args[2].ToString();
            userNode.AppendChild(node);

            node = xmlDoc.CreateElement("factor");
            node.InnerText = args[4].ToString();
            userNode.AppendChild(node);

            XmlNode point = xmlDoc.CreateElement("point");
            node = xmlDoc.CreateElement("x");
            node.InnerText = args[5].ToString();
            point.AppendChild(node);

            node = xmlDoc.CreateElement("y");
            node.InnerText = args[6].ToString();
            point.AppendChild(node);
            userNode.AppendChild(point);
            rootNode.AppendChild(userNode);
        }

        public void Save(string path) => xmlDoc.Save(path);

        public bool IsEmpty() => rootNode.ChildNodes.Count == 0;
    }
}
