using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class MySourcePosition : ISourcePosition
	{
		private Guid _objectGuid;

		private ISourcePosition _posOriginal;

		public short Length => _posOriginal.Length;

		public long Position => _posOriginal.Position;

		public Guid ObjectGuid => _objectGuid;

		public short PositionOffset => _posOriginal.PositionOffset;

		public long PositionCombination => _posOriginal.PositionCombination;

		public int ProjectHandle => _posOriginal.ProjectHandle;

		public MySourcePosition(Guid objectGuid, ISourcePosition posOriginal)
		{
			_objectGuid = objectGuid;
			_posOriginal = posOriginal;
		}
	}
}
