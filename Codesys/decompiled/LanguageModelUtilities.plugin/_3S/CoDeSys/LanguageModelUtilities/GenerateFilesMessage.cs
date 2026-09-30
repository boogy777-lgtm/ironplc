using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{0710B44D-8C95-4d94-90E9-175B6C18B768}")]
	[StorageVersion("3.3.0.0")]
	public class GenerateFilesMessage : GenericObject2, IMessage
	{
		private string _stMsg;

		private Guid _objectGuid;

		private Severity _severity;

		private short _sLength;

		public int ProjectHandle => 0;

		public Guid ObjectGuid
		{
			get
			{
				return _objectGuid;
			}
			set
			{
				_objectGuid = value;
			}
		}

		public long Position => 0L;

		public short PositionOffset => -1;

		public short Length
		{
			get
			{
				return _sLength;
			}
			set
			{
				_sLength = value;
			}
		}

		public string Text
		{
			get
			{
				return _stMsg;
			}
			set
			{
				_stMsg = value;
			}
		}

		public Severity Severity
		{
			get
			{
				return _severity;
			}
			set
			{
				_severity = value;
			}
		}

		public GenerateFilesMessage()
		{
		}

		public GenerateFilesMessage(string stMsg, Guid objectGuid, Severity severity)
		{
			_stMsg = stMsg;
			_objectGuid = objectGuid;
			_severity = severity;
		}
	}
}
