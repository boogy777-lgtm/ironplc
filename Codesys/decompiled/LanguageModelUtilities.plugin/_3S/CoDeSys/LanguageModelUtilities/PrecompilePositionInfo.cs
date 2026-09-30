using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class PrecompilePositionInfo : IPrecompilePositionInfo2, IPrecompilePositionInfo
	{
		private ICodePosition _codePosition;

		private int _referencingPrecompileSignatureId;

		private string _name;

		private short _length;

		public ICodePosition CodePosition => _codePosition;

		public int ReferencingPrecompileSignatureId => _referencingPrecompileSignatureId;

		public string Name => _name;

		public short Length => _length;

		internal PrecompilePositionInfo(string name, ICodePosition codePosition, short length, int referencingPrecompileSignatureId)
		{
			_length = length;
			_name = name;
			_codePosition = codePosition;
			_referencingPrecompileSignatureId = referencingPrecompileSignatureId;
		}

		public override int GetHashCode()
		{
			return (_codePosition.GetHashCode() * 11) ^ _referencingPrecompileSignatureId;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PrecompilePositionInfo precompilePositionInfo))
			{
				return false;
			}
			if (precompilePositionInfo._codePosition.Equals(_codePosition))
			{
				return precompilePositionInfo._referencingPrecompileSignatureId == _referencingPrecompileSignatureId;
			}
			return false;
		}
	}
}
