using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000202 RID: 514
	internal abstract class LiteralExpression_Green : Expression_Green, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x060022FE RID: 8958 RVA: 0x0005B788 File Offset: 0x0005A788
		public bool LiteralValueEquals(_ILiteralExpression other)
		{
			bool flag = true;
			if (this.ConstantType != other.ConstantType)
			{
				switch (other.ConstantType)
				{
				case TypeClass.AnyInt:
					flag = TypeTable.IsInteger(this.ConstantType);
					break;
				case TypeClass.AnyNum:
					flag = TypeTable.IsNumber(this.ConstantType);
					break;
				case TypeClass.AnyReal:
					flag = TypeTable.IsReal(this.ConstantType);
					break;
				default:
					flag = (this.ConstantType == other.ConstantType);
					break;
				}
			}
			if (flag)
			{
				flag = (this.LongValue == other.LongValue && this.ULongValue == other.ULongValue && this.StringValue == other.StringValue && this.RealValue == other.RealValue);
			}
			return flag;
		}

		// Token: 0x170009C8 RID: 2504
		// (get) Token: 0x060022FF RID: 8959
		// (set) Token: 0x06002300 RID: 8960
		public abstract long LongValue { get; set; }

		// Token: 0x170009C9 RID: 2505
		// (get) Token: 0x06002301 RID: 8961
		// (set) Token: 0x06002302 RID: 8962
		public abstract bool Negative { get; set; }

		// Token: 0x170009CA RID: 2506
		// (get) Token: 0x06002303 RID: 8963
		public abstract ulong ULongValue { get; }

		// Token: 0x170009CB RID: 2507
		// (get) Token: 0x06002304 RID: 8964
		// (set) Token: 0x06002305 RID: 8965
		public abstract string StringValue { get; set; }

		// Token: 0x170009CC RID: 2508
		// (get) Token: 0x06002306 RID: 8966
		// (set) Token: 0x06002307 RID: 8967
		public abstract double RealValue { get; set; }

		// Token: 0x170009CD RID: 2509
		// (get) Token: 0x06002308 RID: 8968
		// (set) Token: 0x06002309 RID: 8969
		public abstract TypeClass ConstantType { get; set; }

		// Token: 0x170009CE RID: 2510
		// (get) Token: 0x0600230A RID: 8970 RVA: 0x0005B841 File Offset: 0x0005A841
		public TypeClass OriginalType
		{
			get
			{
				return this.m_tcOriginal;
			}
		}

		// Token: 0x170009CF RID: 2511
		// (get) Token: 0x0600230B RID: 8971 RVA: 0x00010208 File Offset: 0x0000F208
		// (set) Token: 0x0600230C RID: 8972 RVA: 0x0005B849 File Offset: 0x0005A849
		public virtual int Base
		{
			get
			{
				return 10;
			}
			set
			{
				throw new NotSupportedException("do not attempt to change a green tree");
			}
		}

		// Token: 0x0600230D RID: 8973 RVA: 0x0001020C File Offset: 0x0000F20C
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600230E RID: 8974 RVA: 0x0001021E File Offset: 0x0000F21E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x170009D0 RID: 2512
		// (get) Token: 0x0600230F RID: 8975 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLiteral
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170009D1 RID: 2513
		// (get) Token: 0x06002310 RID: 8976 RVA: 0x0005B858 File Offset: 0x0005A858
		public ILiteralValue LiteralValue
		{
			get
			{
				TypeClass constantType = this.ConstantType;
				if (constantType > TypeClass.LReal)
				{
					if (constantType - TypeClass.String > 1)
					{
						if (constantType == TypeClass.AnyReal)
						{
							goto IL_2B;
						}
						if (constantType != TypeClass.XString)
						{
							goto IL_62;
						}
					}
					return new LiteralValue(this.StringValue);
				}
				if (constantType == TypeClass.Bool)
				{
					return new LiteralValue(this.LongValue == 1L);
				}
				if (constantType - TypeClass.Real > 1)
				{
					goto IL_62;
				}
				IL_2B:
				return new LiteralValue(this.RealValue);
				IL_62:
				return (this.Type != null && !TypeTable.IsSigned(this.Type.DeRefType.Class) && TypeTable.IsInteger(this.Type.DeRefType.Class)) ? new LiteralValue(this.ULongValue) : new LiteralValue(this.LongValue);
			}
		}

		// Token: 0x040006B9 RID: 1721
		protected TypeClass m_tcOriginal = TypeClass.None;
	}
}
