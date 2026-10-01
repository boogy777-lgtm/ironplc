using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000247 RID: 583
	public class CreatorService : ILMCreatorService3, ILMCreatorService2, ILMCreatorService
	{
		// Token: 0x17000AF5 RID: 2805
		// (get) Token: 0x060026FB RID: 9979 RVA: 0x00061E70 File Offset: 0x00060E70
		public ITypeInfo TypeInfo
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo;
			}
		}

		// Token: 0x060026FC RID: 9980 RVA: 0x00061E81 File Offset: 0x00060E81
		public ILanguageModelBuilder CreateLanguageModelBuilder()
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder();
		}

		// Token: 0x060026FD RID: 9981 RVA: 0x00061E94 File Offset: 0x00060E94
		public IParser CreateParser(string stText)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stText, false, false, false, false);
			return APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner);
		}

		// Token: 0x060026FE RID: 9982 RVA: 0x00061EC6 File Offset: 0x00060EC6
		public IParser CreateParser(IScanner scanner)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner);
		}

		// Token: 0x060026FF RID: 9983 RVA: 0x00061ED8 File Offset: 0x00060ED8
		public IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stText, bIncludeComments, bIncludeEndOfLines, bIncludePragmas, bIncludeWhitespaces);
		}

		// Token: 0x06002700 RID: 9984 RVA: 0x00061EF0 File Offset: 0x00060EF0
		public IRawSTParser CreateRawSTParser(string stText, bool bImplicit)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stText, true, false, true, false, true);
			((IScanner9)scanner).AutoIncrementPositionOnLineBreaks = true;
			scanner.AllowMultipleUnderlines = bImplicit;
			return CompilerProxy.CreateParser(scanner, bImplicit) as IRawSTParser;
		}

		// Token: 0x06002701 RID: 9985 RVA: 0x00061F25 File Offset: 0x00060F25
		public IRawSTParser CreateRawSTParser(char[] stText, bool bImplicit)
		{
			IScanner9 scanner = (IScanner9)APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", true, false, true, false, true);
			scanner.Initialize(stText);
			scanner.AutoIncrementPositionOnLineBreaks = true;
			scanner.AllowMultipleUnderlines = bImplicit;
			return CompilerProxy.CreateParser(scanner, bImplicit) as IRawSTParser;
		}

		// Token: 0x06002702 RID: 9986 RVA: 0x00061F65 File Offset: 0x00060F65
		public IRawSTParser CreateRawSTParser(string stText, bool bImplicit, Version version, ILMCompileOptions3 compileOptions)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stText, version, true, false, true, false, true);
			((IScanner9)scanner).AutoIncrementPositionOnLineBreaks = true;
			scanner.AllowMultipleUnderlines = bImplicit;
			return CompilerProxy.CreateParser(scanner, bImplicit, version, compileOptions) as IRawSTParser;
		}

		// Token: 0x06002703 RID: 9987 RVA: 0x00061FA0 File Offset: 0x00060FA0
		public IRawSTParser CreateRawSTParser(char[] stText, bool bImplicit, Version version, ILMCompileOptions3 compileOptions)
		{
			IScanner9 scanner = (IScanner9)APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", version, true, false, true, false, true);
			scanner.Initialize(stText);
			scanner.AutoIncrementPositionOnLineBreaks = true;
			scanner.AllowMultipleUnderlines = bImplicit;
			return CompilerProxy.CreateParser(scanner, bImplicit, version, compileOptions) as IRawSTParser;
		}
	}
}
