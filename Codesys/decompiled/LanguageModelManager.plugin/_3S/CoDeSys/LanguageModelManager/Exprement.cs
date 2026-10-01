using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200003C RID: 60
	[TypeGuid("{6751c006-173c-426d-a088-38b92a197acb}")]
	[StorageVersion("3.3.0.0")]
	public abstract class Exprement : GenericObject2, _IExprement3, _IExprement2, _IExprement, IExprement3, IExprement2, IExprement
	{
		// Token: 0x060002C7 RID: 711 RVA: 0x0000AC39 File Offset: 0x00009C39
		protected Exprement()
		{
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000AC41 File Offset: 0x00009C41
		protected Exprement(IToken token)
		{
			this.SetPosition(token);
		}

		// Token: 0x060002C9 RID: 713
		[SuppressMessage("Critical Code Smell", "S927:parameter names should match base declaration and other partial definitions", Justification = "stupid name in interface will not be changed")]
		public abstract void Accept(IExprementVisitor visitor);

		// Token: 0x060002CA RID: 714
		public abstract T Accept<T>(IExprementVisitor<T> visitor);

		// Token: 0x060002CB RID: 715
		public abstract void AcceptVisitor(IExprVisitor visitor);

		// Token: 0x060002CC RID: 716
		public abstract _IExprement Duplicate();

		// Token: 0x060002CD RID: 717 RVA: 0x0000AC50 File Offset: 0x00009C50
		public IMessage[] GetAllMessages()
		{
			return CompilerProxy.GetExprementMessages(this);
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000AC58 File Offset: 0x00009C58
		public virtual void DuplicateCommon(Exprement exprem)
		{
			exprem.PositionIntern = this.PositionIntern;
			exprem.LengthIntern = this.LengthIntern;
			exprem.Info = this.Info;
			if (this.CompilerMessageList == null || this.CompilerMessageList.Count == 0)
			{
				return;
			}
			LList<_ICompilerMessage> llist = new LList<_ICompilerMessage>(this.CompilerMessageList.Count);
			foreach (_ICompilerMessage icompilerMessage in this.CompilerMessageList)
			{
				llist.Add(new CompilerMessage(new SourcePosition(icompilerMessage.ProjectHandle, icompilerMessage.ObjectGuid, icompilerMessage.Position, icompilerMessage.PositionOffset, icompilerMessage.Length), icompilerMessage.Text, icompilerMessage.Severity, icompilerMessage.MessageId)
				{
					ShowAttribute = icompilerMessage.ShowAttribute
				});
			}
			llist.TrimExcess();
			exprem.CompilerMessageList = llist;
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x060002CF RID: 719 RVA: 0x0000AD44 File Offset: 0x00009D44
		public ISourcePosition Position
		{
			get
			{
				return this.GetPosition();
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x060002D0 RID: 720 RVA: 0x0000AD4C File Offset: 0x00009D4C
		// (set) Token: 0x060002D1 RID: 721 RVA: 0x0000AD54 File Offset: 0x00009D54
		public virtual IMinimalPosition _Position
		{
			get
			{
				return this.PositionIntern;
			}
			set
			{
				this.PositionIntern = value;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x060002D2 RID: 722 RVA: 0x00007F42 File Offset: 0x00006F42
		public static long InvalidPosition
		{
			get
			{
				return -1L;
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x060002D3 RID: 723 RVA: 0x0000AD60 File Offset: 0x00009D60
		// (set) Token: 0x060002D4 RID: 724 RVA: 0x0000ADAC File Offset: 0x00009DAC
		[DefaultSerialization("PositionToSave")]
		[StorageVersion("3.3.0.0")]
		protected long PositionToSave
		{
			get
			{
				if (this.PositionIntern == null)
				{
					return -1L;
				}
				return new IntegerUnion
				{
					m_long = this.PositionIntern.EditorPosition,
					m_short3 = this.PositionIntern.PositionOffset
				}.m_long;
			}
			set
			{
				if (value == -1L)
				{
					this.PositionIntern = null;
					return;
				}
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(value, ref nPosition, ref sPositionOffset);
				this.PositionIntern = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000ADDD File Offset: 0x00009DDD
		public virtual IBreakpoint CreateBreakpoint(int nOffset, byte bySize, short sTryCatchId)
		{
			if (sTryCatchId >= 0)
			{
				return new TryCatchBreakpoint(nOffset, this._Position, (short)bySize, sTryCatchId);
			}
			return this.CreateBreakpoint(nOffset, bySize);
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000ADFA File Offset: 0x00009DFA
		public virtual IBreakpoint CreateBreakpoint(int nOffset, byte bySize)
		{
			return new Breakpoint(nOffset, this._Position, this.LengthIntern);
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00005F0F File Offset: 0x00004F0F
		[Obsolete("obsolete!")]
		[SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Cannot remove because of released interfaces")]
		public virtual IBreakpoint SetBreakpoint(int nOffset, byte bySize)
		{
			return null;
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x060002D8 RID: 728 RVA: 0x00005F0F File Offset: 0x00004F0F
		[Obsolete("obsolete!")]
		[SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Cannot remove because of released interfaces")]
		public IBreakpoint Breakpoint
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x060002D9 RID: 729 RVA: 0x0000AE10 File Offset: 0x00009E10
		// (set) Token: 0x060002DA RID: 730 RVA: 0x0000AE4C File Offset: 0x00009E4C
		[DefaultSerialization("MessagesToSave")]
		[StorageVersion("3.3.0.0")]
		public CompilerMessage[] MessagesToSave
		{
			get
			{
				if (this.CompilerMessageList == null)
				{
					return Array.Empty<CompilerMessage>();
				}
				CompilerMessage[] array = new CompilerMessage[this.CompilerMessageList.Count];
				LList<_ICompilerMessage> compilerMessageList = this.CompilerMessageList;
				_ICompilerMessage[] array2 = array;
				compilerMessageList.CopyTo(array2);
				return array;
			}
			set
			{
				LList<_ICompilerMessage> llist = this.CompilerMessageList;
				if (llist == null)
				{
					llist = new LList<_ICompilerMessage>();
				}
				else
				{
					llist.Clear();
				}
				llist.AddRange(value);
				llist.TrimExcess();
				if (llist.Count > 0)
				{
					this.CompilerMessageList = llist;
				}
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x060002DB RID: 731 RVA: 0x0000AE8E File Offset: 0x00009E8E
		// (set) Token: 0x060002DC RID: 732 RVA: 0x0000AE98 File Offset: 0x00009E98
		public IList<_ICompilerMessage> MessagesList
		{
			get
			{
				return this.CompilerMessageList;
			}
			set
			{
				if (value == null)
				{
					if (this.CompilerMessageList != null && this.CompilerMessageList.Count > 0)
					{
						this.CompilerMessageList.Clear();
					}
					return;
				}
				LList<_ICompilerMessage> llist = this.CompilerMessageList;
				if (llist == null)
				{
					llist = new LList<_ICompilerMessage>(1);
					this.CompilerMessageList = llist;
				}
				llist.Clear();
				llist.AddRange(value);
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x060002DD RID: 733 RVA: 0x0000AEEF File Offset: 0x00009EEF
		// (set) Token: 0x060002DE RID: 734 RVA: 0x0000AEF7 File Offset: 0x00009EF7
		private LList<_ICompilerMessage> CompilerMessageList
		{
			get
			{
				return this._messages;
			}
			set
			{
				if (this._messages == null)
				{
					this._messages = new LList<_ICompilerMessage>(value.Count);
				}
				else
				{
					this._messages.Clear();
				}
				this._messages.AddRange(value);
			}
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0000AF2B File Offset: 0x00009F2B
		public _ICompilerMessage AddMessage(_ICompilerMessage cm)
		{
			return this.AddMessage(cm, false, false);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0000AF36 File Offset: 0x00009F36
		public _ICompilerMessage AddMessage(_ICompilerMessage cm, bool bAddAlways)
		{
			return this.AddMessage(cm, bAddAlways, false);
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0000AF44 File Offset: 0x00009F44
		public _ICompilerMessage AddMessage(_ICompilerMessage cm, bool bAddAlways, bool bCompareObjectGuid)
		{
			LList<_ICompilerMessage> compilerMessageList = this.CompilerMessageList;
			if (compilerMessageList == null)
			{
				this._messages = new LList<_ICompilerMessage>(1);
				compilerMessageList = this.CompilerMessageList;
			}
			else if (!bAddAlways)
			{
				foreach (_ICompilerMessage icompilerMessage in compilerMessageList)
				{
					if ((!bCompareObjectGuid || icompilerMessage.ObjectGuid == cm.ObjectGuid) && icompilerMessage.Length == cm.Length && Exprement.SeverityIsEqual(icompilerMessage.Severity, cm.Severity) && icompilerMessage.Text == cm.Text && icompilerMessage.Position == cm.Position)
					{
						return icompilerMessage;
					}
				}
			}
			compilerMessageList.Add(cm);
			return cm;
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000B018 File Offset: 0x0000A018
		private static bool SeverityIsEqual(Severity sev1, Severity sev2)
		{
			return sev1 == sev2 || (sev1 == Severity.SuppressedWarning && sev2 == Severity.Warning) || (sev1 == Severity.Warning && sev2 == Severity.SuppressedWarning);
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000B037 File Offset: 0x0000A037
		public _ICompilerMessage AddMessage(string stMessage, IMinimalPosition sourcepos, Severity severity, short sLength, MessageId nid)
		{
			return this.AddMessage(new CompilerMessage(sourcepos, stMessage, severity, sLength, nid));
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0000B04C File Offset: 0x0000A04C
		public void AddError(string stError, IToken tokenPos, MessageId mid)
		{
			short sLength;
			IMinimalPosition sourcepos = Exprement.PositionOfToken(tokenPos, out sLength);
			this.AddMessage(stError, sourcepos, Severity.Error, sLength, mid);
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0000B06E File Offset: 0x0000A06E
		public void AddError(string stError)
		{
			this.AddMessage(stError, this._Position, Severity.Error, this.LengthIntern, MessageId.None);
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0000B086 File Offset: 0x0000A086
		public void AddError(string stError, MessageId mid)
		{
			this.AddMessage(stError, this._Position, Severity.Error, this.LengthIntern, mid);
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0000B09E File Offset: 0x0000A09E
		public void AddError(string stError, MessageId mid, Guid messageGuid)
		{
			this.AddMessage(stError, this._Position, Severity.Error, this.LengthIntern, mid).ObjectGuid = messageGuid;
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000B0BB File Offset: 0x0000A0BB
		public void AddWarning(string stWarning)
		{
			this.AddMessage(stWarning, this._Position, Severity.Warning, this.LengthIntern, MessageId.None);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0000B0D4 File Offset: 0x0000A0D4
		public void AddWarning(string stWarning, MessageId mid)
		{
			Severity severity = Severity.Warning;
			this.AddMessage(stWarning, this._Position, severity, this.LengthIntern, mid);
		}

		// Token: 0x060002EA RID: 746 RVA: 0x0000B0FC File Offset: 0x0000A0FC
		public void AddWarning(string stError, IToken tokenPos, MessageId mid)
		{
			short sLength;
			IMinimalPosition sourcepos = Exprement.PositionOfToken(tokenPos, out sLength);
			this.AddMessage(stError, sourcepos, Severity.Warning, sLength, mid);
		}

		// Token: 0x060002EB RID: 747 RVA: 0x0000B11E File Offset: 0x0000A11E
		public void ClearMessages()
		{
			if (this._messages != null)
			{
				this._messages.Clear();
			}
		}

		// Token: 0x060002EC RID: 748 RVA: 0x0000B133 File Offset: 0x0000A133
		public virtual ISourcePosition GetPosition()
		{
			if (this.PositionIntern == null)
			{
				return null;
			}
			return new SourcePosition(-1, Guid.Empty, this.PositionIntern.EditorPosition, this.PositionIntern.PositionOffset, this.LengthIntern);
		}

		// Token: 0x060002ED RID: 749 RVA: 0x0000B166 File Offset: 0x0000A166
		public ISourcePosition CreatePosition(int nProjectHandle, Guid objectGuid)
		{
			if (this.PositionIntern == null)
			{
				return new SourcePosition(nProjectHandle, objectGuid, 0L, 0, 0);
			}
			return new SourcePosition(nProjectHandle, objectGuid, this.PositionIntern.EditorPosition, this.PositionIntern.PositionOffset, this.LengthIntern);
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060002EE RID: 750 RVA: 0x0000B19F File Offset: 0x0000A19F
		// (set) Token: 0x060002EF RID: 751 RVA: 0x0000B1A7 File Offset: 0x0000A1A7
		public object VisitorAttribute { get; set; }

		// Token: 0x060002F0 RID: 752 RVA: 0x0000B1B0 File Offset: 0x0000A1B0
		public virtual short MergePosition(IMinimalPosition pos1, short sLen1, IMinimalPosition pos2, short sLen2, out bool bMergeDone)
		{
			bMergeDone = false;
			if (pos1.EditorPosition != pos2.EditorPosition)
			{
				return sLen1;
			}
			short result;
			if (pos1.PositionOffset > pos2.PositionOffset)
			{
				result = pos1.PositionOffset - pos2.PositionOffset + sLen1;
			}
			else
			{
				result = pos2.PositionOffset - pos1.PositionOffset + sLen2;
			}
			bMergeDone = true;
			return result;
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0000B20C File Offset: 0x0000A20C
		public virtual bool MergePosition(IToken tokenIn)
		{
			short sLen;
			IMinimalPosition pos = Exprement.PositionOfToken(tokenIn, out sLen);
			bool result;
			this.LengthIntern = this.MergePosition(this.PositionIntern, this.LengthIntern, pos, sLen, out result);
			return result;
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0000B23F File Offset: 0x0000A23F
		// (set) Token: 0x060002F3 RID: 755 RVA: 0x0000B247 File Offset: 0x0000A247
		public virtual short PositionLength
		{
			get
			{
				return this.LengthIntern;
			}
			set
			{
				this.LengthIntern = value;
			}
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x0000B250 File Offset: 0x0000A250
		public void SetPosition(IToken token)
		{
			short lengthIntern;
			this.PositionIntern = Exprement.PositionOfToken(token, out lengthIntern);
			this.LengthIntern = lengthIntern;
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x0000B272 File Offset: 0x0000A272
		internal static IMinimalPosition PositionOfToken(IToken token, out short sLen)
		{
			sLen = (short)token.Length;
			return MinimalPosition.CreateMinimalPosition(token.Position, token.PositionOffset);
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x0000B28E File Offset: 0x0000A28E
		public override string ToString()
		{
			return CompilerProxy.DumpExprement(this);
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000B298 File Offset: 0x0000A298
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

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x060002F8 RID: 760 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x060002F9 RID: 761 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual IExprInfo Info
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060002FA RID: 762 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x060002FB RID: 763 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obsolete("obsolete")]
		[SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Cannot remove because of released interfaces")]
		public ICodeGeneratorAttributes CGAttributes
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060002FC RID: 764 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool KeepMessages
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060002FD RID: 765 RVA: 0x0000B2C0 File Offset: 0x0000A2C0
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && !this.KeepMessages && this.CompilerMessageList != null && this.CompilerMessageList.Count > 0)
			{
				LList<_ICompilerMessage> llist = new LList<_ICompilerMessage>();
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
				{
					llist = Enumerable.ToLList<_ICompilerMessage>(from m in this.CompilerMessageList
					where m.Severity == Severity.Error || m.Severity == Severity.FatalError
					select m);
					llist.ForEach(delegate(_ICompilerMessage m)
					{
						m.ProjectHandle = -1;
					});
				}
				this.CompilerMessageList = llist;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060002FE RID: 766
		// (set) Token: 0x060002FF RID: 767
		[Obfuscation(Feature = "rename")]
		public abstract IMinimalPosition PositionIntern { get; set; }

		// Token: 0x06000300 RID: 768 RVA: 0x0000B380 File Offset: 0x0000A380
		[Obfuscation(Feature = "rename")]
		public virtual void SetPositionIntern(IMinimalPosition minpos)
		{
			this.PositionIntern = minpos;
			if (this.MessagesList != null)
			{
				LList<_ICompilerMessage> llist = Enumerable.ToLList<_ICompilerMessage>(this.MessagesList);
				this.CompilerMessageList.Clear();
				foreach (_ICompilerMessage icompilerMessage in llist)
				{
					this.AddMessage(icompilerMessage.Text, minpos, icompilerMessage.Severity, icompilerMessage.Length, icompilerMessage.MessageId);
				}
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000301 RID: 769
		// (set) Token: 0x06000302 RID: 770
		[DefaultSerialization("Length")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public abstract short LengthIntern { get; set; }

		// Token: 0x04000070 RID: 112
		private LList<_ICompilerMessage> _messages;
	}
}
