using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IRTSComponentExporter6 : IRTSComponentExporter5, IRTSComponentExporter4, IRTSComponentExporter3, IRTSComponentExporter2, IRTSComponentExporter
	{
		bool ExportLibTypes { get; set; }
	}
}
