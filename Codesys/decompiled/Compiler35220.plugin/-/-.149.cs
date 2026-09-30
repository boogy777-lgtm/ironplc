using System;
using System.Collections.Generic;
using System.Linq;
using \u0013;
using \u0018;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0002
{
	// Token: 0x0200019D RID: 413
	internal sealed class \u0005 : \u0013.\u0001
	{
		// Token: 0x06001DA7 RID: 7591 RVA: 0x0005FE98 File Offset: 0x0005E098
		private \u0005(\u0018.\u0002 \u001B\u0004) : base(\u001B\u0004)
		{
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x0005FEA4 File Offset: 0x0005E0A4
		public static IEnumerable<ISourcePosition> \u0001(IExprement \u0002)
		{
			global::\u0002.\u0005 u = new global::\u0002.\u0005(new \u0018.\u0002());
			(\u0002 as _IExprement).Accept(u);
			return u.UnusedStatementPositions;
		}

		// Token: 0x170005D4 RID: 1492
		// (get) Token: 0x06001DA9 RID: 7593 RVA: 0x0005FED0 File Offset: 0x0005E0D0
		public IEnumerable<ISourcePosition> UnusedStatementPositions
		{
			get
			{
				if (this.\u0001 == null)
				{
					return Enumerable.Empty<ISourcePosition>();
				}
				return this.\u0001;
			}
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x0005FEE8 File Offset: 0x0005E0E8
		public override void visit(_ISequenceStatement seq)
		{
			if (global::\u0002.\u0005.\u0001(seq))
			{
				this.\u0001(seq);
				return;
			}
			foreach (_IStatement istatement in seq._StatementList)
			{
				istatement.Accept(this);
			}
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x0005FF44 File Offset: 0x0005E144
		public override void visit(_IExpressionStatement expstat)
		{
			expstat._Expr.Accept(this);
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x0005FF54 File Offset: 0x0005E154
		private void \u0001(_ISequenceStatement \u0002)
		{
			if (this.\u0001 == null)
			{
				this.\u0001 = new List<ISourcePosition>();
			}
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				if (istatement.Position != null)
				{
					this.\u0001.Add(istatement.Position);
				}
			}
		}

		// Token: 0x06001DAD RID: 7597 RVA: 0x0005FFC8 File Offset: 0x0005E1C8
		private static bool \u0001(_IStatement \u0002)
		{
			return \u0002.GetFlag(StatementFlag.Unused);
		}

		// Token: 0x040004DA RID: 1242
		private new List<ISourcePosition> \u0001;
	}
}
