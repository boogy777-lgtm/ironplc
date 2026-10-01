using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0083;

namespace \u000E
{
	// Token: 0x0200010E RID: 270
	internal sealed class \u0006 : EmptyVisitor351900
	{
		// Token: 0x060013FD RID: 5117 RVA: 0x0003A3AC File Offset: 0x000385AC
		private \u0006(IList<IAccessInfo> \u0093\u0002, string \u0017\u0002, RefType \u0094\u0002, Guid \u0095\u0002, int \u0096\u0002, IPreCompileContext \u0097\u0002)
		{
			this.\u0001 = \u0017\u0002;
			if (\u0017\u0002.Contains(".") && !\u0017\u0002.StartsWith("%"))
			{
				this.\u0001 = true;
			}
			else
			{
				this.\u0001 = false;
			}
			this.\u0001 = \u0093\u0002;
			this.\u0001 = null;
			this.\u0001 = null;
			this.\u0001 = \u0094\u0002;
			this.\u0001 = \u0095\u0002;
			this.\u0001 = \u0096\u0002;
			this.\u0001 = \u0097\u0002;
		}

		// Token: 0x060013FE RID: 5118 RVA: 0x0003A424 File Offset: 0x00038624
		private \u0006(IDictionary<string, IList<IAccessInfo>> \u0098\u0002, Regex \u0099\u0002, RefType \u0094\u0002, Guid \u0095\u0002, int \u0096\u0002, IPreCompileContext \u0097\u0002)
		{
			this.\u0001 = null;
			this.\u0001 = false;
			this.\u0001 = null;
			this.\u0001 = \u0099\u0002;
			this.\u0001 = \u0098\u0002;
			this.\u0001 = \u0094\u0002;
			this.\u0001 = \u0095\u0002;
			this.\u0001 = \u0096\u0002;
			this.\u0001 = \u0097\u0002;
		}

		// Token: 0x060013FF RID: 5119 RVA: 0x0003A47C File Offset: 0x0003867C
		internal static void \u0001(IList<IAccessInfo> \u0002, _ICompiledPOU \u0003, string \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			IStandardTraverser ivisit = new StandardTraverser(new global::\u000E.\u0006(\u0002, \u0004, \u0005, \u0003.ObjectGuid, \u0006, \u0007));
			\u0003.GetParseTree().Accept(ivisit);
		}

		// Token: 0x06001400 RID: 5120 RVA: 0x0003A4B0 File Offset: 0x000386B0
		internal static void \u0001(IList<IAccessInfo> \u0002, ISignature \u0003, string \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			global::\u000E.\u0006 u = new global::\u000E.\u0006(\u0002, \u0004, \u0005, \u0003.ObjectGuid, \u0006, \u0007);
			new StandardTraverser(u);
			u.\u0001(\u0003);
		}

		// Token: 0x06001401 RID: 5121 RVA: 0x0003A4D4 File Offset: 0x000386D4
		internal static void \u0001(IDictionary<string, IList<IAccessInfo>> \u0002, _ICompiledPOU \u0003, Regex \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			IStandardTraverser ivisit = new StandardTraverser(new global::\u000E.\u0006(\u0002, \u0004, \u0005, \u0003.ObjectGuid, \u0006, \u0007));
			\u0003.GetParseTree().Accept(ivisit);
		}

		// Token: 0x06001402 RID: 5122 RVA: 0x0003A508 File Offset: 0x00038708
		internal static void \u0001(IDictionary<string, IList<IAccessInfo>> \u0002, ISignature \u0003, Regex \u0004, RefType \u0005, int \u0006, IPreCompileContext \u0007)
		{
			global::\u000E.\u0006 u = new global::\u000E.\u0006(\u0002, \u0004, \u0005, \u0003.ObjectGuid, \u0006, \u0007);
			new StandardTraverser(u);
			u.\u0001(\u0003);
		}

