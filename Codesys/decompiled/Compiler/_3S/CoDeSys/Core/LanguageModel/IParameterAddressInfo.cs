using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IParameterAddressInfo : IAddressInfo
	{
		int ModuleType { get; }

		int ModuleInstance { get; }

		long ParameterId { get; }

		int BitOffset { get; }

		int BitSize { get; }

		string Type { get; }

		ICompiledType CompiledType { get; }
	}
}
