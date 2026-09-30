using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000062 RID: 98
	[TypeGuid("{60e07f4f-23d9-424e-a27b-bf5e093fd92a}")]
	[StorageVersion("3.3.0.0")]
	public class MultipleIndexInitialisation : PositionExpression, _IMultipleIndexInitialization, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IMultipleIndexInitialization
	{
		// Token: 0x060005FC RID: 1532 RVA: 0x0000B408 File Offset: 0x0000A408
		public MultipleIndexInitialisation()
		{
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x0000B9FB File Offset: 0x0000A9FB
		internal MultipleIndexInitialisation(IToken token) : base(token)
		{
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x0001036A File Offset: 0x0000F36A
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x00010373 File Offset: 0x0000F373
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000602 RID: 1538 RVA: 0x0001037C File Offset: 0x0000F37C
		// (set) Token: 0x06000603 RID: 1539 RVA: 0x00010392 File Offset: 0x0000F392
		public _IExpression _Number
		{
			get
			{
				if (this.m_expNumber == null)
				{
					return new NullExpression();
				}
				return this.m_expNumber;
			}
			set
			{
				this.m_expNumber = value;
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000604 RID: 1540 RVA: 0x0001039B File Offset: 0x0000F39B
		// (set) Token: 0x06000605 RID: 1541 RVA: 0x000103B1 File Offset: 0x0000F3B1
		public _IExpression _Value
		{
			get
			{
				if (this.m_expValue == null)
				{
					return new NullExpression();
				}
				return this.m_expValue;
			}
			set
			{
				this.m_expValue = value;
			}
		}

		// Token: 0x06000606 RID: 1542 RVA: 0x000103BC File Offset: 0x0000F3BC
		public override _IExprement Duplicate()
		{
			MultipleIndexInitialisation multipleIndexInitialisation = new MultipleIndexInitialisation();
			this.DuplicateCommon(multipleIndexInitialisation);
			if (this.m_expValue != null)
			{
				multipleIndexInitialisation.m_expValue = (this.m_expValue.Duplicate() as _IExpression);
			}
			if (this.m_expNumber != null)
			{
				multipleIndexInitialisation.m_expNumber = (this.m_expNumber.Duplicate() as _IExpression);
			}
			return multipleIndexInitialisation;
		}

		// Token: 0x06000607 RID: 1543 RVA: 0x00010413 File Offset: 0x0000F413
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Value.IsConstant(scope, bAllocatedOK) && this._Number.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000608 RID: 1544 RVA: 0x00010433 File Offset: 0x0000F433
		// (set) Token: 0x06000609 RID: 1545 RVA: 0x0001043B File Offset: 0x0000F43B
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x0600060A RID: 1546 RVA: 0x00010444 File Offset: 0x0000F444
		public IExpression Number
		{
			get
			{
				return this._Number;
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x0600060B RID: 1547 RVA: 0x0001044C File Offset: 0x0000F44C
		public IExpression Value
		{
			get
			{
				return this._Value;
			}
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00010454 File Offset: 0x0000F454
		public int NumberInt(out bool bValid, IScope scope)
		{
			int result = TypeHelper.GetInt(this.m_expNumber, scope, out bValid);
			if (!bValid)
			{
				result = -1;
			}
			return result;
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x00010478 File Offset: 0x0000F478
		public int NumberInt(out bool bValid, IPrecompileScope scope)
		{
			int result = TypeHelper.GetInt(this.m_expNumber, scope, out bValid);
			if (!bValid)
			{
				result = -1;
			}
			return result;
		}

		// Token: 0x040000CF RID: 207
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000D0 RID: 208
		[DefaultSerialization("Value")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expValue;

		// Token: 0x040000D1 RID: 209
		[DefaultSerialization("Number")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expNumber;
	}
}
