using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IAddressInfoCreator
	{
		IAddressInfo GetAddressInfo(string stInstancePath, IExpression exp, out string stErrorMsg);
	}
}
