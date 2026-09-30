using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISignature4 : _ISignature3, _ISignature2, _ISignature, ISignature7, ISignature6, ISignature5, ISignature4, ISignature3, ISignature2, ISignature
	{
		IList<_ISignature> GetOverloadedSignatures(string stName);

		IEnumerable<string> GetOverloadedNames();

		void SetOverloadedSignatures(string stName, IList<_ISignature> signatures);

		void CreateOverloadPlaceholderSignatures(IList<_ISignature> subsignatures);

		void ChangeSubSignatureName(_ISignature signsub, _IExpression nameexpression);
	}
}
