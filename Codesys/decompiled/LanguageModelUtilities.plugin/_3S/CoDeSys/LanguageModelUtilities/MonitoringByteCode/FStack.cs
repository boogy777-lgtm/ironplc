using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal class FStack
	{
		private readonly ByteCmdCreator _bcc;

		private readonly Stack<int> _tempStackAreaStart = new Stack<int>();

		internal FStack(ByteCmdCreator bcc)
		{
			_bcc = bcc;
		}

		internal void Alloc(int bytes)
		{
			_bcc.FAlloc(bytes);
		}

		internal int Align(int pushedBytes)
		{
			int num = (pushedBytes + 3) & -4;
			int bytes = num - pushedBytes;
			Alloc(bytes);
			return num;
		}

		internal void BeginTemporaryArea()
		{
			_tempStackAreaStart.Push(_bcc.FStackTopBytes);
		}

		internal void EndTemporaryArea()
		{
			if (_tempStackAreaStart.Count > 0)
			{
				int num = _tempStackAreaStart.Pop();
				int fStackTopBytes = _bcc.FStackTopBytes;
				if (fStackTopBytes != num)
				{
					Alloc(num - fStackTopBytes);
				}
			}
		}
	}
}
