using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x02000192 RID: 402
	public static class TypeAcceptor<T>
	{
		// Token: 0x06001C68 RID: 7272 RVA: 0x0005C168 File Offset: 0x0005A368
		public static T Accept(_IType type, ITypeVisitorX<T> visitor)
		{
			_IAliasType ialiasType = type as _IAliasType;
			if (ialiasType != null)
			{
				return visitor.visit(ialiasType);
			}
			IGenericUserdefType genericUserdefType = type as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				return visitor.visit(genericUserdefType);
			}
			_IXStringType ixstringType = type as _IXStringType;
			if (ixstringType != null)
			{
				return visitor.visit(ixstringType);
			}
			_IBitConstType ibitConstType = type as _IBitConstType;
			if (ibitConstType != null)
			{
				return visitor.visit(ibitConstType);
			}
			_IBitType ibitType = type as _IBitType;
			if (ibitType != null)
			{
				return visitor.visit(ibitType);
			}
			_IBoolType iboolType = type as _IBoolType;
			if (iboolType != null)
			{
				return visitor.visit(iboolType);
			}
			_IByteType ibyteType = type as _IByteType;
			if (ibyteType != null)
			{
				return visitor.visit(ibyteType);
			}
			_ISIntType isintType = type as _ISIntType;
			if (isintType != null)
			{
				return visitor.visit(isintType);
			}
			_IUSIntType iusintType = type as _IUSIntType;
			if (iusintType != null)
			{
				return visitor.visit(iusintType);
			}
			_IIntType iintType = type as _IIntType;
			if (iintType != null)
			{
				return visitor.visit(iintType);
			}
			_IUIntType iuintType = type as _IUIntType;
			if (iuintType != null)
			{
				return visitor.visit(iuintType);
			}
			_IWordType iwordType = type as _IWordType;
			if (iwordType != null)
			{
				return visitor.visit(iwordType);
			}
			_IDIntType idintType = type as _IDIntType;
			if (idintType != null)
			{
				return visitor.visit(idintType);
			}
			_IUDIntType iudintType = type as _IUDIntType;
			if (iudintType != null)
			{
				return visitor.visit(iudintType);
			}
			_IDWordType idwordType = type as _IDWordType;
			if (idwordType != null)
			{
				return visitor.visit(idwordType);
			}
			_ILIntType ilintType = type as _ILIntType;
			if (ilintType != null)
			{
				return visitor.visit(ilintType);
			}
			_IULIntType iulintType = type as _IULIntType;
			if (iulintType != null)
			{
				return visitor.visit(iulintType);
			}
			_ILWordType ilwordType = type as _ILWordType;
			if (ilwordType != null)
			{
				return visitor.visit(ilwordType);
			}
			_IRealType irealType = type as _IRealType;
			if (irealType != null)
			{
				return visitor.visit(irealType);
			}
			_ILRealType ilrealType = type as _ILRealType;
			if (ilrealType != null)
			{
				return visitor.visit(ilrealType);
			}
			_ILazyType ilazyType = type as _ILazyType;
			if (ilazyType != null)
			{
				return visitor.visit(ilazyType);
			}
			_IUserdefType iuserdefType = type as _IUserdefType;
			if (iuserdefType != null)
			{
				return visitor.visit(iuserdefType);
			}
			_IPointerType ipointerType = type as _IPointerType;
			if (ipointerType != null)
			{
				return visitor.visit(ipointerType);
			}
			_IReferenceType ireferenceType = type as _IReferenceType;
			if (ireferenceType != null)
			{
				return visitor.visit(ireferenceType);
			}
			_ISubrangeType isubrangeType = type as _ISubrangeType;
			if (isubrangeType != null)
			{
				return visitor.visit(isubrangeType);
			}
			_IEnumType ienumType = type as _IEnumType;
			if (ienumType != null)
			{
				return visitor.visit(ienumType);
			}
			IImplicitEnumerationType implicitEnumerationType = type as IImplicitEnumerationType;
			if (implicitEnumerationType != null)
			{
				return visitor.visit(implicitEnumerationType);
			}
			_IParamsType iparamsType = type as _IParamsType;
			if (iparamsType != null)
			{
				return visitor.visit(iparamsType);
			}
			_IArrayType iarrayType = type as _IArrayType;
			if (iarrayType != null)
			{
				return visitor.visit(iarrayType);
			}
			_IVectorType ivectorType = type as _IVectorType;
			if (ivectorType != null)
			{
				return visitor.visit(ivectorType);
			}
			_IStringType istringType = type as _IStringType;
			if (istringType != null)
			{
				return visitor.visit(istringType);
			}
			_IWStringType iwstringType = type as _IWStringType;
			if (iwstringType != null)
			{
				return visitor.visit(iwstringType);
			}
			_IAnyType ianyType = type as _IAnyType;
			if (ianyType != null)
			{
				return visitor.visit(ianyType);
			}
			_IAnyRealType ianyRealType = type as _IAnyRealType;
			if (ianyRealType != null)
			{
				return visitor.visit(ianyRealType);
			}
			_IAnyIntType ianyIntType = type as _IAnyIntType;
			if (ianyIntType != null)
			{
				return visitor.visit(ianyIntType);
			}
			_IAnyNumType ianyNumType = type as _IAnyNumType;
			if (ianyNumType != null)
			{
				return visitor.visit(ianyNumType);
			}
			_IAnyBitType ianyBitType = type as _IAnyBitType;
			if (ianyBitType != null)
			{
				return visitor.visit(ianyBitType);
			}
			_IAnyDateType ianyDateType = type as _IAnyDateType;
			if (ianyDateType != null)
			{
				return visitor.visit(ianyDateType);
			}
			_IAnyBitButBoolIsPreferred ianyBitButBoolIsPreferred = type as _IAnyBitButBoolIsPreferred;
			if (ianyBitButBoolIsPreferred != null)
			{
				return visitor.visit(ianyBitButBoolIsPreferred);
			}
			_IDateType idateType = type as _IDateType;
			if (idateType != null)
			{
				return visitor.visit(idateType);
			}
			_ITimeOfDayType itimeOfDayType = type as _ITimeOfDayType;
			if (itimeOfDayType != null)
			{
				return visitor.visit(itimeOfDayType);
			}
			_IDateAndTimeType idateAndTimeType = type as _IDateAndTimeType;
			if (idateAndTimeType != null)
			{
				return visitor.visit(idateAndTimeType);
			}
			_ITimeType itimeType = type as _ITimeType;
			if (itimeType != null)
			{
				return visitor.visit(itimeType);
			}
			_ILTimeType iltimeType = type as _ILTimeType;
			if (iltimeType != null)
			{
				return visitor.visit(iltimeType);
			}
			_IXIntType ixintType = type as _IXIntType;
			if (ixintType != null)
			{
				return visitor.visit(ixintType);
			}
			_IXWordType ixwordType = type as _IXWordType;
			if (ixwordType != null)
			{
				return visitor.visit(ixwordType);
			}
			_IXUDIntType ixudintType = type as _IXUDIntType;
			if (ixudintType != null)
			{
				return visitor.visit(ixudintType);
			}
			_IXULIntType ixulintType = type as _IXULIntType;
			if (ixulintType != null)
			{
				return visitor.visit(ixulintType);
			}
			_IXLIntType ixlintType = type as _IXLIntType;
			if (ixlintType != null)
			{
				return visitor.visit(ixlintType);
			}
			_IUXIntType iuxintType = type as _IUXIntType;
			if (iuxintType != null)
			{
				return visitor.visit(iuxintType);
			}
			_IVariableLengthArrayType ivariableLengthArrayType = type as _IVariableLengthArrayType;
			if (ivariableLengthArrayType != null)
			{
				return visitor.visit(ivariableLengthArrayType);
			}
			_IAnyStringType ianyStringType = type as _IAnyStringType;
			if (ianyStringType != null)
			{
				return visitor.visit(ianyStringType);
			}
			_ILDateType ildateType = type as _ILDateType;
			if (ildateType != null)
			{
				return visitor.visit(ildateType);
			}
			_ILTimeOfDayType iltimeOfDayType = type as _ILTimeOfDayType;
			if (iltimeOfDayType != null)
			{
				return visitor.visit(iltimeOfDayType);
			}
			_ILDateAndTimeType ildateAndTimeType = type as _ILDateAndTimeType;
			if (ildateAndTimeType != null)
			{
				return visitor.visit(ildateAndTimeType);
			}
			_IXDIntType ixdintType = type as _IXDIntType;
			if (ixdintType != null)
			{
				return visitor.visit(ixdintType);
			}
			_IXDWordType ixdwordType = type as _IXDWordType;
			if (ixdwordType != null)
			{
				return visitor.visit(ixdwordType);
			}
			_IXLWordType ixlwordType = type as _IXLWordType;
			if (ixlwordType == null)
			{
				return default(T);
			}
			return visitor.visit(ixlwordType);
		}
	}
}
