using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRegisterManager
	{
		IRegister CreateRegister(uint uiRegNum, RegisterType RegType);

		void AddRegister(IRegister Reg);

		IRegister AllocateRegister(RegisterType RegType);

		IRegister AllocateRegisterPair(bool bAligned);

		IRegister AllocateRegisterPair(RegisterType RegType, bool bAligned);

		void FreeRegister(IRegister Reg);

		ArrayList GetRegisterList(RegisterType RegType);

		bool CheckState();

		void Reset();
	}
}
