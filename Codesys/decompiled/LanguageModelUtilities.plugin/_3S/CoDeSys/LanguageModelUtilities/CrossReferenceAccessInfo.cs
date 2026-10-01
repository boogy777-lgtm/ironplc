using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class CrossReferenceAccessInfo : IAccessInfo2, IAccessInfo
	{
		public Guid ApplicationGuid { get; private set; }

		public Guid MessageGuid { get; private set; }

		public ISourcePosition Position { get; private set; }

		public AccessFlag Access { get; internal set; }

		public CrossReferenceAccessInfo(ISourcePosition position, AccessFlag access, Guid applicationGuid, Guid messageGuid)
		{
			if (position == null)
			{
				throw new ArgumentNullException("position");
			}
			Position = position;
			Access = access;
			ApplicationGuid = applicationGuid;
			MessageGuid = messageGuid;
		}
	}
}
