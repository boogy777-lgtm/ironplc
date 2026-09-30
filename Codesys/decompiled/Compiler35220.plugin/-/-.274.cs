using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000E
{
	// Token: 0x020002DA RID: 730
	internal sealed class \u0014
	{
		// Token: 0x17000794 RID: 1940
		// (get) Token: 0x06002BE8 RID: 11240 RVA: 0x00099F58 File Offset: 0x00098158
		// (set) Token: 0x06002BE9 RID: 11241 RVA: 0x00099F60 File Offset: 0x00098160
		private IScope5 Scope { get; set; }

		// Token: 0x06002BEA RID: 11242 RVA: 0x00099F6C File Offset: 0x0009816C
		internal \u0014(IScope5 \u009B\u0002)
		{
			this.Scope = \u009B\u0002;
		}

		// Token: 0x06002BEB RID: 11243 RVA: 0x00099F7C File Offset: 0x0009817C
		private ILiteralValue \u0001(_IHasConstantValueExpression2 \u0002)
		{
			ILiteralValue result = null;
			if (\u0002.Constant != null)
			{
				result = ((_IExpression)\u0002.Constant).Literal(this.Scope, true);
			}
			return result;
		}

		// Token: 0x06002BEC RID: 11244 RVA: 0x00099FAC File Offset: 0x000981AC
		private ILiteralValue \u0002(_IHasConstantValueExpression2 \u0002)
		{
			ILiteralValue literalValue = null;
			if (\u0002.ConstantValue != null)
			{
				literalValue = \u0002.ConstantValue.LiteralValue;
			}
			else if (\u0002._ConstantValue != null)
			{
				bool flag;
				literalValue = \u0002._ConstantValue.LiteralWithRecursionCheck(this.Scope, new Dictionary<IVariable, IVariable>(), true, out flag);
				if (literalValue == null)
				{
					IVariable variable = \u0002._ConstantValue.GetVariable(this.Scope);
					if (variable != null && variable.Initial != null)
					{
						ILiteralExpression literalExpression = variable.Initial as ILiteralExpression;
						if (literalExpression != null)
						{
							literalValue = literalExpression.LiteralValue;
						}
					}
				}
			}
			return literalValue;
		}

		// Token: 0x06002BED RID: 11245 RVA: 0x0009A02C File Offset: 0x0009822C
		public bool \u0001(_IHasConstantValueExpression \u0002, out _IHasConstantValueExpression2 \u0003, out bool \u0004)
		{
			bool result = false;
			\u0003 = (_IHasConstantValueExpression2)\u0002;
			ILiteralValue literalValue = this.\u0001(\u0003);
			ILiteralValue literalValue2 = this.\u0002(\u0003);
			\u0004 = false;
			if (literalValue != null && literalValue2 != null)
			{
				\u0004 = ((literalValue2.KindOf == literalValue.KindOf) ? PragmaEvaluationHelper.MatchesWhereKindOfMatch(\u0003, literalValue, literalValue2) : PragmaEvaluationHelper.MatchesWhereKindOfDontMatch(\u0003, literalValue, literalValue2));
			}
			if ((\u0003.Constant != null && literalValue == null) || literalValue2 == null)
			{
				result = true;
			}
			return result;
		}

		// Token: 0x04000858 RID: 2136
		[CompilerGenerated]
		private IScope5 \u0001;
	}
}
