using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001D9 RID: 473
	internal abstract class Exprement_Green : _IExprement, IExprement3, IExprement2, IExprement, IGreenTreeExprement
	{
		// Token: 0x17000928 RID: 2344
		// (get) Token: 0x06002149 RID: 8521 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IBreakpoint Breakpoint
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000929 RID: 2345
		// (get) Token: 0x0600214A RID: 8522 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600214B RID: 8523 RVA: 0x0005A448 File Offset: 0x00059448
		[Obsolete("obsolete")]
		public ICodeGeneratorAttributes CGAttributes
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700092A RID: 2346
		// (get) Token: 0x0600214C RID: 8524 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600214D RID: 8525 RVA: 0x0005A448 File Offset: 0x00059448
		public virtual IExprInfo Info
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700092B RID: 2347
		// (get) Token: 0x0600214E RID: 8526 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x0600214F RID: 8527 RVA: 0x0005A448 File Offset: 0x00059448
		public virtual short LengthIntern
		{
			get
			{
				return 0;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700092C RID: 2348
		// (get) Token: 0x06002150 RID: 8528 RVA: 0x0005A454 File Offset: 0x00059454
		// (set) Token: 0x06002151 RID: 8529 RVA: 0x0005A448 File Offset: 0x00059448
		public IList<_ICompilerMessage> MessagesList
		{
			get
			{
				return Array.Empty<_ICompilerMessage>();
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700092D RID: 2349
		// (get) Token: 0x06002152 RID: 8530 RVA: 0x0005A45B File Offset: 0x0005945B
		public ISourcePosition Position
		{
			get
			{
				return Exprement_Green.s_pos;
			}
		}

		// Token: 0x1700092E RID: 2350
		// (get) Token: 0x06002153 RID: 8531 RVA: 0x0005A462 File Offset: 0x00059462
		// (set) Token: 0x06002154 RID: 8532 RVA: 0x0005A448 File Offset: 0x00059448
		public IMinimalPosition PositionIntern
		{
			get
			{
				return Exprement_Green.s_minpos;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700092F RID: 2351
		// (get) Token: 0x06002155 RID: 8533 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x06002156 RID: 8534 RVA: 0x0005A448 File Offset: 0x00059448
		public virtual short PositionLength
		{
			get
			{
				return 0;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x17000930 RID: 2352
		// (get) Token: 0x06002157 RID: 8535 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06002158 RID: 8536 RVA: 0x0005A448 File Offset: 0x00059448
		public object VisitorAttribute
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x17000931 RID: 2353
		// (get) Token: 0x06002159 RID: 8537 RVA: 0x0005A469 File Offset: 0x00059469
		// (set) Token: 0x0600215A RID: 8538 RVA: 0x0005A448 File Offset: 0x00059448
		public IMinimalPosition _Position
		{
			get
			{
				return this.PositionIntern;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x0600215B RID: 8539
		[SuppressMessage("Critical Code Smell", "S927:parameter names should match base declaration and other partial definitions", Justification = "Name is not good")]
		public abstract void Accept(IExprementVisitor visitor);

		// Token: 0x0600215C RID: 8540
		public abstract void AcceptVisitor(IExprVisitor visitor);

		// Token: 0x0600215D RID: 8541 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddError(string stError)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x0600215E RID: 8542 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddError(string stError, MessageId mid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x0600215F RID: 8543 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddError(string stError, IToken tokenPos, MessageId mid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddError(string stError, MessageId mid, Guid messageGuid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x0005A448 File Offset: 0x00059448
		public _ICompilerMessage AddMessage(_ICompilerMessage cm)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x0005A448 File Offset: 0x00059448
		public _ICompilerMessage AddMessage(_ICompilerMessage cm, bool bAddAlways)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002163 RID: 8547 RVA: 0x0005A448 File Offset: 0x00059448
		public _ICompilerMessage AddMessage(_ICompilerMessage cm, bool bAddAlways, bool bCompareObjectGuid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002164 RID: 8548 RVA: 0x0005A448 File Offset: 0x00059448
		public _ICompilerMessage AddMessage(string stMessage, IMinimalPosition sourcepos, Severity severity, short sLength, MessageId nid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002165 RID: 8549 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddWarning(string stWarning)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002166 RID: 8550 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddWarning(string stWarning, MessageId mid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002167 RID: 8551 RVA: 0x0005A448 File Offset: 0x00059448
		public void AddWarning(string stError, IToken tokenPos, MessageId mid)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002168 RID: 8552 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void ClearMessages()
		{
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x0005A471 File Offset: 0x00059471
		public virtual IBreakpoint CreateBreakpoint(int nOffset, byte bySize, short sTryCatchId)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600216A RID: 8554 RVA: 0x0005A471 File Offset: 0x00059471
		public virtual IBreakpoint CreateBreakpoint(int nOffset, byte bySize)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x0005A478 File Offset: 0x00059478
		public ISourcePosition CreatePosition(int nProjectHandle, Guid objectGuid)
		{
			if (this.PositionIntern == null)
			{
				return new SourcePosition(nProjectHandle, objectGuid, 0L, 0, 0);
			}
			return new SourcePosition(nProjectHandle, objectGuid, this.PositionIntern.EditorPosition, this.PositionIntern.PositionOffset, this.LengthIntern);
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExprement Duplicate()
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x0005A4B4 File Offset: 0x000594B4
		public IMessage[] GetAllMessages()
		{
			return Array.Empty<_ICompilerMessage>();
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x0005A4C8 File Offset: 0x000594C8
		public ISourcePosition GetPosition()
		{
			return this.Position;
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x0005A4D0 File Offset: 0x000594D0
		public virtual bool IsEqual(_IExprement exprementRight)
		{
			if (exprementRight == null)
			{
				return false;
			}
			string a = this.ToString();
			string b = exprementRight.ToString();
			return a == b;
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool MergePosition(IToken tokenIn)
		{
			return true;
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x0005A4F5 File Offset: 0x000594F5
		public short MergePosition(IMinimalPosition pos1, short sLen1, IMinimalPosition pos2, short sLen2, out bool bMergeDone)
		{
			bMergeDone = false;
			return 0;
		}

		// Token: 0x06002172 RID: 8562 RVA: 0x0005A448 File Offset: 0x00059448
		public IBreakpoint SetBreakpoint(int nOffset, byte bySize)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002173 RID: 8563 RVA: 0x0005A448 File Offset: 0x00059448
		public void SetPosition(IToken token)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002174 RID: 8564 RVA: 0x0005A448 File Offset: 0x00059448
		public void SetPositionIntern(IMinimalPosition minpos)
		{
			throw new NotSupportedException("Do not attempt to manipulate green trees");
		}

		// Token: 0x06002175 RID: 8565 RVA: 0x0000B28E File Offset: 0x0000A28E
		public override string ToString()
		{
			return CompilerProxy.DumpExprement(this);
		}

		// Token: 0x04000671 RID: 1649
		private static readonly SourcePosition s_pos = SourcePosition.Empty;

		// Token: 0x04000672 RID: 1650
		private static readonly IMinimalPosition s_minpos = MinimalPosition.CreateMinimalPosition(0L, 0);
	}
}
