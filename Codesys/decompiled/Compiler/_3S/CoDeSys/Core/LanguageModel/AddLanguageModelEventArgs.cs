using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class AddLanguageModelEventArgs : EventArgs
	{
		private Guid _guidApplication;

		private ILanguageModelList _languagemodellist;

		public Guid ApplicationGuid => _guidApplication;

		public ILanguageModelList LanguageModelList => _languagemodellist;

		public AddLanguageModelEventArgs(Guid guidApplication, ILanguageModelList languagemodellist)
		{
			_guidApplication = guidApplication;
			_languagemodellist = languagemodellist;
		}
	}
}
