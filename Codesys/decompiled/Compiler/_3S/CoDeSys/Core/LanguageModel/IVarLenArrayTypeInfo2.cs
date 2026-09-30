using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarLenArrayTypeInfo2 : IVarLenArrayTypeInfo
	{
		bool IsVarLenArray(IVariable var, out string stOriginalType);

		VarFlag GetOriginalVariableDeclarationScope(IVariable var);
	}
}
