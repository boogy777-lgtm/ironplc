using System;
using System.Collections.Generic;
using System.Linq;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;
using \u0084;

namespace \u0007
{
	// Token: 0x0200010D RID: 269
	internal sealed class \u0003 : EmptyVisitor351900
	{
		// Token: 0x060013DE RID: 5086 RVA: 0x00039218 File Offset: 0x00037418
		private \u0003(int \u008F\u0002, int \u0090\u0002, int \u0091\u0002, bool \u0092\u0002)
		{
			this.\u0001 = new LList<IPrecompilePositionInfo>();
			this.\u0002 = \u008F\u0002;
			this.\u0003 = \u0090\u0002;
			this.\u0001 = \u0091\u0002;
			this.\u0001 = \u0092\u0002;
		}

		// Token: 0x060013DF RID: 5087 RVA: 0x00039248 File Offset: 0x00037448
		internal static \u007F.\u0002 \u0001(string \u0002, ISourcePosition \u0003, AccessFlag \u0004, int \u0005)
		{
			ICodePosition u008A_u = \u0019.\u0003.\u0001(\u0003, \u0004);
			return new \u007F.\u0002(\u0002, u008A_u, 0, \u0005, \u0005, -1);
		}

		// Token: 0x060013E0 RID: 5088 RVA: 0x00039268 File Offset: 0x00037468
		internal static \u007F.\u0002 \u0002(string \u0002, ISourcePosition \u0003, AccessFlag \u0004, int \u0005)
		{
			return global::\u0007.\u0003.\u0001(\u0002, \u0003, \u0004, \u0005, -1);
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x00039274 File Offset: 0x00037474
		internal static \u007F.\u0002 \u0001(string \u0002, ISourcePosition \u0003, AccessFlag \u0004, int \u0005, int \u0006)
		{
			return global::\u0007.\u0003.\u0001(\u0002, \u0003, \u0004, \u0005, \u0005, \u0006);
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x00039284 File Offset: 0x00037484
		internal static \u007F.\u0002 \u0001(string \u0002, ISourcePosition \u0003, AccessFlag \u0004, int \u0005, int \u0006, int \u0007)
		{
			short u008B_u = 0;
			if (\u0003 != null)
			{
				u008B_u = \u0003.Length;
			}
			else if (\u0002 != null)
			{
				u008B_u = (short)\u0002.Length;
			}
			ICodePosition u008A_u = \u0019.\u0003.\u0001(\u0003, \u0004);
			return new \u007F.\u0002(\u0002, u008A_u, u008B_u, \u0005, \u0006, \u0007);
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x000392C0 File Offset: 0x000374C0
		internal static IList<IPrecompilePositionInfo> \u0001(_IStatement \u0002, int \u0003, int \u0004, int \u0005, bool \u0006)
		{
			global::\u0007.\u0003 u = new global::\u0007.\u0003(\u0003, \u0004, \u0005, \u0006);
			StandardTraverser ivisit = new StandardTraverser(u);
			\u0002.Accept(ivisit);
			return u.\u0001;
		}

		// Token: 0x060013E4 RID: 5092 RVA: 0x000392EC File Offset: 0x000374EC
		internal static IList<IPrecompilePositionInfo> \u0001(int \u0002, int \u0003, int \u0004, bool \u0005)
		{
			global::\u0007.\u0003 u = new global::\u0007.\u0003(\u0002, \u0003, \u0004, \u0005);
			new StandardTraverser(u);
			_ISignature u2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002) as _ISignature;
			u.\u0001(u2);
			return u.\u0001;
		}

		// Token: 0x060013E5 RID: 5093 RVA: 0x0003932C File Offset: 0x0003752C
		internal static IList<IPrecompilePositionInfo> \u0001(int \u0002, int \u0003, bool \u0004)
		{
			return global::\u0007.\u0003.\u0001(\u0002, \u0003, Helper.InvalidId, \u0004);
		}

		// Token: 0x060013E6 RID: 5094 RVA: 0x0003933C File Offset: 0x0003753C
		public void \u0001(_ISignature \u0002)
		{
			bool flag = this.\u0001 == Helper.InvalidId;
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.\u0003);
			bool flag2 = this.\u0002 == this.\u0003;
			this.\u0001(\u0002, flag, signatureForPrecompileID, flag2);
			this.\u0002(\u0002);
			bool u = true;
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				this.\u0001(flag, signatureForPrecompileID, flag2, ivariable);
				_IExpression iexpression = ivariable.Initial as _IExpression;
				if (iexpression != null)
				{
					base.Traverser.Push(AccessFlag.Read);
					iexpression.Accept(base.Traverser);
				}
				if (ivariable.HasAttribute("map_to"))
				{
					_IVariable ivariable2 = signatureForPrecompileID.GetVariableForPrecompileId(this.\u0001) as _IVariable;
					if (ivariable2 != null)
					{
						this.\u0001(\u0002, ivariable, ivariable2);
					}
				}
				_IUserdefType underlyingUserdefType = ivariable.Type.GetUnderlyingUserdefType();
				this.\u0001(ivariable, underlyingUserdefType);
				if (underlyingUserdefType != null && underlyingUserdefType.SignatureId == this.\u0003 && this.\u0001 == Helper.InvalidId)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0002(underlyingUserdefType.NameExpression.ToString(), underlyingUserdefType.NameExpression.Position, AccessFlag.Type, this.\u0002));
				}
				else
				{
					this.\u0001(ivariable, u);
				}
				u = false;
			}
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x000394B4 File Offset: 0x000376B4
		private void \u0002(_ISignature \u0002)
		{
			_ITypeExpression itypeExpression = \u0002.BaseExpression as _ITypeExpression;
			if (itypeExpression != null)
			{
				IGenericUserdefType genericUserdefType = itypeExpression.Type as IGenericUserdefType;
				if (genericUserdefType != null)
				{
					base.Traverser.Push(AccessFlag.Read);
					foreach (_IExpression iexpression in genericUserdefType.GenericConstantsInitializations)
					{
						iexpression.Accept(base.Traverser);
					}
					base.Traverser.Pop();
				}
			}
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x0003953C File Offset: 0x0003773C
		private void \u0001(bool \u0002, ISignature6 \u0003, bool \u0004, _IVariable \u0005)
		{
			if (\u0004)
			{
				if (\u0005.PrecompileId == this.\u0001)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0001(\u0005.OrgName, \u0005.SourcePosition, AccessFlag.Declarative, this.\u0002, \u0005.PrecompileId));
					return;
				}
				if (\u0002 && \u0005.GetFlag(VarFlag.Output) && \u0005.Name == \u0003.Name)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0001(\u0005.Name, \u0005.SourcePosition, AccessFlag.Declarative, this.\u0002, \u0005.PrecompileId));
				}
			}
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x000395DC File Offset: 0x000377DC
		private void \u0001(_IVariable \u0002, _IUserdefType \u0003)
		{
			if (\u0003 != null && \u0002.InputAssignments != null && \u0002.InputAssignments.Any<IAssignmentExpression>())
			{
				foreach (_IAssignmentExpression iassignmentExpression in \u0002.InputAssignments.Cast<_IAssignmentExpression>())
				{
					_IExpression iexpression = iassignmentExpression.RValue as _IExpression;
					if (iexpression != null)
					{
						base.Traverser.Push(AccessFlag.Read);
						iexpression.Accept(base.Traverser);
						base.Traverser.Pop();
					}
				}
			}
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x00039670 File Offset: 0x00037870
		private void \u0001(_ISignature \u0002, bool \u0003, ISignature6 \u0004, bool \u0005)
		{
			if (\u0004 == null || !\u0003)
			{
				return;
			}
			if (this.\u0002 == this.\u0003)
			{
				this.\u0001.Add(global::\u0007.\u0003.\u0002(\u0004.Name, \u0004.NameExpression.Position, AccessFlag.Declarative, this.\u0002));
			}
			foreach (_IExpression iexpression in \u0002.InterfaceExpressions.OfType<_IExpression>())
			{
				_IVariableExpression ivariableExpression = iexpression as _IVariableExpression;
				if (iexpression is _ICompoAccessExpression)
				{
					ivariableExpression = (((_ICompoAccessExpression)iexpression)._Right as _IVariableExpression);
				}
				if (ivariableExpression != null && ivariableExpression.PrecompileSignatureId == this.\u0003)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0002(ivariableExpression.Name, iexpression.Position, AccessFlag.Type, this.\u0002));
				}
			}
			IExpression baseExpression = \u0002.BaseExpression;
			if (baseExpression != null)
			{
				this.\u0001(baseExpression);
			}
			if (!\u0005 && \u0002.POUType == Operator.Method && \u0002.POUType == \u0004.POUType && \u0002.Name == \u0004.Name)
			{
				this.\u0001.Add(global::\u0007.\u0003.\u0002(\u0002.Name, \u0002.NameExpression.Position, AccessFlag.Type, this.\u0002));
			}
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x000397C0 File Offset: 0x000379C0
		private void \u0001(IExprement \u0002)
		{
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				if (ivariableExpression.PrecompileSignatureId == this.\u0003)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0002(ivariableExpression.Name, ivariableExpression.Position, AccessFlag.Type, this.\u0002));
					return;
				}
			}
			else
			{
				_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
				if (icompoAccessExpression != null)
				{
					this.\u0001(icompoAccessExpression._Right);
					return;
				}
				_ITypeExpression itypeExpression = \u0002 as _ITypeExpression;
				if (itypeExpression != null && itypeExpression.Type != null)
				{
					this.\u0001(itypeExpression.Type);
				}
			}
		}

		// Token: 0x060013EC RID: 5100 RVA: 0x00039844 File Offset: 0x00037A44
		private void \u0001(_IVariable \u0002, bool \u0003)
		{
			LList<IPrecompilePositionInfo> llist = null;
			if (\u0002.IsProperty)
			{
				llist = new LList<IPrecompilePositionInfo>();
				llist.AddRange(this.\u0001);
				this.\u0001.Clear();
			}
			if (\u0003)
			{
				_IEnumType ienumType = \u0002.Type as _IEnumType;
				if (ienumType != null)
				{
					base.Traverser.Push(AccessFlag.Read);
					_IVariableExpression defaultValue = ienumType._DefaultValue;
					if (defaultValue != null)
					{
						defaultValue.Accept(base.Traverser);
					}
					base.Traverser.Pop();
					goto IL_75;
				}
			}
			this.\u0001(\u0002.Type);
			IL_75:
			if (\u0002.IsProperty)
			{
				foreach (\u007F.\u0002 u in this.\u0001.OfType<\u007F.\u0002>())
				{
					u.MessageGuid = \u0002.MessageGuid;
				}
				this.\u0001.AddRange(llist);
			}
		}

		// Token: 0x060013ED RID: 5101 RVA: 0x00039924 File Offset: 0x00037B24
		private void \u0001(IType \u0002)
		{
			if (\u0002.Class == TypeClass.Userdef)
			{
				this.\u0001(\u0002 as IUserdefType);
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
				this.\u0001(\u0002 as IArrayType);
				return;
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
				this.\u0001(\u0002 as ISubrangeType);
			}
		}

		// Token: 0x060013EE RID: 5102 RVA: 0x00039A38 File Offset: 0x00037C38
		private void \u0001(ISubrangeType \u0002)
		{
			_ISubrangeType isubrangeType = \u0002 as _ISubrangeType;
			base.Traverser.Push(AccessFlag.Read);
			isubrangeType._LowerBorder.Accept(base.Traverser);
			base.Traverser.Pop();
			base.Traverser.Push(AccessFlag.Read);
			isubrangeType._UpperBorder.Accept(base.Traverser);
			base.Traverser.Pop();
		}

		// Token: 0x060013EF RID: 5103 RVA: 0x00039A9C File Offset: 0x00037C9C
		private void \u0001(IArrayType \u0002)
		{
			_IArrayType iarrayType = \u0002 as _IArrayType;
			this.\u0001(iarrayType._Base);
			foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
			{
				base.Traverser.Push(AccessFlag.Read);
				iarrayDimension._LowerBorder.Accept(base.Traverser);
				base.Traverser.Pop();
				base.Traverser.Push(AccessFlag.Read);
				iarrayDimension._UpperBorder.Accept(base.Traverser);
				base.Traverser.Pop();
			}
		}

		// Token: 0x060013F0 RID: 5104 RVA: 0x00039B44 File Offset: 0x00037D44
		private void \u0001(IUserdefType \u0002)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			base.Traverser.Push(AccessFlag.Type);
			(iuserdefType.NameExpression as _IExpression).Accept(base.Traverser);
			base.Traverser.Pop();
			IGenericUserdefType genericUserdefType = \u0002 as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				foreach (_IExprement iexprement in genericUserdefType.GenericConstantsInitializations)
				{
					base.Traverser.Push(AccessFlag.Read);
					iexprement.Accept(base.Traverser);
					base.Traverser.Pop();
				}
			}
		}

		// Token: 0x060013F1 RID: 5105 RVA: 0x00039BEC File Offset: 0x00037DEC
		private void \u0001(_ISignature \u0002, _IVariable \u0003, _IVariable \u0004)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0003.GetAttributeValue("map_to"), false, false, false, false);
			_IExpression iexpression = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser4).ParseExpression() as _IExpression;
			while (iexpression != null)
			{
				string text;
				short length;
				short positionOffset;
				if (iexpression is _ICompoAccessExpression)
				{
					text = (iexpression as _ICompoAccessExpression).Right.ToString();
					length = (iexpression as _ICompoAccessExpression).Right.Position.Length;
					positionOffset = (iexpression as _ICompoAccessExpression).Right.Position.PositionOffset;
					iexpression = ((iexpression as _ICompoAccessExpression).Left as _IExpression);
				}
				else
				{
					text = iexpression.ToString();
					length = iexpression.Position.Length;
					positionOffset = iexpression.Position.PositionOffset;
					iexpression = null;
				}
				if (string.Compare(text, \u0004.VersionedName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					_ISourcePosition isourcePosition = \u0003.SourcePosition as _ISourcePosition;
					short u = isourcePosition.PositionOffset + positionOffset;
					isourcePosition = \u0019.\u0003.\u0001(isourcePosition.ProjectHandle, isourcePosition.ObjectGuid, isourcePosition.Position, u, length);
					isourcePosition.SetObjectIdentification(isourcePosition.ProjectHandle, \u0002.ObjectGuid);
					this.\u0001.Add(global::\u0007.\u0003.\u0002(text, isourcePosition, AccessFlag.Read, this.\u0002));
					return;
				}
			}
		}

		// Token: 0x060013F2 RID: 5106 RVA: 0x00039D38 File Offset: 0x00037F38
		private void \u0001(_ICallExpression \u0002)
		{
			IList<_IExpression> emptyAssigns = \u0002.EmptyAssigns;
			if (emptyAssigns == null || emptyAssigns.Count == 0)
			{
				return;
			}
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.\u0003) as _ISignature;
			if (isignature != null)
			{
				IVariable[] all = isignature.All;
				if (all != null && 0 <= this.\u0001 && this.\u0001 < all.Length)
				{
					IVariable variable = all[this.\u0001];
					foreach (_IExpression iexpression in emptyAssigns)
					{
						_IVariableExpression ivariableExpression = iexpression as _IVariableExpression;
						if (ivariableExpression != null && variable.OrgName.Equals(ivariableExpression.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							this.\u0001.Add(global::\u0007.\u0003.\u0002(ivariableExpression.Name, ivariableExpression.Position, AccessFlag.None, this.\u0002));
						}
					}
				}
			}
		}

		// Token: 0x060013F3 RID: 5107 RVA: 0x00039E24 File Offset: 0x00038024
		public override void visit(_IIndexAccessExpression indexaccess)
		{
			if (this.\u0001 != Helper.InvalidId)
			{
				return;
			}
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(indexaccess.PrecompileSignatureId);
			if (signatureForPrecompileID != null)
			{
				IVariable variable = signatureForPrecompileID[indexaccess.PrecompileVariableId];
				if (variable != null)
				{
					_IArrayType iarrayType = variable.Type as _IArrayType;
					if (iarrayType != null && iarrayType.BaseType != null)
					{
						_IUserdefType iuserdefType = iarrayType.BaseType.DeRefType as _IUserdefType;
						if (iuserdefType != null && iuserdefType.SignatureId == this.\u0003)
						{
							this.\u0001.Add(global::\u0007.\u0003.\u0001(indexaccess.ToString(), indexaccess.Position, base.Traverser.TopOfStack, this.\u0002, indexaccess.PrecompileVariableId));
						}
					}
				}
			}
		}

		// Token: 0x060013F4 RID: 5108 RVA: 0x00039ED4 File Offset: 0x000380D4
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if (this.\u0001 != Helper.InvalidId)
			{
				if (variable.PrecompileVariableId == this.\u0001 && variable.PrecompileSignatureId == this.\u0003)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0001(variable.Name, variable.Position, access, this.\u0002, variable.PrecompileVariableId));
					return;
				}
			}
			else
			{
				this.\u0001(variable, access);
			}
		}

		// Token: 0x060013F5 RID: 5109 RVA: 0x00039F3C File Offset: 0x0003813C
		private void \u0001(_IVariableExpression \u0002, AccessFlag \u0003)
		{
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileSignatureId);
			if (signatureForPrecompileID == null)
			{
				return;
			}
			IVariable variable = signatureForPrecompileID[\u0002.PrecompileVariableId];
			if (variable != null)
			{
				_IUserdefType iuserdefType = \u0084.\u0004.\u0001(variable.Type) as _IUserdefType;
				if (iuserdefType != null && iuserdefType.SignatureId == this.\u0003)
				{
					bool flag = \u0003 == AccessFlag.Call;
					bool inLeftSideOfCompoAccess = ((_IStandardTraverser)base.Traverser).InLeftSideOfCompoAccess;
					if (!flag || !inLeftSideOfCompoAccess || !this.\u0001)
					{
						this.\u0001.Add(global::\u0007.\u0003.\u0001(\u0002.Name, \u0002.Position, \u0003, this.\u0002, signatureForPrecompileID.PrecompileId, \u0002.PrecompileVariableId));
					}
				}
				bool flag2 = this.\u0003 == \u0002.PrecompileSignatureId;
				bool flag3 = variable.GetFlag(VarFlag.Output);
				if (flag2 && flag3 && variable.Name == signatureForPrecompileID.Name)
				{
					this.\u0001.Add(global::\u0007.\u0003.\u0001(\u0002.Name, \u0002.Position, \u0003, this.\u0002, signatureForPrecompileID.PrecompileId, \u0002.PrecompileVariableId));
					return;
				}
			}
			else if (\u0002.PrecompileSignatureId == this.\u0003)
			{
				\u0003 = this.\u0001(\u0003);
				this.\u0001.Add(global::\u0007.\u0003.\u0002(\u0002.Name, \u0002.Position, \u0003, this.\u0002));
			}
		}

		// Token: 0x060013F6 RID: 5110 RVA: 0x0003A088 File Offset: 0x00038288
		private AccessFlag \u0001(AccessFlag \u0002)
		{
			if (base.Traverser.ImplicitOn && APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.\u0002).HasAttribute(CompileAttributes.ATTRIBUTE_TRANSITION))
			{
				\u0002 |= AccessFlag.Implicit;
			}
			return \u0002;
		}

		// Token: 0x060013F7 RID: 5111 RVA: 0x0003A0C0 File Offset: 0x000382C0
		public override void visit(_IBaseExpression baseexp)
		{
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.\u0002) as _ISignature;
			if (isignature != null && isignature.PrecompileBaseSignatureId != Helper.InvalidId && this.\u0001 == Helper.InvalidId)
			{
				bool flag = isignature.PrecompileBaseSignatureId != Helper.InvalidId && isignature.PrecompileBaseSignatureId == this.\u0003;
				if (!flag)
				{
					_ISignature isignature2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(isignature.PrecompileParentId) as _ISignature;
					if (isignature2 != null)
					{
						flag = (isignature2.PrecompileBaseSignatureId != Helper.InvalidId && isignature2.PrecompileBaseSignatureId == this.\u0003);
					}
				}
				if (flag)
				{
					AccessFlag topOfStack = base.Traverser.TopOfStack;
					this.\u0001.Add(global::\u0007.\u0003.\u0002(baseexp.ToString(), baseexp.Position, topOfStack, this.\u0002));
				}
			}
		}

		// Token: 0x060013F8 RID: 5112 RVA: 0x0003A1A0 File Offset: 0x000383A0
		public override void visit(_IThisExpression thisexp)
		{
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.\u0002) as _ISignature;
			if (isignature != null && this.\u0001 == Helper.InvalidId && (isignature.PrecompileId == this.\u0003 || isignature.PrecompileParentId == this.\u0003))
			{
				AccessFlag topOfStack = base.Traverser.TopOfStack;
				this.\u0001.Add(global::\u0007.\u0003.\u0002(thisexp.ToString(), thisexp.Position, topOfStack, this.\u0002));
			}
		}

		// Token: 0x060013F9 RID: 5113 RVA: 0x0003A228 File Offset: 0x00038428
		public override void visit(_ICallExpression call)
		{
			_IVariableExpression ivariableExpression;
			if (call.Callee is _ICompoAccessExpression)
			{
				ivariableExpression = (((_ICompoAccessExpression)call.Callee)._Right as _IVariableExpression);
			}
			else if (call.Callee is IGlobalScopeExpression)
			{
				ivariableExpression = (((IGlobalScopeExpression)call.Callee).Base as _IVariableExpression);
			}
			else
			{
				ivariableExpression = (call.Callee as _IVariableExpression);
			}
			if (ivariableExpression == null)
			{
				return;
			}
			this.\u0001(call, ivariableExpression);
			this.\u0001(call);
		}

		// Token: 0x060013FA RID: 5114 RVA: 0x0003A2A0 File Offset: 0x000384A0
		private void \u0001(_ICallExpression \u0002, _IVariableExpression \u0003)
		{
			if (\u0003.PrecompileSignatureId != this.\u0003)
			{
				return;
			}
			_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0003.PrecompileSignatureId) as _ISignature;
			if (isignature.POUType != Operator.Function && isignature.POUType != Operator.Method)
			{
				return;
			}
			IVariable[] allInputs = isignature.AllInputs;
			IAssignmentExpression[] inputAssigns = \u0002.InputAssigns;
			int num = Math.Min(allInputs.Length, inputAssigns.Length);
			for (int i = 0; i < num; i++)
			{
				_IAssignmentExpression iassignmentExpression = inputAssigns[i] as _IAssignmentExpression;
				if (iassignmentExpression.LValue is INullExpression)
				{
					_IVariable ivariable = allInputs[i] as _IVariable;
					if (this.\u0001 == ivariable.PrecompileId)
					{
						ISourcePosition u = (iassignmentExpression.Position == null) ? iassignmentExpression.RValue.Position : iassignmentExpression.Position;
						this.\u0001.Add(global::\u0007.\u0003.\u0001(ivariable.Name, u, AccessFlag.Write, this.\u0002));
					}
				}
			}
		}

		// Token: 0x060013FB RID: 5115 RVA: 0x0003A38C File Offset: 0x0003858C
		public override void visit(_INewExpression newexp)
		{
			this.\u0001(newexp.TypeToCreate);
		}

		// Token: 0x060013FC RID: 5116 RVA: 0x0003A39C File Offset: 0x0003859C
		public override void visit(_IHasTypeExpression hastype)
		{
			this.\u0001(hastype.ReferencedType);
		}

		// Token: 0x04000360 RID: 864
		private readonly LList<IPrecompilePositionInfo> \u0001;

		// Token: 0x04000361 RID: 865
		private readonly int \u0001;

		// Token: 0x04000362 RID: 866
		private readonly int \u0002;

		// Token: 0x04000363 RID: 867
		private readonly int \u0003;

		// Token: 0x04000364 RID: 868
		private readonly bool \u0001;
	}
}
