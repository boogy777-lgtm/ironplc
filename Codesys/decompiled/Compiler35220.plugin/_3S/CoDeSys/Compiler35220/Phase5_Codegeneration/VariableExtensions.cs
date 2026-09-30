using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x0200024E RID: 590
	public static class VariableExtensions
	{
		// Token: 0x060026C4 RID: 9924 RVA: 0x000868C4 File Offset: 0x00084AC4
		public static bool IsSpecialParameter(this IVariable varLeft)
		{
			return varLeft != null && (varLeft.GetFlag(VarFlag.ImplicitParamsStruct) || varLeft.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INPUT));
		}
	}
}
