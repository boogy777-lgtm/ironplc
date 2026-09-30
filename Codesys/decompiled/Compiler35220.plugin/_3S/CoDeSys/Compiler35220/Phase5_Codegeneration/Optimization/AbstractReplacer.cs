using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000250 RID: 592
	[ExcludeFromCodeCoverage]
	public abstract class AbstractReplacer : ISpecificExpressionReplacer
	{
		// Token: 0x060026C6 RID: 9926 RVA: 0x000868F0 File Offset: 0x00084AF0
		public virtual _IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess)
		{
			return variableExpression;
		}

		// Token: 0x060026C7 RID: 9927 RVA: 0x000868F4 File Offset: 0x00084AF4
		public virtual _IExpression ReplaceIndexAccessExpression(_IIndexAccessExpression indexAccessExpression, bool bReadAccess)
		{
			return indexAccessExpression;
		}

		// Token: 0x060026C8 RID: 9928 RVA: 0x000868F8 File Offset: 0x00084AF8
		public virtual _IExpression ReplaceDeRefAccessExpression(_IDeRefAccessExpression deRefAccessExpression, bool bReadAccess)
		{
			return deRefAccessExpression;
		}

		// Token: 0x060026C9 RID: 9929 RVA: 0x000868FC File Offset: 0x00084AFC
		public virtual _IExpression ReplaceCompoAccessExpression(_ICompoAccessExpression compoAccessExpression, bool bReadAccess)
		{
			return compoAccessExpression;
		}

		// Token: 0x060026CA RID: 9930 RVA: 0x00086900 File Offset: 0x00084B00
		public virtual _IExpression ReplacePartialAccessExpression(_IPartialAccessExpression partialAccessExpression, bool bReadAccess)
		{
			return partialAccessExpression;
		}

		// Token: 0x060026CB RID: 9931 RVA: 0x00086904 File Offset: 0x00084B04
		public virtual _IExpression ReplaceAssignmentExpression(_IAssignmentExpression assignmentExpression)
		{
			return assignmentExpression;
		}

		// Token: 0x060026CC RID: 9932 RVA: 0x00086908 File Offset: 0x00084B08
		public virtual _IExpression ReplaceOperatorExpression(_IOperatorExpression operatorExpression)
		{
			return operatorExpression;
		}

		// Token: 0x060026CD RID: 9933 RVA: 0x0008690C File Offset: 0x00084B0C
		public virtual _IExpression ReplaceConversionExpression(_IConversionExpression conversionExpression)
		{
			return conversionExpression;
		}

		// Token: 0x060026CE RID: 9934 RVA: 0x00086910 File Offset: 0x00084B10
		public virtual _IExpression ReplaceThisExpression(_IThisExpression thisExpression)
		{
			return thisExpression;
		}

		// Token: 0x060026CF RID: 9935 RVA: 0x00086914 File Offset: 0x00084B14
		public virtual _IExpression ReplaceBaseExpression(_IBaseExpression baseExpression)
		{
			return baseExpression;
		}

		// Token: 0x060026D0 RID: 9936 RVA: 0x00086918 File Offset: 0x00084B18
		public virtual _IExpression ReplaceCurrentTaskExpression(_ICurrentTaskExpression currentTaskExpression)
		{
			return currentTaskExpression;
		}

		// Token: 0x060026D1 RID: 9937 RVA: 0x0008691C File Offset: 0x00084B1C
		public virtual _IExpression ReplaceCallExpression(_ICallExpression callExpression)
		{
			return callExpression;
		}
	}
}
