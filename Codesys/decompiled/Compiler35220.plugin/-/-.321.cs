using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using \u0011;
using _3S.CoDeSys.Compiler35220.OnlineChange;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0018
{
	// Token: 0x02000369 RID: 873
	internal sealed class \u0010
	{
		// Token: 0x1700088F RID: 2191
		// (get) Token: 0x06003401 RID: 13313 RVA: 0x000CCC64 File Offset: 0x000CAE64
		internal \u001B CompileInformation { get; }

		// Token: 0x17000890 RID: 2192
		// (get) Token: 0x06003402 RID: 13314 RVA: 0x000CCC6C File Offset: 0x000CAE6C
		// (set) Token: 0x06003403 RID: 13315 RVA: 0x000CCC7C File Offset: 0x000CAE7C
		internal _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
			set
			{
				this.CompileInformation.ComconNew = value;
			}
		}

		// Token: 0x17000891 RID: 2193
		// (get) Token: 0x06003404 RID: 13316 RVA: 0x000CCC8C File Offset: 0x000CAE8C
		internal _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x17000892 RID: 2194
		// (get) Token: 0x06003405 RID: 13317 RVA: 0x000CCC9C File Offset: 0x000CAE9C
		internal _IPreCompileContext Precomp
		{
			get
			{
				return this.CompileInformation.Precomp;
			}
		}

		// Token: 0x17000893 RID: 2195
		// (get) Token: 0x06003406 RID: 13318 RVA: 0x000CCCAC File Offset: 0x000CAEAC
		internal _IPreCompileContext PrecompPool
		{
			get
			{
				return this.CompileInformation.PrecompPool;
			}
		}

		// Token: 0x17000894 RID: 2196
		// (get) Token: 0x06003407 RID: 13319 RVA: 0x000CCCBC File Offset: 0x000CAEBC
		internal Guid ApplicationGuid
		{
			get
			{
				return this.CompileInformation.ApplicationGuid;
			}
		}

		// Token: 0x17000895 RID: 2197
		// (get) Token: 0x06003408 RID: 13320 RVA: 0x000CCCCC File Offset: 0x000CAECC
		internal Guid DeviceGuid
		{
			get
			{
				return this.CompileInformation.DeviceGuid;
			}
		}

		// Token: 0x17000896 RID: 2198
		// (get) Token: 0x06003409 RID: 13321 RVA: 0x000CCCDC File Offset: 0x000CAEDC
		internal bool KeepCompileInformation
		{
			get
			{
				return this.CompileInformation.KeepCompileInformation;
			}
		}

		// Token: 0x17000897 RID: 2199
		// (get) Token: 0x0600340A RID: 13322 RVA: 0x000CCCEC File Offset: 0x000CAEEC
		// (set) Token: 0x0600340B RID: 13323 RVA: 0x000CCCF4 File Offset: 0x000CAEF4
		internal LList<_ICompiledPOU> compiledpous { get; set; }

		// Token: 0x17000898 RID: 2200
		// (get) Token: 0x0600340C RID: 13324 RVA: 0x000CCD00 File Offset: 0x000CAF00
		// (set) Token: 0x0600340D RID: 13325 RVA: 0x000CCD08 File Offset: 0x000CAF08
		internal LList<global::\u0011.\u0014> changedSignatures { get; set; }

		// Token: 0x17000899 RID: 2201
		// (get) Token: 0x0600340E RID: 13326 RVA: 0x000CCD14 File Offset: 0x000CAF14
		// (set) Token: 0x0600340F RID: 13327 RVA: 0x000CCD1C File Offset: 0x000CAF1C
		internal Dictionary<int, _ICompiledPOU> changedpous { get; set; }

		// Token: 0x1700089A RID: 2202
		// (get) Token: 0x06003410 RID: 13328 RVA: 0x000CCD28 File Offset: 0x000CAF28
		// (set) Token: 0x06003411 RID: 13329 RVA: 0x000CCD30 File Offset: 0x000CAF30
		internal ICodegenerator codegen { get; set; }

		// Token: 0x1700089B RID: 2203
		// (get) Token: 0x06003412 RID: 13330 RVA: 0x000CCD3C File Offset: 0x000CAF3C
		// (set) Token: 0x06003413 RID: 13331 RVA: 0x000CCD44 File Offset: 0x000CAF44
		internal Codegeneration codegeneration { get; set; }

		// Token: 0x1700089C RID: 2204
		// (get) Token: 0x06003414 RID: 13332 RVA: 0x000CCD50 File Offset: 0x000CAF50
		// (set) Token: 0x06003415 RID: 13333 RVA: 0x000CCD58 File Offset: 0x000CAF58
		internal IMessageCategory cmc { get; private set; }

		// Token: 0x1700089D RID: 2205
		// (get) Token: 0x06003416 RID: 13334 RVA: 0x000CCD64 File Offset: 0x000CAF64
		// (set) Token: 0x06003417 RID: 13335 RVA: 0x000CCD6C File Offset: 0x000CAF6C
		internal IProgressCallback callback { get; private set; }

		// Token: 0x1700089E RID: 2206
		// (get) Token: 0x06003418 RID: 13336 RVA: 0x000CCD78 File Offset: 0x000CAF78
		// (set) Token: 0x06003419 RID: 13337 RVA: 0x000CCD80 File Offset: 0x000CAF80
		public bool HasFullAbortError { get; private set; }

		// Token: 0x0600341A RID: 13338 RVA: 0x000CCD8C File Offset: 0x000CAF8C
		internal \u0010(\u001B \u008F\u0004, IMessageCategory \u0089\u0005, IProgressCallback \u009E\u0005)
		{
			this.CompileInformation = \u008F\u0004;
			this.\u0001 = new OnlineChangeDetails(false, true);
			this.cmc = \u0089\u0005;
			this.callback = \u009E\u0005;
		}

		// Token: 0x0600341B RID: 13339 RVA: 0x000CCDC4 File Offset: 0x000CAFC4
		private bool \u0001(_ISignature \u0002, out _ICompiledPOU \u0003, out _ICompiledPOU \u0004)
		{
			_ISignature isignature = \u0002;
			if (isignature.POUType == Operator.Method)
			{
				isignature = (this.ComconNew.GetSignatureById(isignature.ParentSignatureId) as _ISignature);
			}
			if (\u0018.\u0003.\u0001(isignature))
			{
				_ISignature isignature2 = \u0018.\u0003.\u0001(this.ComconNew, isignature);
				if (\u0002.POUType == Operator.Method)
				{
					isignature2 = (isignature2.GetSubSignature(\u0002.Name) as _ISignature);
				}
				if (this.\u0002(isignature2, out \u0003, out \u0004))
				{
					if (\u0002.POUType == Operator.FunctionBlock)
					{
						ISignature subSignature = \u0002.GetSubSignature(IdentifierConstants.MainSignatureName);
						\u0003 = (this.ComconNew.GetCompiledPOUById(subSignature.Id) as _ICompiledPOU);
					}
					else
					{
						\u0003 = (this.ComconNew.GetCompiledPOUById(\u0002.Id) as _ICompiledPOU);
					}
					return true;
				}
			}
			\u0003 = null;
			\u0004 = null;
			return false;
		}

		// Token: 0x0600341C RID: 13340 RVA: 0x000CCE84 File Offset: 0x000CB084
		internal bool \u0002(_ISignature \u0002, out _ICompiledPOU \u0003, out _ICompiledPOU \u0004)
		{
			if (this.\u0001(\u0002, out \u0003, out \u0004))
			{
				return true;
			}
			if (\u0002.POUType == Operator.FunctionBlock)
			{
				ISignature subSignature = \u0002.GetSubSignature(IdentifierConstants.MainSignatureName);
				\u0003 = (this.ComconNew.GetCompiledPOUById(subSignature.Id) as _ICompiledPOU);
			}
			else
			{
				\u0003 = (this.ComconNew.GetCompiledPOUById(\u0002.Id) as _ICompiledPOU);
			}
			if (\u0003 == null)
			{
				return false;
			}
			\u0004 = Helper.\u0001(this.ComconNew, \u0003, this.Precomp, this.PrecompPool);
			return true;
		}

		// Token: 0x0600341D RID: 13341 RVA: 0x000CCF08 File Offset: 0x000CB108
		internal _ICompiledPOU \u0001(_ICompiledPOU \u0002, _ICompiledPOU \u0003)
		{
			_ICompiledPOU icompiledPOU = null;
			if (this.changedpous.TryGetValue(\u0003.SignatureId, out icompiledPOU))
			{
				return icompiledPOU;
			}
			icompiledPOU = this.\u0002(\u0002, \u0003);
			this.changedpous[icompiledPOU.SignatureId] = \u0003;
			return icompiledPOU;
		}

		// Token: 0x0600341E RID: 13342 RVA: 0x000CCF4C File Offset: 0x000CB14C
		internal _ICompiledPOU \u0002(_ICompiledPOU \u0002, _ICompiledPOU \u0003)
		{
			_ICompiledPOU icompiledPOU = \u0002.CreateCompiledPOU();
			icompiledPOU.SignatureId = \u0003.SignatureId;
			this.ComconNew.RemoveCompiledPOU(\u0003);
			this.ComconNew.AddCompiledPOUSimple(icompiledPOU);
			foreach (CompiledPOUFlags cpFlag in new CompiledPOUFlags[]
			{
				CompiledPOUFlags.TopLevel,
				CompiledPOUFlags.ContainsDirVarAccess
			})
			{
				icompiledPOU.SetFlag(cpFlag, \u0003.GetFlag(cpFlag));
			}
			return icompiledPOU;
		}

		// Token: 0x0600341F RID: 13343 RVA: 0x000CCFB8 File Offset: 0x000CB1B8
		public bool \u0001(_ICompiledPOU \u0002, IErrorVisitor \u0003)
		{
			\u0002.Accept(\u0003);
			if (\u0003.MessageList.Any(new Func<_ICompilerMessage, bool>(\u0018.\u0010.\u0001)))
			{
				_ICompiledPOU icompiledPOU = this.ComconNew.GetCompiledPOUById(\u0002.SignatureId) as _ICompiledPOU;
				if (icompiledPOU != null && \u0003.MessageList.Any(new Func<_ICompilerMessage, bool>(\u0018.\u0010.\u0002)))
				{
					icompiledPOU.SetMessages(\u0003.MessageList);
					this.HasFullAbortError = true;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06003420 RID: 13344 RVA: 0x000CD030 File Offset: 0x000CB230
		public bool \u0001(_ISignature \u0002)
		{
			IList<_ICompilerMessage> messages = \u0002.GetMessages(false);
			if (messages.Any(new Func<_ICompilerMessage, bool>(\u0018.\u0010.\u0001)))
			{
				this.HasFullAbortError = messages.Any(new Func<_ICompilerMessage, bool>(\u0018.\u0010.\u0002));
				return true;
			}
			return false;
		}

		// Token: 0x06003421 RID: 13345 RVA: 0x000CD074 File Offset: 0x000CB274
		private static bool \u0001(_ICompilerMessage \u0002)
		{
			return (\u0002.Severity == Severity.Error || \u0002.Severity == Severity.FatalError) && \u0002.ShowCompile;
		}

		// Token: 0x06003422 RID: 13346 RVA: 0x000CD090 File Offset: 0x000CB290
		private static bool \u0002(_ICompilerMessage \u0002)
		{
			return \u0018.\u0010.\u0001.Contains(\u0002.MessageId);
		}

		// Token: 0x04000A0F RID: 2575
		[CompilerGenerated]
		private readonly \u001B \u0001;

		// Token: 0x04000A10 RID: 2576
		internal readonly LHashSet<int> \u0001 = new LHashSet<int>();

		// Token: 0x04000A11 RID: 2577
		internal readonly _IOnlineChangeDetails \u0001;

		// Token: 0x04000A12 RID: 2578
		internal bool \u0001;

		// Token: 0x04000A13 RID: 2579
		[CompilerGenerated]
		private LList<_ICompiledPOU> \u0001;

		// Token: 0x04000A14 RID: 2580
		[CompilerGenerated]
		private LList<global::\u0011.\u0014> \u0001;

		// Token: 0x04000A15 RID: 2581
		[CompilerGenerated]
		private Dictionary<int, _ICompiledPOU> \u0001;

		// Token: 0x04000A16 RID: 2582
		[CompilerGenerated]
		private ICodegenerator \u0001;

		// Token: 0x04000A17 RID: 2583
		[CompilerGenerated]
		private Codegeneration \u0001;

		// Token: 0x04000A18 RID: 2584
		[CompilerGenerated]
		private IMessageCategory \u0001;

		// Token: 0x04000A19 RID: 2585
		[CompilerGenerated]
		private IProgressCallback \u0001;

		// Token: 0x04000A1A RID: 2586
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000A1B RID: 2587
		private static readonly HashSet<MessageId> \u0001 = new HashSet<MessageId>
		{
			MessageId.Err_AddressExpected,
			MessageId.Err_AnyTypeOnlyInFunction,
			MessageId.Err_ArrayInitialisationNotPossible,
			MessageId.Err_AttributeNameExpected,
			MessageId.Err_CalcNeedsCall,
			MessageId.Err_ComparisonOperatorExpected,
			MessageId.Err_DefineValueExpected,
			MessageId.Err_DuplicateElseInCaseStatement,
			MessageId.Err_ExpressionExpectedInstead,
			MessageId.Err_FunctionBlockNoLongerValid,
			MessageId.Err_IdentifierExpected,
			MessageId.Err_IllegalOperator,
			MessageId.Err_IncompletePOUDeclaration,
			MessageId.Err_InvalidJumpDestination,
			MessageId.Err_NamesNotEqual,
			MessageId.Err_NewNeedsType,
			MessageId.Err_NoCaseLabelFound,
			MessageId.Err_OpNeedsExactInputs,
			MessageId.Err_Operator1of2Expected,
			MessageId.Err_Operator1of3ExpectedInsteadofEOF,
			MessageId.Err_OperatorExpected,
			MessageId.Err_OperatorExpectedInsteadofEOF,
			MessageId.Err_OverflowInAddress,
			MessageId.Err_ProjectDefinedNotSupportedFor,
			MessageId.Err_ReferenceNotAllowed,
			MessageId.Err_SemicolonExpected,
			MessageId.Err_SemicolonExpectedInsteadOfEnd,
			MessageId.Err_StringLiteralExpected,
			MessageId.Err_StringSizeExpected,
			MessageId.Err_StructureInitialisationNotPossible,
			MessageId.Err_TypeExpected,
			MessageId.Err_UnexpectedPragmaif,
			MessageId.Err_UnexpectedTokenFound,
			MessageId.Err_VarLengthArrayTopLevel,
			MessageId.Err_VersionInvalidFormat,
			MessageId.Err_VersionOverflow,
			MessageId.Err_VersionPartNegative
		};
	}
}
