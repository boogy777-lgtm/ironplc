using System;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000287 RID: 647
	public class LateCodeGeneratorAfterInterfaceAndReferenceReplacement : LateCodeGenerator
	{
		// Token: 0x060028CC RID: 10444 RVA: 0x0008ED88 File Offset: 0x0008CF88
		public LateCodeGeneratorAfterInterfaceAndReferenceReplacement(_ICompileContext comcon) : base(comcon)
		{
		}

		// Token: 0x060028CD RID: 10445 RVA: 0x0008ED94 File Offset: 0x0008CF94
		internal override void \u0001(ExpressionTypifierWithSpecialTasks \u0002, TypeCheckerVisitor \u0003)
		{
			\u0002.InterfaceAsInterface = true;
			\u0002.TreatReferenceAsPointer = true;
			\u0003.InterfaceAsInterface = true;
			\u0003.TreatReferenceAsPointer = true;
		}
	}
}
