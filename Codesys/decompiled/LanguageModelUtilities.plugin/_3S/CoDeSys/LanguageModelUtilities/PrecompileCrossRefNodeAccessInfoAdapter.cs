using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class PrecompileCrossRefNodeAccessInfoAdapter : IAccessInfo2, IAccessInfo
	{
		private PrecompileCrossRefNode _node;

		private Guid? _appGuid;

		public ISourcePosition Position => _node;

		public AccessFlag Access => _node.Access;

		public Guid ApplicationGuid
		{
			get
			{
				if (!_appGuid.HasValue)
				{
					_appGuid = APEnvironmentFacade.Instance.GetApplicationGuid(_node.ObjectGuid, _node.ProjectHandle);
				}
				return _appGuid.Value;
			}
		}

		public Guid MessageGuid => _node.MessageGuid;

		public PrecompileCrossRefNodeAccessInfoAdapter(PrecompileCrossRefNode node)
		{
			_node = node;
		}

		public override bool Equals(object obj)
		{
			return _node.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _node.GetHashCode();
		}
	}
}
