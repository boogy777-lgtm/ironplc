using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[TypeGuid("{F2F4D3AD-A63E-4233-A94F-A6ECF83297F5}")]
	[ReleasedInterface]
	public interface _ICompilerMessage : IMessage4, IMessage3, IMessage2, IMessage
	{
		new int ProjectHandle { get; set; }

		new Guid ObjectGuid { get; set; }

		new short Length { get; set; }

		new string Text { get; set; }

		new Severity Severity { get; set; }

		ShowAttribute ShowAttribute { get; set; }

		bool ShowPrecompile { get; }

		bool ShowCompile { get; }

		MessageId MessageId { get; }

		Guid SignatureGuid { get; set; }
	}
}
