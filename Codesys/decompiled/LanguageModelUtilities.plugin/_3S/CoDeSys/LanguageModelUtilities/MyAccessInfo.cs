using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class MyAccessInfo : IAccessInfo2, IAccessInfo
	{
		private readonly Guid _applicationGuid;

		private readonly ISourcePosition _sourcePos;

		private readonly Guid _messageGuid;

		public Guid ApplicationGuid => _applicationGuid;

		public Guid MessageGuid => _messageGuid;

		public ISourcePosition Position => _sourcePos;

		public AccessFlag Access => AccessFlag.Type;

		public MyAccessInfo(Guid applicationGuid, Guid messageGuid, ISourcePosition sourcePos)
		{
			_applicationGuid = applicationGuid;
			_sourcePos = sourcePos;
			_messageGuid = messageGuid;
		}
	}
}
