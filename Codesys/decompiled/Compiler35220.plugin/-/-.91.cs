using System;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x0200011B RID: 283
	internal sealed class \u0004 : global::\u000F.\u0006
	{
		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x0600148C RID: 5260 RVA: 0x0003CAD0 File Offset: 0x0003ACD0
		internal static global::\u000F.\u0006 Instance { get; } = new global::\u0003.\u0004();

		// Token: 0x0600148D RID: 5261 RVA: 0x0003CAD8 File Offset: 0x0003ACD8
		private \u0004()
		{
		}

		// Token: 0x0600148E RID: 5262 RVA: 0x0003CAE0 File Offset: 0x0003ACE0
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002.CompileContext.ApplicationGuid);
			Guid parentApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(\u0002.CompileContext.ApplicationGuid);
			if (parentApplication != Guid.Empty && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(parentApplication) != null && !(APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(parentApplication) as _ICompileContext).IsUpToDate() && \u0002.Strategy.ParentContextChanged())
			{
				return \u0002.Strategy.IsUpToDate;
			}
			if (parentApplication != Guid.Empty && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(parentApplication) == null)
			{
				return !\u0002.Strategy.ParentContextNull() || \u0002.Strategy.IsUpToDate;
			}
			return global::\u0003.\u0004.\u0001(\u0002, deviceOfApplication, parentApplication);
		}

		// Token: 0x0600148F RID: 5263 RVA: 0x0003CBC4 File Offset: 0x0003ADC4
		private static bool \u0001(global::\u0014.\u0003 \u0002, Guid \u0003, Guid \u0004)
		{
			uint num = APEnvironmentFacade.Instance.LanguageModelMgr.MemorySettingsHelper.GetMemorySettings(\u0003, \u0004, \u0002.CompileContext.ApplicationGuid, \u0002.CompileContext.SimulationMode).CalculateChecksum();
			return \u0002.CompileContext.MemorySettingsChecksum == 0U || \u0002.CompileContext.MemorySettingsChecksum == num || !\u0002.Strategy.MemorySettingsChanged() || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x0400038E RID: 910
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
