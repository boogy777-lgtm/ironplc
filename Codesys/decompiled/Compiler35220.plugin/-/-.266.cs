using System;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001B
{
	// Token: 0x020002CC RID: 716
	internal sealed class \u000E : AbstractToVisitchecker
	{
		// Token: 0x06002B4D RID: 11085 RVA: 0x000986A8 File Offset: 0x000968A8
		public override bool ToVisit(_IStructureInitialization structureInitialization)
		{
			return true;
		}

		// Token: 0x06002B4E RID: 11086 RVA: 0x000986AC File Offset: 0x000968AC
		public override bool ToVisit(_IArrayInitialization arrayInitialization)
		{
			return true;
		}

		// Token: 0x06002B4F RID: 11087 RVA: 0x000986B0 File Offset: 0x000968B0
		public override bool ToVisit(_IOperatorExpression op)
		{
			return op.Code == Operator.__Init;
		}
	}
}
