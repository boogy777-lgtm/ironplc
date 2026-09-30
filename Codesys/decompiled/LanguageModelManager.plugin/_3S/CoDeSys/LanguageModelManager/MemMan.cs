using System;
using System.Diagnostics;
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
	// Token: 0x02000129 RID: 297
	[TypeGuid("{17b1421c-2984-4921-865a-ab1609aa9f92}")]
	[StorageVersion("3.3.0.0")]
	public class MemMan : GenericObject2, _IMemoryManager, IMemoryManager, IMemoryManagerSerializable
	{
		// Token: 0x060019A5 RID: 6565 RVA: 0x000496A8 File Offset: 0x000486A8
		public MemMan()
		{
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x000496BB File Offset: 0x000486BB
		public MemMan(int nSize, int nBaseAddress)
		{
			this.m_nSize = nSize;
			this.m_nBaseAddress = nBaseAddress;
			this.m_alMemManGaps.Add(new MemManGap(0, nSize));
		}

		// Token: 0x060019A7 RID: 6567 RVA: 0x000496EE File Offset: 0x000486EE
		public void AddGap(int nAddress, int nSize)
		{
			this.m_alMemManGaps.Add(new MemManGap(nAddress, nSize));
		}

		// Token: 0x060019A8 RID: 6568 RVA: 0x00049704 File Offset: 0x00048704
		public _IMemoryManager Duplicate()
		{
			MemMan memMan = new MemMan(this.m_nSize, this.m_nBaseAddress);
			memMan.m_alMemManGaps = new LList<MemManGap>();
			foreach (MemManGap memManGap in this.m_alMemManGaps)
			{
				memMan.m_alMemManGaps.Add(memManGap.Duplicate() as MemManGap);
			}
			return memMan;
		}

		// Token: 0x170006A3 RID: 1699
		// (get) Token: 0x060019A9 RID: 6569 RVA: 0x00049780 File Offset: 0x00048780
		// (set) Token: 0x060019AA RID: 6570 RVA: 0x00049788 File Offset: 0x00048788
		public int Size
		{
			get
			{
				return this.m_nSize;
			}
			set
			{
				this.m_nSize = value;
			}
		}

		// Token: 0x170006A4 RID: 1700
		// (get) Token: 0x060019AB RID: 6571 RVA: 0x00049791 File Offset: 0x00048791
		// (set) Token: 0x060019AC RID: 6572 RVA: 0x00049799 File Offset: 0x00048799
		public int Base
		{
			get
			{
				return this.m_nBaseAddress;
			}
			set
			{
				this.m_nBaseAddress = value;
			}
		}

		// Token: 0x170006A5 RID: 1701
		// (get) Token: 0x060019AD RID: 6573 RVA: 0x000497A4 File Offset: 0x000487A4
		public int SizeAllocated
		{
			get
			{
				if (this.Count == 0)
				{
					return this.m_nSize;
				}
				_IMemManGap imemManGap = this[this.Count - 1];
				if (imemManGap.Offset + imemManGap.Size == this.m_nSize)
				{
					return imemManGap.Offset;
				}
				return this.m_nSize;
			}
		}

		// Token: 0x170006A6 RID: 1702
		// (get) Token: 0x060019AE RID: 6574 RVA: 0x000497F4 File Offset: 0x000487F4
		public int SizeWithoutGaps
		{
			get
			{
				int num = this.m_nSize;
				if (this.Count == 0)
				{
					return this.m_nSize;
				}
				LList<MemManGap> alMemManGaps = this.m_alMemManGaps;
				lock (alMemManGaps)
				{
					foreach (MemManGap memManGap in this.m_alMemManGaps)
					{
						num -= memManGap.Size;
					}
				}
				return num;
			}
		}

		// Token: 0x170006A7 RID: 1703
		// (get) Token: 0x060019AF RID: 6575 RVA: 0x00049884 File Offset: 0x00048884
		public int MaxContiguosMemory
		{
			get
			{
				if (this.Count == 0)
				{
					return 0;
				}
				int num = 0;
				for (int i = 0; i < this.Count; i++)
				{
					if (this[i].Size > num)
					{
						num = this[i].Size;
					}
				}
				return num;
			}
		}

		// Token: 0x060019B0 RID: 6576 RVA: 0x000498CC File Offset: 0x000488CC
		public bool Shrink(int nNewSize)
		{
			if (this.Count == 0)
			{
				return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300;
			}
			_IMemManGap imemManGap = this[this.Count - 1];
			if (imemManGap.Offset > nNewSize)
			{
				return false;
			}
			this.m_nSize = nNewSize;
			imemManGap.Size = this.m_nSize - imemManGap.Offset;
			return true;
		}

		// Token: 0x060019B1 RID: 6577 RVA: 0x0004992B File Offset: 0x0004892B
		public bool Shrink(int nMinSize, int nAdditionalPercentage, int nMaxSize)
		{
			return this.Shrink(nMinSize, nAdditionalPercentage, nMaxSize, 1);
		}

		// Token: 0x060019B2 RID: 6578 RVA: 0x00049938 File Offset: 0x00048938
		public bool Shrink(int nMinSize, int nAdditionalPercentage, int nMaxSize, int areaAlign)
		{
			if (this.Count == 0)
			{
				return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300;
			}
			_IMemManGap imemManGap = this[this.Count - 1];
			if (imemManGap.Offset + imemManGap.Size == this.m_nSize)
			{
				double num = 1.0 + (double)nAdditionalPercentage / 100.0;
				int num3;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35110)
				{
					ulong num2 = (ulong)((double)imemManGap.Offset * num);
					if (num2 > 2147483647UL)
					{
						num3 = int.MaxValue;
					}
					else
					{
						num3 = (int)num2;
					}
				}
				else
				{
					num3 = (int)((double)imemManGap.Offset * num);
				}
				int num4 = Help.max(new int[]
				{
					num3,
					nMinSize
				});
				if (num4 % areaAlign != 0 && num4 <= 2147483647 - areaAlign)
				{
					num4 = (num4 / areaAlign + 1) * areaAlign;
				}
				if (num4 > nMaxSize && imemManGap.Offset < nMaxSize)
				{
					num4 = nMaxSize;
				}
				return this.Shrink(num4);
			}
			return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33140;
		}

		// Token: 0x170006A8 RID: 1704
		// (get) Token: 0x060019B3 RID: 6579 RVA: 0x00049A3C File Offset: 0x00048A3C
		public int Count
		{
			get
			{
				return this.m_alMemManGaps.Count;
			}
		}

		// Token: 0x170006A9 RID: 1705
		public _IMemManGap this[int i]
		{
			get
			{
				if (i < 0 || i >= this.Count)
				{
					return null;
				}
				return this.m_alMemManGaps[i];
			}
		}

		// Token: 0x060019B5 RID: 6581 RVA: 0x00049A68 File Offset: 0x00048A68
		private int FindLower(int nOffset)
		{
			for (int i = 0; i < this.Count - 1; i++)
			{
				if (this[i + 1].Offset > nOffset)
				{
					return i;
				}
			}
			return this.Count - 1;
		}

		// Token: 0x060019B6 RID: 6582 RVA: 0x00049AA4 File Offset: 0x00048AA4
		private int FindUpper(int nOffset)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].Offset > nOffset)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x00049AD4 File Offset: 0x00048AD4
		public bool CheckConsistency()
		{
			for (int i = 0; i < this.Count - 1; i++)
			{
				if (this[i + 1].Offset < this[i].Offset + this[i].Size)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x00049B20 File Offset: 0x00048B20
		public void DeleteGapsButLast()
		{
			LList<MemManGap> alMemManGaps = this.m_alMemManGaps;
			lock (alMemManGaps)
			{
				if (this.m_alMemManGaps.Count > 1)
				{
					for (int i = this.m_alMemManGaps.Count - 2; i >= 0; i--)
					{
						this.m_alMemManGaps.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x060019B9 RID: 6585 RVA: 0x00049B90 File Offset: 0x00048B90
		public int AllocateHighestAddress(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, out bool bSuccess)
		{
			LList<MemManGap> alMemManGaps = this.m_alMemManGaps;
			int invalidOffset;
			lock (alMemManGaps)
			{
				bSuccess = true;
				int num = iSizeRequiredPrm;
				if (num < iMinSize)
				{
					num = iMinSize;
				}
				for (int i = this.Count - 1; i >= 0; i--)
				{
					MemManGap memManGap = this.m_alMemManGaps[i];
					int num2;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500)
					{
						num2 = (memManGap.Offset + memManGap.Size - num) % iGranularity;
					}
					else
					{
						num2 = (memManGap.Offset + memManGap.Size) % iGranularity;
					}
					if (memManGap.Size >= num + num2)
					{
						int num3 = memManGap.Offset + memManGap.Size - num2 - num;
						if (iSegmentSize != MemorySettings.NoSegmentation && (num3 + this.Base) / iSegmentSize != (num3 + this.Base + num) / iSegmentSize)
						{
							int num4 = (num3 + num + this.Base) % iSegmentSize;
							num2 += num4;
							num3 = memManGap.Offset + memManGap.Size - num2 - num;
							if (num + num4 > memManGap.Size)
							{
								goto IL_156;
							}
						}
						Debug.Assert(num3 % iGranularity == 0);
						int iOffset = num3 + num;
						int num5 = num2;
						if (num5 > 0)
						{
							this.m_alMemManGaps.Insert(i + 1, new MemManGap(iOffset, num5));
						}
						int num6 = num3 - memManGap.Offset;
						if (num6 == 0)
						{
							this.m_alMemManGaps.RemoveAt(i);
						}
						else
						{
							memManGap.Size = num6;
						}
						return num3;
					}
					IL_156:;
				}
				bSuccess = false;
				invalidOffset = SignatureConstant.InvalidOffset;
			}
			return invalidOffset;
		}

		// Token: 0x060019BA RID: 6586 RVA: 0x00049D34 File Offset: 0x00048D34
		public int AllocateWithoutGap(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, out bool bSuccess)
		{
			return this.Allocate(iGranularity, iSegmentSize, iSizeRequiredPrm, iPackMode, iMinSize, true, out bSuccess);
		}

		// Token: 0x060019BB RID: 6587 RVA: 0x00049D46 File Offset: 0x00048D46
		public int Allocate(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, out bool bSuccess)
		{
			return this.Allocate(iGranularity, iSegmentSize, iSizeRequiredPrm, iPackMode, iMinSize, false, out bSuccess);
		}

		// Token: 0x060019BC RID: 6588 RVA: 0x00049D58 File Offset: 0x00048D58
		internal int Allocate(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, bool bWithoutGap, out bool bSuccess)
		{
			LList<MemManGap> alMemManGaps = this.m_alMemManGaps;
			int invalidOffset;
			lock (alMemManGaps)
			{
				bSuccess = true;
				int num = iSizeRequiredPrm;
				if (num < iMinSize)
				{
					num = iMinSize;
				}
				for (int i = 0; i < this.Count; i++)
				{
					MemManGap memManGap = this.m_alMemManGaps[i];
					int num2 = memManGap.Offset % iGranularity;
					if (num2 != 0)
					{
						num2 = iGranularity - num2;
					}
					if (memManGap.Size >= num + num2)
					{
						int num3 = memManGap.Offset + num2;
						if (iSegmentSize != MemorySettings.NoSegmentation && (num3 + this.Base) / iSegmentSize != (num3 + this.Base + num) / iSegmentSize)
						{
							int num4 = (num3 + this.Base) % iSegmentSize;
							if (num4 != 0)
							{
								num4 = iSegmentSize - num4;
							}
							num2 += num4;
							num3 = memManGap.Offset + num2;
							if (num + num4 > memManGap.Size)
							{
								goto IL_121;
							}
						}
						int iOffset = num3 + num;
						int num5 = memManGap.Size - (num + num2);
						if (num5 > 0)
						{
							this.m_alMemManGaps.Insert(i + 1, new MemManGap(iOffset, num5));
						}
						if (num2 == 0 || bWithoutGap)
						{
							this.m_alMemManGaps.RemoveAt(i);
						}
						else
						{
							memManGap.Size = num2;
						}
						Debug.Assert(num3 % iGranularity == 0);
						return num3;
					}
					IL_121:;
				}
				bSuccess = false;
				invalidOffset = SignatureConstant.InvalidOffset;
			}
			return invalidOffset;
		}

		// Token: 0x060019BD RID: 6589 RVA: 0x00049ECC File Offset: 0x00048ECC
		public bool AllocateHard(int iOffset, int iSize)
		{
			LList<MemManGap> alMemManGaps = this.m_alMemManGaps;
			bool result;
			lock (alMemManGaps)
			{
				for (int i = 0; i < this.Count; i++)
				{
					MemManGap memManGap = this.m_alMemManGaps[i];
					if (memManGap.Offset <= iOffset && memManGap.Offset + memManGap.Size >= iOffset + iSize)
					{
						int num = iOffset - memManGap.Offset;
						int num2 = memManGap.Size - num - iSize;
						if (num2 > 0)
						{
							this.m_alMemManGaps.Insert(i + 1, new MemManGap(iOffset + iSize, num2));
						}
						if (num == 0)
						{
							this.m_alMemManGaps.RemoveAt(i);
						}
						else
						{
							memManGap.Size = num;
						}
						return true;
					}
					if (memManGap.Offset > iOffset && memManGap.Offset < iOffset + iSize && memManGap.Offset + memManGap.Size >= iOffset + iSize)
					{
						int num3 = iOffset + iSize - memManGap.Offset;
						memManGap.Size -= num3;
						memManGap.Offset += num3;
						if (memManGap.Size == 0)
						{
							this.m_alMemManGaps.RemoveAt(i);
						}
						return true;
					}
					if (memManGap.Offset <= iOffset && memManGap.Offset + memManGap.Size > iOffset && memManGap.Offset + memManGap.Size < iOffset + iSize)
					{
						int size = iOffset - memManGap.Offset;
						memManGap.Size = size;
						if (memManGap.Size == 0)
						{
							this.m_alMemManGaps.RemoveAt(i);
						}
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060019BE RID: 6590 RVA: 0x0004A06C File Offset: 0x0004906C
		public bool Allocate(int iOffset, int iSize)
		{
			LList<MemManGap> alMemManGaps = this.m_alMemManGaps;
			bool result;
			lock (alMemManGaps)
			{
				for (int i = 0; i < this.Count; i++)
				{
					MemManGap memManGap = this.m_alMemManGaps[i];
					if (memManGap.Offset <= iOffset && memManGap.Offset + memManGap.Size >= iOffset + iSize)
					{
						int num = iOffset - memManGap.Offset;
						int num2 = memManGap.Size - num - iSize;
						if (num2 > 0)
						{
							this.m_alMemManGaps.Insert(i + 1, new MemManGap(iOffset + iSize, num2));
						}
						if (num == 0)
						{
							this.m_alMemManGaps.RemoveAt(i);
						}
						else
						{
							memManGap.Size = num;
						}
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060019BF RID: 6591 RVA: 0x0004A13C File Offset: 0x0004913C
		public bool Free(int iOffset, int iSize)
		{
			if (iSize == 0 && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
			{
				return true;
			}
			int num = this.FindLower(iOffset);
			int num2 = this.FindUpper(iOffset);
			if (num < 0 || num >= this.Count || num2 < 0 || num2 >= this.Count)
			{
				return false;
			}
			_IMemManGap imemManGap = this[num];
			_IMemManGap imemManGap2 = this[num2];
			if (imemManGap != null && imemManGap.Offset <= iOffset)
			{
				Debug.Assert(imemManGap.Offset + imemManGap.Size <= iOffset);
			}
			if (imemManGap2 != null && iOffset <= imemManGap2.Offset)
			{
				Debug.Assert(iOffset + iSize <= imemManGap2.Offset);
			}
			if (imemManGap != null && imemManGap.Offset + imemManGap.Size == iOffset)
			{
				imemManGap.Size += iSize;
			}
			else if (imemManGap2 == null || iOffset + iSize != imemManGap2.Offset)
			{
				if (num2 != -1)
				{
					this.m_alMemManGaps.Insert(num2, new MemManGap(iOffset, iSize));
				}
				else
				{
					this.m_alMemManGaps.Insert(num + 1, new MemManGap(iOffset, iSize));
				}
			}
			else
			{
				imemManGap2.Offset = iOffset;
				imemManGap2.Size += iSize;
			}
			if (imemManGap != null && imemManGap2 != null && imemManGap.Offset + imemManGap.Size == imemManGap2.Offset)
			{
				imemManGap.Size += imemManGap2.Size;
				this.m_alMemManGaps.RemoveAt(num2);
			}
			return true;
		}

		// Token: 0x04000532 RID: 1330
		[DefaultDuplication(DuplicationMethod.Deep)]
		[DefaultSerialization("MemManGaps")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private LList<MemManGap> m_alMemManGaps = new LList<MemManGap>();

		// Token: 0x04000533 RID: 1331
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nSize;

		// Token: 0x04000534 RID: 1332
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("BaseAddress")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nBaseAddress;
	}
}
