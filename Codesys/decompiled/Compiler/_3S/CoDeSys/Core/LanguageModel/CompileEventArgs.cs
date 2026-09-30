using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class CompileEventArgs : EventArgs
	{
		private Guid _guidApplication;

		public Guid ApplicationGuid => _guidApplication;

		public CompileEventArgs(Guid guidApplication)
		{
			_guidApplication = guidApplication;
		}
	}
}
