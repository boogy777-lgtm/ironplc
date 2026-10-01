using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class BeforeGenerateRelinkCodeEventArgs : CompileEventArgs
	{
		private IOnlineChangeDetails2 _ocd;

		private Exception _ex;

		public IOnlineChangeDetails2 OnlineChangeDetails => _ocd;

		public Exception Exception => _ex;

		public BeforeGenerateRelinkCodeEventArgs(Guid guidApplication, IOnlineChangeDetails2 ocd)
			: base(guidApplication)
		{
			_ocd = ocd;
		}

		public void Cancel(Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}
			if (_ex == null)
			{
				_ex = ex;
			}
		}
	}
}
