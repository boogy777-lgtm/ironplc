using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x020001FB RID: 507
	internal sealed class \u0010 : EmptyVisitor351900
	{
		// Token: 0x060021FA RID: 8698 RVA: 0x00075BAC File Offset: 0x00073DAC
		public bool \u0001(_IExprement \u0002)
		{
			this.\u0001 = false;
			this.\u0001.Reset(this);
			\u0002.Accept(this.\u0001);
			return this.\u0001;
		}

		// Token: 0x17000671 RID: 1649
		// (get) Token: 0x060021FB RID: 8699 RVA: 0x00075BD4 File Offset: 0x00073DD4
		public override bool DoCallExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000672 RID: 1650
		// (get) Token: 0x060021FC RID: 8700 RVA: 0x00075BD8 File Offset: 0x00073DD8
		public override bool DoAssignExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060021FD RID: 8701 RVA: 0x00075BDC File Offset: 0x00073DDC
		internal static bool \u0001(Operator \u0002)
		{
			return \u0002 - Operator.__vcAdd <= 12;
		}

		// Token: 0x060021FE RID: 8702 RVA: 0x00075BEC File Offset: 0x00073DEC
		public override void visit(_IOperatorExpression op)
		{
			if (\u0010.\u0001(op.Code))
			{
				this.\u0001 = true;
				this.\u0001.Abort = true;
			}
		}

		// Token: 0x060021FF RID: 8703 RVA: 0x00075C10 File Offset: 0x00073E10
		public bool \u0001(_IIfStatement \u0002)
		{
			if (this.\u0001(\u0002._Condition))
			{
				return true;
			}
			for (int i = 0; i < \u0002._ElseIf.Count; i++)
			{
				if (this.\u0001(\u0002._ElseIf[i]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040005EB RID: 1515
		private readonly IStandardTraverser \u0001 = new StandardTraverser();

		// Token: 0x040005EC RID: 1516
		private bool \u0001;
	}
}
