using System;
using System.Reflection;
using System.Xml;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200014E RID: 334
	[TypeGuid("{ad152e46-7c71-4d56-a570-459acdedc2c0}")]
	[StorageVersion("3.3.0.0")]
	public class AuxiliaryInformation : GenericObject2, IAuxiliaryCompileInformation
	{
		// Token: 0x17000763 RID: 1891
		// (get) Token: 0x06001B77 RID: 7031 RVA: 0x0004DDE0 File Offset: 0x0004CDE0
		// (set) Token: 0x06001B78 RID: 7032 RVA: 0x0004DDF0 File Offset: 0x0004CDF0
		[DefaultSerialization("Content")]
		[StorageVersion("3.3.0.0")]
		private string SerializationContent
		{
			get
			{
				return this._xnContent.OuterXml;
			}
			set
			{
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.LoadXml(value);
				}
				catch
				{
					this._xnContent = null;
					return;
				}
				this._xnContent = xmlDocument["auxiliary-data"];
			}
		}

		// Token: 0x06001B79 RID: 7033 RVA: 0x0000AC39 File Offset: 0x00009C39
		public AuxiliaryInformation()
		{
		}

		// Token: 0x06001B7A RID: 7034 RVA: 0x0004DE38 File Offset: 0x0004CE38
		public AuxiliaryInformation(Guid guid, string stType, XmlNode xnContent, Guid guidObject)
		{
			this._guid = guid;
			this._stType = stType;
			this._xnContent = xnContent;
			this._guidObject = guidObject;
		}

		// Token: 0x17000764 RID: 1892
		// (get) Token: 0x06001B7B RID: 7035 RVA: 0x0004DE5D File Offset: 0x0004CE5D
		public Guid ObjectGuid
		{
			get
			{
				return this._guidObject;
			}
		}

		// Token: 0x17000765 RID: 1893
		// (get) Token: 0x06001B7C RID: 7036 RVA: 0x0004DE65 File Offset: 0x0004CE65
		public Guid Id
		{
			get
			{
				return this._guid;
			}
		}

		// Token: 0x17000766 RID: 1894
		// (get) Token: 0x06001B7D RID: 7037 RVA: 0x0004DE6D File Offset: 0x0004CE6D
		public string Type
		{
			get
			{
				return this._stType;
			}
		}

		// Token: 0x17000767 RID: 1895
		// (get) Token: 0x06001B7E RID: 7038 RVA: 0x0004DE75 File Offset: 0x0004CE75
		public XmlNode Content
		{
			get
			{
				return this._xnContent;
			}
		}

		// Token: 0x040005C6 RID: 1478
		[DefaultSerialization("guid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid _guid;

		// Token: 0x040005C7 RID: 1479
		[DefaultSerialization("objectguid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid _guidObject;

		// Token: 0x040005C8 RID: 1480
		[DefaultSerialization("type")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stType;

		// Token: 0x040005C9 RID: 1481
		[Obfuscation(Feature = "rename")]
		private XmlNode _xnContent;
	}
}
