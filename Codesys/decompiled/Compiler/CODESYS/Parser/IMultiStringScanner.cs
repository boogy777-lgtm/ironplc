using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IMultiStringScanner : _IScanner5, _IScanner4, _IScanner3, _IScanner2, _IScanner, IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner, IScanner7, IScanner8, IScanner9
	{
		void InitializeMulti(IList<string> strings);
	}
}
