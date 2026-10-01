using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class CompiledPOUChangedEventArgs : CompileEventArgs
	{
		private ICompiledPOU _cpouOld;

		private ICompiledPOU _cpouNew;

		private bool _bSignificant;

		public ICompiledPOU OldCompiledPOU => _cpouOld;

		public ICompiledPOU NewCompiledPOU => _cpouNew;

		public bool Significant => _bSignificant;

		public CompiledPOUChangedEventArgs(Guid guidApplication, ICompiledPOU cpouOld, ICompiledPOU cpouNew, bool bSignificant)
			: base(guidApplication)
		{
			_cpouOld = cpouOld;
			_cpouNew = cpouNew;
			_bSignificant = bSignificant;
		}
	}
}
