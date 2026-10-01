using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x02000112 RID: 274
	internal sealed class \u0005 : EmptyVisitor351900
	{
		// Token: 0x06001432 RID: 5170 RVA: 0x0003B220 File Offset: 0x00039420
		private \u0005(int \u007F\u0008, IScope5 \u009B\u0002)
		{
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u007F\u0008;
		}

		// Token: 0x06001433 RID: 5171 RVA: 0x0003B238 File Offset: 0x00039438
		internal static void \u0001(_IStatement \u0002, int \u0003, IScope5 \u0004)
		{
			StandardTraverser ivisit = new StandardTraverser(new \u0005(\u0003, \u0004));
			\u0002.Accept(ivisit);
		}

		// Token: 0x06001434 RID: 5172 RVA: 0x0003B25C File Offset: 0x0003945C
		private static bool \u0001(AccessFlag \u0002)
		{
			return (\u0002 & AccessFlag.Write) == AccessFlag.Write || (\u0002 & AccessFlag.Call) == AccessFlag.Call;
		}

		// Token: 0x06001435 RID: 5173 RVA: 0x0003B270 File Offset: 0x00039470
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if (!\u0005.\u0001(access))
			{
				return;
			}
			_ISignature isignature = this.\u0001[variable.SignatureId] as _ISignature;
			if (isignature == null)
			{
				return;
			}
			IVariableWithModifyingAccesses variableWithModifyingAccesses = isignature[variable.VariableId] as IVariableWithModifyingAccesses;
			if (variableWithModifyingAccesses == null)
			{
				return;
			}
			variableWithModifyingAccesses.AddModifyingCrossReference(this.\u0001);
		}

		// Token: 0x04000373 RID: 883
		private readonly IScope5 \u0001;

		// Token: 0x04000374 RID: 884
		private readonly int \u0001;
	}
}
