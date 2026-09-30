using System;
using System.IO;
using \u0013;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x02000091 RID: 145
	internal sealed class \u0004 : \u0002
	{
		// Token: 0x06000C44 RID: 3140 RVA: 0x0001D7B8 File Offset: 0x0001B9B8
		protected \u0004(BinaryWriter \u009C\u0002) : base(\u009C\u0002)
		{
		}

		// Token: 0x06000C45 RID: 3141 RVA: 0x0001D7C4 File Offset: 0x0001B9C4
		public static void \u0001(BinaryWriter \u0002, _IExprement \u0003)
		{
			if (\u0003 == null)
			{
				\u0002.Write(0U);
				return;
			}
			ICompactedParseTreeInformation compactedParseTreeInformation = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder2).CreateCompactedParseTreeInformation();
			PrecompileParseTreeInformationCollector.CollectParseTreeInformation(\u0003, compactedParseTreeInformation);
			\u0004 ivisit = new \u0004(\u0002);
			\u0003.Accept(ivisit);
			\u0013.\u0002.\u0001(\u0002, compactedParseTreeInformation);
		}

		// Token: 0x06000C46 RID: 3142 RVA: 0x0001D814 File Offset: 0x0001BA14
		public static void \u0002(BinaryWriter \u0002, _IExprement \u0003)
		{
			if (\u0003 == null)
			{
				\u0002.Write(0U);
				return;
			}
			ICompactedCompiledParseTreeInformation compactedCompiledParseTreeInformation = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder3).CreateCompactedCompiledParseTreeInformation();
			CompiledParseTreeInformationCollector.CollectParseTreeInformation(\u0003, compactedCompiledParseTreeInformation);
			\u0004 ivisit = new \u0004(\u0002);
			\u0003.Accept(ivisit);
			\u0013.\u0002.\u0001(\u0002, compactedCompiledParseTreeInformation);
		}
	}
}
