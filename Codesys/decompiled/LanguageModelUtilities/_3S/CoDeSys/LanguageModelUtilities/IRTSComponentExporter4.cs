using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IRTSComponentExporter4 : IRTSComponentExporter3, IRTSComponentExporter2, IRTSComponentExporter
	{
		bool UseOriginalTypeNames { get; set; }
	}
}
