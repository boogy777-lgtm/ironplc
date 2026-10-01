using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000274 RID: 628
	public class CheckRangeInserter
	{
		// Token: 0x060027ED RID: 10221 RVA: 0x0008AFFC File Offset: 0x000891FC
		public CheckRangeInserter(CheckFunctionReplacerContext checkFunctionReplacerContext)
		{
			this.\u0001 = checkFunctionReplacerContext;
		}

		// Token: 0x060027EE RID: 10222 RVA: 0x0008B00C File Offset: 0x0008920C
		public AssignmentInfo ReplaceAssignment(AssignmentInfo assignmentExpression)
		{
			if (assignmentExpression.LValue.Type.Class != TypeClass.Subrange || assignmentExpression.RValue.Literal(this.\u0001._Scope) != null)
			{
				return assignmentExpression;
			}
			TypeClass @class = assignmentExpression.LValue.Type.DeRefType.Class;
			bool flag = TypeTable.IsLInteger(@class, this.\u0001._Scope);
			bool bSigned = TypeTable.IsSigned(@class);
			string checkRangeFunction = this.\u0001.CheckFunctions.GetCheckRangeFunction(flag, bSigned);
			if (checkRangeFunction == null || !this.\u0001.DoChecks())
			{
				return assignmentExpression;
			}
			_ISubrangeType isubrangeType = (_ISubrangeType)assignmentExpression.LValue.Type;
			_IExpression rvalue = assignmentExpression.RValue;
			long num;
			long num2;
			if (this.\u0001(isubrangeType, out num, out num2, flag) && this.\u0001.CheckFunctions.CheckForCheckFunHide(this.\u0001._Scope, checkRangeFunction))
			{
				string str = "TO_";
				_IType @base = isubrangeType._Base;
				string arg = str + ((@base != null) ? @base.ToString() : null);
				string text = string.Format("{0}({1})", arg, rvalue);
				string text2 = string.Format("{0}({1})", arg, num);
				string text3 = string.Format("{0}({1})", arg, num2);
				ISignature signature = this.\u0001.CheckFunctions.GetSignature(checkRangeFunction);
				string stInput = string.Format("{0}({1} := {2}, {3} := {4}, {5} := {6})", new object[]
				{
					checkRangeFunction,
					signature.AllInputs[0].OrgName,
					text,
					signature.AllInputs[1].OrgName,
					text2,
					signature.AllInputs[2].OrgName,
					text3
				});
				_IExpression iexpression = this.\u0001.Generator.GenerateExpression(stInput, this.\u0001._Scope, this.\u0001.CompiledPOU);
				iexpression.SetPositionIntern(rvalue._Position);
				OptionalInputsProvider.\u0001(this.\u0001.\u0001, iexpression);
				return new AssignmentInfo(assignmentExpression.LValue, iexpression, assignmentExpression.KindOf);
			}
			return assignmentExpression;
		}

		// Token: 0x060027EF RID: 10223 RVA: 0x0008B210 File Offset: 0x00089410
		private bool \u0001(_ISubrangeType \u0002, out long \u0003, out long \u0004, bool \u0005)
		{
			bool flag;
			bool flag2;
			if (\u0005)
			{
				\u0003 = ((ILiteralValue2)\u0002.LowerBorder.Literal(this.\u0001._Scope)).GetAnyLong(out flag);
				\u0004 = ((ILiteralValue2)\u0002.UpperBorder.Literal(this.\u0001._Scope)).GetAnyLong(out flag2);
			}
			else
			{
				\u0003 = (long)\u0002.LowerBorder.Literal(this.\u0001._Scope).GetInt(out flag);
				\u0004 = (long)\u0002.UpperBorder.Literal(this.\u0001._Scope).GetInt(out flag2);
			}
			return flag && flag2;
		}

		// Token: 0x04000763 RID: 1891
		private CheckFunctionReplacerContext \u0001;
	}
}
