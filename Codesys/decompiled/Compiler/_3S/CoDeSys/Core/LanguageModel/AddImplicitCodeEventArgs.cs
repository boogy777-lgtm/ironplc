using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class AddImplicitCodeEventArgs : CompileEventArgs
	{
		private ICompileContext3 _comcon;

		private ISignature _signWithCode;

		private List<string> _stCodeList = new List<string>();

		public ICompileContext3 CompileContext => _comcon;

		public ISignature SignatureOfPOUToAddCode => _signWithCode;

		public IList<string> Code => _stCodeList;

		public AddImplicitCodeEventArgs(Guid guidApplication, ICompileContext3 comcon, ISignature signOfFunction)
			: base(guidApplication)
		{
			_comcon = comcon;
			_signWithCode = signOfFunction;
		}
	}
}
