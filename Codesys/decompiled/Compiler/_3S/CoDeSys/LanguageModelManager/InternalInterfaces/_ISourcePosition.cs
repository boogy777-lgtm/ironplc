using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISourcePosition : ISourcePosition
	{
		new short Length { get; set; }

		bool IsHidden { get; }

		void SetObjectIdentification(int nHandle, Guid messageGuid);

		IMinimalPosition GetMinimalPosition();
	}
}
