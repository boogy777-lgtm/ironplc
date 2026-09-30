using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200002A RID: 42
	[TypeGuid("{5CA0E8E2-E20D-417a-9B36-00EFB92B2B25}")]
	[StorageVersion("3.3.0.0")]
	public class LittleBreakpointList : GenericObject2, _IBreakpointList, IBreakpointCollection, IBreakpointList2, IBreakpointList, ICollection, IEnumerable, IBreakpointListSerializable
	{
		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060001C1 RID: 449 RVA: 0x0000628E File Offset: 0x0000528E
		// (set) Token: 0x060001C2 RID: 450 RVA: 0x0000629B File Offset: 0x0000529B
		[DefaultSerialization("List")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ArrayList SerializationCompatibilityList
		{
			get
			{
				return new ArrayList(this.m_breakpoints);
			}
			set
			{
				if (value != null)
				{
					this.m_breakpoints = new IBreakpoint[value.Count];
					value.CopyTo(this.m_breakpoints);
				}
			}
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x000062BD File Offset: 0x000052BD
		public LittleBreakpointList()
		{
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x000062CC File Offset: 0x000052CC
		public LittleBreakpointList(LList<IBreakpoint> alBPL, int nFirstIndex)
		{
			this.m_breakpoints = new IBreakpoint[alBPL.Count];
			alBPL.CopyTo(this.m_breakpoints);
			this.m_nIndexFirst = nFirstIndex;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00006300 File Offset: 0x00005300
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

		// Token: 0x060001C6 RID: 454 RVA: 0x00006360 File Offset: 0x00005360
		public _IBreakpoint FindNearestByCodePosition(int nCodeOffset, bool bBackward)
		{
			if (this.m_breakpoints.Length == 0)
			{
				return null;
			}
			_IBreakpoint ibreakpoint = this.GetByCodePosition(nCodeOffset) as _IBreakpoint;
			if (ibreakpoint != null)
			{
				return ibreakpoint;
			}
			if (bBackward)
			{
				this.FindBackwards(nCodeOffset, ref ibreakpoint);
				return ibreakpoint;
			}
			this.FindForwards(nCodeOffset, ref ibreakpoint);
			return ibreakpoint;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x000063A4 File Offset: 0x000053A4
		private bool FindForwards(int nCodeOffset, ref _IBreakpoint bp)
		{
			for (nCodeOffset++; nCodeOffset <= this[0].Offset; nCodeOffset++)
			{
				bp = (this.GetByCodePosition(nCodeOffset) as _IBreakpoint);
				if (bp != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x000063D6 File Offset: 0x000053D6
		private bool FindBackwards(int nCodeOffset, ref _IBreakpoint bp)
		{
			for (nCodeOffset--; nCodeOffset >= 0; nCodeOffset--)
			{
				bp = (this.GetByCodePosition(nCodeOffset) as _IBreakpoint);
				if (bp != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x000063FD File Offset: 0x000053FD
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

		// Token: 0x17000058 RID: 88
		public IBreakpoint this[int nIndex]
		{
			get
			{
				return this.m_breakpoints[nIndex];
			}
			set
			{
				this.m_breakpoints[nIndex] = value;
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00006444 File Offset: 0x00005444
		public void UpdateSourcePositions(IDictionary<IMinimalPosition, IMinimalPosition> lSourcePosMap)
		{
			foreach (object obj in this)
			{
				Breakpoint breakpoint = (Breakpoint)obj;
				if (breakpoint._Position != null && lSourcePosMap.ContainsKey(breakpoint._Position))
				{
					breakpoint._Position = lSourcePosMap[breakpoint._Position];
				}
			}
		}

		// Token: 0x060001CD RID: 461 RVA: 0x000064BC File Offset: 0x000054BC
		public IBreakpoint FindBySourcePosition(ISourcePosition sourcepos)
		{
			LSortedList<short, IBreakpoint> lsortedList = new LSortedList<short, IBreakpoint>();
			foreach (object obj in this)
			{
				IBreakpoint breakpoint = (IBreakpoint)obj;
				if (breakpoint.Position.Position == sourcepos.Position)
				{
					lsortedList[breakpoint.Position.PositionOffset] = breakpoint;
				}
			}
			if (lsortedList.Count == 0)
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

		// Token: 0x060001CE RID: 462 RVA: 0x000065C4 File Offset: 0x000055C4
		public IBreakpoint GetStepIntoBreakpointBySourcePosition(ISourcePosition sourcepos)
		{
			foreach (object obj in this)
			{
				IBreakpoint breakpoint = (IBreakpoint)obj;
				if (breakpoint.StepInSuccessors != null)
				{
					IStepInPosition[] stepInSuccessors = breakpoint.StepInSuccessors;
					for (int i = 0; i < stepInSuccessors.Length; i++)
					{
						IStepInPosition2 stepInPosition = stepInSuccessors[i] as IStepInPosition2;
						if (stepInPosition.StepInBreakpoint.Position.Position == sourcepos.Position && stepInPosition.StepInBreakpoint.Position.PositionOffset == sourcepos.PositionOffset)
						{
							return stepInPosition.StepInBreakpoint;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000667C File Offset: 0x0000567C
		public IBreakpoint GetByCodePosition(int nCodeOffset)
		{
			foreach (object obj in this)
			{
				IBreakpoint breakpoint = (IBreakpoint)obj;
				if (breakpoint.Offset == nCodeOffset)
				{
					return breakpoint;
				}
			}
			return null;
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x000066DC File Offset: 0x000056DC
		public IBreakpoint GetByStepOutPosition(int nCodeOffset, out IStepInPosition stepinpos)
		{
			stepinpos = null;
			for (int i = 0; i < this.m_breakpoints.Length; i++)
			{
				IBreakpoint breakpoint = this.m_breakpoints[i];
				IStepInPosition[] stepInSuccessors = breakpoint.StepInSuccessors;
				if (stepInSuccessors != null)
				{
					foreach (IStepInPosition stepInPosition in stepInSuccessors)
					{
						if (stepInPosition.StepOutBreakpoint != null && stepInPosition.StepOutBreakpoint.Offset == nCodeOffset)
						{
							stepinpos = stepInPosition;
							return breakpoint;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060001D2 RID: 466 RVA: 0x0000674B File Offset: 0x0000574B
		public int Count
		{
			get
			{
				return this.m_breakpoints.Length;
			}
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00006755 File Offset: 0x00005755
		public void CopyTo(Array array, int index)
		{
			this.m_breakpoints.CopyTo(array, index);
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x00006764 File Offset: 0x00005764
		public object SyncRoot
		{
			get
			{
				return this.m_breakpoints.SyncRoot;
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x00006771 File Offset: 0x00005771
		public IEnumerator GetEnumerator()
		{
			return this.m_breakpoints.GetEnumerator();
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x0000677E File Offset: 0x0000577E
		// (set) Token: 0x060001D7 RID: 471 RVA: 0x0000677E File Offset: 0x0000577E
		public int FirstIndex
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000677E File Offset: 0x0000577E
		public int Add(ref _IBreakpoint bp)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000677E File Offset: 0x0000577E
		public void Remove(int nIndex)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000677E File Offset: 0x0000577E
		public void ChangeSourcePos(IBreakpoint bp, long lPosition)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400004C RID: 76
		private IBreakpoint[] m_breakpoints;

		// Token: 0x0400004D RID: 77
		[DefaultSerialization("First")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nIndexFirst = -1;
	}
}
