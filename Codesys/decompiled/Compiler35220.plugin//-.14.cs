using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x02000280 RID: 640
	internal sealed class \u0013 : EmptyVisitor351900
	{
		// Token: 0x06002891 RID: 10385 RVA: 0x0008E2E4 File Offset: 0x0008C4E4
		internal static bool \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			if (\u0002 == null)
			{
				return false;
			}
			\u0013 u = new \u0013();
			u.\u0001 = \u0003;
			IStandardTraverser ivisit = new StandardTraverser(u);
			\u0002.Accept(ivisit);
			return u.\u0001;
		}

		// Token: 0x06002892 RID: 10386 RVA: 0x0008E318 File Offset: 0x0008C518
		public override void visit(_ICallExpression call)
		{
			this.\u0001 = true;
		}

		// Token: 0x06002893 RID: 10387 RVA: 0x0008E324 File Offset: 0x0008C524
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			_IVariable ivariable = variable.GetVariable(this.\u0001) as _IVariable;
			if (ivariable != null && (ivariable.IsProperty || (ivariable.Type != null && ivariable.Type.Class == TypeClass.Reference)))
			{
				this.\u0001 = true;
			}
		}

		// Token: 0x06002894 RID: 10388 RVA: 0x0008E36C File Offset: 0x0008C56C
		public override void visit(_IDeRefAccessExpression deref)
		{
			if (!(deref.Base is _IBaseExpression) && !(deref.Base is _IThisExpression) && !(deref._Base is IVariableExpression))
			{
				this.\u0001 = true;
			}
		}

		// Token: 0x06002895 RID: 10389 RVA: 0x0008E39C File Offset: 0x0008C59C
		public override void visit(_IIndexAccessExpression indexaccess)
		{
			using (IEnumerator<_IExpression> enumerator = indexaccess._Accesses.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.IsConstant(this.\u0001, false))
					{
						this.\u0001 = true;
					}
				}
			}
			if (indexaccess._Var.Type != null && indexaccess._Var.Type.Class == TypeClass.Pointer)
			{
				this.\u0001 = true;
			}
			if (indexaccess._Var is _IDeRefAccessExpression)
			{
				this.\u0001 = true;
			}
		}

		// Token: 0x04000772 RID: 1906
		private bool \u0001;

		// Token: 0x04000773 RID: 1907
		private IScope5 \u0001;
	}
}
