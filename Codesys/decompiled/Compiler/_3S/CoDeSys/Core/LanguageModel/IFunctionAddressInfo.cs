using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IFunctionAddressInfo : IAddressInfo
	{
		int FunctionPointerArea { get; }

		int FunctionPointerOffset { get; }

		int ResultSize { get; }

		int ResultOffset { get; }

		ICompiledType ResultCompiledType { get; }

		int SignatureSize { get; }

		short ImplementationStyle { get; }

		short InputParameterCount { get; }

		int[] InputParameterOffsets { get; }

		IAddressInfo[] InputParameterAddressInfos { get; }
	}
}
