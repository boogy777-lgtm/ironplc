using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal sealed class DeRefAccessInfo : IDeRefAccessInfo2, IDeRefAccessInfo, IAddressInfo, IAddressInfo3, IAddressInfo2
	{
		private IAddressInfo _baseAddressInfo;

		private int _iPointerSize;

		private ICompiledType _type;

		public IAddressInfo Base => _baseAddressInfo;

		public int Offset => 0;

		public int Size => _iPointerSize;

		public int SignatureID => -1;

		public int VariableID => -1;

		public IType Type => _type;

		internal DeRefAccessInfo(IAddressInfo baseAddressInfo, int iPointerSize, ICompiledType type)
		{
			_baseAddressInfo = baseAddressInfo;
			_iPointerSize = iPointerSize;
			_type = type;
		}
	}
}
