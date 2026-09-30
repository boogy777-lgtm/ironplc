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
	internal class PrecompileCrossRefNode : IPrecompileCrossReferenceNode2, IPrecompileCrossReferenceNode, ICrossReferenceNode, IMessage, _ICrossReferenceNode, ICrossReferenceNode3, ICrossReferenceNode2, ISourcePosition
	{
		internal class InternalIdentifierInfo : IIdentifierInfo
		{
			public string Name { get; private set; }

			public string Comment { get; private set; }

			public IdentifierInfoFlag Flags { get; private set; }

			public IScope Scope { get; private set; }

			public ISignature Signature { get; private set; }

			public IVariable Variable { get; private set; }

			public IType Type { get; private set; }

			private static IdentifierInfoFlag GetFlags(PrecompileCrossRefNode node)
			{
				IdentifierInfoFlag identifierInfoFlag = IdentifierInfoFlag.None;
				if (node.Variable != null)
				{
					identifierInfoFlag |= IdentifierInfoFlag.Variable;
					if (node.Variable.GetFlag(VarFlag.Input))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Input;
					}
					if (node.Variable.GetFlag(VarFlag.Output))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Output;
					}
					if (node.Variable.GetFlag(VarFlag.Inout))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Inout;
					}
					if (node.Variable.GetFlag(VarFlag.External))
					{
						identifierInfoFlag |= IdentifierInfoFlag.External;
					}
					if (node.Variable.GetFlag(VarFlag.Local))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Local;
					}
					if (node.Variable.GetFlag(VarFlag.Global))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Global;
					}
					if (node.Variable.GetFlag(VarFlag.Temp))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Temporary;
					}
					if (node.Variable.GetFlag(VarFlag.Static))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Static;
					}
				}
				if (node.Signature != null)
				{
					identifierInfoFlag |= IdentifierInfoFlag.Signature;
					if (node.Signature.POUType == Operator.Program)
					{
						identifierInfoFlag |= IdentifierInfoFlag.Program;
					}
					if (node.Signature.POUType == Operator.Function)
					{
						identifierInfoFlag |= IdentifierInfoFlag.Function;
					}
					if (node.Signature.POUType == Operator.FunctionBlock)
					{
						identifierInfoFlag |= IdentifierInfoFlag.Functionblock;
					}
					if (node.Signature.POUType == Operator.Action)
					{
						identifierInfoFlag |= IdentifierInfoFlag.Action;
					}
					if (node.Signature.POUType == Operator.Method)
					{
						identifierInfoFlag |= IdentifierInfoFlag.Method;
					}
				}
				return identifierInfoFlag;
			}

			public InternalIdentifierInfo(PrecompileCrossRefNode node)
			{
				Name = node.Name;
				Comment = node.CommentText;
				Flags = GetFlags(node);
				Scope = null;
				Signature = node.ReferencedSignature;
				Variable = node.Variable;
				if (Variable != null)
				{
					Type = Variable.Type;
				}
			}
		}

		private readonly IPrecompilePositionInfo _posInfo;

		private readonly int _referencedSignatureId;

		private readonly int _referencedVariableId;

		private readonly Guid _alternativeMessageGuid;

		private IExprement _expressionAtSourcePosition;

		private string _stShadowedNewText;

		private string _shadowConflictQualifier;

		private readonly LList<ICrossReferenceNode> _additionalNodes = new LList<ICrossReferenceNode>();

		private IIdentifierInfo _info;

		public int ProjectHandle
		{
			get
			{
				if (ReferencedSignature == null)
				{
					return -1;
				}
				ISignature6 referencingSignature = ReferencingSignature;
				if (referencingSignature == null)
				{
					return -1;
				}
				return APEnvironmentFacade.Instance.GetProjectHandle(referencingSignature.LibraryPath);
			}
		}

		public CrossReferenceScopeType ScopeType
		{
			get
			{
				if (Variable != null)
				{
					if (Variable.GetFlag(VarFlag.Global))
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
				switch (ScopeType)
				{
				case CrossReferenceScopeType.Global:
					return Strings.Scope_Global;
				case CrossReferenceScopeType.Local:
					return Strings.Scope_Local;
				case CrossReferenceScopeType.Unknown:
					return string.Empty;
				default:
					throw new ArgumentException("Unknown CrossReferenceScopeType: " + ScopeType);
				}
			}
		}

		public string AddressText
		{
			get
			{
				if (Variable != null && Variable.Address != null)
				{
					return "AT " + Variable.Address.ToString();
				}
				return string.Empty;
			}
		}

		public string NameText
		{
			get
			{
				if (Variable != null)
				{
					IExprement expressionAtSourcePosition = ExpressionAtSourcePosition;
					if (expressionAtSourcePosition != null && expressionAtSourcePosition.ToString().ToUpper().Contains(Variable.Name.ToUpper()))
					{
						return expressionAtSourcePosition.ToString();
					}
					return Variable.OrgName;
				}
				IPreCompileContext precom = null;
				IExprement exprement = APEnvironmentFacade.Instance.LanguageModelMgr.FindExpressionAtSourcePosition(this, WhatToFind.PartialInstancePath, out precom);
				if (exprement != null && exprement.ToString() != string.Empty)
				{
					bool num = (Access & AccessFlag.Call) != 0;
					IVariableExpression variableExpression = exprement as IVariableExpression;
					if (num && variableExpression != null && variableExpression.Name.StartsWith("ImpVar__"))
					{
						return ReferencedSignature.OrgName;
					}
					return exprement.ToString();
				}
				if (ReferencedSignature.PrecompileParentId != -1)
				{
					ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(ReferencedSignature.PrecompileParentId);
					return $"{signatureForPrecompileID.OrgName}.{ReferencedSignature.OrgName}";
				}
				return ReferencedSignature.OrgName;
			}
		}

		public string TypeText
		{
			get
			{
				if (Variable != null)
				{
					return Variable.Type.ToString();
				}
				return ReferencedSignature.OrgName;
			}
		}

		public string Location
		{
			get
			{
				bool num = Access == AccessFlag.Declarative && ReferencedSignature != null && ReferencedSignature.POUType == Operator.Action;
				bool flag = Access == AccessFlag.Declarative && Variable != null && Variable.HasAttribute(CompileAttributes.ATTRIBUTE_TRANSITION);
				if (num || flag)
				{
					return Strings.LocationInImplicitDeclaration;
				}
				IMetaObject metaObject = MetaObject;
				if (metaObject != null)
				{
					return metaObject.Object.GetPositionText(PositionCombination);
				}
				return string.Empty;
			}
		}

		public string AccessText
		{
			get
			{
				AccessFlag access = _posInfo.CodePosition.Access;
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
				string result = string.Empty;
				if (_posInfo.CodePosition.Access == AccessFlag.Declarative && Variable != null && Variable.Comment != null)
				{
					result = Variable.Comment;
				}
				return result;
			}
		}

		public string Name => _posInfo.Name;

		public IIdentifierInfo IdentifierInfo
		{
			get
			{
				if (_info == null)
				{
					_info = new InternalIdentifierInfo(this);
				}
				return _info;
			}
		}

		public IIdentifierInfo ShadowConflictIdentifierInfo => null;

		public ISignature Signature => ReferencingSignature;

		public ISignature6 ReferencedSignature
		{
			get
			{
				if (_referencedSignatureId == -1)
				{
					return null;
				}
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(_referencedSignatureId);
			}
		}

		internal ISignature6 ReferencingSignature => APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(_posInfo.ReferencingPrecompileSignatureId);

		public IMetaObject MetaObject => APEnvironmentFacade.Instance.GetObjectToRead(ProjectHandle, MessageGuid);

		public IVariable Variable
		{
			get
			{
				if (ReferencedSignature != null && _referencedVariableId != -1)
				{
					return ReferencedSignature?.GetVariableForPrecompileId(_referencedVariableId);
				}
				if (ReferencingSignature != null && _posInfo is IPrecompilePositionInfo3)
				{
					IPrecompilePositionInfo3 precompilePositionInfo = _posInfo as IPrecompilePositionInfo3;
					return ReferencingSignature.GetVariableForPrecompileId(precompilePositionInfo.VariableIDAtCodePosition);
				}
				return null;
			}
		}

		public bool Visible => true;

		public CrossRefSearchType SearchType
		{
			get
			{
				if (Variable != null)
				{
					return CrossRefSearchType.Variables;
				}
				return CrossRefSearchType.Signatures;
			}
		}

		public CrossRefOccurence Occurence => CrossRefOccurence.LanguageModel;

		public CrossReferenceMatchType MatchType { get; set; } = CrossReferenceMatchType.Existing;


		private bool IsPropertyDeclaration
		{
			get
			{
				if (Access == AccessFlag.Declarative && Variable != null)
				{
					return Variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY);
				}
				return false;
			}
		}

		public Guid MessageGuid
		{
			get
			{
				if (_alternativeMessageGuid != Guid.Empty)
				{
					return _alternativeMessageGuid;
				}
				ISignature referencingSignature = ReferencingSignature;
				if (referencingSignature == null)
				{
					return Guid.Empty;
				}
				if (IsPropertyDeclaration)
				{
					return FindPropertyGuidForSignature(referencingSignature);
				}
				Guid guid = Guid.Empty;
				if (ReferencedSignature == ReferencingSignature && Variable != null && Access == AccessFlag.Declarative)
				{
					string attributeValue = Variable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID);
					if (!string.IsNullOrEmpty(attributeValue))
					{
						try
						{
							guid = new Guid(attributeValue);
						}
						catch
						{
							guid = Guid.Empty;
						}
					}
				}
				if (guid != Guid.Empty && APEnvironmentFacade.Instance.ExistsObject(ProjectHandle, guid))
				{
					return guid;
				}
				if (APEnvironmentFacade.Instance.ExistsObject(ProjectHandle, referencingSignature.ObjectGuid))
				{
					return referencingSignature.ObjectGuid;
				}
				if (APEnvironmentFacade.Instance.ExistsObject(ProjectHandle, referencingSignature.MessageGuid))
				{
					return referencingSignature.MessageGuid;
				}
				if (APEnvironmentFacade.Instance.ExistsObject(ProjectHandle, referencingSignature.ParentObjectGuid))
				{
					return referencingSignature.ParentObjectGuid;
				}
				return Guid.Empty;
			}
		}

		public Guid SignatureGuid => ReferencingSignature?.ObjectGuid ?? Guid.Empty;

		Guid ISourcePosition.ObjectGuid => SignatureGuid;

		Guid IMessage.ObjectGuid => MessageGuid;

		public long Position => _posInfo.CodePosition.EditorPosition;

		public short PositionOffset => _posInfo.CodePosition.PositionOffset;

		public short Length
		{
			get
			{
				if (!(_posInfo is IPrecompilePositionInfo2 precompilePositionInfo))
				{
					return (short)_posInfo.Name.Length;
				}
				return precompilePositionInfo.Length;
			}
		}

		public string Text
		{
			get
			{
				string addressText = AddressText;
				if (string.IsNullOrEmpty(addressText))
				{
					return $"{NameText} ({AccessText})";
				}
				return $"{NameText} {addressText} ({AccessText})";
			}
		}

		public Severity Severity => Severity.Text;

		public long PositionCombination => PositionHelper.CombinePosition(_posInfo.CodePosition.EditorPosition, _posInfo.CodePosition.PositionOffset);

		public AccessFlag Access => _posInfo.CodePosition.Access;

		public IAccessInfo AccessInfo => new PrecompileCrossRefNodeAccessInfoAdapter(this);

		public ISourcePosition SourcePosition => this;

		public string ShadowingQualifier
		{
			get
			{
				if (!string.IsNullOrEmpty(_shadowConflictQualifier) && Shadowed)
				{
					return _shadowConflictQualifier;
				}
				if (Signature != null && MatchType.HasFlag(CrossReferenceMatchType.Shadowed) && Shadowed)
				{
					return Signature.OrgName;
				}
				return string.Empty;
			}
		}

		public bool Shadowed => MatchType.HasFlag(CrossReferenceMatchType.Shadowed);

		public string ShadowedOldText
		{
			get
			{
				if (!Shadowed)
				{
					return string.Empty;
				}
				return Name;
			}
		}

		public string ShadowedNewText => _stShadowedNewText;

		public Guid ObjectGuid
		{
			get
			{
				if (AccessInfo != null && AccessInfo.Position != null)
				{
					return AccessInfo.Position.ObjectGuid;
				}
				return Guid.Empty;
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

		public Guid ApplicationGuid
		{
			get
			{
				if (AccessInfo != null && AccessInfo is IAccessInfo2)
				{
					return (AccessInfo as IAccessInfo2).ApplicationGuid;
				}
				return Guid.Empty;
			}
		}

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

		public PrecompileCrossRefNode(IPrecompilePositionInfo posInfo, int referencedSignatureId, int referencedVariableId, Guid alternativeMessageGuid)
		{
			_posInfo = posInfo;
			_referencedSignatureId = referencedSignatureId;
			_referencedVariableId = referencedVariableId;
			_alternativeMessageGuid = alternativeMessageGuid;
		}

		private Guid FindPropertyGuidForSignature(ISignature sign)
		{
			return APEnvironmentFacade.Instance.GetChildObject(ProjectHandle, sign.MessageGuid, Name);
		}

		public override int GetHashCode()
		{
			return _posInfo.GetHashCode() * 23 * _referencedSignatureId * 29 * _referencedVariableId;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PrecompileCrossRefNode precompileCrossRefNode))
			{
				return false;
			}
			if (precompileCrossRefNode._posInfo.Equals(_posInfo) && precompileCrossRefNode._referencedSignatureId == _referencedSignatureId)
			{
				return precompileCrossRefNode._referencedVariableId == _referencedVariableId;
			}
			return false;
		}

		public void ShadowedBy(IIdentifierInfo2 shadowConfilictIdentInfo, string stShadowedNewText)
		{
			if (AccessInfo.Access != AccessFlag.Read && AccessInfo.Access != AccessFlag.Write && AccessInfo.Access != AccessFlag.Call && AccessInfo.Access != (AccessFlag.Read | AccessFlag.Write) && AccessInfo.Access != (AccessFlag.Write | AccessFlag.Address))
			{
				return;
			}
			if (shadowConfilictIdentInfo != null && shadowConfilictIdentInfo.Signature != null)
			{
				ISignature signature = null;
				IPreCompileContext precom;
				if (shadowConfilictIdentInfo.Signature.ParentObjectGuid != Guid.Empty && Signature != shadowConfilictIdentInfo.Signature)
				{
					if (shadowConfilictIdentInfo.Signature.ParentObjectGuid == Signature.ObjectGuid)
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
					if (IdentifierInfo != null)
					{
						CrossReferenceSourcePosition sourcePosition = new CrossReferenceSourcePosition(SourcePosition.ProjectHandle, SourcePosition.ObjectGuid, SourcePosition.Position, SourcePosition.PositionOffset, short.Parse(signature.OrgName.Length.ToString()));
						CrossReferenceIdentifierInfo crossReferenceIdentifierInfo = CrossReferenceService.CreateIdentifierInfo((IdentifierInfo is IIdentifierInfo2) ? ((IIdentifierInfo2)IdentifierInfo).ContainingSignature : null, IdentifierInfo.Name, IdentifierInfo.Comment, IdentifierInfoFlag.Signature, null, null, IdentifierInfo.Signature, IdentifierInfo.Scope) as CrossReferenceIdentifierInfo;
						crossReferenceIdentifierInfo.Variable = null;
						ICrossReferenceNode crossReferenceNode = CrossReferenceService.CreateAdditionalCrossRefNode(sourcePosition, signature.OrgName, precom.ApplicationGuid, AccessFlag.Read, crossReferenceIdentifierInfo);
						if (crossReferenceNode != null)
						{
							_additionalNodes.Add(crossReferenceNode);
						}
					}
				}
			}
			else if (Signature != null)
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
	}
}
