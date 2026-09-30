using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000243 RID: 579
	internal class TaskLMService : ITaskLMService
	{
		// Token: 0x17000AEF RID: 2799
		// (get) Token: 0x0600267C RID: 9852 RVA: 0x00060431 File Offset: 0x0005F431
		public string GeneratedTaskConfigStruct
		{
			get
			{
				return "_Implicit_Task_Config_Variables";
			}
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x00060438 File Offset: 0x0005F438
		public string GetTaskMemberName(string stTaskName)
		{
			string text = "__" + stTaskName;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 16, 30))
			{
				string text2 = text;
				IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(text2, false, false, false, false);
				scanner.AllowMultipleUnderlines = true;
				IToken token;
				if (TokenType.Identifier != scanner.GetNext(out token))
				{
					text2 += "__tco";
				}
				return text2;
			}
			return text;
		}
	}
}
