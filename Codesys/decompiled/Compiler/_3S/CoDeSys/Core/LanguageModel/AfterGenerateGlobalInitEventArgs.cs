using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class AfterGenerateGlobalInitEventArgs : CompileEventArgs
	{
		private ICompiledPOU _cpou;

		public ICompiledPOU cpouGlobalInit => _cpou;

		public AfterGenerateGlobalInitEventArgs(Guid guidApplication, ICompiledPOU cpou)
			: base(guidApplication)
		{
			_cpou = cpou;
		}
	}
}
