using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class SignatureChangedEventArgs : CompileEventArgs
	{
		private ISignature _signOld;

		private ISignature _signNew;

		private bool _bSignificant;

		public ISignature OldSignature => _signOld;

		public ISignature NewSignature => _signNew;

		public bool Significant => _bSignificant;

		public SignatureChangedEventArgs(Guid guidApplication, ISignature signOld, ISignature signNew, bool bSignificant)
			: base(guidApplication)
		{
			_signOld = signOld;
			_signNew = signNew;
			_bSignificant = bSignificant;
		}
	}
}
