using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000298 RID: 664
	public class InstanceVarReplacer : AbstractReplacer, IReplacer, IExprementReplacer
	{
		// Token: 0x1700074C RID: 1868
		// (get) Token: 0x06002A21 RID: 10785 RVA: 0x000930C0 File Offset: 0x000912C0
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002A22 RID: 10786 RVA: 0x000930C8 File Offset: 0x000912C8
		internal InstanceVarReplacer(global::\u000E.\u0011 context)
		{
			this.Context = context;
			this.\u0001 = \u0081.\u0010.\u0001(this, context);
		}

		// Token: 0x06002A23 RID: 10787 RVA: 0x000930E4 File Offset: 0x000912E4
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			if (this.Context._Scope.MethodSignature == null)
			{
				ISignature localSignature = this.Context._Scope.LocalSignature;
				if (localSignature == null || localSignature.POUType != Operator.FunctionBlock)
				{
					return;
				}
			}
			this.\u0001.ReplaceCode(cpou);
		}

		// Token: 0x06002A24 RID: 10788 RVA: 0x00093138 File Offset: 0x00091338
		public void ReplaceExprement(_IExprement exprement, _ICompiledPOU cpou)
		{
			this.\u0001.\u0001(exprement);
		}

		// Token: 0x06002A25 RID: 10789 RVA: 0x00093148 File Offset: 0x00091348
		public override _IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess)
		{
			_IVariable ivariable = variableExpression.GetVariable(this.Context._Scope) as _IVariable;
			if (ivariable != null && ivariable.GetFlag(VarFlag.RelativeInstance) && !ivariable.GetFlag(VarFlag.ReplacedConstant))
			{
				_IVariableExpression ivariableExpression = variableExpression;
				if (ivariable.GetFlag(VarFlag.AllocateInInstance))
				{
					ISignature signature = this.Context._Scope[variableExpression.SignatureId];
					if (signature != null)
					{
						ivariableExpression = global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitMethodInstVarName352000(this.Context._Scope[signature.ParentSignatureId] as _ISignature, signature, ivariable));
						ivariableExpression.Type = ivariable.CompiledType;
					}
				}
				_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(this.ThisExpressionDeref, Token.Empty);
				icompoAccessExpression._Right = ivariableExpression;
				icompoAccessExpression.Type = ivariableExpression.Type;
				_ICompoAccessExpression icompoAccessExpression2;
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) || ivariable.GetFlag(VarFlag.AllocateInInstance))
				{
					icompoAccessExpression2 = this.Context.Generator.\u0001<_ICompoAccessExpression>(icompoAccessExpression, this.Context._Scope, this.Context.CompiledPOU);
				}
				else
				{
					icompoAccessExpression2 = icompoAccessExpression;
				}
				icompoAccessExpression2._Position = ivariableExpression._Position;
				_IDeRefAccessExpression ideRefAccessExpression = icompoAccessExpression2.Left as _IDeRefAccessExpression;
				if (ideRefAccessExpression != null)
				{
					ideRefAccessExpression._Position = ivariableExpression._Position;
					ideRefAccessExpression._Base._Position = ivariableExpression._Position;
				}
				return icompoAccessExpression2;
			}
			return variableExpression;
		}

		// Token: 0x1700074D RID: 1869
		// (get) Token: 0x06002A26 RID: 10790 RVA: 0x000932BC File Offset: 0x000914BC
		private _IDeRefAccessExpression ThisExpressionDeref
		{
			get
			{
				_IVariableExpression ivariableExpression = this.ThisExpression;
				_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.\u0001(ivariableExpression, Token.Empty);
				_IPointerType ipointerType = ivariableExpression.Type as _IPointerType;
				if (ipointerType != null)
				{
					ideRefAccessExpression.Type = ipointerType.BaseType;
				}
				global::\u0014.\u000F info = new global::\u0014.\u000F
				{
					InstanceAccess = true
				};
				ideRefAccessExpression.Info = info;
				return ideRefAccessExpression;
			}
		}

		// Token: 0x1700074E RID: 1870
		// (get) Token: 0x06002A27 RID: 10791 RVA: 0x0009330C File Offset: 0x0009150C
		private _IVariableExpression ThisExpression
		{
			get
			{
				ISignature[] array;
				_IVariable ivariable = (_IVariable)this.Context._Scope.FindVariable(IdentifierConstants.InstancePointer, out array)[0];
				_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(ivariable.VersionedName);
				ISignature localSignature = this.Context._Scope.LocalSignature;
				if (localSignature != null && (localSignature.POUType == Operator.FunctionBlock || localSignature.GetFlag(SignatureFlag.Structure)))
				{
					_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(localSignature.Name);
					iuserdefType.SignatureId = localSignature.Id;
					_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
					ivariableExpression.Type = type;
					ivariableExpression.VariableId = ivariable.Id;
					ivariableExpression.SignatureId = array[0].Id;
				}
				return ivariableExpression;
			}
		}

		// Token: 0x040007B4 RID: 1972
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x040007B5 RID: 1973
		private readonly \u0081.\u0010 \u0001;
	}
}
