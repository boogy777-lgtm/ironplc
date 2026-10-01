using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IPrecompileErrors
	{
		void ShowPrecompileErrors(_IPreCompileContext pcc, IList<_ISignature> signs);
	}
}
