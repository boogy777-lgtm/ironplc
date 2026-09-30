using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IVariableWithModifyingAccesses
	{
		void AddModifyingCrossReference(int nCodeId);

		int[] GetModifyingCrossReferences();
	}
}
