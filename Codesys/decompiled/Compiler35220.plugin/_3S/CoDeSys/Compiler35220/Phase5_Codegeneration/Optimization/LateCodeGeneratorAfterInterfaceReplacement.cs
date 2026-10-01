using System;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000286 RID: 646
	public class LateCodeGeneratorAfterInterfaceReplacement : LateCodeGenerator
	{
		// Token: 0x060028CA RID: 10442 RVA: 0x0008ED6C File Offset: 0x0008CF6C
		public LateCodeGeneratorAfterInterfaceReplacement(_ICompileContext comcon) : base(comcon)
		{
		}

		// Token: 0x060028CB RID: 10443 RVA: 0x0008ED78 File Offset: 0x0008CF78
		internal override void \u0001(ExpressionTypifierWithSpecialTasks \u0002, TypeCheckerVisitor \u0003)
		{
			\u0002.InterfaceAsInterface = true;
			\u0003.InterfaceAsInterface = true;
		}
	}
}
