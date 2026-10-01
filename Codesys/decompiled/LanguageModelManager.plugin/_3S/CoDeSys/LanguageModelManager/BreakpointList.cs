using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000023 RID: 35
	[TypeGuid("{9e894478-12b9-4f7b-ae88-4807ccb19a10}")]
	[StorageVersion("3.3.0.0")]
	public class BreakpointList : GenericObject2, _IBreakpointList, IBreakpointCollection, IBreakpointList2, IBreakpointList, ICollection, IEnumerable, IBreakpointListSerializable
	{
		// Token: 0x06000160 RID: 352 RVA: 0x00004584 File Offset: 0x00003584
		public override void AfterDeserialize()
		{
			try
			{
				base.AfterDeserialize();
				foreach (IBreakpoint bp in this.m_list)
				{
					this.AddToCodePosTable(bp);
					this.AddToSourcePosTable(bp);
				}
			}
			catch (Exception ex)
			{
				Debug.Assert(false, ex.ToString());
			}
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00004600 File Offset: 0x00003600
		public string Dump(ICompileContext comcon)
		{
			StringWriter stringWriter = new StringWriter();
			stringWriter.WriteLine("Breakpoints:");
			IScope scope = comcon.CreateGlobalIScope();
			for (int i = this.Count - 1; i >= 0; i--)
			{
				((Breakpoint)this[i]).Dump(stringWriter, scope, this.Count - i, this.Count);
			}
			return stringWriter.ToString();
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00004660 File Offset: 0x00003660
		private IBreakpoint AddToSourcePosTable(IBreakpoint bp)
		{
			if (this.m_htBySourcePos == null)
			{
				this.m_htBySourcePos = new LDictionary<long, object>();
			}
			Breakpoint breakpoint = bp as Breakpoint;
			if (breakpoint == null || breakpoint._Position == null)
			{
				return null;
			}
			if (!this.m_htBySourcePos.ContainsKey(breakpoint._Position.EditorPosition))
			{
				this.m_htBySourcePos[breakpoint._Position.EditorPosition] = bp;
				return null;
			}
			if (this.m_htBySourcePos[breakpoint._Position.EditorPosition] is IBreakpoint)
			{
				LSortedList<short, IBreakpoint> lsortedList = new LSortedList<short, IBreakpoint>(2);
				Breakpoint breakpoint2 = this.m_htBySourcePos[breakpoint._Position.EditorPosition] as Breakpoint;
				lsortedList[breakpoint2._Position.PositionOffset] = breakpoint2;
				this.m_htBySourcePos[breakpoint._Position.EditorPosition] = lsortedList;
			}
			LSortedList<short, IBreakpoint> lsortedList2 = this.m_htBySourcePos[breakpoint._Position.EditorPosition] as LSortedList<short, IBreakpoint>;
			IBreakpoint result;
			if (lsortedList2.TryGetValue(bp.Position.PositionOffset, ref result))
			{
				return result;
			}
			lsortedList2[bp.Position.PositionOffset] = bp;
			return null;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00004778 File Offset: 0x00003778
		private IBreakpoint AddToCodePosTable(IBreakpoint bp)
		{
			if (this.m_htByCodePos == null)
			{
				this.m_htByCodePos = new LDictionary<long, IBreakpoint>();
			}
			IBreakpoint result;
			if (this.m_htByCodePos.TryGetValue((long)bp.Offset, ref result))
			{
				return result;
			}
			this.m_htByCodePos[(long)bp.Offset] = bp;
			return null;
		}

		// Token: 0x06000164 RID: 356 RVA: 0x000047C4 File Offset: 0x000037C4
		internal _IBreakpointList CreateLittleList()
		{
			return new LittleBreakpointList(this.m_list, this.m_nIndexFirst);
		}

		// Token: 0x06000165 RID: 357 RVA: 0x000047D8 File Offset: 0x000037D8
		public void ChangeSourcePos(IBreakpoint bp, long lPosition)
		{
			long nPosition;
			short sPositionOffset;
			PositionHelper.SplitPosition(lPosition, ref nPosition, ref sPositionOffset);
			(bp as Breakpoint)._Position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			this.AddToSourcePosTable(bp);
		}

		// Token: 0x06000166 RID: 358 RVA: 0x0000480C File Offset: 0x0000380C
		public void UpdateSourcePositions(IDictionary<IMinimalPosition, IMinimalPosition> lSourcePosMap)
		{
			if (this.m_htBySourcePos == null)
			{
				return;
			}
			this.m_htBySourcePos.Clear();
			foreach (object obj in this)
			{
				Breakpoint breakpoint = (Breakpoint)obj;
				if (breakpoint._Position != null)
				{
					if (lSourcePosMap.ContainsKey(breakpoint._Position))
					{
						breakpoint._Position = lSourcePosMap[breakpoint._Position];
					}
					this.AddToSourcePosTable(breakpoint);
				}
			}
		}

		// Token: 0x06000167 RID: 359 RVA: 0x000048A0 File Offset: 0x000038A0
		public IBreakpoint FindBySourcePosition(ISourcePosition sourcepos)
		{
			if (this.m_htBySourcePos == null)
			{
				return null;
			}
			object obj = null;
			if (!this.m_htBySourcePos.TryGetValue(sourcepos.Position, ref obj))
			{
				return null;
			}
			IBreakpoint breakpoint = obj as IBreakpoint;
			if (breakpoint != null)
			{
				return breakpoint;
			}
			LSortedList<short, IBreakpoint> lsortedList = obj as LSortedList<short, IBreakpoint>;
			if (lsortedList == null || lsortedList.Count == 0)
			{
				return null;
			}
			IList<short> keys = lsortedList.Keys;
			if (lsortedList.Count == 1)
			{
				return lsortedList[keys[0]];
			}
			int i = 0;
			while (i < lsortedList.Count)
			{
				IBreakpoint breakpoint2 = lsortedList[keys[i]];
				if (breakpoint2.Position.PositionOffset > sourcepos.PositionOffset)
				{
					if (i == 0)
					{
						return breakpoint2;
					}
					return lsortedList[keys[i - 1]];
				}
				else
				{
					i++;
				}
			}
			return lsortedList[keys[keys.Count - 1]];
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00004974 File Offset: 0x00003974
		public IBreakpoint GetStepIntoBreakpointBySourcePosition(ISourcePosition sourcepos)
		{
			foreach (IBreakpoint breakpoint in this.m_list)
			{
				if (breakpoint.StepInSuccessors != null)
				{
					IStepInPosition[] stepInSuccessors = breakpoint.StepInSuccessors;
					for (int i = 0; i < stepInSuccessors.Length; i++)
					{
						IStepInPosition2 stepInPosition = stepInSuccessors[i] as IStepInPosition2;
						if (stepInPosition.StepInBreakpoint != null && stepInPosition.StepInBreakpoint.Position.Position == sourcepos.Position && stepInPosition.StepInBreakpoint.Position.PositionOffset == sourcepos.PositionOffset)
						{
							return stepInPosition.StepInBreakpoint;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00004A2C File Offset: 0x00003A2C
		public IBreakpoint GetByCodePosition(int nCodeOffset)
		{
			if (this.m_htByCodePos == null)
			{
				return null;
			}
			IBreakpoint result = null;
			if (!this.m_htByCodePos.TryGetValue((long)nCodeOffset, ref result))
			{
				return null;
			}
			return result;
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00004A5C File Offset: 0x00003A5C
		public _IBreakpoint FindNearestByCodePosition(int nCodeOffset, bool bBackward)
		{
			if (this.m_htByCodePos == null || this.m_list.Count == 0)
			{
				return null;
			}
			IBreakpoint breakpoint = null;
			if (!this.m_htByCodePos.TryGetValue((long)nCodeOffset, ref breakpoint))
			{
				if (bBackward)
				{
					if (this.FindBackwards(nCodeOffset, ref breakpoint))
					{
						return breakpoint as _IBreakpoint;
					}
				}
				else if (this.FindForwards(nCodeOffset, ref breakpoint))
				{
					return breakpoint as _IBreakpoint;
				}
			}
			return breakpoint as Breakpoint;
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00004AC0 File Offset: 0x00003AC0
		private bool FindForwards(int nCodeOffset, ref IBreakpoint bp)
		{
			for (nCodeOffset++; nCodeOffset <= this[0].Offset; nCodeOffset++)
			{
				if (this.m_htByCodePos.TryGetValue((long)nCodeOffset, ref bp))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00004AF0 File Offset: 0x00003AF0
		private bool FindBackwards(int nCodeOffset, ref IBreakpoint bp)
		{
			for (nCodeOffset--; nCodeOffset >= 0; nCodeOffset--)
			{
				if (this.m_htByCodePos.TryGetValue((long)nCodeOffset, ref bp))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00004B18 File Offset: 0x00003B18
		public IBreakpoint GetByStepOutPosition(int nCodeOffset, out IStepInPosition stepinpos)
		{
			stepinpos = null;
			foreach (IBreakpoint breakpoint in this.m_list)
			{
				if (breakpoint.Offset < nCodeOffset)
				{
					IStepInPosition[] stepInSuccessors = breakpoint.StepInSuccessors;
					if (stepInSuccessors != null)
					{
						foreach (IStepInPosition stepInPosition in from pos in stepInSuccessors
						where pos.StepOutBreakpoint != null
						select pos into p
						orderby p.StepOutBreakpoint.Offset
						select p)
						{
							if (stepInPosition.StepOutBreakpoint.Offset >= nCodeOffset)
							{
								stepinpos = stepInPosition;
								return breakpoint;
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00004C18 File Offset: 0x00003C18
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "legacy code")]
		private int AddBefore35160(ref _IBreakpoint bp)
		{
			Breakpoint breakpoint = this.AddToSourcePosTable(bp) as Breakpoint;
			if (breakpoint == null)
			{
				breakpoint = (this.AddToCodePosTable(bp) as Breakpoint);
			}
			if (breakpoint != null)
			{
				if (breakpoint != bp)
				{
					if (bp.Successors != null)
					{
						for (int i = 0; i < bp.Successors.Length; i++)
						{
							breakpoint.AddSuccessor(bp.Successors[i]);
						}
					}
					if (bp.AssemblySuccessors != null)
					{
						for (int j = 0; j < bp.AssemblySuccessors.Length; j++)
						{
							breakpoint.AddAssemblySuccessor(bp.AssemblySuccessors[j]);
						}
					}
					if (bp.StepInSuccessors != null)
					{
						for (int k = 0; k < bp.StepInSuccessors.Length; k++)
						{
							breakpoint.AddStepInSuccessor(bp.StepInSuccessors[k]);
						}
					}
				}
				for (int l = 0; l < this.m_list.Count; l++)
				{
					if (this.m_list[l] == breakpoint)
					{
						bp = breakpoint;
						return l;
					}
				}
			}
			this.m_list.Add(bp);
			return this.m_list.Count - 1;
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00004D20 File Offset: 0x00003D20
		public int Add(ref _IBreakpoint bp)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 16, 0))
			{
				return this.AddBefore35160(ref bp);
			}
			this.AddToSourcePosTable(bp);
			Breakpoint breakpoint = this.AddToCodePosTable(bp) as Breakpoint;
			Debug.Assert(breakpoint == null || breakpoint == bp);
			if (breakpoint == null)
			{
				this.m_list.Add(bp);
			}
			else
			{
				for (int i = 0; i < this.m_list.Count; i++)
				{
					if (this.m_list[i] == breakpoint)
					{
						bp = breakpoint;
						return i;
					}
				}
			}
			return this.m_list.Count - 1;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00004DBC File Offset: 0x00003DBC
		public void Remove(int nIndex)
		{
			IBreakpoint breakpoint = this.m_list[nIndex];
			this.m_list.RemoveAt(nIndex);
			this.m_htByCodePos.Remove((long)breakpoint.Offset);
			this.m_htBySourcePos.Remove(breakpoint.Position.Position);
		}

		// Token: 0x1700003C RID: 60
		public IBreakpoint this[int nIndex]
		{
			get
			{
				return this.m_list[nIndex];
			}
			set
			{
				this.m_list[nIndex] = value;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000173 RID: 371 RVA: 0x00004E29 File Offset: 0x00003E29
		public IBreakpoint First
		{
			get
			{
				if (this.m_nIndexFirst != -1)
				{
					return this[this.m_nIndexFirst];
				}
				if (this.Count > 0)
				{
					return this[this.Count - 1];
				}
				return null;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000174 RID: 372 RVA: 0x00004E5A File Offset: 0x00003E5A
		// (set) Token: 0x06000175 RID: 373 RVA: 0x00004E62 File Offset: 0x00003E62
		public int FirstIndex
		{
			get
			{
				return this.m_nIndexFirst;
			}
			set
			{
				this.m_nIndexFirst = value;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000176 RID: 374 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000177 RID: 375 RVA: 0x00004E6E File Offset: 0x00003E6E
		public int Count
		{
			get
			{
				return this.m_list.Count;
			}
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00004E7B File Offset: 0x00003E7B
		public void CopyTo(Array array, int index)
		{
			ArrayList arrayList = new ArrayList();
			arrayList.AddRange(this.m_list);
			arrayList.CopyTo(array, index);
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000179 RID: 377 RVA: 0x00004E95 File Offset: 0x00003E95
		public object SyncRoot
		{
			get
			{
				return this.m_list.SyncRoot;
			}
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00004EA2 File Offset: 0x00003EA2
		public IEnumerator GetEnumerator()
		{
			return this.m_list.GetEnumerator();
		}

		// Token: 0x04000026 RID: 38
		[DefaultSerialization("List")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.0.255")]
		[Obfuscation(Feature = "rename")]
		private LList<IBreakpoint> m_list = new LList<IBreakpoint>();

		// Token: 0x04000027 RID: 39
		[DefaultSerialization("First")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nIndexFirst = -1;

		// Token: 0x04000028 RID: 40
		[Obfuscation(Feature = "rename")]
		private LDictionary<long, object> m_htBySourcePos;

		// Token: 0x04000029 RID: 41
		[Obfuscation(Feature = "rename")]
		private LDictionary<long, IBreakpoint> m_htByCodePos;
	}
}
