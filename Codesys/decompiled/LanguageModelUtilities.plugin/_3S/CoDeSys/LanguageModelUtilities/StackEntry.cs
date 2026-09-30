using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class StackEntry
	{
		private IAddressInfo m_ai;

		private string m_stErrorMsg;

		public IAddressInfo AddressInfo => m_ai;

		public string ErrorMsg => m_stErrorMsg;

		public StackEntry(IAddressInfo ai, string stErrorMsg)
		{
			m_ai = ai;
			m_stErrorMsg = stErrorMsg;
		}
	}
}
