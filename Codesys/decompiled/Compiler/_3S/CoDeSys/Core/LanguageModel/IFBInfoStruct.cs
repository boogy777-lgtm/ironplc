using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IFBInfoStruct : IPOUInfoStruct, ICompiledElementInfoStruct
	{
		uint CRCVFTable { get; }

		uint AreaVFTableLocation { get; }

		uint OffsetVFTableLocation { get; }
	}
}
