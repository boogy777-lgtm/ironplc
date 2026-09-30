using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IRTSComponentExporter3 : IRTSComponentExporter2, IRTSComponentExporter
	{
		string Placeholder { get; set; }
	}
}
