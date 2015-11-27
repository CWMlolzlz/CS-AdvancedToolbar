using System;
using System.IO;
using System.Xml;

namespace AdvancedToolbar{
	public class XmlUtil{

		public static void SaveFile(XmlDocument doc, string path){
			doc.Save(path);
			//File.WriteAllText(path, doc.OuterXml);
		}

		public static XmlElement CreateElementNode(XmlDocument doc, string name, object innerText){
			XmlElement node = doc.CreateElement(name);
			node.InnerText = innerText.ToString();
			return node;
		}

		public static int GetElementNodeInt(XmlNode parentNode, string name){
			return GetElementNodeInt(parentNode,name,0);
		}

		public static int GetElementNodeInt(XmlNode parentNode, string name, int defaultValue){
			XmlNode node = parentNode.SelectSingleNode(name);
			if(node == null)
				return defaultValue;
			try{
				return int.Parse(node.InnerText);
			}catch{
				return defaultValue;
			}
		}

		public static bool GetElementNodeBoolean(XmlNode parentNode, string name){
			return GetElementNodeBoolean(parentNode, name, false);
		}

		public static bool GetElementNodeBoolean(XmlNode parentNode, string name, bool defaultValue){
			XmlNode node = parentNode.SelectSingleNode(name);
			if(node == null)
				return defaultValue;
			try{
				return bool.Parse(node.InnerText);
			}catch{
				return defaultValue;
			}
		}
	}
}

