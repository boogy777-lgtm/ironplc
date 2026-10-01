using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class AfterGenerateCodeEventArgs : CompileEventArgs
	{
		public AfterGenerateCodeEventArgs(Guid guidApplication)
			: base(guidApplication)
		{
		}
	}
}
