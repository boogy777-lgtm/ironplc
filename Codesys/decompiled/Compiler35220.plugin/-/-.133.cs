using System;
using System.IO;
using System.Xml;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0016
{
	// Token: 0x0200017E RID: 382
	internal abstract class \u0008
	{
		// Token: 0x06001A16 RID: 6678 RVA: 0x0005370C File Offset: 0x0005190C
		internal static string \u0001(Guid \u0002, Guid \u0003, int \u0004)
		{
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlTextWriter.WriteStartElement("language-model");
			xmlTextWriter.WriteAttributeString("application-id", \u0002.ToString());
			xmlTextWriter.WriteAttributeString("plclogic-id", \u0003.ToString());
			xmlTextWriter.WriteStartElement("global-interface");
			xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(\u0008.\u0001));
			xmlTextWriter.WriteAttributeString("name", \u0008.\u0001);
			string value = \u0008.\u0003 + string.Format("VAR_GLOBAL\r\n\t__ByteArray : ARRAY [0..{0}] OF BYTE;\r\n\t__MemMan : CMM.LinearMemoryManager(ADR(__ByteArray), {0});\r\nEND_VAR\r\n", \u0004);
			xmlTextWriter.WriteElementString("interface", value);
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Close();
			return stringWriter.ToString();
		}

		// Token: 0x04000481 RID: 1153
		internal static readonly string \u0001 = "__MemManDefinition";

		// Token: 0x04000482 RID: 1154
		private const string \u0002 = "VAR_GLOBAL\r\n\t__ByteArray : ARRAY [0..{0}] OF BYTE;\r\n\t__MemMan : CMM.LinearMemoryManager(ADR(__ByteArray), {0});\r\nEND_VAR\r\n";

		// Token: 0x04000483 RID: 1155
		private static readonly string \u0003 = string.Format("{{attribute 'hide'}}\r\n{{implicit on}}\r\n{{attribute '{0}' := '{1}'}}\r\n", CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT, 49985);

		// Token: 0x04000484 RID: 1156
		private static readonly Guid \u0001 = new Guid("{90B26D49-28A9-4468-B69A-5D38FF55C2D2}");
	}
}