		// Token: 0x06001403 RID: 5123 RVA: 0x0003A52C File Offset: 0x0003872C
		private bool \u0001(string \u0002)
		{
			if (this.\u0001 != null)
			{
				if (this.\u0001.Match(\u0002).Success)
				{
					return true;
				}
			}
			else if (this.\u0001)
			{
				if (\u0002.ToUpperInvariant().EndsWith(this.\u0001.ToUpperInvariant()))
				{
					return true;
				}
			}
			else if (string.Compare(\u0002, this.\u0001, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			return false;
		}

		// Token: 0x06001404 RID: 5124 RVA: 0x0003A58C File Offset: 0x0003878C
		private void \u0001(string \u0002, \u0083.\u0001 \u0003)
		{
			if (this.\u0001 != null)
			{
				this.\u0001.Add(\u0003);
			}
			if (this.\u0001 != null)
			{
				if (!this.\u0001.ContainsKey(\u0002))
				{
					this.\u0001[\u0002] = new LList<IAccessInfo>();
				}
				this.\u0001[\u0002].Add(\u0003);
			}
		}

		// Token: 0x06001405 RID: 5125 RVA: 0x0003A5E8 File Offset: 0x000387E8
		private void \u0001(string \u0002, int \u0003, \u0083.\u0001 \u0004)
		{
			if (this.\u0001 != null)
			{
				this.\u0001.Insert(\u0003, \u0004);
			}
			if (this.\u0001 != null)
			{
				if (!this.\u0001.ContainsKey(\u0002))
				{
					this.\u0001[\u0002] = new LList<IAccessInfo>();
				}
				this.\u0001[\u0002].Insert(\u0003, \u0004);
			}
		}

		// Token: 0x06001406 RID: 5126 RVA: 0x0003A644 File Offset: 0x00038844
		public void \u0001(ISignature \u0002)
		{
			foreach (_IVariable ivariable in ((_ISignature)\u0002).AllVariables)
			{
				Guid empty = Guid.Empty;
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID))
				{
					empty = new Guid(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID));
				}
				Guid u = this.MessageGuid;
				if (empty != Guid.Empty)
				{
					this.MessageGuid = empty;
				}
				if (ivariable.HasAttribute("map_to"))
				{
					this.\u0001(ivariable, empty);
				}
				else if (this.\u0001(ivariable.VersionedName) || this.\u0001(\u0002.Name + "." + ivariable.VersionedName))
				{
					_ISourcePosition isourcePosition = ivariable.SourcePosition as _ISourcePosition;
					isourcePosition.SetObjectIdentification(this.\u0001, this.ObjectGuid);
					if (this.\u0001 == RefType.var)
					{
						this.\u0001(ivariable.VersionedName, 0, new \u0083.\u0001(isourcePosition, AccessFlag.Declarative, this.\u0001.ApplicationGuid, empty));
					}
				}
				if (ivariable.Type != null)
				{
					this.\u0001(ivariable.Type);
				}
				if (ivariable.Initial != null)
				{
					base.Traverser.Push(AccessFlag.Read);
					(ivariable.Initial as _IExpression).Accept(base.Traverser);
					base.Traverser.Pop();
				}
				if (empty != Guid.Empty)
				{
					this.MessageGuid = u;
				}
			}
		}

