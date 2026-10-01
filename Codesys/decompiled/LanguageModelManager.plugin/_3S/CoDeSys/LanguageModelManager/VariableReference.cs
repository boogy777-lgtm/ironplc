using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200007A RID: 122
	[TypeGuid("{9c119240-503d-47c6-b5ce-e3aecee0e1f1}")]
	[StorageVersion("3.3.0.0")]
	public class VariableReference : ItemReference, _IVariableReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IVariableReference
	{
		// Token: 0x060007F3 RID: 2035 RVA: 0x0000E359 File Offset: 0x0000D359
		public VariableReference()
		{
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x0000E361 File Offset: 0x0000D361
		public VariableReference(IToken token) : base(token)
		{
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x00013E52 File Offset: 0x00012E52
		public VariableReference(IToken token, _IExpression expPath) : base(token)
		{
			this.m_expPath = expPath;
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060007F6 RID: 2038 RVA: 0x00013E62 File Offset: 0x00012E62
		// (set) Token: 0x060007F7 RID: 2039 RVA: 0x00013E6A File Offset: 0x00012E6A
		public _IExpression InstancePath
		{
			get
			{
				return this.m_expPath;
			}
			set
			{
				this.m_expPath = value;
			}
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060007F8 RID: 2040 RVA: 0x00013E62 File Offset: 0x00012E62
		public IExpression Instance
		{
			get
			{
				return this.m_expPath;
			}
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x00013E73 File Offset: 0x00012E73
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x00013E7C File Offset: 0x00012E7C
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x00013E85 File Offset: 0x00012E85
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x00013E8E File Offset: 0x00012E8E
		public override IVariable GetVariable(IScope scope)
		{
			return this.InstancePath.GetVariable(scope);
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x00013E9C File Offset: 0x00012E9C
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			return this.InstancePath.GetVariable(scope);
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x00013EAC File Offset: 0x00012EAC
		public override bool HasAttribute(string stAttribute, IScope scope)
		{
			IVariable variable = this.GetVariable(scope);
			return variable != null && variable.HasAttribute(stAttribute);
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x00013ED0 File Offset: 0x00012ED0
		public override bool HasAttribute(string stAttribute, IPrecompileScope2 scope)
		{
			IVariable variable = this.GetVariable(scope);
			return variable != null && variable.HasAttribute(stAttribute);
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x00013EF4 File Offset: 0x00012EF4
		public override _IExprement Duplicate()
		{
			VariableReference variableReference = new VariableReference();
			if (this.m_expPath != null)
			{
				variableReference.m_expPath = (this.m_expPath.Duplicate() as Expression);
			}
			this.DuplicateCommon(variableReference);
			return variableReference;
		}

		// Token: 0x0400010C RID: 268
		[DefaultSerialization("Path")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expPath;
	}
}
