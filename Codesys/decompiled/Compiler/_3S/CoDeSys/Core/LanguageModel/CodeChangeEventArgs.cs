using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class CodeChangeEventArgs : CompileEventArgs
	{
		private IOnlineChangeDetails2 _ocd;

		private ICompileContext8 _comcon;

		public IOnlineChangeDetails2 OnlineChangeDetails => _ocd;

		public ICompileContext8 CompileContext => _comcon;

		public CodeChangeEventArgs(Guid guidApplication, IOnlineChangeDetails2 ocd, ICompileContext8 comcon)
			: base(guidApplication)
		{
			_ocd = ocd;
			_comcon = comcon;
		}
	}
}
