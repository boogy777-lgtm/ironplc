using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeTable
	{
		_IBoolType Bool { get; }

		_IDirectAddressBitType DirectAddressBitType { get; }

		_ISafeBoolType SafeBool { get; }

		_ISafeByteType SafeByte { get; }

		_ISafeSIntType SafeSInt { get; }

		_ISafeUSIntType SafeUSInt { get; }

		_ISafeWordType SafeWord { get; }

		_ISafeIntType SafeInt { get; }

		_ISafeUIntType SafeUInt { get; }

		_ISafeDWordType SafeDWord { get; }

		_ISafeDIntType SafeDInt { get; }

		_ISafeUDIntType SafeUDInt { get; }

		_ISafeLWordType SafeLWord { get; }

		_ISafeLIntType SafeLInt { get; }

		_ISafeULIntType SafeULInt { get; }

		_ISafeTimeType SafeTime { get; }

		_IBool16Type Bool16 { get; }

		_IBitConstType BitConst { get; }

		_IBitType Bit { get; }

		_IByteType Byte { get; }

		_IWordType Word { get; }

		_IDWordType DWord { get; }

		_ILWordType LWord { get; }

		_IXWordType XWord { get; }

		_IXDWordType XDWord { get; }

		_IXLWordType XLWord { get; }

		_IUXIntType UXInt { get; }

		_IXUDIntType XUDInt { get; }

		_IXULIntType XULInt { get; }

		_IXIntType XInt { get; }

		_IXDIntType XDInt { get; }

		_IXLIntType XLInt { get; }

		_ISIntType SInt { get; }

		_IIntType Int { get; }

		_IDIntType DInt { get; }

		_ILIntType LInt { get; }

		_IUSIntType USInt { get; }

		_IUIntType UInt { get; }

		_IUDIntType UDInt { get; }

		_IULIntType ULInt { get; }

		_IRealType Real { get; }

		_ILRealType LReal { get; }

		_ITimeType Time { get; }

		_ILTimeType LTime { get; }

		_IDateType Date { get; }

		_IDateAndTimeType DateAndTime { get; }

		_ITimeOfDayType TimeOfDay { get; }

		_IAnyType Any { get; }

		_IAnyBitType AnyBit { get; }

		_IAnyBitButBoolIsPreferred AnyBitButBoolIsPreferred { get; }

		_IAnyDateType AnyDate { get; }

		_IAnyIntType AnyInt { get; }

		_IAnyNumType AnyNum { get; }

		_IAnyRealType AnyReal { get; }

		_IAnyStringType AnyString { get; }

		_IStringType String { get; }

		_IWStringType WString { get; }

		_IXStringType XString { get; }

		_IPointerType Pointer { get; }

		_ILazyType Lazy { get; }

		int BoolSize { get; }

		int BitSize { get; }

		int ByteSize { get; }

		int WordSize { get; }

		int DWordSize { get; }

		int LWordSize { get; }

		int SIntSize { get; }

		int IntSize { get; }

		int DIntSize { get; }

		int LIntSize { get; }

		int USIntSize { get; }

		int UIntSize { get; }

		int UDIntSize { get; }

		int ULIntSize { get; }

		int RealSize { get; }

		int LRealSize { get; }

		int TimeSize { get; }

		int LTimeSize { get; }

		int DateSize { get; }

		int DateAndTimeSize { get; }

		int TimeOfDaySize { get; }

		_IType GetStaticType(ICompiledType type);

		bool IsEquivalent(TypeClass tc1, TypeClass tc2);

		bool IsResolvedXType(IType type);

		bool IsXType(IType type);

		bool IsLikePointer(IType type, int pointerSize);

		TypeClass GetEquivalent64BitTypeOfResolvedXType(IType type);

		bool IsBlock(TypeClass tc);

		int GetOptimalVectorSize(IScope scope, TypeClass basetc);

		bool IsSigned(TypeClass tc);

		bool IsLType(TypeClass tc);

		bool IsBoolean(TypeClass tc);

		bool IsBit(TypeClass tc);

		bool IsNumber(TypeClass tc);

		bool IsTime(TypeClass tc);

		ulong GetTypeRangeHigh(TypeClass tc);

		long GetTypeRangeLow(TypeClass tc);

		bool IsInteger(TypeClass tc);

		bool IsLInteger(TypeClass tc, IScope scope);

		bool IsLInteger2(TypeClass tc, ICommonScope psp);

		bool IsReal(TypeClass tc);

		bool IsString(TypeClass tc);

		bool IsConcreterType(TypeClass type, TypeClass typeCompare);

		bool IsAnyType(TypeClass type);

		bool IsConcreteType(TypeClass type);

		Operator GetOperatorByType(TypeClass tc);

		TypeClass GetTypeByOperator(Operator op);

		int GetSize(TypeClass tc, IScope scope);

		int GetSize2(TypeClass tc, ICommonScope psp);

		string GetExternalOperatorName(Operator op);

		_IType Get(TypeClass tc);

		_IType Get(string stType);

		_IType Get(Operator op);

		int PointerSize(ICommonScope psp);

		int ReferenceSize(ICommonScope psp);

		int InterfaceSize(ICommonScope psp);

		TypeClass GetCorrespondingSignedType(TypeClass tcUnsigned);
	}
}
