using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IDeRefExprInfo : IExprInfo
	{
		int Offset { get; set; }

		bool InstanceAccess { get; set; }

		IAccessMode AccessMode { get; }

		byte BitNr { get; set; }

		ICompiledType CompiledType { get; set; }

		IIndexInfo IndexInfo { get; set; }
	}
}
