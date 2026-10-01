using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IRTSComponentExporter5 : IRTSComponentExporter4, IRTSComponentExporter3, IRTSComponentExporter2, IRTSComponentExporter
	{
		IList<ISignature> FilterSignatures(ISignature[] signatures);
	}
}
