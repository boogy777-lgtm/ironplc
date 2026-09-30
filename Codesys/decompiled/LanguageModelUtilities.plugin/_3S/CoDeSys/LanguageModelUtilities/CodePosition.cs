using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class CodePosition : ICodePosition2, ICodePosition
	{
		private AccessFlag _access;

		private ISourcePosition _Position;

		public long EditorPosition => _Position.Position;

		public short PositionOffset => _Position.PositionOffset;

		public AccessFlag Access => _access;

		public CodePosition(ISourcePosition sp, AccessFlag access)
		{
			_access = access;
			_Position = sp;
		}

		public bool GetAccessFlag(AccessFlag acc)
		{
			return (_access & acc) == acc;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CodePosition codePosition))
			{
				return false;
			}
			if (codePosition.Access == _access)
			{
				return codePosition._Position.Equals(_Position);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = ((_Position != null) ? _Position.GetHashCode() : 0);
			int access = (int)_access;
			int num2 = 449;
			return (num * num2) ^ access;
		}
	}
}
