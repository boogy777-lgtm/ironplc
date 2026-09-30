using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200005F RID: 95
	[TypeGuid("{F1B29078-6F12-465f-8121-D4FDFF1F04FA}")]
	[StorageVersion("3.3.1.0")]
	public class IsEnumTypeExpression : PragmaExpression, _IIsEnumTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IIsEnumTypeExpression
	{
		// Token: 0x060005B9 RID: 1465 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public IsEnumTypeExpression()
		{
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public IsEnumTypeExpression(IToken token) : base(token)
		{
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x0000FE5F File Offset: 0x0000EE5F
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x0000FE68 File Offset: 0x0000EE68
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x0000FE74 File Offset: 0x0000EE74
		public override _IExprement Duplicate()
		{
			IsEnumTypeExpression isEnumTypeExpression = new IsEnumTypeExpression();
			isEnumTypeExpression.m_type = this.m_type;
			this.DuplicateCommon(isEnumTypeExpression);
			return isEnumTypeExpression;
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060005C0 RID: 1472 RVA: 0x0000FE9B File Offset: 0x0000EE9B
		// (set) Token: 0x060005C1 RID: 1473 RVA: 0x0000FEA3 File Offset: 0x0000EEA3
		public ICompiledType ReferencedType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		// Token: 0x040000CB RID: 203
		[DefaultSerialization("ReferenceType")]
		[StorageVersion("3.3.1.0")]
		protected ICompiledType m_type;
	}
}
