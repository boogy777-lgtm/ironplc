#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{Text}")]
	internal class CrossRefNode : _ICrossReferenceNode, ICrossReferenceNode3, ICrossReferenceNode2, ICrossReferenceNode, IMessage
	{
		private static readonly string ACCESS_READ = Strings.Access_Read;

		private static readonly string ACCESS_WRITE = Strings.Access_Write;

		private static readonly string ACCESS_CALL = Strings.Access_Call;

		private static readonly string ACCESS_DECLARATIVE = Strings.Access_Declarative;

		private static readonly string ACCESS_UNKNOWN = Strings.Access_Unknown;

		private static readonly string ACCESS_TYPE = Strings.Access_Type;

		private static readonly string SCOPE_GLOBAL = Strings.Scope_Global;

		private static readonly string SCOPE_LOCAL = Strings.Scope_Local;

		private readonly string _stName;

		private readonly IAccessInfo _accessInfo;

		private readonly ISignature _signature;

		private readonly IMetaObject _metaObject;

		private readonly IIdentifierInfo _identInfo;

		private string _shadowConflictQualifier;

		private string _stShadowedNewText;

		private readonly ISourcePosition _sourcePos;

		private readonly IVariable _variable;

		private readonly string _stLocation = string.Empty;

		private IExprement _expressionAtSourcePosition;

		private CrossRefSearchType _searchType;

		private CrossRefOccurence _occurence;

		private CrossReferenceMatchType _matchType;

		private bool _bVisible = true;

		private LList<ICrossReferenceNode> _additionalNodes = new LList<ICrossReferenceNode>();

		public string NameText
		{
			get
			{
				if (_accessInfo.Access == AccessFlag.Declarative && _variable != null)
				{
					return _variable.OrgName;
				}
				if (_identInfo != null && _identInfo.Variable != null && (_accessInfo.Access == AccessFlag.Read || _accessInfo.Access == AccessFlag.Write || _accessInfo.Access == (AccessFlag.Read | AccessFlag.Write)))
				{
					if (!string.IsNullOrEmpty(_identInfo.Name) && Common.IsCompoAccess(_identInfo.Name))
					{
						return _identInfo.Name;
					}
					return _identInfo.Variable.OrgName;
				}
				return _stName;
			}
		}

		public string TypeText
		{
			get
			{
				if (_accessInfo.Access == AccessFlag.Declarative && _variable != null)
				{
					_ = string.Empty;
					if (_variable.Type != null)
					{
						return _variable.Type.ToString();
					}
					return "???";
				}
				if (_identInfo != null && _identInfo.Type != null && (_accessInfo.Access == AccessFlag.Read || _accessInfo.Access == AccessFlag.Write || _accessInfo.Access == (AccessFlag.Read | AccessFlag.Write) || _accessInfo.Access == AccessFlag.Call))
				{
					return _identInfo.Type.ToString();
				}
				if (_accessInfo.Access == AccessFlag.Implicit || _accessInfo.Access == AccessFlag.Call || _accessInfo.Access == AccessFlag.Unknown)
				{
					return string.Empty;
				}
				if (_identInfo != null && _identInfo.Signature != null)
				{
					return _identInfo.Signature.OrgName;
				}
				return "";
			}
		}

		public CrossReferenceScopeType ScopeType
		{
			get
			{
				if (_accessInfo.Access == AccessFlag.Declarative && _variable != null)
				{
					if (_variable.GetFlag(VarFlag.Global))
					{
						return CrossReferenceScopeType.Global;
					}
					return CrossReferenceScopeType.Local;
				}
				if ((_accessInfo.Access == AccessFlag.Read || _accessInfo.Access == AccessFlag.Write || _accessInfo.Access == (AccessFlag.Read | AccessFlag.Write)) && _identInfo != null && _identInfo.Variable != null)
				{
					if (_identInfo.Variable.GetFlag(VarFlag.Global))
					{
						return CrossReferenceScopeType.Global;
					}
					return CrossReferenceScopeType.Local;
				}
				return CrossReferenceScopeType.Unknown;
			}
		}

		public string ScopeTypeText
		{
			get
			{
				CrossReferenceScopeType scopeType = ScopeType;
				switch (scopeType)
				{
				case CrossReferenceScopeType.Global:
					return SCOPE_GLOBAL;
				case CrossReferenceScopeType.Local:
					return SCOPE_LOCAL;
				case CrossReferenceScopeType.Unknown:
					return "";
				default:
					return scopeType.ToString();
				}
			}
		}

		public string AddressText
		{
			get
			{
				if (_accessInfo.Access == AccessFlag.Declarative && _variable != null)
				{
					IDirectVariable address = _variable.Address;
					if (address != null)
					{
						return "AT " + address.ToString();
					}
				}
				if ((_accessInfo.Access == AccessFlag.Read || _accessInfo.Access == AccessFlag.Write) && _identInfo != null && _identInfo.Variable != null)
				{
					IDirectVariable address2 = _identInfo.Variable.Address;
					if (address2 != null)
					{
						return "AT " + address2.ToString();
					}
				}
				return string.Empty;
			}
		}

		public string AccessText
		{
			get
			{
				AccessFlag access = _accessInfo.Access;
				LList<string> val = new LList<string>();
				if ((access & AccessFlag.Call) != 0)
				{
					val.Add(Strings.Access_Call);
				}
				if ((access & AccessFlag.Declarative) != 0)
				{
					val.Add(Strings.Access_Declarative);
				}
				if ((access & AccessFlag.Read) != 0)
				{
					val.Add(Strings.Access_Read);
				}
				if ((access & AccessFlag.Type) != 0)
				{
					val.Add(Strings.Access_Type);
				}
				if ((access & AccessFlag.Unknown) != 0)
				{
					val.Add(Strings.Access_Unknown);
				}
				if ((access & AccessFlag.Write) != 0)
				{
					val.Add(Strings.Access_Write);
				}
				if ((access & AccessFlag.Address) != 0)
				{
					val.Add(Strings.Access_Address);
				}
				return string.Join(" | ", (IEnumerable<string>)val);
			}
		}

		public string CommentText
		{
			get
			{
				if (_accessInfo.Access == AccessFlag.Declarative && _variable != null && _variable.Comment != null)
				{
					return _variable.Comment;
				}
				return string.Empty;
			}
		}

		public IMetaObject MetaObject => _metaObject;

		public ISourcePosition SourcePosition
		{
			get
			{
				if (_sourcePos != null)
				{
					return _sourcePos;
				}
				if (_accessInfo != null)
				{
					return _accessInfo.Position;
				}
				return null;
			}
		}

		public IAccessInfo AccessInfo => _accessInfo;

		public CrossRefSearchType SearchType
		{
			get
			{
				return _searchType;
			}
			internal set
			{
				CheckFlag(value);
				_searchType = value;
			}
		}

		public CrossRefOccurence Occurence
		{
			get
			{
				return _occurence;
			}
			internal set
			{
				CheckFlag(value);
				_occurence = value;
			}
		}

		public CrossReferenceMatchType MatchType
		{
			get
			{
				return _matchType;
			}
			set
			{
				CheckFlag(value);
				_matchType = value;
			}
		}

		public IVariable Variable
		{
			get
			{
				if (_accessInfo.Access == AccessFlag.Declarative)
				{
					return _variable;
				}
				if (_identInfo != null && _identInfo.Variable != null && _variable == null)
				{
					return _identInfo.Variable;
				}
				if (_variable != null)
				{
					return _variable;
				}
				return null;
			}
		}

		public ISignature Signature => _signature;

		public string POUName => GetQualifiedName();

		public string Location => _stLocation;

		public bool Visible
		{
			get
			{
				return _bVisible;
			}
			internal set
			{
				_bVisible = value;
			}
		}

		public string Name => _stName;

		public IIdentifierInfo IdentifierInfo => _identInfo;

		public int ProjectHandle
		{
			get
			{
				if (_accessInfo != null && _accessInfo.Position != null)
				{
					return _accessInfo.Position.ProjectHandle;
				}
				return -1;
			}
		}

		public Guid PositionGuid
		{
			get
			{
				if (MessageGuid != Guid.Empty)
				{
					return MessageGuid;
				}
				return ObjectGuid;
			}
		}

		public Guid MessageGuid
		{
			get
			{
				if (_accessInfo != null && _accessInfo is IAccessInfo2)
				{
					return (_accessInfo as IAccessInfo2).MessageGuid;
				}
				return Guid.Empty;
			}
		}

		public Guid ObjectGuid
		{
			get
			{
				if (_accessInfo != null && _accessInfo.Position != null)
				{
					return _accessInfo.Position.ObjectGuid;
				}
				return Guid.Empty;
			}
		}

		public Guid ApplicationGuid
		{
			get
			{
				if (_accessInfo != null && _accessInfo is IAccessInfo2)
				{
					return (_accessInfo as IAccessInfo2).ApplicationGuid;
				}
				return Guid.Empty;
			}
		}

		public long Position
		{
			get
			{
				if (_sourcePos != null)
				{
					return _sourcePos.Position;
				}
				if (_accessInfo != null && _accessInfo.Position != null)
				{
					return _accessInfo.Position.Position;
				}
				return 0L;
			}
		}

		public short PositionOffset
		{
			get
			{
				if (_sourcePos != null)
				{
					return _sourcePos.PositionOffset;
				}
				if (_accessInfo != null && _accessInfo.Position != null)
				{
					return _accessInfo.Position.PositionOffset;
				}
				return 0;
			}
		}

		public short Length
		{
			get
			{
				if (_sourcePos != null)
				{
					return _sourcePos.Length;
				}
				if (_accessInfo != null && _accessInfo.Position != null)
				{
					return _accessInfo.Position.Length;
				}
				return 0;
			}
		}

		public string Text
		{
			get
			{
				string nameText = NameText;
				nameText += " (";
				nameText += AccessText;
				string addressText = AddressText;
				if (addressText != string.Empty)
				{
					nameText += ", ";
					nameText += addressText;
				}
				return nameText + ")";
			}
		}

		public Severity Severity => Severity.Text;

		public IIdentifierInfo ShadowConflictIdentifierInfo => null;

		public string ShadowingQualifier
		{
			get
			{
				if (!string.IsNullOrEmpty(_shadowConflictQualifier) && Shadowed)
				{
					return _shadowConflictQualifier;
				}
				if (Signature != null && _matchType.HasFlag(CrossReferenceMatchType.Shadowed) && Shadowed)
				{
					return Signature.OrgName;
				}
				return string.Empty;
			}
		}

		public string ShadowedOldText
		{
			get
			{
				if (!Shadowed)
				{
					return string.Empty;
				}
				return _stName;
			}
		}

		public string ShadowedNewText
		{
			get
			{
				if (!Shadowed)
				{
					return string.Empty;
				}
				return _stShadowedNewText;
			}
		}

		public bool Shadowed => _matchType.HasFlag(CrossReferenceMatchType.Shadowed);

		public IList<ICrossReferenceNode> AdditionalNodes
		{
			get
			{
				if (!Shadowed)
				{
					return (IList<ICrossReferenceNode>)new LList<ICrossReferenceNode>();
				}
				return (IList<ICrossReferenceNode>)_additionalNodes;
			}
		}

		public IExprement ExpressionAtSourcePosition
		{
			get
			{
				if (_expressionAtSourcePosition == null)
				{
					IPreCompileContext precom = null;
					_expressionAtSourcePosition = APEnvironmentFacade.Instance.LanguageModelMgr.FindExpressionAtSourcePosition(AccessInfo.Position, WhatToFind.WholeInstancePath, out precom);
				}
				return _expressionAtSourcePosition;
			}
		}

		public CrossRefNode(string stName, IAccessInfo accessInfo, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, IIdentifierInfo shadowConfilictIdentInfo, ISourcePosition sourcePos, string stShadowedNewText, IVariable variable, ISignature signature, IExprement expressionAtSourcePosition, IIdentifierInfo externalIdentifierInfo = null)
		{
			if (stName == null)
			{
				throw new ArgumentNullException("stName");
			}
			if (stName.Length == 0)
			{
				throw new ArgumentException("stName");
			}
			if (accessInfo == null)
			{
				throw new ArgumentNullException("accessInfo");
			}
			if (accessInfo.Position == null)
			{
				throw new ArgumentException("accessInfo");
			}
			_stName = stName;
			_accessInfo = accessInfo;
			_expressionAtSourcePosition = expressionAtSourcePosition;
			CheckFlag(searchType);
			CheckFlag(occurence);
			CheckFlag(matchType);
			_searchType = searchType;
			_occurence = occurence;
			_matchType = matchType;
			_sourcePos = sourcePos;
			if (signature != null)
			{
				_signature = signature;
			}
			else
			{
				_signature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(_accessInfo.Position.ObjectGuid, out var _);
			}
			_metaObject = APEnvironmentFacade.Instance.GetObjectToRead(_accessInfo.Position.ProjectHandle, GetRelevantGuid(_accessInfo));
			if (_metaObject != null)
			{
				_stLocation = _metaObject.Object.GetPositionText(_accessInfo.Position.PositionCombination);
				if (_stLocation == null)
				{
					_stLocation = string.Empty;
				}
			}
			if (_metaObject != null && (_accessInfo.Access == AccessFlag.Read || _accessInfo.Access == AccessFlag.Write || _accessInfo.Access == (AccessFlag.Read | AccessFlag.Write) || _accessInfo.Access == AccessFlag.Call))
			{
				if (externalIdentifierInfo == null)
				{
					_identInfo = Common.GetIdentifierInfoAtPosition(_accessInfo.Position, _stName);
				}
				else
				{
					_identInfo = externalIdentifierInfo;
				}
				if (_identInfo == null && _accessInfo is IAccessInfo2 && (_accessInfo as IAccessInfo2).MessageGuid != Guid.Empty && _accessInfo.Position != null)
				{
					MySourcePosition pos = new MySourcePosition(_accessInfo.Position.ObjectGuid, _accessInfo.Position);
					_identInfo = Common.GetIdentifierInfoAtPosition(pos, _stName);
				}
				if (_identInfo != null && _identInfo.Variable == null)
				{
					_variable = CrossReferenceService.GetVariable(stName, _accessInfo.Position.ObjectGuid, searchType, (_accessInfo is IAccessInfo2) ? ((IAccessInfo2)_accessInfo).ApplicationGuid : Guid.Empty);
				}
				if (_identInfo == null && signature != null)
				{
					if (variable != null)
					{
						_identInfo = new IdentifierInfo(variable, signature, stName, string.Empty, IdentifierInfoFlag.Variable, null);
					}
					else
					{
						_identInfo = new IdentifierInfo(signature, stName, string.Empty, IdentifierInfoFlag.Signature, null);
					}
				}
				if (_variable == null && variable != null)
				{
					_variable = variable;
				}
			}
			ShadowedBy((IIdentifierInfo2)shadowConfilictIdentInfo, stShadowedNewText);
			if (_accessInfo.Access != AccessFlag.Declarative || _signature == null)
			{
				return;
			}
			IVariable[] all = _signature.All;
			foreach (IVariable variable2 in all)
			{
				if (variable2.Name == _stName.ToUpperInvariant())
				{
					_variable = variable2;
					break;
				}
			}
		}

		private Guid GetRelevantGuid(IAccessInfo accessInfo)
		{
			if (!(accessInfo is IAccessInfo2 accessInfo2) || Guid.Empty == accessInfo2.MessageGuid)
			{
				return accessInfo.Position.ObjectGuid;
			}
			return accessInfo2.MessageGuid;
		}

		public void ShadowedBy(IIdentifierInfo2 shadowConfilictIdentInfo, string stShadowedNewText)
		{
			if (_metaObject == null || (_accessInfo.Access != AccessFlag.Read && _accessInfo.Access != AccessFlag.Write && _accessInfo.Access != (AccessFlag.Read | AccessFlag.Write) && _accessInfo.Access != AccessFlag.Call))
			{
				return;
			}
			if (shadowConfilictIdentInfo != null && shadowConfilictIdentInfo.Signature != null)
			{
				ISignature signature = null;
				IPreCompileContext precom;
				if (shadowConfilictIdentInfo.Signature.ParentObjectGuid != Guid.Empty && _signature != shadowConfilictIdentInfo.Signature)
				{
					if (shadowConfilictIdentInfo.Signature.ParentObjectGuid == _signature.ObjectGuid)
					{
						_shadowConflictQualifier = "this^";
					}
					else
					{
						signature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(shadowConfilictIdentInfo.Signature.ParentObjectGuid, out precom);
						if (signature != null)
						{
							_shadowConflictQualifier = "super^";
						}
					}
				}
				else
				{
					signature = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(shadowConfilictIdentInfo.Signature.ObjectGuid, out precom);
					_shadowConflictQualifier = signature.OrgName;
					CrossReferenceSourcePosition sourcePosition = new CrossReferenceSourcePosition(SourcePosition.ProjectHandle, SourcePosition.ObjectGuid, SourcePosition.Position, SourcePosition.PositionOffset, short.Parse(signature.OrgName.Length.ToString()));
					CrossReferenceIdentifierInfo crossReferenceIdentifierInfo = CrossReferenceService.CreateIdentifierInfo((IdentifierInfo is IIdentifierInfo2) ? ((IIdentifierInfo2)IdentifierInfo).ContainingSignature : null, IdentifierInfo.Name, IdentifierInfo.Comment, IdentifierInfo.Flags, IdentifierInfo.Type, IdentifierInfo.Variable, IdentifierInfo.Signature, IdentifierInfo.Scope) as CrossReferenceIdentifierInfo;
					crossReferenceIdentifierInfo.Variable = null;
					ICrossReferenceNode crossReferenceNode = CrossReferenceService.CreateAdditionalCrossRefNode(sourcePosition, signature.OrgName, precom.ApplicationGuid, AccessFlag.Read, crossReferenceIdentifierInfo);
					if (crossReferenceNode != null)
					{
						_additionalNodes.Add(crossReferenceNode);
					}
				}
			}
			else if (_signature != null)
			{
				_shadowConflictQualifier = "this^";
			}
			if (!string.IsNullOrEmpty(_shadowConflictQualifier) && !string.IsNullOrEmpty(stShadowedNewText))
			{
				_stShadowedNewText = _shadowConflictQualifier + "." + stShadowedNewText;
			}
			else
			{
				_stShadowedNewText = stShadowedNewText;
			}
		}

		private static void CheckFlag(CrossReferenceMatchType matchType)
		{
			if (Debugger.IsAttached)
			{
				Trace.Assert(Enum.IsDefined(typeof(CrossReferenceMatchType), matchType), "Conflicting match type flags defined.");
			}
		}

		private static void CheckFlag(CrossRefOccurence occurence)
		{
			if (Debugger.IsAttached)
			{
				Trace.Assert(Enum.IsDefined(typeof(CrossRefOccurence), occurence), "Conflicting occurence flags defined.");
			}
		}

		private static void CheckFlag(CrossRefSearchType searchType)
		{
			if (Debugger.IsAttached)
			{
				Trace.Assert(Enum.IsDefined(typeof(CrossRefSearchType), searchType), "Conflicting search type flags defined.");
			}
		}

		private string GetQualifiedName()
		{
			if (_signature == null)
			{
				return "???";
			}
			if ((_signature.POUType == Operator.Action || _signature.POUType == Operator.Method || _signature.POUType == Operator.Property) && _signature.ParentObjectGuid != Guid.Empty && _accessInfo != null && _accessInfo.Position != null && APEnvironmentFacade.Instance.ExistsObject(_accessInfo.Position.ProjectHandle, _signature.ParentObjectGuid))
			{
				string parentObjectName = APEnvironmentFacade.Instance.GetParentObjectName(_accessInfo.Position.ProjectHandle, _signature.ParentObjectGuid);
				string text = _signature.OrgName;
				if (_signature.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
				{
					if (text.StartsWith("__get"))
					{
						text = text.Replace("__get", "");
						if (!_signature.HasAttribute(CompileAttributes.ATTRIBUTE_TRANSITION))
						{
							text += ".Get";
						}
					}
					if (text.StartsWith("__set"))
					{
						text = text.Replace("__set", "");
						if (!_signature.HasAttribute(CompileAttributes.ATTRIBUTE_TRANSITION))
						{
							text += ".Set";
						}
					}
				}
				return parentObjectName + "." + text;
			}
			return _signature.OrgName;
		}

		public override bool Equals(object obj)
		{
			CrossRefNode crossRefNode = obj as CrossRefNode;
			if (crossRefNode != null && SourcePosition != null && crossRefNode.SourcePosition != null && AccessInfo != null && crossRefNode.AccessInfo != null)
			{
				if (PositionGuid == crossRefNode.PositionGuid && SourcePosition.Position == crossRefNode.SourcePosition.Position && SourcePosition.PositionOffset == crossRefNode.SourcePosition.PositionOffset && SourcePosition.Length == crossRefNode.SourcePosition.Length && AccessInfo.Access == crossRefNode.AccessInfo.Access)
				{
					return AccessInfo.Position.PositionCombination == crossRefNode.AccessInfo.Position.PositionCombination;
				}
				return false;
			}
			if ((crossRefNode != null && crossRefNode.SourcePosition == null && SourcePosition != null) || (crossRefNode != null && crossRefNode.AccessInfo == null && AccessInfo != null))
			{
				return false;
			}
			if (crossRefNode == null || crossRefNode.SourcePosition == null || SourcePosition != null)
			{
				if (crossRefNode == null || crossRefNode.AccessInfo == null)
				{
					return false;
				}
				_ = AccessInfo;
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (SourcePosition != null && AccessInfo != null)
			{
				return PositionGuid.GetHashCode() ^ SourcePosition.Position.GetHashCode() ^ SourcePosition.PositionOffset.GetHashCode() ^ SourcePosition.Length.GetHashCode() ^ AccessInfo.Access.GetHashCode() ^ AccessInfo.Position.PositionCombination.GetHashCode();
			}
			return base.GetHashCode();
		}
	}
}
