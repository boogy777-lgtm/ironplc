using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000044 RID: 68
	[TypeGuid("{A9D98A90-49C9-4313-BEC9-E8ED40EF7C96}")]
	[StorageVersion("3.3.0.0")]
	internal class CallInstanceExpression : Expression, _ICallInstanceExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICallInstanceExpression
	{
		// Token: 0x0600039B RID: 923 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public CallInstanceExpression()
		{
		}

		// Token: 0x0600039C RID: 924 RVA: 0x0000C3DE File Offset: 0x0000B3DE
		internal CallInstanceExpression(bool bWriteAccess, ICompiledType ctype, IIntermediateValueLocation ivl)
		{
			this.WriteAccess = bWriteAccess;
			this._CompiledType = ctype;
			this.IntermediateValueLocation = ivl;
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600039D RID: 925 RVA: 0x0000C3FB File Offset: 0x0000B3FB
		// (set) Token: 0x0600039E RID: 926 RVA: 0x0000C403 File Offset: 0x0000B403
		public bool WriteAccess { get; set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x0600039F RID: 927 RVA: 0x0000C40C File Offset: 0x0000B40C
		// (set) Token: 0x060003A0 RID: 928 RVA: 0x0000C414 File Offset: 0x0000B414
		public IIntermediateValueLocation IntermediateValueLocation { get; set; }

		// Token: 0x060003A1 RID: 929 RVA: 0x0000C41D File Offset: 0x0000B41D
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitorAdapter)
			{
				(visitor as IExprementVisitorAdapter).visit(this);
			}
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x0000C433 File Offset: 0x0000B433
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x0000C43C File Offset: 0x0000B43C
		public override _IExprement Duplicate()
		{
			CallInstanceExpression callInstanceExpression = new CallInstanceExpression(this.WriteAccess, this._CompiledType, this.IntermediateValueLocation);
			this.DuplicateCommon(callInstanceExpression);
			return callInstanceExpression;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return true;
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x060003A6 RID: 934 RVA: 0x0000C469 File Offset: 0x0000B469
		// (set) Token: 0x060003A7 RID: 935 RVA: 0x0000C471 File Offset: 0x0000B471
		public override ICompiledType _CompiledType { get; set; }

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x060003A8 RID: 936 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x060003A9 RID: 937 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x060003AA RID: 938 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060003AB RID: 939 RVA: 0x00003AE9 File Offset: 0x00002AE9
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
	}
}
