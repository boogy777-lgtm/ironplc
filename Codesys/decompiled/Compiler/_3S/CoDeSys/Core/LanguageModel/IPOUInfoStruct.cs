using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPOUInfoStruct : ICompiledElementInfoStruct
	{
		uint CRCCode { get; }

		uint CRCInterface { get; }

		ushort AreaCodeLocation { get; }

		ushort AreaFPPointerLocation { get; }

		uint OffsetCodeLocation { get; }

		uint OffsetFPPointerLocation { get; }
	}
}
