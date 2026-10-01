using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRegisterManager2 : IRegisterManager
	{
		IRegister GetRegister(uint uiRegNum, RegisterType RegType);

		IRegisterManager CreateClone();

		void RemoveClone(IRegisterManager RegManClone);

		void MoveRegister(uint uiRegNum, RegisterType RegType, int nNewIndex);
	}
}
