using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000273 RID: 627
	public class CheckDivInserter
	{
		// Token: 0x060027E9 RID: 10217 RVA: 0x0008AD80 File Offset: 0x00088F80
		public CheckDivInserter(CheckFunctionReplacerContext checkFunctionCheckFunctionReplacer)
		{
			this.\u0001 = checkFunctionCheckFunctionReplacer;
		}

		// Token: 0x060027EA RID: 10218 RVA: 0x0008AD90 File Offset: 0x00088F90
		private string \u0001(_IExpression \u0002, out string \u0003)
		{
			\u0003 = null;
			if (\u0002.Type.DeRefType.Class == TypeClass.Real)
			{
				return this.\u0001.CheckFunctions.m_stCheckDivRealFun;
			}
			if (\u0002.Type.DeRefType.Class == TypeClass.LReal)
			{
				return this.\u0001.CheckFunctions.m_stCheckDivReal64Fun;
			}
			if (\u0002.Type.DeRefType.Size(this.\u0001._Scope) <= 4)
			{
				if (\u0002.Type.DeRefType.Class != TypeClass.DInt)
				{
					\u0003 = \u0002.Type.DeRefType.ToString();
					if (\u0002.Type.DeRefType.Class == TypeClass.Pointer)
					{
						\u0003 = TypeTable.DWord.ToString();
					}
				}
				return this.\u0001.CheckFunctions.m_stCheckDivInt32Fun;
			}
			if (\u0002.Type.DeRefType.Size(this.\u0001._Scope) == 8)
			{
				CheckDivInserter.\u0001(\u0002, ref \u0003);
				return this.\u0001.CheckFunctions.m_stCheckDivInt64Fun;
			}
			return null;
		}

		// Token: 0x060027EB RID: 10219 RVA: 0x0008AE98 File Offset: 0x00089098
		[ExcludeFromCodeCoverage]
		private static void \u0001(_IExpression \u0002, ref string \u0003)
		{
			if (\u0002.Type.DeRefType.Class != TypeClass.LInt)
			{
				\u0003 = \u0002.Type.DeRefType.ToString();
				if (\u0002.Type.DeRefType.Class == TypeClass.Pointer)
				{
					\u0003 = TypeTable.LWord.ToString();
				}
			}
		}

		// Token: 0x060027EC RID: 10220 RVA: 0x0008AEEC File Offset: 0x000890EC
		public _IExpression ReplaceDivision(_IOperatorExpression operatorExpression)
		{
			if (!this.\u0001.DoChecks())
			{
				return operatorExpression;
			}
			if (operatorExpression.Code != Operator.Div && operatorExpression.Code != Operator.Divide)
			{
				return operatorExpression;
			}
			_IExpression iexpression = operatorExpression[1];
			string text = null;
			if (iexpression.Literal(this.\u0001._Scope) != null)
			{
				return operatorExpression;
			}
			string text3;
			string text2 = this.\u0001(iexpression, out text3);
			if (text2 != null && this.\u0001.CheckFunctions.CheckForCheckFunHide(this.\u0001._Scope, text2))
			{
				text = string.Format("{0}({1})", text2, iexpression);
				if (text3 != null)
				{
					text = string.Concat(new string[]
					{
						"TO_",
						text3,
						"(",
						text,
						")"
					});
				}
			}
			if (text != null)
			{
				_IExpression iexpression2 = this.\u0001.Generator.GenerateExpression(text, this.\u0001._Scope, this.\u0001.CompiledPOU);
				iexpression2.SetPositionIntern(operatorExpression._Position);
				OptionalInputsProvider.\u0001(this.\u0001.\u0001, iexpression2);
				operatorExpression[1] = iexpression2;
			}
			return operatorExpression;
		}

		// Token: 0x04000762 RID: 1890
		private CheckFunctionReplacerContext \u0001;
	}
}
