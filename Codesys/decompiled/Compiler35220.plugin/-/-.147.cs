using System;
using System.Collections;
using System.Runtime.CompilerServices;
using \u0003;
using \u0013;
using \u0015;
using \u0018;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0006
{
	// Token: 0x0200019B RID: 411
	internal sealed class \u0003 : global::\u0013.\u0001
	{
		// Token: 0x170005D0 RID: 1488
		// (get) Token: 0x06001D99 RID: 7577 RVA: 0x0005FC18 File Offset: 0x0005DE18
		private Hashtable Defines { get; }

		// Token: 0x170005D1 RID: 1489
		// (get) Token: 0x06001D9A RID: 7578 RVA: 0x0005FC20 File Offset: 0x0005DE20
		private \u0015.\u0002 Scope { get; }

		// Token: 0x170005D2 RID: 1490
		// (get) Token: 0x06001D9B RID: 7579 RVA: 0x0005FC28 File Offset: 0x0005DE28
		private _IPreCompileContext PreCompileContext { get; }

		// Token: 0x170005D3 RID: 1491
		// (get) Token: 0x06001D9D RID: 7581 RVA: 0x0005FC3C File Offset: 0x0005DE3C
		// (set) Token: 0x06001D9C RID: 7580 RVA: 0x0005FC30 File Offset: 0x0005DE30
		public bool HasUnusedStatements { get; private set; }

		// Token: 0x06001D9E RID: 7582 RVA: 0x0005FC44 File Offset: 0x0005DE44
		private \u0003(\u0018.\u0002 \u001B\u0004, Hashtable \u007F\u0005, _IPreCompileContext \u0080\u0005, \u0015.\u0002 \u009B\u0002) : base(\u001B\u0004)
		{
			this.Defines = \u007F\u0005;
			this.Scope = \u009B\u0002;
			this.PreCompileContext = \u0080\u0005;
		}

		// Token: 0x06001D9F RID: 7583 RVA: 0x0005FC64 File Offset: 0x0005DE64
		public static bool \u0001(_IExprement \u0002, Hashtable \u0003, _IPreCompileContext \u0004, \u0015.\u0002 \u0005)
		{
			bool result;
			try
			{
				global::\u0006.\u0003 u = new global::\u0006.\u0003(new \u0018.\u0002(), \u0003, \u0004, \u0005);
				\u0002.Accept(u);
				result = u.HasUnusedStatements;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06001DA0 RID: 7584 RVA: 0x0005FCA8 File Offset: 0x0005DEA8
		public override void visit(_IDefineStatement defstate)
		{
			if (defstate.Define)
			{
				this.Defines[defstate.Ident] = defstate.Value;
				return;
			}
			if (this.Defines.ContainsKey(defstate.Ident))
			{
				this.Defines.Remove(defstate.Ident);
			}
		}

		// Token: 0x06001DA1 RID: 7585 RVA: 0x0005FCFC File Offset: 0x0005DEFC
		public override void visit(_IPragmaIfStatement pifst)
		{
			_IPragmaExpression ipragmaExpression = pifst.Condition as _IPragmaExpression;
			if (ipragmaExpression == null)
			{
				return;
			}
			if (this.\u0001(ipragmaExpression))
			{
				this.\u0002(pifst);
				return;
			}
			this.\u0001(pifst);
		}

		// Token: 0x06001DA2 RID: 7586 RVA: 0x0005FD34 File Offset: 0x0005DF34
		private void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001(\u0002.IfThen);
			bool flag = false;
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				if (this.\u0001(ipragmaElseIf.Condition as _IPragmaExpression) && !flag)
				{
					ipragmaElseIf.Controlled.Accept(this);
					flag = true;
				}
				else
				{
					this.\u0001(ipragmaElseIf.Controlled);
				}
			}
			if (\u0002.IfElse != null)
			{
				if (flag)
				{
					this.\u0001(\u0002.IfElse);
					return;
				}
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x06001DA3 RID: 7587 RVA: 0x0005FDE0 File Offset: 0x0005DFE0
		private void \u0002(_IPragmaIfStatement \u0002)
		{
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				this.\u0001(ipragmaElseIf.Controlled);
			}
			if (\u0002.IfElse != null)
			{
				this.\u0001(\u0002.IfElse);
			}
		}

		// Token: 0x06001DA4 RID: 7588 RVA: 0x0005FE54 File Offset: 0x0005E054
		private void \u0001(_IStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.Unused, true);
			this.HasUnusedStatements = true;
		}

		// Token: 0x06001DA5 RID: 7589 RVA: 0x0005FE68 File Offset: 0x0005E068
		private bool \u0001(_IPragmaExpression \u0002)
		{
			if (\u0002 == null)
			{
				return false;
			}
			global::\u0003.\u0007.\u0001(\u0002, this.Defines, this.PreCompileContext, this.Scope);
			return \u0002.Value;
		}

		// Token: 0x040004D6 RID: 1238
		[CompilerGenerated]
		private new readonly Hashtable \u0001;

		// Token: 0x040004D7 RID: 1239
		[CompilerGenerated]
		private new readonly \u0015.\u0002 \u0001;

		// Token: 0x040004D8 RID: 1240
		[CompilerGenerated]
		private new readonly _IPreCompileContext \u0001;

		// Token: 0x040004D9 RID: 1241
		[CompilerGenerated]
		private new bool \u0001;
	}
}
