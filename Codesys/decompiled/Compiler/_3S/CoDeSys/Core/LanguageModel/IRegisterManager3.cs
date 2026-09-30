using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRegisterManager3 : IRegisterManager2, IRegisterManager
	{
		IRegister NextRegister { get; set; }
	}
}
