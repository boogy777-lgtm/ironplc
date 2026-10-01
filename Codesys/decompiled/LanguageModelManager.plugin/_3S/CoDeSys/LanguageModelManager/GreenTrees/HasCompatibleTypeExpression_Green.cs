using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000223 RID: 547
	internal class HasCompatibleTypeExpression_Green : HasTypeExpression_Green, _IHasCompatibleTypeExpression, _IHasTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasTypeExpression
	{
		// Token: 0x06002414 RID: 9236 RVA: 0x0005C019 File Offset: 0x0005B019
		public HasCompatibleTypeExpression_Green(_IVariableReference varref, ICompiledType type) : base(varref, type)
		{
		}

		// Token: 0x17000A2E RID: 2606
		// (get) Token: 0x06002415 RID: 9237 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool Exact
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002416 RID: 9238 RVA: 0x0005C023 File Offset: 0x0005B023
		public void AssignFrom(_IHasTypeExpression exp)
		{
			if (exp.VarRef != null)
			{
				this.m_varref = (exp.VarRef.Duplicate() as _IVariableReference);
			}
			else
			{
				this.m_varref = null;
			}
			this.m_type = exp.ReferencedType;
		}
	}
}
