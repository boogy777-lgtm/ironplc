using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0011;
using \u0012;
using \u0017;
using \u0019;
using \u001A;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x0200015E RID: 350
	public class InterfaceParser
	{
		// Token: 0x0600182B RID: 6187 RVA: 0x0004B3FC File Offset: 0x000495FC
		internal InterfaceParser(global::\u0011.\u0006 parser, bool bAllowDefinesInInterfaces)
		{
			this.Parser = parser;
			this.AllowDefinesInInterface = bAllowDefinesInInterfaces;
		}

		// Token: 0x1700057B RID: 1403
		// (get) Token: 0x0600182C RID: 6188 RVA: 0x0004B414 File Offset: 0x00049614
		private global::\u0011.\u0006 Parser { get; }

		// Token: 0x1700057C RID: 1404
		// (get) Token: 0x0600182D RID: 6189 RVA: 0x0004B41C File Offset: 0x0004961C
		// (set) Token: 0x0600182E RID: 6190 RVA: 0x0004B42C File Offset: 0x0004962C
		private _ISignature Signature
		{
			get
			{
				return this.Parser.Signature;
			}
			set
			{
				this.Parser.Signature = value;
			}
		}

		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x0600182F RID: 6191 RVA: 0x0004B43C File Offset: 0x0004963C
		private IInternalParser InternalParser
		{
			get
			{
				return this.Parser.InternalParser;
			}
		}

		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06001830 RID: 6192 RVA: 0x0004B44C File Offset: 0x0004964C
		private bool AllowDefinesInInterface { get; }

		// Token: 0x06001831 RID: 6193 RVA: 0x0004B454 File Offset: 0x00049654
		internal _ISignature \u0001(string \u0002, _IPreCompileContext \u0003, string \u0004)
		{
			IToken position;
			this.InternalParser.Next(out position, true, true);
			this.InternalParser.Scanner.SetPosition(position);
			this.Signature = \u0019.\u0003.\u0001();
			IInternalParser internalParser = this.InternalParser;
			Operator[] array = new Operator[3];
			RuntimeHelpers.InitializeArray(array, fieldof(\u0019.\u0002).FieldHandle);
			_ISignature isignature;
			if (internalParser.MatchOperator(array) == Operator.None)
			{
				isignature = this.Signature;
				isignature.Name = \u0002;
			}
			else
			{
				this.InternalParser.Scanner.SetPosition(position);
				_IStatement istatement = this.InternalParser.ParseInterfaceStatement();
				this.Parser.\u0001(istatement);
				if (\u0003 != null)
				{
					PragmaVisitor ivisit = new PragmaVisitor(\u0003, \u0004, \u0002)
					{
						ConditionalCompilationAllowed = this.AllowDefinesInInterface
					};
					istatement.Accept(ivisit);
				}
				\u001A.\u0006 u = new \u001A.\u0006(\u0002, \u0003);
				u.\u0001(istatement);
				ErrorVisitor errorVisitor = new ErrorVisitor();
				istatement.Accept(errorVisitor);
				global::\u0012.\u0002 u2 = new global::\u0012.\u0002(false);
				istatement.Accept(u2.Traverser);
				global::\u0012.\u0002 u3 = new global::\u0012.\u0002(true);
				istatement.Accept(u3.Traverser);
				isignature = u.Signature;
				isignature.Checksum = u2.Checksum;
				isignature.ChecksumNoInit = u3.Checksum;
				if (errorVisitor.MessageList.Count != 0)
				{
					isignature.AddMessages(errorVisitor.Messages);
				}
				if (this.Signature.Messages.Length != 0)
				{
					isignature.AddMessages(this.Signature.Messages);
				}
			}
			return isignature;
		}

		// Token: 0x06001832 RID: 6194 RVA: 0x0004B5A8 File Offset: 0x000497A8
		public _ISequenceStatement ParseGlobalVarlistNaked(string stName)
		{
			IToken position;
			this.InternalParser.Next(out position, true, true);
			this.InternalParser.Scanner.SetPosition(position);
			this.Signature = \u0019.\u0003.\u0001();
			IInternalParser internalParser = this.InternalParser;
			Operator[] array = new Operator[3];
			RuntimeHelpers.InitializeArray(array, fieldof(\u0019.\u0002).FieldHandle);
			if (internalParser.MatchOperator(array) == Operator.None)
			{
				return \u0019.\u0003.\u0001();
			}
			this.InternalParser.Scanner.SetPosition(position);
			_ISequenceStatement isequenceStatement = this.InternalParser.ParseInterfaceStatement() as _ISequenceStatement;
			if (isequenceStatement == null)
			{
				return \u0019.\u0003.\u0001();
			}
			IList<_ICompilerMessage> messages = this.Signature.GetMessages(true);
			if (messages != null && messages.Count > 0)
			{
				foreach (_ICompilerMessage cm in messages)
				{
					isequenceStatement.AddMessage(cm);
				}
			}
			return isequenceStatement;
		}

		// Token: 0x06001833 RID: 6195 RVA: 0x0004B688 File Offset: 0x00049888
		public _ISequenceStatement ParseNakedInterface()
		{
			IToken position;
			this.InternalParser.Next(out position, true, true);
			this.InternalParser.Scanner.SetPosition(position);
			this.Signature = \u0019.\u0003.\u0001();
			IInternalParser internalParser = this.InternalParser;
			Operator[] array = new Operator[8];
			RuntimeHelpers.InitializeArray(array, fieldof(\u0019.\u0001).FieldHandle);
			if (internalParser.MatchOperator(array) == Operator.None)
			{
				return \u0019.\u0003.\u0001();
			}
			this.InternalParser.Scanner.SetPosition(position);
			_ISequenceStatement isequenceStatement = this.InternalParser.ParseInterfaceStatement() as _ISequenceStatement;
			if (isequenceStatement == null)
			{
				return \u0019.\u0003.\u0001();
			}
			IList<_ICompilerMessage> messages = this.Signature.GetMessages(true);
			if (messages != null && messages.Count > 0)
			{
				foreach (_ICompilerMessage cm in messages)
				{
					isequenceStatement.AddMessage(cm);
				}
			}
			return isequenceStatement;
		}

		// Token: 0x06001834 RID: 6196 RVA: 0x0004B768 File Offset: 0x00049968
		public _ISignature _ParseInterface(_IPreCompileContext precom, string stCompilerDefines)
		{
			IToken position;
			this.InternalParser.Next(out position, true, true);
			this.InternalParser.Scanner.SetPosition(position);
			this.Signature = \u0019.\u0003.\u0001();
			IInternalParser internalParser = this.InternalParser;
			Operator[] array = new Operator[6];
			RuntimeHelpers.InitializeArray(array, fieldof(\u0019.\u0002).FieldHandle);
			_ISignature result;
			if (internalParser.MatchOperator(array) == Operator.None)
			{
				result = this.Signature;
			}
			else
			{
				this.InternalParser.Scanner.SetPosition(position);
				_IStatement u = this.InternalParser.ParseInterfaceStatement();
				result = this.Parser.\u0001(precom, stCompilerDefines, u);
			}
			return result;
		}

		// Token: 0x06001835 RID: 6197 RVA: 0x0004B7F8 File Offset: 0x000499F8
		public _IStatement ParseInterfaceSnippet()
		{
			this.Signature = \u0019.\u0003.\u0001();
			_IStatement istatement = this.InternalParser.ParseInterfaceStatement();
			foreach (_ICompilerMessage cm in this.Signature.Messages.OfType<_ICompilerMessage>())
			{
				_ISequenceStatement isequenceStatement = istatement as _ISequenceStatement;
				if (isequenceStatement != null && isequenceStatement._StatementList.Count > 0)
				{
					isequenceStatement._StatementList[0].AddMessage(cm);
				}
				else
				{
					istatement.AddMessage(cm);
				}
			}
			this.Signature = null;
			return istatement;
		}

		// Token: 0x06001836 RID: 6198 RVA: 0x0004B89C File Offset: 0x00049A9C
		internal _ISignature \u0001(_IPreCompileContext \u0002, string \u0003, _IStatement \u0004, string \u0005, bool \u0006, SignatureFlag \u0007, _ISignature \u0008)
		{
			List<ISourcePosition> unusedDeclarationPositions = null;
			if (\u0002 != null)
			{
				PragmaVisitor pragmaVisitor = new PragmaVisitor(\u0002, \u0003, \u0005)
				{
					ConditionalCompilationAllowed = this.AllowDefinesInInterface
				};
				\u0004.Accept(pragmaVisitor);
				unusedDeclarationPositions = pragmaVisitor.UnusedStatementPositions;
			}
			this.Parser.\u0001(\u0004);
			\u001A.\u0006 u = new \u001A.\u0006(\u0005, \u0002);
			if (\u0006)
			{
				u.\u0001(\u0004);
			}
			else
			{
				\u0004.Accept(u);
			}
			this.Parser.MessageGuid = u.MessageGuid;
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0004.Accept(errorVisitor);
			_ISignature3 isignature = (_ISignature3)u.Signature;
			if (isignature != null)
			{
				isignature.UnusedDeclarationPositions = unusedDeclarationPositions;
			}
			global::\u0012.\u0002 u2 = new global::\u0012.\u0002(false);
			SignatureFlag signatureFlag = \u0007 & ~SignatureFlag.PoolSignature;
			if (signatureFlag != SignatureFlag.None)
			{
				u2.Writer.Write((long)signatureFlag);
			}
			\u0004.Accept(u2.Traverser);
			global::\u0012.\u0002 u3 = new global::\u0012.\u0002(true);
			\u0004.Accept(u3.Traverser);
			if (isignature == null)
			{
				isignature = (_ISignature3)\u0019.\u0003.\u0001();
				isignature.AddMessages(\u0004.MessagesList);
				isignature.AddMessage(Severity.Error, MessageId.Err_IncompletePOUDeclaration, Array.Empty<object>());
			}
			_ISignatureWithOptionalInputs isignatureWithOptionalInputs = isignature as _ISignatureWithOptionalInputs;
			global::\u0012.\u0002 u4 = null;
			if (isignatureWithOptionalInputs != null)
			{
				u4 = new global::\u0012.\u0002(false);
				u4.\u0001(isignature);
			}
			this.\u0001(isignature, u2, u3);
			this.\u0001(isignature);
			isignature.Checksum = u2.Checksum;
			isignature.ChecksumNoInit = u3.Checksum;
			if (isignatureWithOptionalInputs != null)
			{
				isignatureWithOptionalInputs.ChecksumOptionalInputs = u4.Checksum;
			}
			if (errorVisitor.MessageList.Count != 0)
			{
				isignature.AddMessages(errorVisitor.Messages);
			}
			if (\u0008 != null && \u0008.Messages.Length != 0)
			{
				isignature.AddMessages(\u0008.Messages);
			}
			return isignature;
		}

		// Token: 0x06001837 RID: 6199 RVA: 0x0004BA3C File Offset: 0x00049C3C
		private void \u0001(_ISignature3 \u0002)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_OBJECT_NAME) && string.Compare(\u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBJECT_NAME), \u0002.Name, true, CultureInfo.InvariantCulture) != 0)
			{
				\u0002.AddMessage(null, Severity.Error, MessageId.Err_NamesNotEqual, new object[]
				{
					MessageId.Err_NamesNotEqual
				});
			}
		}

		// Token: 0x06001838 RID: 6200 RVA: 0x0004BA94 File Offset: 0x00049C94
		private void \u0001(_ISignature3 \u0002, global::\u0012.\u0002 \u0003, global::\u0012.\u0002 \u0004)
		{
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_ANYTYPECLASS))
				{
					\u0003.Writer.Write(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_ANYTYPECLASS));
					\u0004.Writer.Write(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_ANYTYPECLASS));
				}
				else if (ivariable.IsProperty && ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_OBJECT_NAME) && !string.Equals(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBJECT_NAME), ivariable.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					Guid u = ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID) ? Guid.Parse(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID)) : Guid.Empty;
					_ISourcePosition sourcepos = \u0019.\u0003.\u0001(-1, u, ivariable._SourcePosition.Position, ivariable._SourcePosition.PositionOffset, ivariable._SourcePosition.Length);
					\u0002.AddMessage(sourcepos, Severity.Error, MessageId.Err_NamesNotEqual, new object[]
					{
						MessageId.Err_NamesNotEqual
					});
				}
			}
		}

		// Token: 0x04000447 RID: 1095
		[CompilerGenerated]
		private readonly global::\u0011.\u0006 \u0001;

		// Token: 0x04000448 RID: 1096
		[CompilerGenerated]
		private readonly bool \u0001;
	}
}
