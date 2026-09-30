using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200011E RID: 286
	[TypeGuid("{8648D553-8DB9-44cb-A943-71AA102DAB5D}")]
	public class FeatureGroupProvider : IFeatureGroupProvider
	{
		// Token: 0x170005D5 RID: 1493
		// (get) Token: 0x06001784 RID: 6020 RVA: 0x00041B71 File Offset: 0x00040B71
		public string Id
		{
			get
			{
				return "language-model";
			}
		}

		// Token: 0x170005D6 RID: 1494
		// (get) Token: 0x06001785 RID: 6021 RVA: 0x00041B78 File Offset: 0x00040B78
		public string Name
		{
			get
			{
				return Strings.FeatureGroup_Name;
			}
		}

		// Token: 0x170005D7 RID: 1495
		// (get) Token: 0x06001786 RID: 6022 RVA: 0x00041B7F File Offset: 0x00040B7F
		public string Description
		{
			get
			{
				return Strings.FeatureGroup_Description;
			}
		}

		// Token: 0x170005D8 RID: 1496
		// (get) Token: 0x06001787 RID: 6023 RVA: 0x00041B86 File Offset: 0x00040B86
		internal static Guid TypeGuid
		{
			get
			{
				return ((TypeGuidAttribute)typeof(FeatureGroupProvider).GetCustomAttributes(typeof(TypeGuidAttribute), false)[0]).Guid;
			}
		}
	}
}
