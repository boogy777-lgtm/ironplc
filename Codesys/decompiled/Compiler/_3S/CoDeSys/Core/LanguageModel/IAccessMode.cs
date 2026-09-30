using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAccessMode
	{
		bool HasAccessMode(AccessModeFlags am);

		bool GetAccessMode(AccessModeFlags am);

		void SetAccessMode(AccessModeFlags am, bool bSetTrue);

		IAccessMode Duplicate();
	}
}
