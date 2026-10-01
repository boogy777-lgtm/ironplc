using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200005B RID: 91
	[TypeGuid("{528c9030-405d-451c-b45a-9aa6b0520783}")]
	[StorageVersion("3.3.0.0")]
	public class ImplicitConversionExpression : ConversionExpression, _IImplicitConversionExpression, _IConversionExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IConversionExpression
	{
		// Token: 0x06000563 RID: 1379 RVA: 0x0000F19B File Offset: 0x0000E19B
		public ImplicitConversionExpression()
		{
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0000F1A3 File Offset: 0x0000E1A3
		internal ImplicitConversionExpression(TypeClass tcFrom, TypeClass tcTo) : base(tcFrom, tcTo)
		{
			if (base.From == TypeClass.Enum)
			{
				base.From = TypeClass.Int;
			}
			if (base.To == TypeClass.Enum)
			{
				base.To = TypeClass.Int;
			}
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0000F1CF File Offset: 0x0000E1CF
		internal ImplicitConversionExpression(TypeClass tcFrom, TypeClass tcTo, IToken token) : base(tcFrom, tcTo, token)
		{
			if (base.From == TypeClass.Enum)
			{
				base.From = TypeClass.Int;
			}
			if (base.To == TypeClass.Enum)
			{
				base.To = TypeClass.Int;
			}
		}

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000566 RID: 1382 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool Implicit
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0000F1FC File Offset: 0x0000E1FC
		protected override ILiteralValue GetConversionValue(ILiteralValue litvalExp)
		{
			if (litvalExp == null)
			{
				return null;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
			{
				bool flag = litvalExp.KindOf == KindOfLiteral.SignedInteger || litvalExp.KindOf == KindOfLiteral.UnsignedInteger;
				bool flag2 = TypeTable.IsInteger(base.To);
				bool flag3 = TypeTable.IsSigned(base.To) == (litvalExp.KindOf == KindOfLiteral.SignedInteger);
				if (flag && flag2 && flag3)
				{
					return litvalExp;
				}
			}
			return base.GetConversionValue(litvalExp);
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000568 RID: 1384 RVA: 0x0000F265 File Offset: 0x0000E265
		// (set) Token: 0x06000569 RID: 1385 RVA: 0x0000F272 File Offset: 0x0000E272
		public override IMinimalPosition _Position
		{
			get
			{
				return this.m_exp._Position;
			}
			set
			{
				this.m_exp._Position = value;
			}
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0000F280 File Offset: 0x0000E280
		public override ISourcePosition GetPosition()
		{
			return this.m_exp.GetPosition();
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x0600056B RID: 1387 RVA: 0x0000F28D File Offset: 0x0000E28D
		// (set) Token: 0x0600056C RID: 1388 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				if (this.m_exp == null)
				{
					return MinimalPosition.CreateMinimalPosition(0L, 0);
				}
				return this.m_exp.PositionIntern;
			}
			set
			{
			}
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0000F2AB File Offset: 0x0000E2AB
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			if (this.m_exp != null)
			{
				this.m_exp.PositionIntern = minpos;
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x0600056E RID: 1390 RVA: 0x0000F2C1 File Offset: 0x0000E2C1
		// (set) Token: 0x0600056F RID: 1391 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				if (this.m_exp == null)
				{
					return 0;
				}
				return (this.m_exp as Expression).LengthIntern;
			}
			set
			{
			}
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0000F2E0 File Offset: 0x0000E2E0
		public override _IExprement Duplicate()
		{
			ImplicitConversionExpression implicitConversionExpression = new ImplicitConversionExpression();
			implicitConversionExpression.m_tcFrom = this.m_tcFrom;
			implicitConversionExpression.m_tcTo = this.m_tcTo;
			this.DuplicateCommon(implicitConversionExpression);
			implicitConversionExpression.m_exp = (this.m_exp.Duplicate() as Expression);
			return implicitConversionExpression;
		}
	}
}
