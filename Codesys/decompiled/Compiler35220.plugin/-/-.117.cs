using System;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0014
{
	// Token: 0x0200015F RID: 351
	internal static class \u0006
	{
		// Token: 0x06001839 RID: 6201 RVA: 0x0004BBC0 File Offset: 0x00049DC0
		internal static IInternalParser \u0001(IScanner9 \u0002, IErrorHandler \u0003)
		{
			return APEnvironmentFacade.Instance.ParserService.CreateParser(\u0002, \u0003, APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder7, TypeTableClass.Singleton, APEnvironmentFacade.Instance.CompileOptions, APEnvironmentFacade.Instance.CompilerVersionSettings);
		}

		// Token: 0x0600183A RID: 6202 RVA: 0x0004BC00 File Offset: 0x00049E00
		internal static IInternalParser \u0001(IScanner9 \u0002, IErrorHandler \u0003, Version \u0004, ILMCompileOptions3 \u0005)
		{
			return APEnvironmentFacade.Instance.GetParserService(\u0004).CreateParser(\u0002, \u0003, APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder7, TypeTableClass.Singleton, \u0005, new \u000E(\u0004));
		}
	}
}
