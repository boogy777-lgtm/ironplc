using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IPrecompileTypeInfo
	{
		int PrecompileSignatureId { get; set; }

		int PrecompileVariableId { get; set; }
	}
}
