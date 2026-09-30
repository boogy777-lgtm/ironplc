using System;
using System.IO;
using System.Xml;
using \u0005;

namespace \u0007
{
	// Token: 0x0200017A RID: 378
	internal sealed class \u0007
	{
		// Token: 0x060019B9 RID: 6585 RVA: 0x000503B4 File Offset: 0x0004E5B4
		public \u0007(string \u0083\u0003)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(\u0083\u0003));
			\u0003 u = null;
			while (xmlTextReader.Read())
			{
				XmlNodeType nodeType = xmlTextReader.NodeType;
				if (nodeType != XmlNodeType.Element)
				{
					if (nodeType - XmlNodeType.Text > 1)
					{
						if (nodeType == XmlNodeType.EndElement)
						{
							u = u.Parent;
						}
					}
					else if (u != null)
					{
						u.Text = xmlTextReader.ReadString();
						if (xmlTextReader.NodeType == XmlNodeType.EndElement)
						{
							u = u.Parent;
						}
					}
				}
				else
				{
					\u0003 u2 = new \u0003();
					u2.Name = xmlTextReader.Name;
					bool isEmptyElement = xmlTextReader.IsEmptyElement;
					for (int i = 0; i < xmlTextReader.AttributeCount; i++)
					{
						string attribute = xmlTextReader.GetAttribute(i);
						xmlTextReader.MoveToAttribute(i);
						string name = xmlTextReader.Name;
						u2.\u0001(name, attribute);
					}
					if (this.\u0001 == null)
					{
						this.\u0001 = u2;
					}
					if (u != null)
					{
						u.\u0001(u2);
					}
					if (!isEmptyElement)
					{
						u = u2;
					}
				}
			}
		}

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x060019BA RID: 6586 RVA: 0x000504A0 File Offset: 0x0004E6A0
		public \u0003 Result
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x04000475 RID: 1141
		private \u0003 \u0001;
	}
}
