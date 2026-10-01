using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMTypeService
	{
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

		bool IsEquivalent(TypeClass tc1, TypeClass tc2);

		bool IsXType(IType type);

		bool IsSigned(TypeClass tc);

		bool IsLType(TypeClass tc);

		bool IsBoolean(TypeClass tc);

		bool IsBit(TypeClass tc);

		bool IsNumber(TypeClass tc);

		bool IsTime(TypeClass tc);

		bool IsInteger(TypeClass tc);

		bool IsReal(TypeClass tc);

		bool IsString(TypeClass tc);

		bool IsAnyType(TypeClass type);

		bool IsConcreteType(TypeClass type);

		bool IsConcreterType(TypeClass type, TypeClass typeCompare);

		ICompiledType5 GetType(TypeClass tc);

		ICompiledType5 GetType(string stType);
	}
}
