using System;
using \u0002;
using \u0004;
using \u000E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.CompilerPhases;

namespace \u001E
{
	// Token: 0x020003EF RID: 1007
	internal static class \u001A
	{
		// Token: 0x060037F1 RID: 14321 RVA: 0x000E5168 File Offset: 0x000E3368
		public static global::\u000E.\u001B \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005)
		{
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002);
			global::\u000E.\u001B u001B = new global::\u000E.\u001B(\u0002, deviceOfApplication, \u0003, \u0004, \u0005);
			u001B.Precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(\u0002);
			u001B.PrecompPool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			u001B.ComconOld = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContextSynchronLoad(\u0002);
			u001B.ComconNew = null;
			u001B.CheckAll = false;
			u001B.CompilerPhase1_Typifier = new CompilerPhase1_Typifier(u001B);
			u001B.CompilerPhase2_AfterTypification = new CompilerPhase2_AfterTypification(u001B);
			u001B.CompilerPhase3_Locator = new CompilerPhase3_Locator(u001B);
			u001B.CompilerPhase4_Typechecker = null;
			u001B.CompilerPhase5_Codegenerator = new CompilerPhase5_Codegenerator(u001B);
			u001B.CompilerPhase6_AfterCodegeneration = new global::\u0002.\u0014(u001B);
			u001B.CompilerPhaseControllerCompile = new global::\u0004.\u001B(u001B);
			u001B.LMM = APEnvironmentFacade.Instance.LanguageModelMgr;
			u001B.ParentApplicationGuid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(\u0002);
			if (deviceOfApplication != Guid.Empty)
			{
				u001B.ComconDevice = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(deviceOfApplication);
				if (u001B.ComconDevice == null)
				{
					u001B.ComconDevice = APEnvironmentFacade.Instance.LanguageModelMgr[deviceOfApplication];
				}
			}
			if (u001B.ParentApplicationGuid != Guid.Empty)
			{
				u001B.ComconParent = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(u001B.ParentApplicationGuid);
			}
			return u001B;
		}
	}
}
