using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IAddressExprInfo : IExprInfo
	{
		IDataLocation DataLocation { get; set; }

		IAccessMode AccessMode { get; }

		ICompiledType CompiledType { get; set; }

		bool HasAccessMode(AccessModeFlags am);

		bool GetAccessMode(AccessModeFlags am);

		void SetAccessMode(AccessModeFlags am, bool bSetTrue);
	}
}
