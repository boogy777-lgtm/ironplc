using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x0200015B RID: 347
	public static class OperatorService
	{
		// Token: 0x06001821 RID: 6177 RVA: 0x0004B248 File Offset: 0x00049448
		public static bool IsEqualOp(Operator op)
		{
			return op == Operator.Equal || op == Operator.Eq;
		}

		// Token: 0x06001822 RID: 6178 RVA: 0x0004B260 File Offset: 0x00049460
		public static bool IsNotEqualOp(Operator op)
		{
			return op == Operator.NotEqual || op == Operator.Ne;
		}

		// Token: 0x06001823 RID: 6179 RVA: 0x0004B278 File Offset: 0x00049478
		public static bool IsMinMaxOp(Operator op)
		{
			return op == Operator.Min || op == Operator.Max;
		}

		// Token: 0x06001824 RID: 6180 RVA: 0x0004B288 File Offset: 0x00049488
		public static bool IsSubtractionOp(Operator op)
		{
			return op == Operator.Sub || op == Operator.Minus;
		}

		// Token: 0x06001825 RID: 6181 RVA: 0x0004B29C File Offset: 0x0004949C
		public static bool IsAdditionOp(Operator op)
		{
			return op == Operator.Plus || op == Operator.Add;
		}
	}
}
