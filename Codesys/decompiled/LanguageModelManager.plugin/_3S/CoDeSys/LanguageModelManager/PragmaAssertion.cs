using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200009B RID: 155
	[TypeGuid("{97fdf1fe-ac11-4e18-847c-5cb4230005bc}")]
	[StorageVersion("3.3.0.0")]
	public class PragmaAssertion : PositionStatement, _IPragmaAssertion, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaAssertion
	{
		// Token: 0x06000969 RID: 2409 RVA: 0x00015F8C File Offset: 0x00014F8C
		public PragmaAssertion()
		{
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x00015F9F File Offset: 0x00014F9F
		public PragmaAssertion(_IExpression expCondition, string stErrorOutput, IToken token) : base(token)
		{
			this.m_expCondition = expCondition;
			this.m_stErrorOutput = stErrorOutput;
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x00015FC1 File Offset: 0x00014FC1
		public PragmaAssertion(_IExpression expCondition, string stErrorOutput)
		{
			this.m_expCondition = expCondition;
			this.m_stErrorOutput = stErrorOutput;
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x0600096C RID: 2412 RVA: 0x00015FE2 File Offset: 0x00014FE2
		public _IExpression Condition
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return new NullExpression();
				}
				return this.m_expCondition;
			}
		}

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x0600096D RID: 2413 RVA: 0x00015FF8 File Offset: 0x00014FF8
		public IExpression ConditionExpression
		{
			get
			{
				return this.Condition;
			}
		}

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x0600096E RID: 2414 RVA: 0x00016000 File Offset: 0x00015000
		public string ErrorOutput
		{
			get
			{
				return this.m_stErrorOutput;
			}
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x00016008 File Offset: 0x00015008
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x00016011 File Offset: 0x00015011
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x0001601A File Offset: 0x0001501A
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x00016024 File Offset: 0x00015024
		public override _IExprement Duplicate()
		{
			PragmaAssertion pragmaAssertion;
			if (this.m_expCondition == null)
			{
				pragmaAssertion = new PragmaAssertion(null, this.m_stErrorOutput);
			}
			else
			{
				pragmaAssertion = new PragmaAssertion(this.m_expCondition.Duplicate() as Expression, this.m_stErrorOutput);
			}
			this.DuplicateCommon(pragmaAssertion);
			return pragmaAssertion;
		}

		// Token: 0x0400014D RID: 333
		[DefaultSerialization("output")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stErrorOutput = string.Empty;

		// Token: 0x0400014E RID: 334
		[DefaultSerialization("condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;
	}
}
