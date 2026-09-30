using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Signature;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E2 RID: 226
	internal class SubSignatureTable
	{
		// Token: 0x0600110C RID: 4364 RVA: 0x00030F4C File Offset: 0x0002FF4C
		internal Signature[] GetSubSignatureArray()
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			Signature[] result;
			lock (obj)
			{
				LList<SubSignatureTable.ImmutableList<_ISignature>> llist = new LList<SubSignatureTable.ImmutableList<_ISignature>>(this.m_htSubSignatures.Values);
				int num = 0;
				foreach (SubSignatureTable.ImmutableList<_ISignature> immutableList in llist)
				{
					num += immutableList.Count;
				}
				if (num == 0)
				{
					result = null;
				}
				else
				{
					LList<Signature> llist2 = new LList<Signature>();
					foreach (SubSignatureTable.ImmutableList<_ISignature> immutableList2 in llist)
					{
						foreach (_ISignature isignature in immutableList2)
						{
							Signature signature = (Signature)isignature;
							llist2.Add(signature);
						}
					}
					result = llist2.ToArray();
				}
			}
			return result;
		}

		// Token: 0x0600110D RID: 4365 RVA: 0x00031070 File Offset: 0x00030070
		internal Signature[] GetSubSignatureArrayWithoutImplicit(PreCompileContext preCompileContext)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			LList<SubSignatureTable.ImmutableList<_ISignature>> llist;
			lock (obj)
			{
				llist = new LList<SubSignatureTable.ImmutableList<_ISignature>>(this.m_htSubSignatures.Values.Count);
				foreach (Guid guid in this.m_htSubSignatures.Keys)
				{
					ISignature signature = preCompileContext[guid];
					if (signature != null && !PreCompileContext.NoSaveToLib(signature))
					{
						llist.Add(this.m_htSubSignatures[guid]);
					}
				}
			}
			int num = 0;
			foreach (SubSignatureTable.ImmutableList<_ISignature> immutableList in llist)
			{
				num += immutableList.Count;
			}
			if (num == 0)
			{
				return null;
			}
			LList<Signature> llist2 = new LList<Signature>();
			foreach (SubSignatureTable.ImmutableList<_ISignature> immutableList2 in llist)
			{
				foreach (_ISignature isignature in immutableList2)
				{
					Signature signature2 = (Signature)isignature;
					llist2.Add(signature2);
				}
			}
			return llist2.ToArray();
		}

		// Token: 0x0600110E RID: 4366 RVA: 0x000311F8 File Offset: 0x000301F8
		internal void SetSubSignatureArray(Signature[] signArray)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				Dictionary<Guid, LList<_ISignature>> dictionary = new Dictionary<Guid, LList<_ISignature>>();
				foreach (Signature isignature in signArray)
				{
					Debug.Assert(isignature.POUType == Operator.Method || isignature.POUType == Operator.Action);
					if (!(isignature.ParentObjectGuid == Guid.Empty))
					{
						LList<_ISignature> llist;
						if (!dictionary.TryGetValue(isignature.ParentObjectGuid, out llist))
						{
							SubSignatureTable.ImmutableList<_ISignature> immutableList;
							if (this.m_htSubSignatures.TryGetValue(isignature.ParentObjectGuid, ref immutableList))
							{
								llist = new LList<_ISignature>(immutableList);
							}
							else
							{
								llist = new LList<_ISignature>();
							}
							dictionary[isignature.ParentObjectGuid] = llist;
						}
						llist.Add(isignature);
					}
				}
				foreach (KeyValuePair<Guid, LList<_ISignature>> keyValuePair in dictionary)
				{
					this.m_htSubSignatures[keyValuePair.Key] = new SubSignatureTable.ImmutableList<_ISignature>(keyValuePair.Value);
				}
			}
		}

		// Token: 0x0600110F RID: 4367 RVA: 0x00031334 File Offset: 0x00030334
		internal void SetSubSignaturesWithoutImplicit(Signature[] signArray)
		{
			if (signArray == null)
			{
				return;
			}
			this.SetSubSignatureArray(signArray);
		}

		// Token: 0x06001110 RID: 4368 RVA: 0x00031344 File Offset: 0x00030344
		internal void AddSignatureDeclarationsSerialization(LList<LMEntity> alHelp)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				foreach (SubSignatureTable.ImmutableList<_ISignature> immutableList in this.m_htSubSignatures.Values)
				{
					foreach (_ISignature isignature in immutableList)
					{
						if (isignature.RawDeclaration != null && !isignature.GetFlag(SignatureFlag.SuperGlobal))
						{
							LMEntity lmentity = ((LMEntity)isignature.RawDeclaration).Obfuscate();
							alHelp.Add(lmentity);
						}
					}
				}
			}
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x00031428 File Offset: 0x00030428
		internal void InsertSignatureChecksums(LDictionary<Guid, uint[]> checksums)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				foreach (SubSignatureTable.ImmutableList<_ISignature> immutableList in this.m_htSubSignatures.Values)
				{
					foreach (_ISignature isignature in immutableList)
					{
						if (isignature.RawDeclaration != null && !isignature.GetFlag(SignatureFlag.SuperGlobal))
						{
							checksums.Add(isignature.ObjectGuid, new uint[]
							{
								isignature.Checksum,
								isignature.ChecksumNoInit
							});
						}
					}
				}
			}
		}

		// Token: 0x06001112 RID: 4370 RVA: 0x00031518 File Offset: 0x00030518
		public void InsertSubSignature(_ISignature sign)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				SubSignatureTable.ImmutableList<_ISignature> immutableList;
				LList<_ISignature> llist;
				if (this.m_htSubSignatures.TryGetValue(sign.ParentObjectGuid, ref immutableList))
				{
					llist = new LList<_ISignature>(immutableList);
				}
				else
				{
					llist = new LList<_ISignature>();
				}
				llist.Add(sign);
				this.m_htSubSignatures[sign.ParentObjectGuid] = new SubSignatureTable.ImmutableList<_ISignature>(llist);
			}
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x00031598 File Offset: 0x00030598
		internal void RemoveTimeStampOnlySubSignatures()
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				if (this.m_htSubSignatures != null)
				{
					LList<Guid> llist = new LList<Guid>(this.m_htSubSignatures.Keys.Count);
					llist.AddRange(this.m_htSubSignatures.Keys);
					foreach (Guid guid in llist)
					{
						SubSignatureTable.ImmutableList<_ISignature> immutableList = SubSignatureTable.RemoveTimeStampOnlySubSignatures(this.m_htSubSignatures[guid]);
						if (immutableList.Count == 0)
						{
							this.m_htSubSignatures.Remove(guid);
						}
						else
						{
							this.m_htSubSignatures[guid] = immutableList;
						}
					}
				}
			}
		}

		// Token: 0x06001114 RID: 4372 RVA: 0x0003166C File Offset: 0x0003066C
		private static SubSignatureTable.ImmutableList<_ISignature> RemoveTimeStampOnlySubSignatures(SubSignatureTable.ImmutableList<_ISignature> alHelp)
		{
			LList<_ISignature> llist = null;
			for (int i = alHelp.Count - 1; i >= 0; i--)
			{
				if (alHelp[i].GetFlag(SignatureFlag.TimeStampOnly))
				{
					if (llist == null)
					{
						llist = new LList<_ISignature>(alHelp);
					}
					llist.RemoveAt(i);
				}
			}
			if (llist != null)
			{
				return new SubSignatureTable.ImmutableList<_ISignature>(llist);
			}
			return alHelp;
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x000316C8 File Offset: 0x000306C8
		internal bool RemoveSubSignature(_ISignature sign)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			bool result;
			lock (obj)
			{
				SubSignatureTable.ImmutableList<_ISignature> alHelp;
				if (!this.m_htSubSignatures.TryGetValue(sign.ParentObjectGuid, ref alHelp))
				{
					result = false;
				}
				else
				{
					SubSignatureTable.ImmutableList<_ISignature> immutableList = SubSignatureTable.RemoveByGuid(sign.ObjectGuid, alHelp);
					if (immutableList.Count == alHelp.Count)
					{
						result = false;
					}
					else
					{
						if (immutableList.Count == 0)
						{
							this.m_htSubSignatures.Remove(sign.ParentObjectGuid);
						}
						else
						{
							this.m_htSubSignatures[sign.ParentObjectGuid] = immutableList;
						}
						result = true;
					}
				}
			}
			return result;
		}

		// Token: 0x06001116 RID: 4374 RVA: 0x00031774 File Offset: 0x00030774
		private static SubSignatureTable.ImmutableList<_ISignature> RemoveByGuid(Guid guid, SubSignatureTable.ImmutableList<_ISignature> alHelp)
		{
			LList<_ISignature> llist = null;
			for (int i = alHelp.Count - 1; i >= 0; i--)
			{
				if (alHelp[i].ObjectGuid == guid)
				{
					if (llist == null)
					{
						llist = new LList<_ISignature>(alHelp);
					}
					llist.RemoveAt(i);
					break;
				}
			}
			if (llist != null)
			{
				return new SubSignatureTable.ImmutableList<_ISignature>(llist);
			}
			return alHelp;
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x000317D0 File Offset: 0x000307D0
		internal void RemoveSubSignature(Guid objectGuid)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				if (!this.m_htSubSignatures.Remove(objectGuid))
				{
					foreach (KeyValuePair<Guid, SubSignatureTable.ImmutableList<_ISignature>> keyValuePair in this.m_htSubSignatures)
					{
						SubSignatureTable.ImmutableList<_ISignature> immutableList = SubSignatureTable.RemoveByGuid(objectGuid, keyValuePair.Value);
						if (immutableList.Count != keyValuePair.Value.Count)
						{
							if (immutableList.Count == 0)
							{
								this.m_htSubSignatures.Remove(keyValuePair.Key);
								break;
							}
							this.m_htSubSignatures[keyValuePair.Key] = immutableList;
							break;
						}
					}
				}
			}
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x000318B4 File Offset: 0x000308B4
		public IList<_ISignature> _GetSubSignatures(Guid objectGuid)
		{
			object obj = SubSignatureTable.s_htSubParentSignaturesAndPOUsLock;
			IList<_ISignature> result;
			lock (obj)
			{
				SubSignatureTable.ImmutableList<_ISignature> immutableList;
				if (this.m_htSubSignatures.TryGetValue(objectGuid, ref immutableList))
				{
					result = immutableList.ReadOnlyList;
				}
				else
				{
					result = new List<_ISignature>(0);
				}
			}
			return result;
		}

		// Token: 0x040003FA RID: 1018
		private static readonly object s_htSubParentSignaturesAndPOUsLock = new object();

		// Token: 0x040003FB RID: 1019
		private readonly LDictionary<Guid, SubSignatureTable.ImmutableList<_ISignature>> m_htSubSignatures = new LDictionary<Guid, SubSignatureTable.ImmutableList<_ISignature>>();

		// Token: 0x020002A7 RID: 679
		private readonly struct ImmutableList<T> : IEnumerable<T>, IEnumerable
		{
			// Token: 0x06002B85 RID: 11141 RVA: 0x0007394F File Offset: 0x0007294F
			public ImmutableList(LList<T> values)
			{
				if (values == null)
				{
					throw new ArgumentException();
				}
				this.ReadOnlyList = values;
			}

			// Token: 0x17000C06 RID: 3078
			// (get) Token: 0x06002B86 RID: 11142 RVA: 0x00073962 File Offset: 0x00072962
			public int Count
			{
				get
				{
					return this.ReadOnlyList.Count;
				}
			}

			// Token: 0x06002B87 RID: 11143 RVA: 0x0007396F File Offset: 0x0007296F
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.ReadOnlyList.GetEnumerator();
			}

			// Token: 0x06002B88 RID: 11144 RVA: 0x0007396F File Offset: 0x0007296F
			public IEnumerator<T> GetEnumerator()
			{
				return this.ReadOnlyList.GetEnumerator();
			}

			// Token: 0x17000C07 RID: 3079
			public T this[int i]
			{
				get
				{
					return this.ReadOnlyList[i];
				}
			}

			// Token: 0x17000C08 RID: 3080
			// (get) Token: 0x06002B8A RID: 11146 RVA: 0x0007398A File Offset: 0x0007298A
			public LList<T> ReadOnlyList { get; }

			// Token: 0x06002B8B RID: 11147 RVA: 0x00073992 File Offset: 0x00072992
			public override string ToString()
			{
				return string.Format("{0} values", this.ReadOnlyList.Count);
			}
		}
	}
}
