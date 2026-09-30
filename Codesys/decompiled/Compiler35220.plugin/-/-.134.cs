using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u0011;
using \u0012;
using \u0019;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;
using \u0082;

namespace \u001A
{
	// Token: 0x02000183 RID: 387
	internal sealed class \u0006 : IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x06001A67 RID: 6759 RVA: 0x0005488C File Offset: 0x00052A8C
		public \u0006()
		{
		}

		// Token: 0x06001A68 RID: 6760 RVA: 0x000548C8 File Offset: 0x00052AC8
		public \u0006(string \u0017\u0002, _IPreCompileContext \u0097\u0002)
		{
			this.\u0001 = \u0017\u0002;
			this.\u0001 = \u0097\u0002;
			_IPreCompileContext3 ipreCompileContext = \u0097\u0002 as _IPreCompileContext3;
			if (ipreCompileContext != null)
			{
				this.\u0001 = ipreCompileContext.ReplaceConstants;
				return;
			}
			this.\u0001 = APEnvironmentFacade.Instance.CompileOptions.ReplaceConstants;
		}

		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x06001A69 RID: 6761 RVA: 0x00054948 File Offset: 0x00052B48
		public _ISignature Signature
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170005A4 RID: 1444
		// (get) Token: 0x06001A6A RID: 6762 RVA: 0x00054950 File Offset: 0x00052B50
		public Guid MessageGuid
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06001A6B RID: 6763 RVA: 0x00054958 File Offset: 0x00052B58
		private void \u0001(_IPOUDeclarationStatement \u0002)
		{
			if (this.\u0001.GetFlag(SignatureFlag.Abstract))
			{
				if (this.\u0001.POUType != Operator.Method && this.\u0001.POUType != Operator.FunctionBlock)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_AbstractOnFunctionBlocksAndMethodsOnly, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_AbstractOnFunctionBlocksAndMethodsOnly);
				}
				if (this.\u0001.GetFlag(SignatureFlag.Final) && (this.\u0001.POUType == Operator.Method || this.\u0001.POUType == Operator.FunctionBlock))
				{
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_AbstractAndFinalNotPossible, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u2, Severity.Error, MessageId.Err_AbstractAndFinalNotPossible);
				}
				if (this.\u0001.GetFlag(SignatureFlag.Private) && this.\u0001.POUType == Operator.Method)
				{
					string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_AbstractAndPrivateNotPossible, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u3, Severity.Error, MessageId.Err_AbstractAndPrivateNotPossible);
				}
			}
		}

		// Token: 0x06001A6C RID: 6764 RVA: 0x00054A70 File Offset: 0x00052C70
		private void \u0002(_IPOUDeclarationStatement \u0002)
		{
			_IMethodDeclarationStatement imethodDeclarationStatement = \u0002 as _IMethodDeclarationStatement;
			if (imethodDeclarationStatement != null && imethodDeclarationStatement.Overload)
			{
				this.\u0001.AddAttribute("overloaded", "");
			}
		}

		// Token: 0x06001A6D RID: 6765 RVA: 0x00054AA4 File Offset: 0x00052CA4
		private void \u0003(_IPOUDeclarationStatement \u0002)
		{
			if (!this.\u0001.HasAttribute("abstract"))
			{
				return;
			}
			if (this.\u0001.GetFlag(SignatureFlag.Abstract))
			{
				return;
			}
			_ISourcePosition isourcePosition = \u0002.Position as _ISourcePosition;
			Guid messageGuid;
			if (this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) && this.\u0001.HasAttribute("propertyobject-guid") && Guid.TryParse(this.\u0001.GetAttributeValue("propertyobject-guid"), out messageGuid))
			{
				isourcePosition = global::\u0019.\u0003.\u0001(isourcePosition.ProjectHandle, isourcePosition.ObjectGuid, isourcePosition.Position, isourcePosition.PositionOffset, isourcePosition.Length);
				isourcePosition.SetObjectIdentification(isourcePosition.ProjectHandle, messageGuid);
			}
			this.\u0001(isourcePosition, global::\u0003.\u0006.\u0001(MessageId.Wrn_AbstractKeywordMissing, Array.Empty<object>()), Severity.Warning, MessageId.Wrn_AbstractKeywordMissing);
		}

		// Token: 0x06001A6E RID: 6766 RVA: 0x00054B74 File Offset: 0x00052D74
		private void \u0001(_IExprement \u0002)
		{
			global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NotSupportedInInterface, Array.Empty<object>());
		}

		// Token: 0x06001A6F RID: 6767 RVA: 0x00054B88 File Offset: 0x00052D88
		private void \u0001(_ISourcePosition \u0002, string \u0003, Severity \u0004, MessageId \u0005)
		{
			if (this.\u0001 != Guid.Empty)
			{
				\u0002.SetObjectIdentification(\u0002.ProjectHandle, this.\u0001);
			}
			if (\u0004 == Severity.Warning)
			{
				this.\u0001.AddWarning(\u0002, \u0003, \u0005);
				return;
			}
			this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002, \u0003, \u0004, \u0005));
		}

		// Token: 0x06001A70 RID: 6768 RVA: 0x00054BE4 File Offset: 0x00052DE4
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				istatement.Accept(this);
			}
		}

		// Token: 0x06001A71 RID: 6769 RVA: 0x00054C30 File Offset: 0x00052E30
		internal void \u0001(_IStatement \u0002)
		{
			if (!(\u0002 is _ISequenceStatement))
			{
				\u0002.Accept(this);
				return;
			}
			\u0002.Accept(this);
			foreach (IStatement statement in (\u0002 as _ISequenceStatement).StatementList)
			{
				if (statement is IVariableDeclarationStatement)
				{
					string u = string.Format(\u0081.\u0002.Err_GlobalVariableListExpected, statement.ToString());
					if (this.\u0001 == null)
					{
						this.\u0001 = global::\u0019.\u0003.\u0001();
						this.\u0001.POUType = Operator.VarGlobal;
					}
					this.\u0001.AddError(global::\u0019.\u0003.\u0001(statement.Position, u, Severity.Error, MessageId.Err_GlobalVariableListExpected));
				}
			}
			if (this.\u0001 == null)
			{
				this.\u0001 = global::\u0019.\u0003.\u0001();
				this.\u0001.POUType = Operator.VarGlobal;
				string u2 = string.Format(\u0081.\u0002.Err_GlobalVariableListExpected, \u0002.ToString());
				this.\u0001.AddError(global::\u0019.\u0003.\u0001(null, u2, Severity.Error, MessageId.Err_GlobalVariableListExpected));
			}
			this.\u0001.SetFlag(SignatureFlag.Internal, true);
			using (IEnumerator<_IVariable> enumerator2 = this.\u0001.AllVariables.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (!enumerator2.Current.GetFlag(VarFlag.Internal))
					{
						this.\u0001.SetFlag(SignatureFlag.Internal, false);
						break;
					}
				}
			}
			Guid guid = new Guid("{d433c4d1-208a-4b7a-b4ba-e3c77e67e46b}");
			if (this.\u0001.GetAttributeValue("authentification") == guid.ToString())
			{
				VarFlag varFlag = VarFlag.None;
				foreach (_IVariable ivariable in this.\u0001.AllVariables)
				{
					VarFlag varFlag2 = ivariable.Flags;
					varFlag2 &= ~VarFlag.NoIECIdent;
					if ((varFlag2 != (VarFlag.Persistent | VarFlag.Global | VarFlag.Absolut) && varFlag2 != (VarFlag.Retain | VarFlag.Persistent | VarFlag.Global | VarFlag.Absolut)) || (varFlag != VarFlag.None && varFlag2 != varFlag))
					{
						string u3 = global::\u000E.\u0018.\u0001(MessageId.Err_PersistentWrongVariable);
						this.\u0001.AddError(global::\u0019.\u0003.\u0001(ivariable._SourcePosition, u3, Severity.Error, MessageId.Err_PersistentWrongVariable));
					}
					else
					{
						varFlag = varFlag2;
					}
					if (ivariable.Address != null)
					{
						string u4 = global::\u000E.\u0018.\u0001(MessageId.Err_PersistentNoAddress);
						this.\u0001.AddError(global::\u0019.\u0003.\u0001(ivariable._SourcePosition, u4, Severity.Error, MessageId.Err_PersistentNoAddress));
					}
				}
			}
		}

		// Token: 0x06001A72 RID: 6770 RVA: 0x00054EC4 File Offset: 0x000530C4
		public void \u0001(_ICommentStatement \u0002)
		{
			if (\u0002.DocComment)
			{
				if (this.\u0002 == null)
				{
					this.\u0002 = new LStringBuilder();
				}
				if (this.\u0002.Length != 0)
				{
					this.\u0002.AppendLine();
				}
				this.\u0002.Append(\u0002.Text);
			}
			else if (this.\u0001 == null || this.\u0001.Length < 10000)
			{
				if (this.\u0001 == null)
				{
					this.\u0001 = new LStringBuilder();
				}
				if (this.\u0001.Length != 0)
				{
					this.\u0001.AppendLine();
				}
				this.\u0001.Append(\u0002.Text);
			}
			if (\u0002.DocComment || this.\u0001 == null)
			{
				return;
			}
			IList<_IVariable> allVariables = this.\u0001.AllVariables;
			bool flag = false;
			for (int i = allVariables.Count - 1; i >= 0; i--)
			{
				_IVariable ivariable = allVariables[i];
				if (ivariable._SourcePosition.Position != \u0002.Position.Position || !(ivariable.MessageGuid == this.MessageGuid))
				{
					break;
				}
				long num = (long)ivariable._SourcePosition.PositionOffset;
				long num2 = (long)\u0002.Position.PositionOffset;
				if ((num < num2 && num + (long)ivariable._SourcePosition.Length > num2) || (num2 < num && num2 + (long)\u0002.Position.Length > num))
				{
					break;
				}
				ivariable.Comment = this.\u0001.ToString();
				flag = true;
			}
			if (flag)
			{
				this.\u0001 = null;
			}
		}

		// Token: 0x06001A73 RID: 6771 RVA: 0x00055054 File Offset: 0x00053254
		public void \u0001(_IPragmaStatement \u0002)
		{
			string text = \u0002.Text;
			IPragmaScanner pragmaScanner = \u0082.\u0005.Singleton.Create(text);
			IPragmaToken pragmaToken;
			if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.attribute && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
			{
				string @string = pragmaToken.String;
				string u = string.Empty;
				if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.assign && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
				{
					u = pragmaToken.String;
				}
				_ICompilerAttribute icompilerAttribute = global::\u0019.\u0003.\u0001(@string, u);
				this.\u0001[icompilerAttribute] = \u0002.Position;
				this.\u0001.Add(icompilerAttribute);
				return;
			}
			if (typeof(_IMessageGuidPragmaStatement).IsAssignableFrom(\u0002.GetType()))
			{
				_IMessageGuidPragmaStatement imessageGuidPragmaStatement = \u0002 as _IMessageGuidPragmaStatement;
				try
				{
					this.\u0001 = imessageGuidPragmaStatement.MessageGuid;
				}
				catch
				{
					this.\u0001 = Guid.Empty;
				}
				this.\u0001 = null;
				this.\u0002 = null;
				return;
			}
			if (typeof(_IWarningDisableRestorePragmaStatement).IsAssignableFrom(\u0002.GetType()))
			{
				_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = \u0002 as _IWarningDisableRestorePragmaStatement;
				if (iwarningDisableRestorePragmaStatement.Restore && this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Remove(iwarningDisableRestorePragmaStatement.Id);
				}
				if (!iwarningDisableRestorePragmaStatement.Restore && !this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Add(iwarningDisableRestorePragmaStatement.Id, iwarningDisableRestorePragmaStatement.Id);
				}
			}
		}

		// Token: 0x06001A74 RID: 6772 RVA: 0x000551D0 File Offset: 0x000533D0
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			if (this.\u0001 == null || \u0002.Type == null)
			{
				return;
			}
			if (\u0002.Type.Class == TypeClass.Bit)
			{
				this.\u0005(\u0002);
			}
			else if (\u0002.Type.Class == TypeClass.__Vector)
			{
				this.\u0004(\u0002);
			}
			this.\u0002(\u0002);
			_IType itype = this.\u0001(\u0002);
			IImplicitEnumerationType implicitEnumerationType = itype as IImplicitEnumerationType;
			if (implicitEnumerationType == null)
			{
				_IArrayType iarrayType = itype as _IArrayType;
				if (iarrayType != null)
				{
					implicitEnumerationType = (iarrayType.BaseType as IImplicitEnumerationType);
				}
			}
			if (implicitEnumerationType != null)
			{
				this.\u0001.AddAttribute("contains_implicit_enum", null);
			}
			bool u = this.\u0001.POUType == Operator.VarGlobal && this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL);
			foreach (_IExpression u2 in \u0002.NameList)
			{
				this.\u0001(\u0002, itype, u, u2);
			}
			this.\u0001.Clear();
			this.\u0001.Clear();
			this.\u0001 = null;
			this.\u0002 = null;
		}

		// Token: 0x06001A75 RID: 6773 RVA: 0x000552F0 File Offset: 0x000534F0
		private void \u0001(_IVariableDeclarationStatement \u0002, _IType \u0003, bool \u0004, _IExpression \u0005)
		{
			\u0005.Accept(this);
			_IVariable ivariable = global::\u0019.\u0003.\u0001(\u0005.Position as _ISourcePosition);
			ivariable.Name = \u0005.ToString();
			\u001A.\u0006.\u0001(ivariable);
			ivariable._Type = \u0003;
			ivariable.Address = \u0002.Address;
			ivariable.SetFlag(VarFlag.TaskLocal, \u0004);
			this.\u0003(\u0002);
			ivariable.Initial = \u0002.Initial;
			ICollection<_IAssignmentExpression> inputAssigns = \u0002.InputAssigns;
			if (inputAssigns != null)
			{
				ivariable.SetInputAssignments(inputAssigns);
			}
			ivariable.SetFlag(this.\u0001, true);
			LList<Tuple<string, ISourcePosition>> llist = new LList<Tuple<string, ISourcePosition>>();
			this.\u0001(\u0002, ivariable, llist);
			this.\u0001(ivariable, llist);
			this.\u0001(ivariable);
			this.\u0002(ivariable);
			this.\u0001(\u0002, ivariable);
			if (this.\u0001 != Guid.Empty)
			{
				ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID, this.\u0001.ToString());
			}
			if (\u0002.OldInputAssigns)
			{
				ivariable.AddAttribute("old_input_assignments", "");
			}
			this.\u0003(ivariable);
			this.\u0004(ivariable);
			if (!this.\u0001(ivariable))
			{
				if (!this.\u0001.AddVariable(ivariable))
				{
					if (!ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_IGNORE_DUPLICATE))
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_VariableDuplicate, new object[]
						{
							ivariable.OrgName,
							this.\u0001.OrgName
						});
						this.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_VariableDuplicate);
					}
				}
				else
				{
					\u001A.\u0006.\u0002(ivariable);
					this.\u0005(ivariable);
				}
			}
			this.\u0006(ivariable);
			if (this.\u0001.HasAttribute("singleton"))
			{
				ivariable.SetFlag(VarFlag.Absolut | VarFlag.Static, true);
			}
			this.\u0007(ivariable);
			this.\u0008(ivariable);
			if (!this.\u0001(ivariable))
			{
				string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_ATDeclarationNotAllowed, Array.Empty<object>());
				this.\u0001(ivariable._SourcePosition, u2, Severity.Error, MessageId.Err_ATDeclarationNotAllowed);
			}
		}

		// Token: 0x06001A76 RID: 6774 RVA: 0x000554D0 File Offset: 0x000536D0
		private void \u0001(_IVariable \u0002, LList<Tuple<string, ISourcePosition>> \u0003)
		{
			if (!this.\u0001.IsCompiledLibraryObject && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(this.\u0001, GUIHidingFlags.AllCommon) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(\u0002, GUIHidingFlags.AllCommon))
			{
				foreach (Tuple<string, ISourcePosition> tuple in \u0003)
				{
					this.\u0001(tuple.Item2 as _ISourcePosition, \u001A.\u0006.\u0001(tuple.Item1), Severity.Warning, MessageId.Wrn_AttributeCheck);
				}
			}
		}

		// Token: 0x06001A77 RID: 6775 RVA: 0x00055580 File Offset: 0x00053780
		private static string \u0001(string \u0002)
		{
			return \u0002 + " " + \u0081.\u0001.AttributeHasNoEffect;
		}

		// Token: 0x06001A78 RID: 6776 RVA: 0x00055594 File Offset: 0x00053794
		private void \u0001(_IVariable \u0002)
		{
			int num = 0;
			foreach (string stValue in this.\u0001.Keys)
			{
				string stAttribute = string.Format("suppress_warning_{0}", num++);
				\u0002.AddAttribute(stAttribute, stValue);
			}
		}

		// Token: 0x06001A79 RID: 6777 RVA: 0x00055604 File Offset: 0x00053804
		private void \u0002(_IVariableDeclarationStatement \u0002)
		{
			if (\u0002.RefAssignInitialisation && \u0002.Type.Class != TypeClass.Reference)
			{
				this.\u0001(\u0002.GetPosition() as _ISourcePosition, \u0081.\u0002.Err_RefAssignOnlyForReferenceTypes, Severity.Error, MessageId.Err_RefAssignOnlyForReferenceTypes);
			}
		}

		// Token: 0x06001A7A RID: 6778 RVA: 0x0005563C File Offset: 0x0005383C
		private static void \u0001(_IVariable \u0002)
		{
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				string name = \u0002.Name;
				for (int i = 0; i < name.Length; i++)
				{
					if (!Scanner.\u0001(name[i]))
					{
						\u0002.SetFlag(VarFlag.NoIECIdent, true);
						return;
					}
				}
			}
		}

		// Token: 0x06001A7B RID: 6779 RVA: 0x00055694 File Offset: 0x00053894
		private void \u0003(_IVariableDeclarationStatement \u0002)
		{
			if (\u0002.Address != null && \u0002.Address.Incomplete && this.\u0001 != null)
			{
				if (this.\u0001.POUType == Operator.Method || this.\u0001.POUType == Operator.Function)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_NoVarConfigInMethodOrFunction, Array.Empty<object>());
					this.\u0001(\u0002.GetPosition() as _ISourcePosition, u, Severity.Error, MessageId.Err_NoVarConfigInMethodOrFunction);
				}
				this.\u0001.SetFlag(SignatureFlag.ContainsVarConfig, true);
			}
		}

		// Token: 0x06001A7C RID: 6780 RVA: 0x00055718 File Offset: 0x00053918
		private void \u0001(_IVariableDeclarationStatement \u0002, _IVariable \u0003, LList<Tuple<string, ISourcePosition>> \u0004)
		{
			foreach (_ICompilerAttribute icompilerAttribute in this.\u0001)
			{
				IList<string> list;
				if (!APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.CheckAttribute(icompilerAttribute.Name, icompilerAttribute.Value, AttributeScope.Variable, this.\u0001, \u0003, out list))
				{
					ISourcePosition item = \u0002.Position;
					if (this.\u0001.ContainsKey(icompilerAttribute))
					{
						item = this.\u0001[icompilerAttribute];
					}
					foreach (string item2 in list)
					{
						\u0004.Add(new Tuple<string, ISourcePosition>(item2, item));
					}
				}
				if (icompilerAttribute.Name == CompileAttributes.ATTRIBUTE_NOINIT || icompilerAttribute.Name == CompileAttributes.ATTRIBUTE_NO_INIT1 || icompilerAttribute.Name == CompileAttributes.ATTRIBUTE_NO_INIT2)
				{
					\u0003.SetFlag(VarFlag.NoInit, true);
				}
				if (icompilerAttribute.Name == CompileAttributes.ATTRIBUTE_READ_ONLY)
				{
					\u0003.SetFlag(VarFlag.Constant, true);
				}
				else
				{
					\u0003.AddAttribute(icompilerAttribute.Name, icompilerAttribute.Value);
				}
			}
		}

		// Token: 0x06001A7D RID: 6781 RVA: 0x00055884 File Offset: 0x00053A84
		private void \u0002(_IVariable \u0002)
		{
			if (this.\u0001 != null)
			{
				\u0002.Comment = this.\u0001.ToString();
			}
			if (this.\u0002 != null)
			{
				\u0002.DocuComment = this.\u0002.ToString();
			}
		}

		// Token: 0x06001A7E RID: 6782 RVA: 0x000558B8 File Offset: 0x00053AB8
		private void \u0001(_IVariableDeclarationStatement \u0002, _IVariable \u0003)
		{
			if (this.\u0001.POUType == Operator.Program && !\u0003.GetFlag(VarFlag.Temp))
			{
				\u0003.SetFlag(VarFlag.Absolut, true);
			}
			if (\u0003.GetFlag(VarFlag.Input) && \u0003.GetFlag(VarFlag.Constant))
			{
				\u0003.SetFlag(VarFlag.Constant, false);
				\u0003.SetFlag(VarFlag.ReplacedConstant, false);
				\u0003.AddAttribute("input_constant", "");
			}
			if (TypeTable.IsBlock(\u0002.Type.Class))
			{
				\u0003.SetFlag(VarFlag.ReplacedConstant, false);
			}
			else if (\u0002.Type.Class == TypeClass.Pointer)
			{
				\u0003.SetFlag(VarFlag.ReplacedConstant, false);
			}
			else if (\u0003.GetFlag(VarFlag.Inout))
			{
				\u0003.SetFlag(VarFlag.ReplacedConstant, false);
			}
			else if (\u0003.GetFlag(VarFlag.Constant))
			{
				\u0003.SetFlag(VarFlag.ReplacedConstant, this.\u0001);
				if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CONSTANT_REPLACED))
				{
					\u0003.SetFlag(VarFlag.ReplacedConstant, true);
				}
				if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CONSTANT_NON_REPLACED))
				{
					\u0003.SetFlag(VarFlag.ReplacedConstant, false);
				}
			}
			if (\u0003.HasAttribute("instancevar"))
			{
				\u0003.SetFlag(VarFlag.Local, false);
				\u0003.SetFlag(VarFlag.AllocateInInstance, true);
			}
			if (\u0002.Type.Class == TypeClass.Lazy)
			{
				\u0003.SetFlag(VarFlag.Lazy, true);
				this.\u0001.SetFlag((SignatureFlag)((ulong)int.MinValue), true);
			}
		}

		// Token: 0x06001A7F RID: 6783 RVA: 0x00055A14 File Offset: 0x00053C14
		private void \u0003(_IVariable \u0002)
		{
			if (this.\u0001.POUType == Operator.VarConfig && !\u0002.Name.Contains("."))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_NoValidInstancePath, new object[]
				{
					\u0002.OrgName
				});
				this.\u0001(\u0002._SourcePosition, u, Severity.Error, MessageId.Err_NoValidInstancePath);
			}
		}

		// Token: 0x06001A80 RID: 6784 RVA: 0x00055A70 File Offset: 0x00053C70
		private void \u0004(_IVariable \u0002)
		{
			if (this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) && (\u0002.GetFlag(VarFlag.Input) || \u0002.GetFlag(VarFlag.Inout)) && (this.\u0001.Name.StartsWith("__GET") || this.\u0001.Name.StartsWith("__SET")))
			{
				string value = this.\u0001.Name.Substring(5);
				if (!\u0002.OrgName.ToUpperInvariant().Equals(value))
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_NoVarInputInPropertyAccessors, new object[]
					{
						\u0002.OrgName,
						\u0002.Type
					});
					this.\u0001(\u0002._SourcePosition, u, Severity.Error, MessageId.Err_NoVarInputInPropertyAccessors);
				}
			}
		}

		// Token: 0x06001A81 RID: 6785 RVA: 0x00055B34 File Offset: 0x00053D34
		private bool \u0001(_IVariable \u0002)
		{
			if (Operator.Interface != this.\u0001.POUType)
			{
				return false;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
			{
				\u001A.\u0006.\u0001 u = new \u001A.\u0006.\u0001();
				u.\u0001 = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_USELOCATION);
				IVariable variable = this.\u0001.AllVariables.FirstOrDefault(new Func<_IVariable, bool>(u.\u0001));
				return variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY);
			}
			return false;
		}

		// Token: 0x06001A82 RID: 6786 RVA: 0x00055BA8 File Offset: 0x00053DA8
		private static void \u0002(_IVariable \u0002)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST) && \u0002.Initial != null)
			{
				global::\u0012.\u0002 u = new global::\u0012.\u0002(false);
				(\u0002.Initial as _IExpression).Accept(u.Traverser);
				\u0002.AddAttribute("initial_value_crc", u.Checksum.ToString());
			}
		}

		// Token: 0x06001A83 RID: 6787 RVA: 0x00055C00 File Offset: 0x00053E00
		private void \u0005(_IVariable \u0002)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NO_COPY))
			{
				this.\u0001.AddAttribute("contains_no_copy", null);
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST))
			{
				this.\u0001.AddAttribute("contains_blobinitconst", null);
				\u0002.SetFlag(VarFlag.Constant, true);
			}
		}

		// Token: 0x06001A84 RID: 6788 RVA: 0x00055C54 File Offset: 0x00053E54
		private void \u0006(_IVariable \u0002)
		{
			if (this.\u0001.POUType == Operator.FunctionBlock && (\u0002.Name == IdentifierConstants.InitMethodName || \u0002.Name == IdentifierConstants.ExitMethodName || \u0002.Name == IdentifierConstants.ReInitMethodName))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_ImplicitMethodNameForVariable, new object[]
				{
					\u0002.OrgName
				});
				this.\u0001(\u0002._SourcePosition, u, Severity.Error, MessageId.Err_ImplicitMethodNameForVariable);
			}
		}

		// Token: 0x06001A85 RID: 6789 RVA: 0x00055CD4 File Offset: 0x00053ED4
		private void \u0007(_IVariable \u0002)
		{
			if (\u0002.Type != null && \u0002.Type.Class == TypeClass.Params && ((this.\u0001.POUType != Operator.Function && this.\u0001.POUType != Operator.Method) || !\u0002.GetFlag(VarFlag.Input)))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_ParamsNotAllowed, Array.Empty<object>());
				this.\u0001(\u0002._SourcePosition, u, Severity.Error, MessageId.Err_ParamsNotAllowed);
			}
		}

		// Token: 0x06001A86 RID: 6790 RVA: 0x00055D44 File Offset: 0x00053F44
		private void \u0008(_IVariable \u0002)
		{
			if (\u0002.Type != null && \u0002.Type.Class == TypeClass.VarLenArray)
			{
				bool flag = true;
				Operator poutype = this.\u0001.POUType;
				if (poutype == Operator.FunctionBlock)
				{
					if (\u0002.GetFlag(VarFlag.Inout))
					{
						flag = false;
					}
				}
				else if ((poutype == Operator.Function || poutype == Operator.Method) && (\u0002.GetFlag(VarFlag.Inout) || \u0002.GetFlag(VarFlag.Input)))
				{
					flag = false;
				}
				if (flag)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_VarLengthArrayInOut, Array.Empty<object>());
					this.\u0001(\u0002._SourcePosition, u, Severity.Error, MessageId.Err_VarLengthArrayInOut);
				}
			}
		}

		// Token: 0x06001A87 RID: 6791 RVA: 0x00055DD0 File Offset: 0x00053FD0
		private void \u0004(_IVariableDeclarationStatement \u0002)
		{
			if (this.\u0001.HasFlag(VarFlag.Persistent) || this.\u0001.HasFlag(VarFlag.Retain))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_VectorNoPersistentRetain, Array.Empty<object>());
				this.\u0001(\u0002.GetPosition() as _ISourcePosition, u, Severity.Error, MessageId.Err_VectorNoPersistentRetain);
				return;
			}
			if (this.\u0001.POUType == Operator.Type && this.\u0001.HasFlag(SignatureFlag.Union))
			{
				string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_VectorTypeCantBePlacedInUnion, Array.Empty<object>());
				this.\u0001(\u0002.GetPosition() as _ISourcePosition, u2, Severity.Error, MessageId.Err_VectorTypeCantBePlacedInUnion);
			}
		}

		// Token: 0x06001A88 RID: 6792 RVA: 0x00055E8C File Offset: 0x0005408C
		private void \u0005(_IVariableDeclarationStatement \u0002)
		{
			if (this.\u0001.POUType != Operator.FunctionBlock && !this.\u0001.GetFlag(SignatureFlag.Structure))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_BitsForStructureOnly, Array.Empty<object>());
				this.\u0001(\u0002.GetPosition() as _ISourcePosition, u, Severity.Error, MessageId.Err_BitsForStructureOnly);
				return;
			}
			if ((this.\u0001 & VarFlag.Temp) == VarFlag.Temp || (this.\u0001 & VarFlag.Static) == VarFlag.Static || (this.\u0001 & VarFlag.Inout) == VarFlag.Inout)
			{
				string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_BitsWrongScope, Array.Empty<object>());
				this.\u0001(\u0002.GetPosition() as _ISourcePosition, u2, Severity.Error, MessageId.Err_BitsWrongScope);
			}
		}

		// Token: 0x06001A89 RID: 6793 RVA: 0x00055F40 File Offset: 0x00054140
		private _IType \u0001(_IVariableDeclarationStatement \u0002)
		{
			if (\u0002.Type == null)
			{
				return null;
			}
			_IType result = \u0002.Type;
			bool flag = TypeTable.IsAnySystemType(\u0002.Type);
			bool flag2 = \u001A.\u0006.\u0001(\u0002);
			if (!flag && !flag2)
			{
				return result;
			}
			bool flag3 = this.\u0001();
			if (flag2 || !flag3)
			{
				if (flag3)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_AnyTypeOnlyInFunction, new object[]
					{
						\u0002.Type.ToString()
					});
					this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002.Position, u, Severity.Error, MessageId.Err_AnyTypeOnlyInFunction));
				}
				if (flag)
				{
					_IAnyType ianyType = global::\u0019.\u0003.\u0001();
					this.\u0001.Add(global::\u0019.\u0003.\u0001(CompileAttributes.ATTRIBUTE_ANYTYPECLASS, ianyType.ToString()));
				}
				else
				{
					this.\u0001.Add(global::\u0019.\u0003.\u0001(CompileAttributes.ATTRIBUTE_ANYTYPECLASS, \u0002.Type.ToString()));
				}
				this.\u0001.AddAttribute(CompileAttributes.ATTRIBUTE_HASANYTYPE, null);
				result = (global::\u0011.\u0006.\u0001("__SYSTEM.AnyType") as _IType);
				foreach (_IExpression iexpression in \u0002.NameList)
				{
					string str = iexpression.ToString();
					_IVariable ivariable = global::\u0019.\u0003.\u0001(\u0002.GetPosition() as _ISourcePosition);
					ivariable.Name = str + "__pValue";
					ivariable._Type = global::\u0019.\u0003.\u0001(TypeTable.Byte);
					ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, str + ".pValue");
					ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_HIDE, "");
					ivariable.SetFlag(VarFlag.Local, true);
					ivariable.SetFlag(VarFlag.NoInit, true);
					this.\u0001.AddVariable(ivariable);
					_IVariable ivariable2 = global::\u0019.\u0003.\u0001(\u0002.GetPosition() as _ISourcePosition);
					ivariable2.Name = str + "__typeClass";
					global::\u0011.\u0006 u2 = new global::\u0011.\u0006("__SYSTEM.Type_Class");
					ivariable2._Type = u2.\u0001();
					ivariable2.AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, str + ".TypeClass");
					ivariable2.AddAttribute(CompileAttributes.ATTRIBUTE_HIDE, "");
					ivariable2.SetFlag(VarFlag.Local, true);
					ivariable2.SetFlag(VarFlag.NoInit, true);
					this.\u0001.AddVariable(ivariable2);
					_IVariable ivariable3 = global::\u0019.\u0003.\u0001(\u0002.GetPosition() as _ISourcePosition);
					ivariable3.Name = str + "__sizeOf";
					ivariable3._Type = TypeTable.DInt;
					ivariable3.AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, str + ".diSize");
					ivariable3.AddAttribute(CompileAttributes.ATTRIBUTE_HIDE, "");
					ivariable3.SetFlag(VarFlag.Local, true);
					ivariable3.SetFlag(VarFlag.NoInit, true);
					this.\u0001.AddVariable(ivariable3);
				}
			}
			return result;
		}

		// Token: 0x06001A8A RID: 6794 RVA: 0x00056220 File Offset: 0x00054420
		private bool \u0001()
		{
			return (this.\u0001.POUType != Operator.Function && this.\u0001.POUType != Operator.Method && this.\u0001.POUType != Operator.FunctionBlock) || this.\u0001 != VarFlag.Input;
		}

		// Token: 0x06001A8B RID: 6795 RVA: 0x00056260 File Offset: 0x00054460
		private static bool \u0001(_IVariableDeclarationStatement \u0002)
		{
			return \u0002.Type != null && (\u0002.Type.Class == TypeClass.Any || \u0002.Type.Class == TypeClass.AnyBit || \u0002.Type.Class == TypeClass.AnyDate || \u0002.Type.Class == TypeClass.AnyInt || \u0002.Type.Class == TypeClass.AnyNum || \u0002.Type.Class == TypeClass.AnyReal || \u0002.Type.Class == TypeClass.AnyString);
		}

		// Token: 0x06001A8C RID: 6796 RVA: 0x000562E4 File Offset: 0x000544E4
		private void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Structure) && \u0002.GetFlag(VarFlag.Constant))
			{
				\u0002.SetFlag(VarFlag.Constant, false);
				\u0002.SetFlag(VarFlag.ReplacedConstant, false);
				Severity severity = Messages.\u0001(this.\u0001, MessageId.Wrn_ConstantInStructDeclaration);
				this.\u0001.AddMessage(severity, MessageId.Wrn_ConstantInStructDeclaration, Array.Empty<object>());
			}
		}

		// Token: 0x06001A8D RID: 6797 RVA: 0x00056348 File Offset: 0x00054548
		private void \u0002(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.VarAccess))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_VarAccessNotSupported, Array.Empty<object>());
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VarAccessNotSupported);
			}
		}

		// Token: 0x06001A8E RID: 6798 RVA: 0x0005638C File Offset: 0x0005458C
		private void \u0003(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.VarConfig) && this.\u0001.POUType != Operator.VarConfig)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_VarConfigNotAllowed, Array.Empty<object>());
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VarConfigNotAllowed);
			}
		}

		// Token: 0x06001A8F RID: 6799 RVA: 0x000563E0 File Offset: 0x000545E0
		private void \u0004(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Global))
			{
				if (this.\u0001.POUType != Operator.VarGlobal)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_VarGlobalNotAllowed, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VarGlobalNotAllowed);
				}
				else if (!string.IsNullOrEmpty(this.\u0001))
				{
					IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(this.\u0001, false, false, false, false);
					scanner.AllowMultipleUnderlines = true;
					IToken token;
					if (scanner.GetNext(out token) != TokenType.Identifier || scanner.GetNext(out token) != TokenType.End)
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_InvalidSignatureName, new object[]
						{
							this.\u0001
						});
						this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_InvalidSignatureName);
					}
				}
				if (this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST) && !\u0002.GetFlag(VarFlag.Constant))
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_ParameterlistNotConst, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_ParameterlistNotConst);
				}
			}
		}

		// Token: 0x06001A90 RID: 6800 RVA: 0x000564F8 File Offset: 0x000546F8
		private void \u0005(_IVariableDeclarationListStatement \u0002)
		{
			if ((\u0002.GetFlag(VarFlag.Structure) || \u0002.GetFlag(VarFlag.Union)) && this.\u0001.POUType != Operator.Type)
			{
				string u;
				MessageId u2;
				if (\u0002.GetFlag(VarFlag.Structure))
				{
					u = global::\u0003.\u0006.\u0001(MessageId.Err_StructureNotAllowed, Array.Empty<object>());
					u2 = MessageId.Err_StructureNotAllowed;
				}
				else
				{
					u = global::\u0003.\u0006.\u0001(MessageId.Err_UnionNotAllowed, Array.Empty<object>());
					u2 = MessageId.Err_UnionNotAllowed;
				}
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, u2);
			}
		}

		// Token: 0x06001A91 RID: 6801 RVA: 0x00056584 File Offset: 0x00054784
		private void \u0006(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Static) && this.\u0001.POUType != Operator.Method && this.\u0001.POUType != Operator.Function && this.\u0001.POUType != Operator.FunctionBlock)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_StaticNotAllowed, Array.Empty<object>());
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_StaticNotAllowed);
			}
		}

		// Token: 0x06001A92 RID: 6802 RVA: 0x000565F4 File Offset: 0x000547F4
		private void \u0007(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Local) && this.\u0001.POUType == Operator.VarGlobal)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_DeclarationKeywordNotAllowed, new object[]
				{
					Operator.Var
				});
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_DeclarationKeywordNotAllowed);
			}
		}

		// Token: 0x06001A93 RID: 6803 RVA: 0x00056650 File Offset: 0x00054850
		private void \u0008(_IVariableDeclarationListStatement \u0002)
		{
			if ((\u0002.GetFlag(VarFlag.Input) || \u0002.GetFlag(VarFlag.Inout) || \u0002.GetFlag(VarFlag.Output)) && (this.\u0001.POUType == Operator.Type || this.\u0001.POUType == Operator.VarGlobal || this.\u0001.POUType == Operator.VarConfig))
			{
				Operator @operator = Operator.Var;
				if (\u0002.GetFlag(VarFlag.Input))
				{
					@operator = Operator.VarInput;
				}
				else if (\u0002.GetFlag(VarFlag.Output))
				{
					@operator = Operator.VarOutput;
				}
				else if (\u0002.GetFlag(VarFlag.Inout))
				{
					@operator = Operator.VarInOut;
				}
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_DeclarationKeywordNotAllowed, new object[]
				{
					@operator.ToString()
				});
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_DeclarationKeywordNotAllowed);
			}
		}

		// Token: 0x06001A94 RID: 6804 RVA: 0x00056714 File Offset: 0x00054914
		private void \u000E(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Temp) && this.\u0001.POUType != Operator.Program && this.\u0001.POUType != Operator.FunctionBlock)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_VarTempNotAllowed, Array.Empty<object>());
				this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VarTempNotAllowed);
			}
		}

		// Token: 0x06001A95 RID: 6805 RVA: 0x00056778 File Offset: 0x00054978
		private void \u000F(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.AllocateInInstance))
			{
				if (this.\u0001.POUType == Operator.Method)
				{
					if (\u0002.Flags == VarFlag.AllocateInInstance)
					{
						this.\u0001.SetFlagInternal(SignatureFlagInternal.ContainsInstanceVars, true);
						return;
					}
					if (\u0002.GetFlag(VarFlag.Constant))
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_DeclarationKeywordNotAllowed, new object[]
						{
							Operator.Constant
						});
						this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_DeclarationKeywordNotAllowed);
						return;
					}
				}
				else
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_VarInstOnlyInMethods, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VarInstOnlyInMethods);
				}
			}
		}

		// Token: 0x06001A96 RID: 6806 RVA: 0x00056834 File Offset: 0x00054A34
		private void \u0010(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Generic))
			{
				if (this.\u0001.POUType != Operator.FunctionBlock)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_GenericOnWrongPosition, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_GenericOnWrongPosition);
				}
				else
				{
					this.\u0001.SetFlagInternal(SignatureFlagInternal.ContainsGenericConstants, true);
					this.\u0001.SetFlag(SignatureFlag.InhibitOnlineChange, true);
				}
				if (!\u0002.GetFlag(VarFlag.Constant))
				{
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_GenericOnlyConst, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u2, Severity.Error, MessageId.Err_GenericOnlyConst);
				}
			}
		}

		// Token: 0x06001A97 RID: 6807 RVA: 0x000568E8 File Offset: 0x00054AE8
		private bool \u0001(IVariable \u0002)
		{
			return \u0002.Address == null || (!\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) && (!\u0002.HasFlag(VarFlag.Inout | VarFlag.Constant | VarFlag.Temp) || (\u0002.Address.Location != DirectVariableLocation.Output && \u0002.Address.Location != DirectVariableLocation.Memory) || \u0002.HasFlag(VarFlag.ReplacedConstant)));
		}

		// Token: 0x06001A98 RID: 6808 RVA: 0x00056948 File Offset: 0x00054B48
		public void \u0011(_IVariableDeclarationListStatement \u0002)
		{
			if (this.\u0001 == null && (\u0002.GetFlag(VarFlag.Global) || \u0002.GetFlag(VarFlag.VarConfig)))
			{
				this.\u0014(\u0002);
			}
			if (this.\u0001 == null)
			{
				return;
			}
			if (\u0002.GetFlag(VarFlag.Persistent))
			{
				this.\u0013(\u0002);
			}
			this.\u0001(\u0002);
			this.\u0002(\u0002);
			this.\u0003(\u0002);
			this.\u0004(\u0002);
			this.\u0005(\u0002);
			this.\u0006(\u0002);
			this.\u0007(\u0002);
			this.\u0008(\u0002);
			this.\u000E(\u0002);
			this.\u000F(\u0002);
			this.\u0010(\u0002);
			this.\u0001 = null;
			this.\u0002 = null;
			if (this.\u0001 != VarFlag.None)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_VariableDeclarationExpected, new object[]
				{
					\u0002.ToString()
				});
				this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VariableDeclarationExpected));
			}
			this.\u0001 = \u0002.Flags;
			\u0002.VariableDeclaration.Accept(this);
			this.\u0001 = VarFlag.None;
			if (\u0002.GetFlag(VarFlag.Constant))
			{
				\u0002.SetFlag(VarFlag.ReplacedConstant, this.\u0001);
			}
			this.\u0012(\u0002);
		}

		// Token: 0x06001A99 RID: 6809 RVA: 0x00056A80 File Offset: 0x00054C80
		private void \u0012(_IVariableDeclarationListStatement \u0002)
		{
			if (\u0002.Declarations.Length != 0 && (\u0002.GetFlag(VarFlag.Input) || \u0002.GetFlag(VarFlag.Inout)))
			{
				foreach (string text in LMMSlotAttributes.SLOT_ATTRIBUTES)
				{
					if (this.\u0001.HasAttribute(text))
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_NoInputsWithSlotAttributes, new object[]
						{
							this.\u0001.OrgName,
							text
						});
						this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_NoInputsWithSlotAttributes);
					}
				}
			}
		}

		// Token: 0x06001A9A RID: 6810 RVA: 0x00056B0C File Offset: 0x00054D0C
		private void \u0013(_IVariableDeclarationListStatement \u0002)
		{
			Guid guid = new Guid("{d433c4d1-208a-4b7a-b4ba-e3c77e67e46b}");
			if (this.\u0001.GetAttributeValue("authentification") != guid.ToString())
			{
				\u0002.SetFlag(VarFlag.LocalPersistent, true);
				\u0002.SetFlag(VarFlag.Persistent, false);
				return;
			}
			this.\u0001.SetFlag(SignatureFlag.Persistent, true);
		}

		// Token: 0x06001A9B RID: 6811 RVA: 0x00056B7C File Offset: 0x00054D7C
		private void \u0014(_IVariableDeclarationListStatement \u0002)
		{
			this.\u0001 = global::\u0019.\u0003.\u0001();
			if (\u0002.GetFlag(VarFlag.Global))
			{
				this.\u0001.POUType = Operator.VarGlobal;
			}
			else
			{
				this.\u0001.POUType = Operator.VarConfig;
			}
			this.\u0001.Name = this.\u0001;
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				string u = this.\u0001;
				for (int i = 0; i < u.Length; i++)
				{
					if (!Scanner.\u0001(u[i]))
					{
						this.\u0001.SetFlag(SignatureFlag.NoIECIdent, true);
						break;
					}
				}
			}
			foreach (_ICompilerAttribute icompilerAttribute in this.\u0001)
			{
				if (icompilerAttribute.Name == "noinit")
				{
					this.\u0001.SetFlag(SignatureFlag.NoInit, true);
				}
				else if (icompilerAttribute.Name == CompileAttributes.ATTRIBUTE_RECREATED_ON_ONLINE_CHANGE)
				{
					this.\u0001.SetFlag(SignatureFlag.NoCopy, true);
				}
				this.\u0001.AddAttribute(icompilerAttribute.Name, icompilerAttribute.Value);
			}
			this.\u0001.Clear();
			this.\u0001.Clear();
			if (this.\u0001 != null)
			{
				this.\u0001.Comment = this.\u0001.ToString();
			}
			if (this.\u0002 != null)
			{
				this.\u0001.DocuComment = this.\u0002.ToString();
			}
		}

		// Token: 0x06001A9C RID: 6812 RVA: 0x00056D08 File Offset: 0x00054F08
		private void \u0004(_IPOUDeclarationStatement \u0002)
		{
			Operator @class = \u0002.Class;
			if (@class - Operator.PropertySet <= 1)
			{
				this.\u0001.POUType = Operator.Method;
				this.\u0001.AddAttribute(CompileAttributes.ATTRIBUTE_PROPERTY, \u0002.Name);
				this.\u0001.SetFlag(SignatureFlag.RawSTProperty, true);
				string str = (\u0002.Class == Operator.PropertySet) ? "__set" : "__get";
				this.\u0001._NameExpression = global::\u0019.\u0003.\u0001(str + \u0002.Name);
				return;
			}
			if (@class != Operator.Transition)
			{
				this.\u0001.POUType = \u0002.Class;
				this.\u0001._NameExpression = \u0002.NameExpression;
				return;
			}
			this.\u0001.POUType = Operator.Method;
			this.\u0001.AddAttribute(CompileAttributes.ATTRIBUTE_PROPERTY, \u0002.Name);
			this.\u0001.AddAttribute(CompileAttributes.ATTRIBUTE_TRANSITION, \u0002.Name);
			this.\u0001.SetFlag(SignatureFlag.RawSTProperty | SignatureFlag.RawSTTransition, true);
			this.\u0001._NameExpression = global::\u0019.\u0003.\u0001("__get" + \u0002.Name);
		}

		// Token: 0x06001A9D RID: 6813 RVA: 0x00056E34 File Offset: 0x00055034
		private void \u0005(_IPOUDeclarationStatement \u0002)
		{
			Operator @class = \u0002.Class;
			if (@class - Operator.PropertySet <= 1 || @class == Operator.Transition)
			{
				_IVariable ivariable = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001());
				ivariable.Name = \u0002.Name;
				ivariable._Type = \u0002.Type;
				if (\u0002.Class == Operator.PropertyGet)
				{
					ivariable.SetFlag(VarFlag.Local, true);
					ivariable.AddAttribute("ignore_in_interface", "");
				}
				else if (Operator.PropertySet == \u0002.Class)
				{
					ivariable.SetFlag(VarFlag.Input, true);
				}
				else
				{
					ivariable.SetFlag(VarFlag.Local, true);
					ivariable.SetType(global::\u0019.\u0003.\u0001());
				}
				this.\u0001.AddVariable(ivariable);
				return;
			}
		}

		// Token: 0x06001A9E RID: 6814 RVA: 0x00056EE4 File Offset: 0x000550E4
		public void \u0006(_IPOUDeclarationStatement \u0002)
		{
			this.\u0001 = global::\u0019.\u0003.\u0001();
			this.\u0001.POUType = \u0002.Class;
			this.\u0001();
			this.\u0004(\u0002);
			this.\u0008(\u0002);
			LList<Tuple<string, ISourcePosition>> llist = this.\u0001(\u0002);
			if (this.\u0001.HasAttribute(CompileAttributes.ATTRIBUTE_RECREATED_ON_ONLINE_CHANGE) && this.\u0001.POUType == Operator.Program)
			{
				this.\u0001.SetFlag(SignatureFlag.NoCopy, true);
			}
			this.\u000E(\u0002);
			this.\u0003(\u0002);
			this.\u0002(\u0002);
			if (this.\u0001.HasAttribute("instancevar"))
			{
				this.\u0001.SetFlagInternal(SignatureFlagInternal.ContainsInstanceVars, true);
			}
			if (this.\u0001.GetFlag(SignatureFlag.RawSTProperty) && this.\u0001.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
			{
				this.\u0001.SetFlag(SignatureFlag.TopLevel, true);
			}
			if (!this.\u0001.IsCompiledLibraryObject && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(this.\u0001, GUIHidingFlags.AllCommon))
			{
				foreach (Tuple<string, ISourcePosition> tuple in llist)
				{
					this.\u0001.AddWarning(tuple.Item2 as _ISourcePosition, \u001A.\u0006.\u0001(tuple.Item1), MessageId.Wrn_AttributeCheck);
				}
			}
			this.\u0001.Clear();
			this.\u0007(\u0002);
			this.\u0005(\u0002);
			if (this.\u0001 != null)
			{
				this.\u0001.Comment = this.\u0001.ToString();
			}
			this.\u0001 = null;
			if (this.\u0002 != null)
			{
				this.\u0001.DocuComment = this.\u0002.ToString();
			}
			this.\u0002 = null;
			this.\u000F(\u0002);
			this.\u0010(\u0002);
			_ISequenceStatement isequenceStatement = \u0002.Declarations as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				this.\u0001(isequenceStatement, 0);
			}
			\u0002.Declarations.Accept(this);
			this.\u0002();
			this.\u0001.Clear();
		}

		// Token: 0x06001A9F RID: 6815 RVA: 0x00057104 File Offset: 0x00055304
		private void \u0007(_IPOUDeclarationStatement \u0002)
		{
			_IType itype = \u0002.Type;
			if (\u0002.Class == Operator.PropertySet)
			{
				itype = global::\u0019.\u0003.\u0001();
			}
			else if (Operator.Transition == \u0002.Class)
			{
				itype = global::\u0019.\u0003.\u0001();
			}
			if (itype != null)
			{
				if (this.\u0001.POUType != Operator.Method && this.\u0001.POUType != Operator.Function)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_ReturnTypeForNonFunction, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_ReturnTypeForNonFunction);
					return;
				}
				_IVariable ivariable = global::\u0019.\u0003.\u0001(\u0002.NameExpression.Position as _ISourcePosition);
				ivariable._Type = itype;
				ivariable.Name = this.\u0001.OrgName;
				ivariable.SetFlag(VarFlag.Output, true);
				this.\u0001.AddVariable(ivariable);
				this.\u0008(ivariable);
			}
		}

		// Token: 0x06001AA0 RID: 6816 RVA: 0x000571D8 File Offset: 0x000553D8
		private void \u0008(_IPOUDeclarationStatement \u0002)
		{
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				string name = \u0002.Name;
				for (int i = 0; i < name.Length; i++)
				{
					if (!Scanner.\u0001(name[i]))
					{
						this.\u0001.SetFlag(SignatureFlag.NoIECIdent, true);
						return;
					}
				}
			}
		}

		// Token: 0x06001AA1 RID: 6817 RVA: 0x00057234 File Offset: 0x00055434
		private LList<Tuple<string, ISourcePosition>> \u0001(_IPOUDeclarationStatement \u0002)
		{
			LList<Tuple<string, ISourcePosition>> llist = new LList<Tuple<string, ISourcePosition>>();
			foreach (_ICompilerAttribute icompilerAttribute in this.\u0001)
			{
				IList<string> list;
				if (!APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.CheckAttribute(icompilerAttribute.Name, icompilerAttribute.Value, AttributeScope.Signature, this.\u0001, null, out list))
				{
					ISourcePosition item = \u0002.Position;
					if (this.\u0001.ContainsKey(icompilerAttribute))
					{
						item = this.\u0001[icompilerAttribute];
					}
					foreach (string item2 in list)
					{
						llist.Add(new Tuple<string, ISourcePosition>(item2, item));
					}
				}
				this.\u0001.AddAttribute(icompilerAttribute.Name, icompilerAttribute.Value);
			}
			return llist;
		}

		// Token: 0x06001AA2 RID: 6818 RVA: 0x00057334 File Offset: 0x00055534
		private void \u000E(_IPOUDeclarationStatement \u0002)
		{
			if (\u0002.Access != SignatureFlag.None)
			{
				this.\u0001.SetFlag(\u0002.Access, true);
				if ((this.\u0001.GetFlag(SignatureFlag.Private) || this.\u0001.GetFlag(SignatureFlag.Protected)) && this.\u0001.POUType != Operator.Method)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_PrivateOnMethodsOnly, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_PrivateOnMethodsOnly);
				}
				if (this.\u0001.GetFlag(SignatureFlag.Final) && this.\u0001.POUType != Operator.Method && this.\u0001.POUType != Operator.FunctionBlock)
				{
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_FinalOnFunctionBlocksAndMethodsOnly, Array.Empty<object>());
					this.\u0001(\u0002.Position as _ISourcePosition, u2, Severity.Error, MessageId.Err_FinalOnFunctionBlocksAndMethodsOnly);
				}
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001AA3 RID: 6819 RVA: 0x00057424 File Offset: 0x00055624
		private void \u000F(_IPOUDeclarationStatement \u0002)
		{
			if (\u0002.Extends != null && \u0002.Extends.Count > 0)
			{
				this.\u0001._BaseSignature = \u0002.Extends[0];
				if (this.\u0001.POUType == Operator.Interface)
				{
					for (int i = 1; i < \u0002.Extends.Count; i++)
					{
						this.\u0001.AddInterface(\u0002.Extends[i]);
					}
					return;
				}
				if (\u0002.Extends.Count > 1)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_NoMultipleInheritance, Array.Empty<object>());
					this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_NoMultipleInheritance));
				}
			}
		}

		// Token: 0x06001AA4 RID: 6820 RVA: 0x000574DC File Offset: 0x000556DC
		private void \u0010(_IPOUDeclarationStatement \u0002)
		{
			if (\u0002.Implements != null)
			{
				if (this.\u0001.POUType == Operator.Interface)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Wrn_ExtendsForInterfaces, Array.Empty<object>());
					this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002.Position as _ISourcePosition, u, Severity.Warning, MessageId.Wrn_ExtendsForInterfaces));
					if (this.\u0001._BaseSignature == null)
					{
						this.\u0001._BaseSignature = \u0002.Implements[0];
					}
					else
					{
						this.\u0001.AddInterface(\u0002.Implements[0]);
					}
					for (int i = 1; i < \u0002.Implements.Count; i++)
					{
						this.\u0001.AddInterface(\u0002.Implements[i]);
					}
					return;
				}
				foreach (_IExpression expInterface in \u0002.Implements)
				{
					this.\u0001.AddInterface(expInterface);
				}
			}
		}

		// Token: 0x06001AA5 RID: 6821 RVA: 0x000575E8 File Offset: 0x000557E8
		private void \u0001()
		{
			int num = 0;
			foreach (string stValue in this.\u0001.Keys)
			{
				string stAttribute = string.Format("suppress_warning_{0}", num++);
				this.\u0001.AddAttribute(stAttribute, stValue);
			}
		}

		// Token: 0x06001AA6 RID: 6822 RVA: 0x00057660 File Offset: 0x00055860
		private void \u0002()
		{
			for (int i = 0; i < this.\u0001.Inputs.Length; i++)
			{
				_IVariable ivariable = this.\u0001.Inputs[i] as _IVariable;
				if (ivariable.Type != null && ivariable.Type.Class == TypeClass.Params && i != this.\u0001.Inputs.Length - 1)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_ParamsNotAllowed, Array.Empty<object>());
					this.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_ParamsNotAllowed);
				}
			}
		}

		// Token: 0x06001AA7 RID: 6823 RVA: 0x000576E4 File Offset: 0x000558E4
		private void \u0001(_ISequenceStatement \u0002, int \u0003)
		{
			for (int i = \u0003; i < \u0002._StatementList.Count; i++)
			{
				_IStatement istatement = \u0002._StatementList[i];
				if (istatement is IVariableDeclarationStatement)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_VariableListExpected, new object[]
					{
						istatement.ToString()
					});
					this.\u0001.AddError(global::\u0019.\u0003.\u0001(istatement.Position as _ISourcePosition, u, Severity.Error, MessageId.Err_VariableListExpected));
				}
				else if (istatement is _IVariableDeclarationListStatement)
				{
					_IVariableDeclarationListStatement ivariableDeclarationListStatement = istatement as _IVariableDeclarationListStatement;
					if ((ivariableDeclarationListStatement.GetFlag(VarFlag.Retain) || ivariableDeclarationListStatement.GetFlag(VarFlag.Persistent)) && (this.\u0001.POUType == Operator.Function || this.\u0001.POUType == Operator.Method || this.\u0001.POUType == Operator.Property))
					{
						string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_RetainOrPersistentNotAllowed, Array.Empty<object>());
						this.\u0001.AddError(global::\u0019.\u0003.\u0001(istatement.Position as _ISourcePosition, u2, Severity.Error, MessageId.Err_RetainOrPersistentNotAllowed));
					}
				}
			}
		}

		// Token: 0x06001AA8 RID: 6824 RVA: 0x000577F8 File Offset: 0x000559F8
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			this.\u0001 = global::\u0019.\u0003.\u0001();
			this.\u0001.POUType = Operator.Type;
			this.\u0001.SetFlag(\u0002.Flags, true);
			this.\u0001._NameExpression = \u0002.NameExpression;
			this.\u0002(\u0002);
			this.\u0001._BaseSignature = \u0002.Extends;
			LList<Tuple<string, ISourcePosition>> llist = this.\u0001(\u0002);
			if (!this.\u0001.IsCompiledLibraryObject && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(this.\u0001, GUIHidingFlags.AllCommon))
			{
				foreach (Tuple<string, ISourcePosition> tuple in llist)
				{
					this.\u0001.AddWarning(tuple.Item2 as _ISourcePosition, \u001A.\u0006.\u0001(tuple.Item1), MessageId.Wrn_AttributeCheck);
				}
			}
			this.\u0001.Clear();
			this.\u0001.Clear();
			if (this.\u0001 != null)
			{
				this.\u0001.Comment = this.\u0001.ToString();
			}
			this.\u0001 = null;
			if (this.\u0002 != null)
			{
				this.\u0001.DocuComment = this.\u0002.ToString();
			}
			this.\u0002 = null;
			if (this.\u0001.GetFlag(SignatureFlag.Enum))
			{
				this.\u0001.POUType = Operator.VarGlobal;
			}
			this.\u0003(\u0002);
			_IStatement declarations = \u0002.Declarations;
			if (declarations != null)
			{
				declarations.Accept(this);
			}
			if (this.\u0001.GetFlag(SignatureFlag.Alias) && \u0002.Type != null)
			{
				if (\u0002.Initial != null)
				{
					\u0002._DefaultValue = \u0002.Initial;
				}
				_IVariable ivariable = global::\u0019.\u0003.\u0001(\u0002.Position as _ISourcePosition);
				ivariable.SetFlag(VarFlag.Alias, true);
				ivariable._Type = \u0002.Type;
				ivariable.Initial = \u0002.Initial;
				this.\u0001.AddVariable(ivariable);
			}
		}

		// Token: 0x06001AA9 RID: 6825 RVA: 0x000579F0 File Offset: 0x00055BF0
		private void \u0002(_ITypeDeclarationStatement \u0002)
		{
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				string name = \u0002.Name;
				for (int i = 0; i < name.Length; i++)
				{
					if (!Scanner.\u0001(name[i]))
					{
						this.\u0001.SetFlag(SignatureFlag.NoIECIdent, true);
						return;
					}
				}
			}
		}

		// Token: 0x06001AAA RID: 6826 RVA: 0x00057A4C File Offset: 0x00055C4C
		private LList<Tuple<string, ISourcePosition>> \u0001(_ITypeDeclarationStatement \u0002)
		{
			LList<Tuple<string, ISourcePosition>> llist = new LList<Tuple<string, ISourcePosition>>();
			foreach (_ICompilerAttribute icompilerAttribute in this.\u0001)
			{
				IList<string> list;
				if (!APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.CheckAttribute(icompilerAttribute.Name, icompilerAttribute.Value, AttributeScope.Signature, this.\u0001, null, out list))
				{
					ISourcePosition item = \u0002.Position;
					ISourcePosition sourcePosition;
					if (this.\u0001.TryGetValue(icompilerAttribute, ref sourcePosition))
					{
						item = sourcePosition;
					}
					foreach (string item2 in list)
					{
						llist.Add(new Tuple<string, ISourcePosition>(item2, item));
					}
				}
				this.\u0001.AddAttribute(icompilerAttribute.Name, icompilerAttribute.Value);
			}
			return llist;
		}

		// Token: 0x06001AAB RID: 6827 RVA: 0x00057B44 File Offset: 0x00055D44
		private void \u0003(_ITypeDeclarationStatement \u0002)
		{
			_IEnumDeclarationListStatement ienumDeclarationListStatement = \u0002.Declarations as _IEnumDeclarationListStatement;
			if (ienumDeclarationListStatement != null)
			{
				ienumDeclarationListStatement._DefaultValue = null;
				if (\u0002.Initial != null)
				{
					\u001A.\u0006.\u0002 u = new \u001A.\u0006.\u0002();
					\u0002.Initial.Accept(this);
					u.\u0001 = (\u0002.Initial as _IVariableExpression);
					if (u.\u0001 == null)
					{
						string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_InvalidEnumDefaultValue, Array.Empty<object>());
						this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002.Initial.Position as _ISourcePosition, u2, Severity.Error, MessageId.Err_InvalidEnumDefaultValue));
						return;
					}
					if (!ienumDeclarationListStatement.Enums.Any(new Func<_IEnumDeclarationStatement, bool>(u.\u0001)))
					{
						string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_InvalidEnumDefaultValue, Array.Empty<object>());
						this.\u0001.AddError(global::\u0019.\u0003.\u0001(\u0002.Initial.Position as _ISourcePosition, u3, Severity.Error, MessageId.Err_InvalidEnumDefaultValue));
					}
					ienumDeclarationListStatement._DefaultValue = u.\u0001;
				}
			}
		}

		// Token: 0x06001AAC RID: 6828 RVA: 0x00057C34 File Offset: 0x00055E34
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001AAD RID: 6829 RVA: 0x00057C38 File Offset: 0x00055E38
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			_IEnumType ienumType = global::\u0019.\u0003.\u0001(this.\u0001.Name);
			ienumType._DefaultValue = \u0002._DefaultValue;
			foreach (_IEnumDeclarationStatement u in \u0002.Enums)
			{
				this.\u0001(ienumType, u);
			}
			ienumType._Base = \u0002._BaseType;
		}

		// Token: 0x06001AAE RID: 6830 RVA: 0x00057CB0 File Offset: 0x00055EB0
		private void \u0001(_IEnumType \u0002, _IEnumDeclarationStatement \u0003)
		{
			this.\u0001.Clear();
			this.\u0001.Clear();
			if (\u0003.AttributesEtc != null)
			{
				\u0003.AttributesEtc.Accept(this);
			}
			_IVariable ivariable = global::\u0019.\u0003.\u0001(\u0003.Position as _ISourcePosition);
			ivariable.Name = \u0003.Name;
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				string name = ivariable.Name;
				for (int i = 0; i < name.Length; i++)
				{
					if (!Scanner.\u0001(name[i]))
					{
						ivariable.SetFlag(VarFlag.NoIECIdent, true);
						break;
					}
				}
			}
			ivariable.Initial = \u0003._Value;
			ivariable._Type = \u0002;
			if (this.\u0001 != null)
			{
				ivariable.Comment = this.\u0001.ToString();
			}
			this.\u0001 = null;
			if (this.\u0002 != null)
			{
				ivariable.DocuComment = this.\u0002.ToString();
			}
			this.\u0002 = null;
			ivariable.SetFlag(VarFlag.Local | VarFlag.ReplacedConstant | VarFlag.Constant | VarFlag.Enum, true);
			foreach (_ICompilerAttribute icompilerAttribute in this.\u0001)
			{
				ivariable.AddAttribute(icompilerAttribute.Name, icompilerAttribute.Value);
			}
			int num = 0;
			foreach (string stValue in this.\u0001.Keys)
			{
				string stAttribute = string.Format("suppress_warning_{0}", num++);
				ivariable.AddAttribute(stAttribute, stValue);
			}
			if (!this.\u0001.AddVariable(ivariable) && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_IGNORE_DUPLICATE))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_VariableDuplicate, new object[]
				{
					ivariable.OrgName,
					this.\u0001.OrgName
				});
				this.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_VariableDuplicate);
			}
		}

		// Token: 0x06001AAF RID: 6831 RVA: 0x00057EBC File Offset: 0x000560BC
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x06001AB0 RID: 6832 RVA: 0x00057EC0 File Offset: 0x000560C0
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB1 RID: 6833 RVA: 0x00057ECC File Offset: 0x000560CC
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB2 RID: 6834 RVA: 0x00057ED8 File Offset: 0x000560D8
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB3 RID: 6835 RVA: 0x00057EE4 File Offset: 0x000560E4
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB4 RID: 6836 RVA: 0x00057EF0 File Offset: 0x000560F0
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB5 RID: 6837 RVA: 0x00057EFC File Offset: 0x000560FC
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB6 RID: 6838 RVA: 0x00057F08 File Offset: 0x00056108
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB7 RID: 6839 RVA: 0x00057F14 File Offset: 0x00056114
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB8 RID: 6840 RVA: 0x00057F20 File Offset: 0x00056120
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AB9 RID: 6841 RVA: 0x00057F2C File Offset: 0x0005612C
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ABA RID: 6842 RVA: 0x00057F38 File Offset: 0x00056138
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ABB RID: 6843 RVA: 0x00057F44 File Offset: 0x00056144
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ABC RID: 6844 RVA: 0x00057F50 File Offset: 0x00056150
		public void \u0001(_IOperatorExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ABD RID: 6845 RVA: 0x00057F5C File Offset: 0x0005615C
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06001ABE RID: 6846 RVA: 0x00057F60 File Offset: 0x00056160
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ABF RID: 6847 RVA: 0x00057F6C File Offset: 0x0005616C
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06001AC0 RID: 6848 RVA: 0x00057F70 File Offset: 0x00056170
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AC1 RID: 6849 RVA: 0x00057F7C File Offset: 0x0005617C
		public void \u0001(_IThisExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AC2 RID: 6850 RVA: 0x00057F88 File Offset: 0x00056188
		public void \u0001(_IBaseExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AC3 RID: 6851 RVA: 0x00057F94 File Offset: 0x00056194
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AC4 RID: 6852 RVA: 0x00057FA0 File Offset: 0x000561A0
		public void \u0001(_IAddressExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001AC5 RID: 6853 RVA: 0x00057FAC File Offset: 0x000561AC
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001AC6 RID: 6854 RVA: 0x00057FB0 File Offset: 0x000561B0
		public void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x06001AC7 RID: 6855 RVA: 0x00057FB4 File Offset: 0x000561B4
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			if (this.\u0001 == null || this.\u0001.POUType != Operator.VarConfig)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001AC8 RID: 6856 RVA: 0x00057FD4 File Offset: 0x000561D4
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			if (this.\u0001 == null || this.\u0001.POUType != Operator.VarConfig)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001AC9 RID: 6857 RVA: 0x00057FF4 File Offset: 0x000561F4
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ACA RID: 6858 RVA: 0x00058000 File Offset: 0x00056200
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001ACB RID: 6859 RVA: 0x0005800C File Offset: 0x0005620C
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			if (this.\u0001 == null || this.\u0001.POUType != Operator.VarConfig)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001ACC RID: 6860 RVA: 0x0005802C File Offset: 0x0005622C
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			if (this.\u0001 == null || this.\u0001.POUType != Operator.VarConfig)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001ACD RID: 6861 RVA: 0x0005804C File Offset: 0x0005624C
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			if (this.\u0001 == null || this.\u0001.POUType != Operator.VarConfig)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001ACE RID: 6862 RVA: 0x0005806C File Offset: 0x0005626C
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			if (this.\u0001 == null || this.\u0001.POUType != Operator.VarConfig)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x06001ACF RID: 6863 RVA: 0x0005808C File Offset: 0x0005628C
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001AD0 RID: 6864 RVA: 0x00058090 File Offset: 0x00056290
		public void \u0001(_ICaseRangeExpression \u0002)
		{
		}

		// Token: 0x06001AD1 RID: 6865 RVA: 0x00058094 File Offset: 0x00056294
		public void \u0001(_ICaseLabelStatement \u0002)
		{
		}

		// Token: 0x06001AD2 RID: 6866 RVA: 0x00058098 File Offset: 0x00056298
		public void \u0001(_ICaseStatement \u0002)
		{
		}

		// Token: 0x06001AD3 RID: 6867 RVA: 0x0005809C File Offset: 0x0005629C
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06001AD4 RID: 6868 RVA: 0x000580A0 File Offset: 0x000562A0
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001AD5 RID: 6869 RVA: 0x000580A4 File Offset: 0x000562A4
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06001AD6 RID: 6870 RVA: 0x000580A8 File Offset: 0x000562A8
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001AD7 RID: 6871 RVA: 0x000580AC File Offset: 0x000562AC
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x06001AD8 RID: 6872 RVA: 0x000580B0 File Offset: 0x000562B0
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x06001AD9 RID: 6873 RVA: 0x000580B4 File Offset: 0x000562B4
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x06001ADA RID: 6874 RVA: 0x000580B8 File Offset: 0x000562B8
		public void \u0001(_IDefineReference \u0002)
		{
			if (this.\u0001 != null)
			{
				\u0002.Value = this.\u0001.IsDefined(\u0002.Define);
			}
		}

		// Token: 0x06001ADB RID: 6875 RVA: 0x000580DC File Offset: 0x000562DC
		public void \u0001(_IVariableReference \u0002)
		{
			\u0002.InstancePath.Accept(this);
			if (\u0002.InstancePath.VariableId != Helper.InvalidId)
			{
				\u0002.Value = true;
			}
		}

		// Token: 0x06001ADC RID: 6876 RVA: 0x00058104 File Offset: 0x00056304
		public void \u0001(_ITypeReference \u0002)
		{
			\u0002.InstancePath.Accept(this);
			\u0002.Value = false;
			if (this.\u0001 == null)
			{
				return;
			}
			ISignature signature = (this.\u0001.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2).FindSignatureGlobal(\u0002.InstancePath);
			if (signature != null && signature.POUType == Operator.Type)
			{
				\u0002.Value = true;
			}
		}

		// Token: 0x06001ADD RID: 6877 RVA: 0x00058164 File Offset: 0x00056364
		public void \u0001(_IPouReference \u0002)
		{
			\u0002.InstancePath.Accept(this);
			\u0002.Value = false;
			if (this.\u0001 == null)
			{
				return;
			}
			ISignature signature = (this.\u0001.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2).FindSignatureGlobal(\u0002.InstancePath);
			if (signature != null && (signature.POUType == Operator.Program || signature.POUType == Operator.Function || signature.POUType == Operator.FunctionBlock))
			{
				\u0002.Value = true;
			}
		}

		// Token: 0x06001ADE RID: 6878 RVA: 0x000581D8 File Offset: 0x000563D8
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06001ADF RID: 6879 RVA: 0x000581DC File Offset: 0x000563DC
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06001AE0 RID: 6880 RVA: 0x000581E0 File Offset: 0x000563E0
		public void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
			_IPragmaExpression ipragmaExpression = \u0002.ItemReference as _IPragmaExpression;
			if (ipragmaExpression != null)
			{
				\u0002.Value = ipragmaExpression.Value;
			}
		}

		// Token: 0x06001AE1 RID: 6881 RVA: 0x00058214 File Offset: 0x00056414
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			bool flag = false;
			bool flag2 = true;
			bool flag3 = false;
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
				if (!flag3)
				{
					_IPragmaExpression ipragmaExpression = iexpression as _IPragmaExpression;
					if (ipragmaExpression == null)
					{
						flag3 = true;
						flag = false;
					}
					else
					{
						if (\u0002.Code == PragmaOperator.Not)
						{
							flag = !ipragmaExpression.Value;
							break;
						}
						if (flag2)
						{
							flag = ipragmaExpression.Value;
							flag2 = false;
						}
						else if (\u0002.Code == PragmaOperator.And)
						{
							flag = (ipragmaExpression.Value && flag);
						}
						else if (\u0002.Code == PragmaOperator.Or)
						{
							flag = (ipragmaExpression.Value || flag);
						}
					}
				}
			}
			\u0002.Value = flag;
		}

		// Token: 0x06001AE2 RID: 6882 RVA: 0x000582DC File Offset: 0x000564DC
		public void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
			_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
			if (ipragmaExpression != null && !ipragmaExpression.Value)
			{
				ipragmaExpression.AddError(\u0002.ErrorOutput, MessageId.None);
			}
		}

		// Token: 0x06001AE3 RID: 6883 RVA: 0x0005831C File Offset: 0x0005651C
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			Operator opComparison = \u0002.OpComparison;
			switch (opComparison)
			{
			case Operator.Eq:
				goto IL_C0;
			case Operator.Ne:
				goto IL_DC;
			case Operator.Ge:
				break;
			case Operator.Gt:
				goto IL_6C;
			case Operator.Le:
				goto IL_A4;
			case Operator.Lt:
				goto IL_88;
			default:
				switch (opComparison)
				{
				case Operator.Less:
					goto IL_88;
				case Operator.Greater:
					goto IL_6C;
				case Operator.LessEqual:
					goto IL_A4;
				case Operator.GreaterEqual:
					break;
				case Operator.Equal:
					goto IL_C0;
				case Operator.NotEqual:
					goto IL_DC;
				default:
					return;
				}
				break;
			}
			\u0002.Value = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() >= \u0002.VersionToTest);
			return;
			IL_6C:
			\u0002.Value = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() > \u0002.VersionToTest);
			return;
			IL_88:
			\u0002.Value = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() < \u0002.VersionToTest);
			return;
			IL_A4:
			\u0002.Value = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() <= \u0002.VersionToTest);
			return;
			IL_C0:
			\u0002.Value = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() == \u0002.VersionToTest);
			return;
			IL_DC:
			\u0002.Value = (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() != \u0002.VersionToTest);
		}

		// Token: 0x06001AE4 RID: 6884 RVA: 0x00058420 File Offset: 0x00056620
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x06001AE5 RID: 6885 RVA: 0x00058424 File Offset: 0x00056624
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06001AE6 RID: 6886 RVA: 0x00058428 File Offset: 0x00056628
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001AE7 RID: 6887 RVA: 0x0005842C File Offset: 0x0005662C
		public void \u0001(_IXRefExpression \u0002)
		{
			if (\u0002.XRef != null)
			{
				\u0002.XRef.Accept(this);
			}
			if (\u0002.XRefFrom != null)
			{
				\u0002.XRefFrom.Accept(this);
			}
		}

		// Token: 0x06001AE8 RID: 6888 RVA: 0x00058458 File Offset: 0x00056658
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002.Variable != null)
			{
				\u0002.Variable.Accept(this);
			}
		}

		// Token: 0x06001AE9 RID: 6889 RVA: 0x00058470 File Offset: 0x00056670
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06001AEA RID: 6890 RVA: 0x00058474 File Offset: 0x00056674
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			_IItemReference iitemReference = \u0002.ItemReference as _IItemReference;
			\u0002.Value = false;
			if (iitemReference != null)
			{
				iitemReference.Accept(this);
				if (this.\u0001 == null)
				{
					return;
				}
				IPrecompileScope2 scope = this.\u0001.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2;
				if (iitemReference.Value && iitemReference.HasAttribute(\u0002.Attribute, scope))
				{
					\u0002.Value = true;
				}
			}
		}

		// Token: 0x06001AEB RID: 6891 RVA: 0x000584DC File Offset: 0x000566DC
		public void \u0001(_IHasValueExpression \u0002)
		{
			if (this.\u0001 == null)
			{
				\u0002.Value = false;
				return;
			}
			\u0002.Value = this.\u0001.DefineHasValue(\u0002.Define, \u0002.DefineValue);
		}

		// Token: 0x06001AEC RID: 6892 RVA: 0x0005850C File Offset: 0x0005670C
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			if (\u0002._Constant != null)
			{
				\u0002._Constant.Accept(this);
			}
			if (\u0002._ConstantValue != null)
			{
				\u0002._ConstantValue.Accept(this);
			}
		}

		// Token: 0x040004A0 RID: 1184
		private _ISignature \u0001;

		// Token: 0x040004A1 RID: 1185
		private LStringBuilder \u0001;

		// Token: 0x040004A2 RID: 1186
		private LStringBuilder \u0002;

		// Token: 0x040004A3 RID: 1187
		private readonly LList<_ICompilerAttribute> \u0001 = new LList<_ICompilerAttribute>();

		// Token: 0x040004A4 RID: 1188
		private readonly LDictionary<_ICompilerAttribute, ISourcePosition> \u0001 = new LDictionary<_ICompilerAttribute, ISourcePosition>();

		// Token: 0x040004A5 RID: 1189
		private VarFlag \u0001;

		// Token: 0x040004A6 RID: 1190
		private Guid \u0001 = Guid.Empty;

		// Token: 0x040004A7 RID: 1191
		private readonly string \u0001;

		// Token: 0x040004A8 RID: 1192
		private readonly bool \u0001 = true;

		// Token: 0x040004A9 RID: 1193
		private readonly _IPreCompileContext \u0001;

		// Token: 0x040004AA RID: 1194
		private readonly LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x02000184 RID: 388
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06001AEE RID: 6894 RVA: 0x00058540 File Offset: 0x00056740
			internal bool \u0001(_IVariable \u0002)
			{
				return \u0002.Name.Equals(this.\u0001, StringComparison.InvariantCultureIgnoreCase);
			}

			// Token: 0x040004AB RID: 1195
			public string \u0001;
		}

		// Token: 0x02000185 RID: 389
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06001AF0 RID: 6896 RVA: 0x0005855C File Offset: 0x0005675C
			internal bool \u0001(_IEnumDeclarationStatement \u0002)
			{
				return \u0002.Name == this.\u0001.Name;
			}

			// Token: 0x040004AC RID: 1196
			public _IVariableExpression \u0001;
		}
	}
}
