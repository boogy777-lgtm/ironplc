using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000214 RID: 532
	internal abstract class ItemReference_Green : PragmaExpression_Green, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060023B0 RID: 9136 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool HasAttribute(string stAttribute, IScope scope)
		{
			return false;
		}

		// Token: 0x060023B1 RID: 9137 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool HasAttribute(string stAttribute, IPrecompileScope2 scope)
		{
			return false;
		}
	}
}
