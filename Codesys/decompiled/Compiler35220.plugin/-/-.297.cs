using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001D
{
	// Token: 0x02000319 RID: 793
	internal sealed class \u000E : EmptyVisitor351900
	{
		// Token: 0x06002F9B RID: 12187 RVA: 0x000B3680 File Offset: 0x000B1880
		public \u000E(IScope \u009B\u0002, ISignature \u0080\u0004)
		{
			if (\u009B\u0002 == null)
			{
				throw new ArgumentNullException("scope");
			}
			if (\u0080\u0004 == null)
			{
				throw new ArgumentNullException("localSign");
			}
			this.Scope = \u009B\u0002;
			this.LocalSign = \u0080\u0004;
		}

		// Token: 0x170007EB RID: 2027
		// (get) Token: 0x06002F9C RID: 12188 RVA: 0x000B36B4 File Offset: 0x000B18B4
		private ISignature LocalSign { get; }

		// Token: 0x170007EC RID: 2028
		// (get) Token: 0x06002F9D RID: 12189 RVA: 0x000B36BC File Offset: 0x000B18BC
		private IScope Scope { get; }

		// Token: 0x06002F9E RID: 12190 RVA: 0x000B36C4 File Offset: 0x000B18C4
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if (variable.GetVariable(this.Scope) == null)
			{
				return;
			}
			if (this.Scope[variable.SignatureId] == this.LocalSign)
			{
				if (this.\u0001 == null)
				{
					this.\u0001 = new List<_IVariableExpression>();
				}
				this.\u0001.Add(variable);
			}
		}

		// Token: 0x06002F9F RID: 12191 RVA: 0x000B3718 File Offset: 0x000B1918
		public IEnumerable<_IVariableExpression> \u0001(_IExpression \u0002)
		{
			StandardTraverser ivisit = new StandardTraverser(this);
			\u0002.Accept(ivisit);
			IEnumerable<_IVariableExpression> u = this.\u0001;
			return u ?? Enumerable.Empty<_IVariableExpression>();
		}

		// Token: 0x06002FA0 RID: 12192 RVA: 0x000B3744 File Offset: 0x000B1944
		public static IEnumerable<_IVariableExpression> \u0001(_IExpression \u0002, IScope \u0003, ISignature \u0004)
		{
			return new \u000E(\u0003, \u0004).\u0001(\u0002);
		}

		// Token: 0x04000916 RID: 2326
		[CompilerGenerated]
		private readonly ISignature \u0001;

		// Token: 0x04000917 RID: 2327
		[CompilerGenerated]
		private readonly IScope \u0001;

		// Token: 0x04000918 RID: 2328
		private List<_IVariableExpression> \u0001;
	}
}
