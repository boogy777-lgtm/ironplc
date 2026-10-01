using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal class InterpreterStringLiteral
	{
		private readonly ByteCmdCreator _bcc;

		public InterpreterStringLiteral(ByteCmdCreator bcc)
		{
			_bcc = bcc;
		}

		public int StoreStringLiteral(ILiteralValue literal, TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.String:
				return StoreString(literal.String);
			case TypeClass.WString:
				return StoreWString(literal.String);
			default:
				return 0;
			}
		}

		private int StoreString(string literal)
		{
			char[] array = literal.ToCharArray();
			ushort num = (ushort)(array.Length + 1);
			_bcc.StartLiteral(num);
			char[] array2 = array;
			foreach (char c in array2)
			{
				LoadCharacter(c);
			}
			LoadCharacter('\0');
			return num;
		}

		private void LoadCharacter(char c)
		{
			_bcc.ByteCode.Add((byte)c);
		}

		private int StoreWString(string literal)
		{
			char[] array = literal.ToCharArray();
			ushort num = (ushort)((array.Length + 1) * 2);
			_bcc.StartLiteral(num);
			char[] array2 = array;
			foreach (char c in array2)
			{
				LoadWCharacter(c);
			}
			LoadWCharacter('\0');
			return num;
		}

		private void LoadWCharacter(char c)
		{
			byte item = (byte)((uint)((int)c >> 8) & 0xFFu);
			byte item2 = (byte)(c & 0xFFu);
			if (_bcc.ByteOrder == ByteOrder.Intel)
			{
				_bcc.ByteCode.Add(item2);
				_bcc.ByteCode.Add(item);
			}
			else
			{
				_bcc.ByteCode.Add(item);
				_bcc.ByteCode.Add(item2);
			}
		}
	}
}