		// Token: 0x06001407 RID: 5127 RVA: 0x0003A7CC File Offset: 0x000389CC
		private void \u0001(_IVariable \u0002, Guid \u0003)
		{
			string u = string.Empty;
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0002.GetAttributeValue("map_to"), false, false, false, false);
			_IExpression iexpression = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser4).ParseExpression() as _IExpression;
			while (iexpression != null)
			{
				short length;
				short positionOffset;
				if (iexpression is _ICompoAccessExpression)
				{
					u = (iexpression as _ICompoAccessExpression).Right.ToString();
					length = (iexpression as _ICompoAccessExpression).Right.Position.Length;
					positionOffset = (iexpression as _ICompoAccessExpression).Right.Position.PositionOffset;
					iexpression = ((iexpression as _ICompoAccessExpression).Left as _IExpression);
				}
				else
				{
					u = iexpression.ToString();
					length = iexpression.Position.Length;
					positionOffset = iexpression.Position.PositionOffset;
					iexpression = null;
				}
				if (this.\u0001(u))
				{
					_ISourcePosition isourcePosition = \u0002.SourcePosition as _ISourcePosition;
					short u2 = isourcePosition.PositionOffset + positionOffset;
					isourcePosition = \u0019.\u0003.\u0001(isourcePosition.ProjectHandle, isourcePosition.ObjectGuid, isourcePosition.Position, u2, length);
					isourcePosition.SetObjectIdentification(this.\u0001, this.ObjectGuid);
					if (this.\u0001 == RefType.var)
					{
						this.\u0001(u, 0, new \u0083.\u0001(isourcePosition, AccessFlag.Declarative, this.\u0001.ApplicationGuid, \u0003));
						return;
					}
					break;
				}
			}
		}

		// Token: 0x06001408 RID: 5128 RVA: 0x0003A928 File Offset: 0x00038B28
		private void \u0001(IType \u0002)
		{
			if (\u0002.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = \u0002 as _IUserdefType;
				base.Traverser.Push(AccessFlag.Type);
				(iuserdefType.NameExpression as _IExpression).Accept(base.Traverser);
				base.Traverser.Pop();
				return;
			}
			if (\u0002.Class == TypeClass.Pointer)
			{
				_IPointerType ipointerType = \u0002 as _IPointerType;
				this.\u0001(ipointerType._Base);
				return;
			}
			if (\u0002.Class == TypeClass.Reference)
			{
				_IReferenceType ireferenceType = \u0002 as _IReferenceType;
				this.\u0001(ireferenceType._Base);
				return;
			}
			if (\u0002.Class == TypeClass.Array)
			{
				_IArrayType iarrayType = \u0002 as _IArrayType;
				this.\u0001(iarrayType._Base);
				using (IEnumerator<_IArrayDimension> enumerator = iarrayType._Dimensions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IArrayDimension iarrayDimension = enumerator.Current;
						if (iarrayDimension._LowerBorder is IVariableExpression)
						{
							base.Traverser.Push(AccessFlag.Read);
							iarrayDimension._LowerBorder.Accept(base.Traverser);
							base.Traverser.Pop();
						}
						if (iarrayDimension._UpperBorder is IVariableExpression)
						{
							base.Traverser.Push(AccessFlag.Read);
							iarrayDimension._UpperBorder.Accept(base.Traverser);
							base.Traverser.Pop();
						}
					}
					return;
				}
			}
			if (\u0002.Class == TypeClass.String)
			{
				_IStringType istringType = \u0002 as _IStringType;
				if (istringType.Length != null)
				{
					base.Traverser.Push(AccessFlag.Read);
					istringType.Length.Accept(base.Traverser);
					base.Traverser.Pop();
					return;
				}
			}
			else if (\u0002.Class == TypeClass.WString)
			{
				_IWStringType iwstringType = \u0002 as _IWStringType;
				if (iwstringType.Length != null)
				{
					base.Traverser.Push(AccessFlag.Read);
					iwstringType.Length.Accept(base.Traverser);
					base.Traverser.Pop();
					return;
				}
			}
			else if (\u0002.Class == TypeClass.Subrange)
			{
				_ISubrangeType isubrangeType = \u0002 as _ISubrangeType;
				if (isubrangeType._LowerBorder is IVariableExpression)
				{
					base.Traverser.Push(AccessFlag.Read);
					isubrangeType._LowerBorder.Accept(base.Traverser);
					base.Traverser.Pop();
				}
				if (isubrangeType._UpperBorder is IVariableExpression)
				{
					base.Traverser.Push(AccessFlag.Read);
					isubrangeType._UpperBorder.Accept(base.Traverser);
					base.Traverser.Pop();
				}
			}
		}

		// Token: 0x06001409 RID: 5129 RVA: 0x0003AB90 File Offset: 0x00038D90
		private string \u0001(_IVariableExpression \u0002)
		{
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileSignatureId) as _ISignature;
			if (isignature == null)
			{
				return \u0002.Name;
			}
			if (isignature.POUType == Operator.VarGlobal)
			{
				return isignature.OrgName + "." + \u0002.Name;
			}
			return \u0002.Name;
		}

		// Token: 0x0600140A RID: 5130 RVA: 0x0003ABEC File Offset: 0x00038DEC
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			string text = variable.Name;
			if (this.\u0001)
			{
				text = this.\u0001(variable);
				if (text == variable.Name || !this.\u0001(text))
				{
					return;
				}
			}
			else if (!this.\u0001(text))
			{
				return;
			}
			_ISourcePosition isourcePosition = variable.Position as _ISourcePosition;
			if (isourcePosition != null)
			{
				if (isourcePosition.IsHidden)
				{
					return;
				}
				isourcePosition.SetObjectIdentification(this.\u0001, this.ObjectGuid);
			}
			if (this.\u0001 == RefType.call && (access & AccessFlag.Call) == AccessFlag.Call)
			{
				this.\u0001(variable.Name, new \u0083.\u0001(isourcePosition, AccessFlag.Call, this.\u0001.ApplicationGuid, this.MessageGuid));
				return;
			}
			if (this.\u0001 == RefType.var)
			{
				if ((access & AccessFlag.Read) == AccessFlag.Read && (access & AccessFlag.Write) == AccessFlag.Write)
				{
					this.\u0001(variable.Name, new \u0083.\u0001(isourcePosition, AccessFlag.Read | AccessFlag.Write, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				if ((access & AccessFlag.Read) == AccessFlag.Read)
				{
					this.\u0001(variable.Name, new \u0083.\u0001(isourcePosition, AccessFlag.Read, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				if ((access & AccessFlag.Write) == AccessFlag.Write)
				{
					this.\u0001(variable.Name, new \u0083.\u0001(isourcePosition, AccessFlag.Write, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				if ((access & AccessFlag.Declarative) == AccessFlag.Declarative)
				{
					this.\u0001(variable.Name, 0, new \u0083.\u0001(isourcePosition, AccessFlag.Declarative, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				if ((access & AccessFlag.Type) == AccessFlag.Type)
				{
					this.\u0001(variable.Name, 0, new \u0083.\u0001(isourcePosition, AccessFlag.Type, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				this.\u0001(variable.Name, new \u0083.\u0001(isourcePosition, AccessFlag.Unknown, this.\u0001.ApplicationGuid, this.MessageGuid));
			}
		}

		// Token: 0x0600140B RID: 5131 RVA: 0x0003ADB0 File Offset: 0x00038FB0
		public override void visit(_ICompoAccessExpression compo, AccessFlag access)
		{
			if (!this.\u0001(compo.ToString()))
			{
				return;
			}
			_ISourcePosition isourcePosition = compo.Position as _ISourcePosition;
			if (isourcePosition != null)
			{
				isourcePosition.SetObjectIdentification(this.\u0001, this.ObjectGuid);
			}
			if (this.\u0001 == RefType.call && (access & AccessFlag.Call) == AccessFlag.Call)
			{
				this.\u0001(compo.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Call, this.\u0001.ApplicationGuid, this.MessageGuid));
				return;
			}
			if (this.\u0001 == RefType.var)
			{
				if ((access & AccessFlag.Read) == AccessFlag.Read)
				{
					this.\u0001(compo.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Read, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				if ((access & AccessFlag.Write) == AccessFlag.Write)
				{
					this.\u0001(compo.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Write, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				if ((access & AccessFlag.Declarative) == AccessFlag.Declarative)
				{
					this.\u0001(compo.ToString(), 0, new \u0083.\u0001(isourcePosition, AccessFlag.Declarative, this.\u0001.ApplicationGuid, this.MessageGuid));
					return;
				}
				this.\u0001(compo.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Unknown, this.\u0001.ApplicationGuid, this.MessageGuid));
			}
		}

		// Token: 0x0600140C RID: 5132 RVA: 0x0003AED8 File Offset: 0x000390D8
		public override void visit(_IAddressExpression address)
		{
			if (this.\u0001 != RefType.address)
			{
				return;
			}
			if (!this.\u0001(address.ToString()))
			{
				return;
			}
			_ISourcePosition isourcePosition = address.Position as _ISourcePosition;
			isourcePosition.SetObjectIdentification(this.\u0001, this.ObjectGuid);
			AccessFlag topOfStack = base.Traverser.TopOfStack;
			if ((topOfStack & AccessFlag.Read) == AccessFlag.Read)
			{
				this.\u0001(address.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Read, this.\u0001.ApplicationGuid, this.MessageGuid));
				return;
			}
			if ((topOfStack & AccessFlag.Write) == AccessFlag.Write)
			{
				this.\u0001(address.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Write, this.\u0001.ApplicationGuid, this.MessageGuid));
				return;
			}
			this.\u0001(address.ToString(), new \u0083.\u0001(isourcePosition, AccessFlag.Unknown, this.\u0001.ApplicationGuid, this.MessageGuid));
		}

		// Token: 0x0600140D RID: 5133 RVA: 0x0003AFA4 File Offset: 0x000391A4
		public override void visit(_IPragmaStatement pragma)
		{
			if (pragma is _IMessageGuidPragmaStatement)
			{
				this.MessageGuid = (pragma as _IMessageGuidPragmaStatement).MessageGuid;
			}
		}

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x0600140E RID: 5134 RVA: 0x0003AFC0 File Offset: 0x000391C0
		// (set) Token: 0x0600140F RID: 5135 RVA: 0x0003AFC8 File Offset: 0x000391C8
		public Guid MessageGuid
		{
			get
			{
				return this.\u0002;
			}
			set
			{
				this.\u0002 = value;
			}
		}

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x06001410 RID: 5136 RVA: 0x0003AFD4 File Offset: 0x000391D4
		public Guid ObjectGuid
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x04000365 RID: 869
		private IList<IAccessInfo> \u0001;

		// Token: 0x04000366 RID: 870
		private string \u0001;

		// Token: 0x04000367 RID: 871
		private IDictionary<string, IList<IAccessInfo>> \u0001;

		// Token: 0x04000368 RID: 872
		private Regex \u0001;

		// Token: 0x04000369 RID: 873
		private bool \u0001;

		// Token: 0x0400036A RID: 874
		private RefType \u0001;

		// Token: 0x0400036B RID: 875
		private Guid \u0001;

		// Token: 0x0400036C RID: 876
		private Guid \u0002;

		// Token: 0x0400036D RID: 877
		private int \u0001;

		// Token: 0x0400036E RID: 878
		private IPreCompileContext \u0001;
	}
}
