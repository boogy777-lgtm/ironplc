using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200011F RID: 287
	[TypeGuid("{5DFBB9BA-2218-4d43-B13A-571E363A75C5}")]
	public class SupportExtendedProgrammingFeatureSettingProvider : IFeatureSettingProvider
	{
		// Token: 0x170005D9 RID: 1497
		// (get) Token: 0x06001789 RID: 6025 RVA: 0x00041BAE File Offset: 0x00040BAE
		public string Id
		{
			get
			{
				return "support-extended-programming-features";
			}
		}

		// Token: 0x170005DA RID: 1498
		// (get) Token: 0x0600178A RID: 6026 RVA: 0x00041BB5 File Offset: 0x00040BB5
		public Guid GroupProvider
		{
			get
			{
				return FeatureGroupProvider.TypeGuid;
			}
		}

		// Token: 0x170005DB RID: 1499
		// (get) Token: 0x0600178B RID: 6027 RVA: 0x00041BBC File Offset: 0x00040BBC
		public string Name
		{
			get
			{
				return Strings.SupportExtendedProgrammingFeatureSettingProvider_Name;
			}
		}

		// Token: 0x170005DC RID: 1500
		// (get) Token: 0x0600178C RID: 6028 RVA: 0x00041BC3 File Offset: 0x00040BC3
		public string Description
		{
			get
			{
				return Strings.SupportExtendedProgrammingFeatureSettingProvider_Description;
			}
		}

		// Token: 0x170005DD RID: 1501
		// (get) Token: 0x0600178D RID: 6029 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool DefaultValue
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005DE RID: 1502
		// (get) Token: 0x0600178E RID: 6030 RVA: 0x00004E6B File Offset: 0x00003E6B
		public FeatureSettingAccess Access
		{
			get
			{
				return FeatureSettingAccess.Editable;
			}
		}

		// Token: 0x0600178F RID: 6031 RVA: 0x00005F12 File Offset: 0x00004F12
		public bool HookBeforeSet(bool bValue)
		{
			return bValue;
		}

		// Token: 0x06001790 RID: 6032 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void HookAfterSet(bool bValue)
		{
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x00005F12 File Offset: 0x00004F12
		public bool HookGet(bool bValue)
		{
			return bValue;
		}
	}
}
