using System;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x0200009F RID: 159
	internal sealed class \u0003 : ILMCallTreeService
	{
		// Token: 0x06000D09 RID: 3337 RVA: 0x000223D0 File Offset: 0x000205D0
		public IStackUsage \u0001(Guid \u0002, bool \u0003, string \u0004)
		{
			return this.\u0001(\u0002, \u0003, 0U, \u0004);
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x000223DC File Offset: 0x000205DC
		public IStackUsage \u0001(Guid \u0002, bool \u0003, string \u0004, uint \u0005)
		{
			return this.\u0001(\u0002, \u0003, \u0005, \u0004);
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x000223EC File Offset: 0x000205EC
		private IStackUsage \u0001(Guid \u0002, bool \u0003, uint \u0004, string \u0005)
		{
			bool u = false;
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetCompiledApplicationSet(\u0002) as _ICompileContext;
			if (icompileContext == null)
			{
				_ILanguageModelManagerConsolidated2 ilanguageModelManagerConsolidated = APEnvironmentFacade.Instance.LanguageModelMgr as _ILanguageModelManagerConsolidated2;
				if (ilanguageModelManagerConsolidated != null)
				{
					icompileContext = ilanguageModelManagerConsolidated.GetCompileContextWithStackOverflow(\u0002);
					u = (icompileContext != null);
				}
			}
			if (icompileContext == null)
			{
				return null;
			}
			int u2;
			int num;
			CallTree.TryGetMaxStackSize(icompileContext, out u2, out num);
			CallStack maxStackUsage = new CallTree(icompileContext, false, int.MaxValue, num).GetMaxStackUsage(\u0003, \u0005);
			if (maxStackUsage == null)
			{
				return null;
			}
			if (\u0004 == 0U)
			{
				maxStackUsage.MaxStackSize = u2;
			}
			else
			{
				maxStackUsage.MaxStackSize = (int)\u0004;
			}
			maxStackUsage.MaxStackSizeForExternalCalls = num;
			maxStackUsage.Scope = icompileContext.CreateGlobalIScope();
			maxStackUsage.StackOverflow = u;
			return maxStackUsage;
		}
	}
}
