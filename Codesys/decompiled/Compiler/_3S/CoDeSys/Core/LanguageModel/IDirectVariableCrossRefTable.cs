using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDirectVariableCrossRefTable
	{
		IDirectVariable[] AllDirectVariables { get; }

		IAddressCrossReference[] GetCrossReferencesOfDirectVariable(IDirectVariable dirvar);
	}
}
