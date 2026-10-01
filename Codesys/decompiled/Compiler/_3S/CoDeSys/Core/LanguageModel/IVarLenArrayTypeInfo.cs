using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarLenArrayTypeInfo
	{
		bool IsVarLenArray(IVariable var);

		int GetDimensions(IVariable var);

		IVariable GetDimensionInfoVariable(ISignature declaringSign, IVariable var);
	}
}
