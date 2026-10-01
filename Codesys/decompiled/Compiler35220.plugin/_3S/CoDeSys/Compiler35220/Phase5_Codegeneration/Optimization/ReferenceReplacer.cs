using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002C2 RID: 706
	public class ReferenceReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x17000775 RID: 1909
		// (get) Token: 0x06002B04 RID: 11012 RVA: 0x00097A04 File Offset: 0x00095C04
		private \u0081.\u0010 ReplacerVisitor { get; }

		// Token: 0x17000776 RID: 1910
		// (get) Token: 0x06002B05 RID: 11013 RVA: 0x00097A0C File Offset: 0x00095C0C
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002B06 RID: 11014 RVA: 0x00097A14 File Offset: 0x00095C14
		internal ReferenceReplacer(global::\u000E.\u0011 context)
		{
			this.Context = context;
			this.ReplacerVisitor = \u0081.\u0010.\u0002(this, this.Context);
		}

		// Token: 0x06002B07 RID: 11015 RVA: 0x00097A38 File Offset: 0x00095C38
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.ReplacerVisitor.ReplaceCode(cpou);
		}

		// Token: 0x06002B08 RID: 11016 RVA: 0x00097A48 File Offset: 0x00095C48
		public override _IExpression ReplaceCallExpression(_ICallExpression callExpression)
		{
			if (callExpression.Type != null && callExpression.Type.Class == TypeClass.Reference)
			{
				_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.Builder.CreateImplicitDeRefAccessExpression(callExpression);
				_IPointerType ipointerType = global::\u0019.\u0003.\u0001(((_IReferenceType)callExpression.Type).DeRefType as _IType);
				callExpression.Type = ipointerType;
				ideRefAccessExpression.Type = ipointerType.BaseType;
				ideRefAccessExpression._Position = callExpression._Position;
				return ideRefAccessExpression;
			}
			return callExpression;
		}

		// Token: 0x06002B09 RID: 11017 RVA: 0x00097AB4 File Offset: 0x00095CB4
		public override _IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess)
		{
			_IVariable ivariable = variableExpression.GetVariable(this.Context._Scope) as _IVariable;
			if (ivariable != null)
			{
				_IReferenceType ireferenceType = ivariable.Type as _IReferenceType;
				if (ireferenceType != null && (variableExpression.Type == null || variableExpression.Type.Class == TypeClass.Reference))
				{
					_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.Builder.CreateImplicitDeRefAccessExpression(variableExpression);
					_IPointerType ipointerType = global::\u0019.\u0003.\u0001(ireferenceType.DeRefType as _IType);
					variableExpression.Type = ipointerType;
					ideRefAccessExpression.Type = ipointerType.BaseType;
					ideRefAccessExpression._Position = variableExpression._Position;
					return ideRefAccessExpression;
				}
			}
			return variableExpression;
		}

		// Token: 0x06002B0A RID: 11018 RVA: 0x00097B44 File Offset: 0x00095D44
		public override _IExpression ReplaceCompoAccessExpression(_ICompoAccessExpression compoAccessExpression, bool bReadAccess)
		{
			_IVariable ivariable = compoAccessExpression._Right.GetVariable(this.Context._Scope) as _IVariable;
			if (ivariable != null)
			{
				_IReferenceType ireferenceType = ivariable.Type as _IReferenceType;
				if (ireferenceType != null && (compoAccessExpression.Type == null || compoAccessExpression.Type.Class == TypeClass.Reference))
				{
					_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.Builder.CreateImplicitDeRefAccessExpression(compoAccessExpression);
					_IPointerType ipointerType = global::\u0019.\u0003.\u0001(ireferenceType.DeRefType as _IType);
					ideRefAccessExpression.Type = ipointerType.BaseType;
					ideRefAccessExpression._Position = compoAccessExpression._Position;
					return ideRefAccessExpression;
				}
			}
			return compoAccessExpression;
		}

		// Token: 0x04000826 RID: 2086
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000827 RID: 2087
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
