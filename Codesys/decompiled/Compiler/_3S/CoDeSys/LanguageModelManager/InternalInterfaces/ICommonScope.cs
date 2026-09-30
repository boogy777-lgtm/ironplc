using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICommonScope
	{
		int PointerSize { get; }

		bool IsPrecompileScope { get; }

		int GetSize(IType type);

		ISignature FindSignature(IUserdefType udtype);

		ISignature[] FindSignature(IExpression expression);

		ISignature FindSignature(IEnumType etype);

		ILiteralValue GetLiteralValue(IExpression expression, bool bAllocatedOk);

		bool IsImplicitConvertable(ISignature signSource, ISignature signDest, ICommonScope scopeDest);

		bool IsEqual(IUserdefType ud1, IUserdefType ud2);
	}
}
