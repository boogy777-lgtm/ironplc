using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0017;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x02000196 RID: 406
	public class GenericTypeReplacer : ITypeVisitorX<_IType>
	{
		// Token: 0x170005CB RID: 1483
		// (get) Token: 0x06001D17 RID: 7447 RVA: 0x0005F058 File Offset: 0x0005D258
		private IConstantFolder3 ConstantFolder { get; }

		// Token: 0x170005CC RID: 1484
		// (get) Token: 0x06001D18 RID: 7448 RVA: 0x0005F060 File Offset: 0x0005D260
		private \u0017.\u0006 Scope { get; }

		// Token: 0x06001D19 RID: 7449 RVA: 0x0005F068 File Offset: 0x0005D268
		private GenericTypeReplacer(IConstantFolder3 constantFolder, \u0017.\u0006 scope)
		{
			this.ConstantFolder = constantFolder;
			this.Scope = scope;
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x0005F080 File Offset: 0x0005D280
		internal static _IType \u0001(_IType \u0002, IConstantFolder3 \u0003, \u0017.\u0006 \u0004)
		{
			GenericTypeReplacer visitor = new GenericTypeReplacer(\u0003, \u0004);
			return TypeAcceptor<_IType>.Accept(\u0002, visitor);
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x0005F09C File Offset: 0x0005D29C
		public _IType visit(_ISubrangeType type)
		{
			_IExpression iexpression = this.\u0001(type._LowerBorder);
			_IExpression iexpression2 = this.\u0001(type._UpperBorder);
			if (iexpression == null || iexpression2 == null)
			{
				return type;
			}
			_ISubrangeType isubrangeType = \u0019.\u0003.\u0001(iexpression, iexpression2);
			isubrangeType._Base = type._Base;
			return isubrangeType;
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x0005F0E0 File Offset: 0x0005D2E0
		private _IExpression \u0001(_IExpression \u0002)
		{
			ILiteralValue literalValue = this.ConstantFolder.GetLiteralValue(\u0002, this.Scope);
			if (literalValue == null)
			{
				return null;
			}
			return GenericTypeReplacer.\u0001(literalValue);
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x0005F10C File Offset: 0x0005D30C
		public _IType visit(_IArrayType type)
		{
			List<Tuple<_IExpression, _IExpression>> list = new List<Tuple<_IExpression, _IExpression>>();
			foreach (_IArrayDimension iarrayDimension in type._Dimensions)
			{
				_IExpression iexpression = this.\u0001(iarrayDimension._LowerBorder);
				_IExpression iexpression2 = this.\u0001(iarrayDimension._UpperBorder);
				if (iexpression == null || iexpression2 == null)
				{
					return type;
				}
				list.Add(new Tuple<_IExpression, _IExpression>(iexpression, iexpression2));
			}
			_IArrayType iarrayType = \u0019.\u0003.\u0001(type._Base);
			foreach (Tuple<_IExpression, _IExpression> tuple in list)
			{
				iarrayType.AddDimension(tuple.Item1, tuple.Item2);
			}
			return iarrayType;
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x0005F1EC File Offset: 0x0005D3EC
		public _IType visit(_IVectorType type)
		{
			_IExpression iexpression = this.\u0001(type._Dimension);
			if (iexpression == null)
			{
				return type;
			}
			return \u0019.\u0003.\u0001(type._Base, iexpression);
		}

		// Token: 0x06001D1F RID: 7455 RVA: 0x0005F218 File Offset: 0x0005D418
		public _IType visit(IGenericUserdefType type)
		{
			IGenericUserdefType genericUserdefType = \u0019.\u0003.\u0001((_IExpression)type.NameExpression);
			foreach (_IExpression u in type.GenericConstantsInitializations)
			{
				_IExpression iexpression = this.\u0001(u);
				if (iexpression == null)
				{
					return type;
				}
				genericUserdefType.AddGenericConstantInitialization(iexpression);
			}
			genericUserdefType.SignatureId = type.SignatureId;
			return genericUserdefType;
		}

		// Token: 0x06001D20 RID: 7456 RVA: 0x0005F298 File Offset: 0x0005D498
		public _IType visit(_IXStringType type)
		{
			_IExpression iexpression = this.\u0001(type.Length);
			if (iexpression == null)
			{
				return type;
			}
			_IXStringType ixstringType = \u0019.\u0003.\u0001();
			ixstringType.Length = iexpression;
			return ixstringType;
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x0005F2C4 File Offset: 0x0005D4C4
		public _IType visit(_IStringType type)
		{
			_IExpression iexpression = this.\u0001(type.Length);
			if (iexpression == null)
			{
				return type;
			}
			_IStringType istringType = \u0019.\u0003.\u0001();
			istringType.Length = iexpression;
			return istringType;
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x0005F2F0 File Offset: 0x0005D4F0
		public _IType visit(_IWStringType type)
		{
			_IExpression iexpression = this.\u0001(type.Length);
			if (iexpression == null)
			{
				return type;
			}
			_IWStringType iwstringType = \u0019.\u0003.\u0001();
			iwstringType.Length = iexpression;
			return iwstringType;
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x0005F31C File Offset: 0x0005D51C
		private static _IExpression \u0001(ILiteralValue \u0002)
		{
			switch (\u0002.KindOf)
			{
			case KindOfLiteral.SignedInteger:
			{
				bool flag;
				return \u0019.\u0003.\u0001(\u0002.GetSignedLong(out flag));
			}
			case KindOfLiteral.UnsignedInteger:
			{
				bool flag;
				return \u0019.\u0003.\u0001(\u0002.GetUnsignedLong(out flag));
			}
			case KindOfLiteral.Float:
				return \u0019.\u0003.\u0001(\u0002.Float);
			case KindOfLiteral.String:
				return \u0019.\u0003.\u0001(\u0002.String);
			case KindOfLiteral.Bool:
				return \u0019.\u0003.\u0001(\u0002.Bool);
			default:
				return null;
			}
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x0005F390 File Offset: 0x0005D590
		public _IType visit(_IBitConstType type)
		{
			return type;
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x0005F394 File Offset: 0x0005D594
		public _IType visit(_IBitType type)
		{
			return type;
		}

		// Token: 0x06001D26 RID: 7462 RVA: 0x0005F398 File Offset: 0x0005D598
		public _IType visit(_IBoolType type)
		{
			return type;
		}

		// Token: 0x06001D27 RID: 7463 RVA: 0x0005F39C File Offset: 0x0005D59C
		public _IType visit(_IByteType type)
		{
			return type;
		}

		// Token: 0x06001D28 RID: 7464 RVA: 0x0005F3A0 File Offset: 0x0005D5A0
		public _IType visit(_ISIntType type)
		{
			return type;
		}

		// Token: 0x06001D29 RID: 7465 RVA: 0x0005F3A4 File Offset: 0x0005D5A4
		public _IType visit(_IUSIntType type)
		{
			return type;
		}

		// Token: 0x06001D2A RID: 7466 RVA: 0x0005F3A8 File Offset: 0x0005D5A8
		public _IType visit(_IIntType type)
		{
			return type;
		}

		// Token: 0x06001D2B RID: 7467 RVA: 0x0005F3AC File Offset: 0x0005D5AC
		public _IType visit(_IUIntType type)
		{
			return type;
		}

		// Token: 0x06001D2C RID: 7468 RVA: 0x0005F3B0 File Offset: 0x0005D5B0
		public _IType visit(_IWordType type)
		{
			return type;
		}

		// Token: 0x06001D2D RID: 7469 RVA: 0x0005F3B4 File Offset: 0x0005D5B4
		public _IType visit(_IDIntType type)
		{
			return type;
		}

		// Token: 0x06001D2E RID: 7470 RVA: 0x0005F3B8 File Offset: 0x0005D5B8
		public _IType visit(_IUDIntType type)
		{
			return type;
		}

		// Token: 0x06001D2F RID: 7471 RVA: 0x0005F3BC File Offset: 0x0005D5BC
		public _IType visit(_IDWordType type)
		{
			return type;
		}

		// Token: 0x06001D30 RID: 7472 RVA: 0x0005F3C0 File Offset: 0x0005D5C0
		public _IType visit(_ILIntType type)
		{
			return type;
		}

		// Token: 0x06001D31 RID: 7473 RVA: 0x0005F3C4 File Offset: 0x0005D5C4
		public _IType visit(_IULIntType type)
		{
			return type;
		}

		// Token: 0x06001D32 RID: 7474 RVA: 0x0005F3C8 File Offset: 0x0005D5C8
		public _IType visit(_ILWordType type)
		{
			return type;
		}

		// Token: 0x06001D33 RID: 7475 RVA: 0x0005F3CC File Offset: 0x0005D5CC
		public _IType visit(_IRealType type)
		{
			return type;
		}

		// Token: 0x06001D34 RID: 7476 RVA: 0x0005F3D0 File Offset: 0x0005D5D0
		public _IType visit(_ILRealType type)
		{
			return type;
		}

		// Token: 0x06001D35 RID: 7477 RVA: 0x0005F3D4 File Offset: 0x0005D5D4
		public _IType visit(_ILazyType type)
		{
			return type;
		}

		// Token: 0x06001D36 RID: 7478 RVA: 0x0005F3D8 File Offset: 0x0005D5D8
		public _IType visit(_IUserdefType type)
		{
			return type;
		}

		// Token: 0x06001D37 RID: 7479 RVA: 0x0005F3DC File Offset: 0x0005D5DC
		public _IType visit(_IPointerType type)
		{
			return type;
		}

		// Token: 0x06001D38 RID: 7480 RVA: 0x0005F3E0 File Offset: 0x0005D5E0
		public _IType visit(_IReferenceType type)
		{
			return type;
		}

		// Token: 0x06001D39 RID: 7481 RVA: 0x0005F3E4 File Offset: 0x0005D5E4
		public _IType visit(_IEnumType type)
		{
			return type;
		}

		// Token: 0x06001D3A RID: 7482 RVA: 0x0005F3E8 File Offset: 0x0005D5E8
		public _IType visit(IImplicitEnumerationType type)
		{
			return type;
		}

		// Token: 0x06001D3B RID: 7483 RVA: 0x0005F3EC File Offset: 0x0005D5EC
		public _IType visit(_IParamsType type)
		{
			return type;
		}

		// Token: 0x06001D3C RID: 7484 RVA: 0x0005F3F0 File Offset: 0x0005D5F0
		public _IType visit(_IAnyType type)
		{
			return type;
		}

		// Token: 0x06001D3D RID: 7485 RVA: 0x0005F3F4 File Offset: 0x0005D5F4
		public _IType visit(_IAnyRealType type)
		{
			return type;
		}

		// Token: 0x06001D3E RID: 7486 RVA: 0x0005F3F8 File Offset: 0x0005D5F8
		public _IType visit(_IAnyIntType type)
		{
			return type;
		}

		// Token: 0x06001D3F RID: 7487 RVA: 0x0005F3FC File Offset: 0x0005D5FC
		public _IType visit(_IAnyNumType type)
		{
			return type;
		}

		// Token: 0x06001D40 RID: 7488 RVA: 0x0005F400 File Offset: 0x0005D600
		public _IType visit(_IAnyBitType type)
		{
			return type;
		}

		// Token: 0x06001D41 RID: 7489 RVA: 0x0005F404 File Offset: 0x0005D604
		public _IType visit(_IAnyDateType type)
		{
			return type;
		}

		// Token: 0x06001D42 RID: 7490 RVA: 0x0005F408 File Offset: 0x0005D608
		public _IType visit(_IAnyBitButBoolIsPreferred type)
		{
			return type;
		}

		// Token: 0x06001D43 RID: 7491 RVA: 0x0005F40C File Offset: 0x0005D60C
		public _IType visit(_IDateType type)
		{
			return type;
		}

		// Token: 0x06001D44 RID: 7492 RVA: 0x0005F410 File Offset: 0x0005D610
		public _IType visit(_ITimeOfDayType type)
		{
			return type;
		}

		// Token: 0x06001D45 RID: 7493 RVA: 0x0005F414 File Offset: 0x0005D614
		public _IType visit(_IDateAndTimeType type)
		{
			return type;
		}

		// Token: 0x06001D46 RID: 7494 RVA: 0x0005F418 File Offset: 0x0005D618
		public _IType visit(_ITimeType type)
		{
			return type;
		}

		// Token: 0x06001D47 RID: 7495 RVA: 0x0005F41C File Offset: 0x0005D61C
		public _IType visit(_ILTimeType type)
		{
			return type;
		}

		// Token: 0x06001D48 RID: 7496 RVA: 0x0005F420 File Offset: 0x0005D620
		public _IType visit(_IXIntType type)
		{
			return type;
		}

		// Token: 0x06001D49 RID: 7497 RVA: 0x0005F424 File Offset: 0x0005D624
		public _IType visit(_IXWordType type)
		{
			return type;
		}

		// Token: 0x06001D4A RID: 7498 RVA: 0x0005F428 File Offset: 0x0005D628
		public _IType visit(_IXUDIntType type)
		{
			return type;
		}

		// Token: 0x06001D4B RID: 7499 RVA: 0x0005F42C File Offset: 0x0005D62C
		public _IType visit(_IXULIntType type)
		{
			return type;
		}

		// Token: 0x06001D4C RID: 7500 RVA: 0x0005F430 File Offset: 0x0005D630
		public _IType visit(_IXLIntType type)
		{
			return type;
		}

		// Token: 0x06001D4D RID: 7501 RVA: 0x0005F434 File Offset: 0x0005D634
		public _IType visit(_IUXIntType type)
		{
			return type;
		}

		// Token: 0x06001D4E RID: 7502 RVA: 0x0005F438 File Offset: 0x0005D638
		public _IType visit(_IVariableLengthArrayType type)
		{
			return type;
		}

		// Token: 0x06001D4F RID: 7503 RVA: 0x0005F43C File Offset: 0x0005D63C
		public _IType visit(_IAnyStringType type)
		{
			return type;
		}

		// Token: 0x06001D50 RID: 7504 RVA: 0x0005F440 File Offset: 0x0005D640
		public _IType visit(_ILDateType type)
		{
			return type;
		}

		// Token: 0x06001D51 RID: 7505 RVA: 0x0005F444 File Offset: 0x0005D644
		public _IType visit(_ILTimeOfDayType type)
		{
			return type;
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x0005F448 File Offset: 0x0005D648
		public _IType visit(_ILDateAndTimeType type)
		{
			return type;
		}

		// Token: 0x06001D53 RID: 7507 RVA: 0x0005F44C File Offset: 0x0005D64C
		public _IType visit(_IAliasType type)
		{
			return type;
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x0005F450 File Offset: 0x0005D650
		public _IType visit(_IXDIntType type)
		{
			return type;
		}

		// Token: 0x06001D55 RID: 7509 RVA: 0x0005F454 File Offset: 0x0005D654
		public _IType visit(_IXDWordType type)
		{
			return type;
		}

		// Token: 0x06001D56 RID: 7510 RVA: 0x0005F458 File Offset: 0x0005D658
		public _IType visit(_IXLWordType type)
		{
			return type;
		}

		// Token: 0x040004D1 RID: 1233
		[CompilerGenerated]
		private readonly IConstantFolder3 \u0001;

		// Token: 0x040004D2 RID: 1234
		[CompilerGenerated]
		private readonly \u0017.\u0006 \u0001;
	}
}
