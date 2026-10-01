using System;
using \u000E;
using \u000F;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000271 RID: 625
	public class CheckFunctionReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x060027D7 RID: 10199 RVA: 0x0008A848 File Offset: 0x00088A48
		internal CheckFunctionReplacer(global::\u000E.\u0011 context)
		{
			this.\u0001 = new CheckFunctionReplacerContext(context, this);
			this.\u0001 = \u0081.\u0010.\u0001(this, context);
			this.\u0001 = new CheckRangeInserter(this.\u0001);
			this.\u0001 = new CheckDivInserter(this.\u0001);
			this.\u0001 = new CheckBoundsInserter(this.\u0001);
		}

		// Token: 0x060027D8 RID: 10200 RVA: 0x0008A8A8 File Offset: 0x00088AA8
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001.CompiledPOU = cpou;
			if (this.\u0001.CheckFunctions.\u0001(this.\u0001.Comcon, cpou, false, this.\u0001._Scope))
			{
				this.\u0001.ReplaceCode(cpou);
			}
		}

		// Token: 0x060027D9 RID: 10201 RVA: 0x0008A8F8 File Offset: 0x00088AF8
		public override _IExpression ReplaceOperatorExpression(_IOperatorExpression operatorExpression)
		{
			return this.\u0001.ReplaceDivision(operatorExpression);
		}

		// Token: 0x060027DA RID: 10202 RVA: 0x0008A908 File Offset: 0x00088B08
		public override _IExpression ReplaceAssignmentExpression(_IAssignmentExpression assignmentExpression)
		{
			AssignmentInfo assignmentInfo = this.\u0001.ReplaceAssignment(new AssignmentInfo(assignmentExpression));
			assignmentExpression._LValue = assignmentInfo.LValue;
			assignmentExpression._RValue = assignmentInfo.RValue;
			return assignmentExpression;
		}

		// Token: 0x060027DB RID: 10203 RVA: 0x0008A944 File Offset: 0x00088B44
		public override _IExpression ReplaceIndexAccessExpression(_IIndexAccessExpression indexAccessExpression, bool bReadAccess)
		{
			return this.\u0001.ReplaceIndexAccess(indexAccessExpression);
		}

		// Token: 0x060027DC RID: 10204 RVA: 0x0008A954 File Offset: 0x00088B54
		public override _IExpression ReplaceCallExpression(_ICallExpression callExpression)
		{
			CallParameterEnumerable callParameterEnumerable = callExpression.\u0001();
			int num = 0;
			foreach (AssignmentInfo assignmentExpression in callParameterEnumerable)
			{
				AssignmentInfo assignmentInfo = this.\u0001.ReplaceAssignment(assignmentExpression);
				callExpression.SetActualParam(assignmentInfo.RValue, num);
				callExpression.SetFormalParam(assignmentInfo.LValue, num);
				num++;
			}
			return callExpression;
		}

		// Token: 0x060027DD RID: 10205 RVA: 0x0008A9B4 File Offset: 0x00088BB4
		public bool DoChecks()
		{
			return this.\u0001.CheckFunctions.\u0001(this.\u0001.Comcon, this.\u0001.CompiledPOU, this.\u0001.\u0001, this.\u0001._Scope);
		}

		// Token: 0x0400075C RID: 1884
		private CheckFunctionReplacerContext \u0001;

		// Token: 0x0400075D RID: 1885
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x0400075E RID: 1886
		private readonly CheckDivInserter \u0001;

		// Token: 0x0400075F RID: 1887
		private readonly CheckRangeInserter \u0001;

		// Token: 0x04000760 RID: 1888
		private readonly CheckBoundsInserter \u0001;
	}
}
