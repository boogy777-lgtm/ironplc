using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u0004;
using \u0014;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0011
{
	// Token: 0x02000164 RID: 356
	internal sealed class \u0006 : _IParser, IParser5, IParser4, IParser3, IParser2, IParser, IRawSTParser2, IRawSTParser
	{
		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x06001859 RID: 6233 RVA: 0x0004BFDC File Offset: 0x0004A1DC
		private bool AllowDefinesInInterface { get; }

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x0600185A RID: 6234 RVA: 0x0004BFE4 File Offset: 0x0004A1E4
		// (set) Token: 0x0600185B RID: 6235 RVA: 0x0004BFEC File Offset: 0x0004A1EC
		internal _ISignature Signature { get; set; }

		// Token: 0x17000584 RID: 1412
		// (get) Token: 0x0600185C RID: 6236 RVA: 0x0004BFF8 File Offset: 0x0004A1F8
		// (set) Token: 0x0600185D RID: 6237 RVA: 0x0004C000 File Offset: 0x0004A200
		public _ICompilerMessage FirstMessage { get; set; }

		// Token: 0x0600185E RID: 6238 RVA: 0x0004C00C File Offset: 0x0004A20C
		private \u0006()
		{
			this.\u0001 = new global::\u0003.\u0006(this);
			this.AllowDefinesInInterface = ParserHelper.\u0001();
			this.InterfaceParser = new InterfaceParser(this, this.AllowDefinesInInterface);
		}

		// Token: 0x0600185F RID: 6239 RVA: 0x0004C040 File Offset: 0x0004A240
		public \u0006(string \u0087\u0003) : this()
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0087\u0003, true, false, true, false);
			scanner.AllowNestedComments = APEnvironmentFacade.Instance.LanguageModelMgr.AllowNestedComments;
			this.\u0001 = global::\u0014.\u0006.\u0001((IScanner9)scanner, this.\u0001);
		}

		// Token: 0x06001860 RID: 6240 RVA: 0x0004C094 File Offset: 0x0004A294
		public \u0006(IList<string> \u0088\u0003) : this()
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0088\u0003, true, false, true, false);
			scanner.AllowNestedComments = APEnvironmentFacade.Instance.LanguageModelMgr.AllowNestedComments;
			this.\u0001 = global::\u0014.\u0006.\u0001((IScanner9)scanner, this.\u0001);
		}

		// Token: 0x06001861 RID: 6241 RVA: 0x0004C0E8 File Offset: 0x0004A2E8
		public \u0006(string \u0087\u0003, bool \u0089\u0003) : this()
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0087\u0003, true, false, true, false);
			scanner.AllowNestedComments = APEnvironmentFacade.Instance.LanguageModelMgr.AllowNestedComments;
			scanner.AllowMultipleUnderlines = \u0089\u0003;
			this.\u0001 = global::\u0014.\u0006.\u0001((IScanner9)scanner, this.\u0001);
			this.\u0001.ImplicitAnyway = \u0089\u0003;
		}

		// Token: 0x06001862 RID: 6242 RVA: 0x0004C150 File Offset: 0x0004A350
		public \u0006(IScanner \u008B\u0003, bool \u0089\u0003) : this()
		{
			this.\u0001 = global::\u0014.\u0006.\u0001((IScanner9)\u008B\u0003, this.\u0001);
			this.\u0001.ImplicitAnyway = \u0089\u0003;
		}

		// Token: 0x06001863 RID: 6243 RVA: 0x0004C17C File Offset: 0x0004A37C
		public \u0006(IScanner \u008B\u0003, bool \u0089\u0003, Version \u0093\u0007, ILMCompileOptions3 \u0004\u0006) : this()
		{
			this.\u0001 = global::\u0014.\u0006.\u0001((IScanner9)\u008B\u0003, this.\u0001, \u0093\u0007, \u0004\u0006);
			this.\u0001.ImplicitAnyway = \u0089\u0003;
		}

		// Token: 0x06001864 RID: 6244 RVA: 0x0004C1AC File Offset: 0x0004A3AC
		public \u0006(IScanner \u008B\u0003) : this()
		{
			this.\u0001 = global::\u0014.\u0006.\u0001((IScanner9)\u008B\u0003, this.\u0001);
		}

		// Token: 0x17000585 RID: 1413
		// (get) Token: 0x06001865 RID: 6245 RVA: 0x0004C1CC File Offset: 0x0004A3CC
		// (set) Token: 0x06001866 RID: 6246 RVA: 0x0004C1DC File Offset: 0x0004A3DC
		public bool ImplicitAnyway
		{
			get
			{
				return this.InternalParser.ImplicitAnyway;
			}
			set
			{
				this.InternalParser.ImplicitAnyway = value;
			}
		}

		// Token: 0x17000586 RID: 1414
		// (get) Token: 0x06001867 RID: 6247 RVA: 0x0004C1EC File Offset: 0x0004A3EC
		private InterfaceParser InterfaceParser { get; }

		// Token: 0x17000587 RID: 1415
		// (get) Token: 0x06001868 RID: 6248 RVA: 0x0004C1F4 File Offset: 0x0004A3F4
		// (set) Token: 0x06001869 RID: 6249 RVA: 0x0004C1FC File Offset: 0x0004A3FC
		public global::\u0004.\u0004 Checker { get; set; }

		// Token: 0x17000588 RID: 1416
		// (get) Token: 0x0600186A RID: 6250 RVA: 0x0004C208 File Offset: 0x0004A408
		public IScanner UsedScanner
		{
			get
			{
				return this.\u0001.Scanner;
			}
		}

		// Token: 0x17000589 RID: 1417
		// (get) Token: 0x0600186B RID: 6251 RVA: 0x0004C218 File Offset: 0x0004A418
		// (set) Token: 0x0600186C RID: 6252 RVA: 0x0004C228 File Offset: 0x0004A428
		public Guid MessageGuid
		{
			get
			{
				return this.\u0001.MessageGuid;
			}
			set
			{
				this.\u0001.MessageGuid = value;
			}
		}

		// Token: 0x0600186D RID: 6253 RVA: 0x0004C238 File Offset: 0x0004A438
		public ISequenceStatement \u0001()
		{
			return this.\u0001() as ISequenceStatement;
		}

		// Token: 0x0600186E RID: 6254 RVA: 0x0004C248 File Offset: 0x0004A448
		public IExpression \u0001()
		{
			bool flag;
			IExpression expression = this.\u0002(out flag);
			if (flag || ParserHelper.\u0001((_IExpression)expression))
			{
				return null;
			}
			return expression;
		}

		// Token: 0x0600186F RID: 6255 RVA: 0x0004C274 File Offset: 0x0004A474
		internal void \u0001(_IStatement \u0002)
		{
			if (this.Checker != null)
			{
				\u0002.Accept(this.Checker);
			}
		}

		// Token: 0x06001870 RID: 6256 RVA: 0x0004C28C File Offset: 0x0004A48C
		public IExpression \u0002()
		{
			bool flag;
			IExpression result = this.\u0001(out flag);
			if (flag)
			{
				return null;
			}
			return result;
		}

		// Token: 0x06001871 RID: 6257 RVA: 0x0004C2A8 File Offset: 0x0004A4A8
		public _IStatement \u0001()
		{
			return this.\u0001(false);
		}

		// Token: 0x06001872 RID: 6258 RVA: 0x0004C2B4 File Offset: 0x0004A4B4
		public _IStatement \u0001(bool \u0002)
		{
			return this.InternalParser.ParseST(\u0002);
		}

		// Token: 0x06001873 RID: 6259 RVA: 0x0004C2C4 File Offset: 0x0004A4C4
		public _IExpression[] \u0001()
		{
			return this.InternalParser.ParseSTSnippet();
		}

		// Token: 0x06001874 RID: 6260 RVA: 0x0004C2D4 File Offset: 0x0004A4D4
		public IStatement \u0001()
		{
			return this.InternalParser.ParseInterfaceStatement();
		}

		// Token: 0x06001875 RID: 6261 RVA: 0x0004C2E4 File Offset: 0x0004A4E4
		public _ISequenceStatement \u0001()
		{
			return this.InternalParser.ParseRawST();
		}

		// Token: 0x06001876 RID: 6262 RVA: 0x0004C2F4 File Offset: 0x0004A4F4
		public IPOUSyntax[] \u0001()
		{
			return this.InternalParser.ParsePOUs();
		}

		// Token: 0x06001877 RID: 6263 RVA: 0x0004C304 File Offset: 0x0004A504
		public IPOUSyntax[] \u0001(out IEnumerable<IMessage> \u0002)
		{
			IInternalParser2 internalParser = this.InternalParser as IInternalParser2;
			if (internalParser != null)
			{
				return internalParser.ParsePOUs(out \u0002);
			}
			\u0002 = Array.Empty<IMessage>();
			return this.InternalParser.ParsePOUs();
		}

		// Token: 0x06001878 RID: 6264 RVA: 0x0004C33C File Offset: 0x0004A53C
		public _IExpression \u0001(out bool \u0002)
		{
			return this.InternalParser.ParseAssignExp(out \u0002);
		}

		// Token: 0x06001879 RID: 6265 RVA: 0x0004C34C File Offset: 0x0004A54C
		public string \u0001(MessageId \u0002, params object[] \u0003)
		{
			return global::\u0003.\u0006.\u0001(\u0002, \u0003);
		}

		// Token: 0x0600187A RID: 6266 RVA: 0x0004C358 File Offset: 0x0004A558
		public _IExpression \u0002(out bool \u0002)
		{
			return this.InternalParser.ParseSTOperand(out \u0002);
		}

		// Token: 0x0600187B RID: 6267 RVA: 0x0004C368 File Offset: 0x0004A568
		internal static IExpression \u0001(string \u0002)
		{
			return new global::\u0011.\u0006(\u0002).\u0002();
		}

		// Token: 0x0600187C RID: 6268 RVA: 0x0004C378 File Offset: 0x0004A578
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			this.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600187D RID: 6269 RVA: 0x0004C388 File Offset: 0x0004A588
		public void \u0001(_IExprement \u0002, IToken \u0003, MessageId \u0004, params object[] \u0005)
		{
			this.\u0001.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600187E RID: 6270 RVA: 0x0004C39C File Offset: 0x0004A59C
		public void \u0002(_IExprement \u0002, IToken \u0003, MessageId \u0004, params object[] \u0005)
		{
			this.\u0001.\u0002(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600187F RID: 6271 RVA: 0x0004C3B0 File Offset: 0x0004A5B0
		public void \u0001(_IExprement \u0002, Severity \u0003, MessageId \u0004, params object[] \u0005)
		{
			this.\u0001.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001880 RID: 6272 RVA: 0x0004C3C4 File Offset: 0x0004A5C4
		public void \u0001(_IExprement \u0002, IToken \u0003, Severity \u0004, MessageId \u0005, params object[] \u0006)
		{
			this.\u0001.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06001881 RID: 6273 RVA: 0x0004C3D8 File Offset: 0x0004A5D8
		public void \u0002(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			this.\u0001.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001882 RID: 6274 RVA: 0x0004C3E8 File Offset: 0x0004A5E8
		public void \u0001(IToken \u0002, MessageId \u0003, params object[] \u0004)
		{
			this.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001883 RID: 6275 RVA: 0x0004C3F8 File Offset: 0x0004A5F8
		internal static ICompiledType \u0001(string \u0002)
		{
			return new global::\u0011.\u0006(\u0002).\u0001();
		}

		// Token: 0x06001884 RID: 6276 RVA: 0x0004C408 File Offset: 0x0004A608
		public IExpression \u0003()
		{
			return this.InternalParser.ParseInitialisation();
		}

		// Token: 0x06001885 RID: 6277 RVA: 0x0004C418 File Offset: 0x0004A618
		public ICompiledType \u0001()
		{
			return this.\u0001();
		}

		// Token: 0x06001886 RID: 6278 RVA: 0x0004C420 File Offset: 0x0004A620
		public ICompiledType \u0001(out IMessage \u0002)
		{
			ICompiledType result = this.\u0001();
			\u0002 = this.FirstMessage;
			return result;
		}

		// Token: 0x06001887 RID: 6279 RVA: 0x0004C430 File Offset: 0x0004A630
		public _IType \u0001()
		{
			return this.InternalParser.ParseType();
		}

		// Token: 0x06001888 RID: 6280 RVA: 0x0004C440 File Offset: 0x0004A640
		public void \u0001(_IForStatement \u0002)
		{
			this.InternalParser.ExtendForLoop(\u0002);
		}

		// Token: 0x06001889 RID: 6281 RVA: 0x0004C450 File Offset: 0x0004A650
		public ISignature \u0001(string \u0002)
		{
			return this.\u0001(\u0002, null, null);
		}

		// Token: 0x0600188A RID: 6282 RVA: 0x0004C45C File Offset: 0x0004A65C
		private _ISignature \u0001(string \u0002, _IPreCompileContext \u0003, string \u0004)
		{
			return this.InterfaceParser.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600188B RID: 6283 RVA: 0x0004C46C File Offset: 0x0004A66C
		public _IStatement \u0002()
		{
			return this.InterfaceParser.ParseInterfaceSnippet();
		}

		// Token: 0x0600188C RID: 6284 RVA: 0x0004C47C File Offset: 0x0004A67C
		public ISignature \u0001()
		{
			return this.\u0001(null, null);
		}

		// Token: 0x0600188D RID: 6285 RVA: 0x0004C488 File Offset: 0x0004A688
		public _ISequenceStatement \u0001(string \u0002)
		{
			return this.InterfaceParser.ParseGlobalVarlistNaked(\u0002);
		}

		// Token: 0x0600188E RID: 6286 RVA: 0x0004C498 File Offset: 0x0004A698
		public _ISequenceStatement \u0002()
		{
			return this.InterfaceParser.ParseNakedInterface();
		}

		// Token: 0x0600188F RID: 6287 RVA: 0x0004C4A8 File Offset: 0x0004A6A8
		internal _ISignature \u0001(_IPreCompileContext \u0002, string \u0003, _IStatement \u0004)
		{
			return this.\u0001(\u0002, \u0003, \u0004, null, false, SignatureFlag.None);
		}

		// Token: 0x06001890 RID: 6288 RVA: 0x0004C4B8 File Offset: 0x0004A6B8
		internal _ISignature \u0001(_IPreCompileContext \u0002, string \u0003, _IStatement \u0004, string \u0005, bool \u0006)
		{
			return this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, SignatureFlag.None);
		}

		// Token: 0x06001891 RID: 6289 RVA: 0x0004C4CC File Offset: 0x0004A6CC
		internal _ISignature \u0001(_IPreCompileContext \u0002, string \u0003, _IStatement \u0004, string \u0005, bool \u0006, SignatureFlag \u0007)
		{
			return this.InterfaceParser.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, this.Signature);
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x0004C4E8 File Offset: 0x0004A6E8
		public _ISignature \u0001(_IPreCompileContext \u0002, string \u0003)
		{
			return this.InterfaceParser._ParseInterface(\u0002, \u0003);
		}

		// Token: 0x1700058A RID: 1418
		// (get) Token: 0x06001893 RID: 6291 RVA: 0x0004C4F8 File Offset: 0x0004A6F8
		public IInternalParser InternalParser
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06001894 RID: 6292 RVA: 0x0004C500 File Offset: 0x0004A700
		public _IStatement \u0001(out bool \u0002, IMinimalPosition \u0003, string \u0004)
		{
			return this.\u0001.ParsePragma(out \u0002, \u0003, \u0004);
		}

		// Token: 0x06001895 RID: 6293 RVA: 0x0004C510 File Offset: 0x0004A710
		public void \u0001(IPOUSyntax[] \u0002, Guid \u0003, Guid \u0004, ILanguageModel \u0005)
		{
			((IInternalParser2)this.\u0001).CreateLanguageModelOfRawST(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0400044C RID: 1100
		[CompilerGenerated]
		private readonly bool \u0001;

		// Token: 0x0400044D RID: 1101
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x0400044E RID: 1102
		[CompilerGenerated]
		private _ICompilerMessage \u0001;

		// Token: 0x0400044F RID: 1103
		[CompilerGenerated]
		private readonly InterfaceParser \u0001;

		// Token: 0x04000450 RID: 1104
		[CompilerGenerated]
		private global::\u0004.\u0004 \u0001;

		// Token: 0x04000451 RID: 1105
		private readonly IInternalParser \u0001;

		// Token: 0x04000452 RID: 1106
		private readonly global::\u0003.\u0006 \u0001;
	}
}
