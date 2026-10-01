using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRegister
	{
		RegisterType Type { get; }

		uint Num { get; }

		int Usage { get; }

		bool EverUsed { get; }

		IRegister PartnerReg { get; set; }

		void Reset();

		void Free();

		void Use();
	}
}
