using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IAttributeManager
	{
		IEnumerable<IAttribute> RegisteredAttributes { get; }

		bool CheckAttribute(string attribute, string value, AttributeScope scope, ISignature sign, IVariable variable, out IList<string> errors);

		bool IsCheckFunction(ISignature sign);
	}
}
