using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRegister2 : IRegister
	{
		new bool EverUsed { get; set; }

		IRegister Clone();
	}
}
