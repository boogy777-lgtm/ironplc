using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200006D RID: 109
	[TypeGuid("{C16C5B28-DC1E-4EB3-BB57-6C521D6E87E8}")]
	[StorageVersion("3.3.0.0")]
	internal class ProgramCounterExpression : Expression, _IProgramCounterExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x06000704 RID: 1796 RVA: 0x00012594 File Offset: 0x00011594
		internal ProgramCounterExpression()
		{
			this._ctype = new PointerType(TypeTable.Byte);
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x000125AC File Offset: 0x000115AC
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitorAdapter)
			{
				(visitor as IExprementVisitorAdapter).visit(this);
			}
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x000125C2 File Offset: 0x000115C2
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x000125CC File Offset: 0x000115CC
		public override _IExprement Duplicate()
		{
			ProgramCounterExpression programCounterExpression = new ProgramCounterExpression();
			this.DuplicateCommon(programCounterExpression);
			return programCounterExpression;
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return true;
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x000125E7 File Offset: 0x000115E7
		// (set) Token: 0x0600070B RID: 1803 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this._ctype;
			}
			set
			{
			}
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x0600070C RID: 1804 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600070D RID: 1805 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x0600070E RID: 1806 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x0600070F RID: 1807 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x040000EE RID: 238
		private readonly ICompiledType _ctype;
	}
}
