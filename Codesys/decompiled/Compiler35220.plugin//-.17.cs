using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x02000341 RID: 833
	internal sealed class \u0016 : EmptyVisitor351900
	{
		// Token: 0x1700082F RID: 2095
		// (get) Token: 0x06003272 RID: 12914 RVA: 0x000C24FC File Offset: 0x000C06FC
		private IScope Scope
		{
			get
			{
				return this.\u0001.Peek().\u0001;
			}
		}

		// Token: 0x17000830 RID: 2096
		// (get) Token: 0x06003273 RID: 12915 RVA: 0x000C2510 File Offset: 0x000C0710
		private IScope DerivedScope
		{
			get
			{
				return this.\u0001.Peek().\u0002;
			}
		}

		// Token: 0x06003274 RID: 12916 RVA: 0x000C2524 File Offset: 0x000C0724
		private void \u0001(IScope \u0002)
		{
			this.\u0001.Peek().\u0002 = \u0002;
		}

		// Token: 0x06003275 RID: 12917 RVA: 0x000C2538 File Offset: 0x000C0738
		private void \u0002(IScope \u0002)
		{
			\u0016.\u0001 item = new \u0016.\u0001
			{
				\u0001 = \u0002,
				\u0002 = null
			};
			this.\u0001.Push(item);
		}

		// Token: 0x06003276 RID: 12918 RVA: 0x000C2568 File Offset: 0x000C0768
		private void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x06003277 RID: 12919 RVA: 0x000C2578 File Offset: 0x000C0778
		private \u0016(IScope \u009B\u0002)
		{
			this.\u0002(\u009B\u0002);
			base.Traverser = new StandardTraverser(this);
		}

		// Token: 0x06003278 RID: 12920 RVA: 0x000C25A0 File Offset: 0x000C07A0
		internal static void \u0001(IScope \u0002, _IExpression \u0003)
		{
			\u0016 u = new \u0016(\u0002);
			\u0003.Accept(u.Traverser);
		}

		// Token: 0x06003279 RID: 12921 RVA: 0x000C25C0 File Offset: 0x000C07C0
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			IVariable[] array;
			ISignature[] array2;
			IScope scope;
			this.Scope.FindDeclaration(variable.Name, out array, out array2, out scope);
			if (this.\u0001 && scope != null)
			{
				_IUserdefType iuserdefType = \u0003.\u0001(scope.Name);
				iuserdefType.ScopeId = scope.Id;
				variable.Type = iuserdefType;
				variable.ScopeId = scope.Id;
				this.\u0001(scope);
				return;
			}
			if (array2 != null && array2.Length == 1)
			{
				ISignature signature = array2[0];
				_IUserdefType iuserdefType2 = \u0003.\u0001(signature.Name);
				iuserdefType2.SignatureId = signature.Id;
				variable.Type = iuserdefType2;
				variable.SignatureId = signature.Id;
			}
		}

		// Token: 0x17000831 RID: 2097
		// (get) Token: 0x0600327A RID: 12922 RVA: 0x000C2664 File Offset: 0x000C0864
		public override bool bResolveCompoAccessExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600327B RID: 12923 RVA: 0x000C2668 File Offset: 0x000C0868
		public override void visit(_ICompoAccessExpression compo, AccessFlag access)
		{
			bool u = this.\u0001;
			this.\u0001 = true;
			IScope u2 = this.Scope;
			this.\u0002(this.Scope);
			_IExpression iexpression = compo.Left as _IExpression;
			if (iexpression != null)
			{
				iexpression.Accept(base.Traverser);
			}
			if (this.DerivedScope != null)
			{
				u2 = this.DerivedScope;
			}
			this.\u0001();
			this.\u0001 = u;
			this.\u0002(u2);
			_IExpression iexpression2 = compo.Right as _IExpression;
			if (iexpression2 != null)
			{
				iexpression2.Accept(base.Traverser);
			}
			if (this.DerivedScope != null)
			{
				u2 = this.DerivedScope;
			}
			this.\u0001();
			this.\u0001(u2);
		}

		// Token: 0x04000975 RID: 2421
		private readonly Stack<\u0016.\u0001> \u0001 = new Stack<\u0016.\u0001>();

		// Token: 0x04000976 RID: 2422
		private bool \u0001;

		// Token: 0x02000342 RID: 834
		private sealed class \u0001
		{
			// Token: 0x04000977 RID: 2423
			public IScope \u0001;

			// Token: 0x04000978 RID: 2424
			public IScope \u0002;
		}
	}
}
