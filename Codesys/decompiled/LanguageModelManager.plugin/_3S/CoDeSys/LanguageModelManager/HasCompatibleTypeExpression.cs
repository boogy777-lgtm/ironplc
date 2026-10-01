using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000056 RID: 86
	[TypeGuid("{6EB9E8CB-2EAF-45a5-A531-FF52EE08333B}")]
	[StorageVersion("3.3.0.0")]
	public class HasCompatibleTypeExpression : HasTypeExpression, _IHasCompatibleTypeExpression, _IHasTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasTypeExpression
	{
		// Token: 0x0600051E RID: 1310 RVA: 0x0000EDA1 File Offset: 0x0000DDA1
		public HasCompatibleTypeExpression()
		{
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x0000EDA9 File Offset: 0x0000DDA9
		public HasCompatibleTypeExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x0000EDB2 File Offset: 0x0000DDB2
		public HasCompatibleTypeExpression(IToken token, VariableReference varref, ICompiledType type) : base(token, varref, type)
		{
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000521 RID: 1313 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool Exact
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x0000EDC0 File Offset: 0x0000DDC0
		public void AssignFrom(_IHasTypeExpression exp)
		{
			if (exp.VarRef != null)
			{
				this.m_varref = (exp.VarRef.Duplicate() as VariableReference);
			}
			else
			{
				this.m_varref = null;
			}
			this.m_type = exp.ReferencedType;
			(exp as Expression).DuplicateCommon(this);
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x0000EE0C File Offset: 0x0000DE0C
		public override _IExprement Duplicate()
		{
			HasCompatibleTypeExpression hasCompatibleTypeExpression = new HasCompatibleTypeExpression();
			if (this.m_varref != null)
			{
				hasCompatibleTypeExpression.m_varref = (this.m_varref.Duplicate() as VariableReference);
			}
			hasCompatibleTypeExpression.m_type = ((_IType)this.m_type).Duplicate;
			this.DuplicateCommon(hasCompatibleTypeExpression);
			return hasCompatibleTypeExpression;
		}
	}
}
