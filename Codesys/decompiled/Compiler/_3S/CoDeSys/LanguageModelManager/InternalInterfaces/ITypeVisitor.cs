using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeVisitor
	{
		void visit(_IBitConstType type);

		void visit(_IBitType type);

		void visit(_IBoolType type);

		void visit(_IByteType type);

		void visit(_ISIntType type);

		void visit(_IUSIntType type);

		void visit(_IIntType type);

		void visit(_IUIntType type);

		void visit(_IWordType type);

		void visit(_IDIntType type);

		void visit(_IUDIntType type);

		void visit(_IDWordType type);

		void visit(_ILIntType type);

		void visit(_IULIntType type);

		void visit(_ILWordType type);

		void visit(_IRealType type);

		void visit(_ILRealType type);

		void visit(_ILazyType type);

		void visit(_IUserdefType type);

		void visit(_IPointerType type);

		void visit(_IReferenceType type);

		void visit(_ISubrangeType type);

		void visit(_IEnumType type);

		void visit(IImplicitEnumerationType type);

		void visit(_IParamsType type);

		void visit(_IArrayType type);

		void visit(_IVectorType type);

		void visit(_IStringType type);

		void visit(_IWStringType type);

		void visit(_IAnyType type);

		void visit(_IAnyRealType type);

		void visit(_IAnyIntType type);

		void visit(_IAnyNumType type);

		void visit(_IAnyBitType type);

		void visit(_IAnyDateType type);

		void visit(_IAnyBitButBoolIsPreferred type);

		void visit(_IDateType type);

		void visit(_ITimeOfDayType type);

		void visit(_IDateAndTimeType type);

		void visit(_ITimeType type);

		void visit(_ILTimeType type);

		void visit(_IXIntType type);

		void visit(_IXWordType type);

		void visit(_IXUDIntType type);

		void visit(_IXULIntType type);

		void visit(_IXLIntType type);

		void visit(_IUXIntType type);

		void visit(_IXStringType type);

		void visit(_IVariableLengthArrayType type);

		void visit(_IAnyStringType type);
	}
}
