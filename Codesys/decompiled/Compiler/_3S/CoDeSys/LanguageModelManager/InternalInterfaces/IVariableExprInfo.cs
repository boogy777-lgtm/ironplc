using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IVariableExprInfo : IExprInfo
	{
		int Area { get; set; }

		int Address { get; set; }

		int Offset { get; set; }

		IAccessMode AccessMode { get; }

		int SignatureToCall { get; set; }

		bool IsBit { get; }

		byte BitNr { get; set; }

		int VariableId { get; set; }

		int SignatureId { get; set; }

		ICompiledType CompiledType { get; set; }

		IIndexInfo IndexInfo { get; set; }

		Operator POUType { get; set; }

		bool HasAccessMode(AccessModeFlags am);

		bool GetAccessMode(AccessModeFlags am);

		void SetAccessMode(AccessModeFlags am, bool bSetTrue);
	}
}
