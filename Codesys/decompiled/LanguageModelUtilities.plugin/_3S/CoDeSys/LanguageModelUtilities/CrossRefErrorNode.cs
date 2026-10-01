using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{Text}")]
	internal class CrossRefErrorNode : ICrossReferenceNode, IMessage
	{
		private readonly Exception _exception;

		public CrossReferenceScopeType ScopeType => CrossReferenceScopeType.Unknown;

		public string ScopeTypeText => "";

		public string AddressText => "";

		public string NameText => _exception.Message;

		public string TypeText => _exception.GetType().Name;

		public string Location => _exception.StackTrace;

		public string AccessText => "";

		public string CommentText => _exception.ToString();

		public string Name => "";

		public IAccessInfo AccessInfo => new MyAccessInfo(Guid.Empty, Guid.Empty, new CrossReferenceSourcePosition(-1, Guid.Empty, 0L, 0, 0));

		public IIdentifierInfo IdentifierInfo => null;

		public IIdentifierInfo ShadowConflictIdentifierInfo => null;

		public ISignature Signature => null;

		public IMetaObject MetaObject => null;

		public IVariable Variable => null;

		public bool Visible => true;

		public CrossRefSearchType SearchType => CrossRefSearchType.None;

		public CrossRefOccurence Occurence => CrossRefOccurence.None;

		public CrossReferenceMatchType MatchType => CrossReferenceMatchType.None;

		public int ProjectHandle => -1;

		public Guid ObjectGuid => Guid.Empty;

		public long Position => 0L;

		public short PositionOffset => 0;

		public short Length => 0;

		public string Text => _exception.Message;

		public Severity Severity => Severity.Error;

		internal CrossRefErrorNode(Exception ex)
		{
			_exception = ex;
		}
	}
}
