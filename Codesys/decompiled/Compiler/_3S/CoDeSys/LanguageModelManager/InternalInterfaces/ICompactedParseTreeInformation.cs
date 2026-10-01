using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompactedParseTreeInformation
	{
		IDictionary<int, IList<_ICompilerMessage>> MessageTable { get; }

		IList<long> SourcePosTable { get; set; }

		IList<short> LengthTable { get; set; }

		IDictionary<int, IPrecompileTypeInfo> TypeInfoTable { get; }

		void Clear();
	}
}
