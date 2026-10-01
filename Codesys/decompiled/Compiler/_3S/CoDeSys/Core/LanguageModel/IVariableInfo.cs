using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariableInfo
	{
		int VariableId { get; }

		int SignatureId { get; }

		VarFlag Flags { get; }
	}
}
