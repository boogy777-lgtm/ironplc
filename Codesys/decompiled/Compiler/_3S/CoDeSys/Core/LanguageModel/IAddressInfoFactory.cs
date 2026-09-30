using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressInfoFactory
	{
		IOperatorAddressInfo CreateOperator(IAddressInfo[] operands, Operator op, IType t);

		ILiteralAddressInfo CreateLiteral(ILiteralValue lv, ICompiledType t);

		IConversionAddressInfo CreateConversion(TypeClass tcFrom, TypeClass tcTo, IAddressInfo aiBase, bool bImplicit, IType t);
	}
}
