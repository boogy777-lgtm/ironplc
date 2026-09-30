using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000281 RID: 641
	public class PreCompCrossReferences : _IPreCompCrossReferences
	{
		// Token: 0x06002AE1 RID: 10977 RVA: 0x0007129C File Offset: 0x0007029C
		internal PreCompCrossReferences(RefType reftype)
		{
			this.m_reftype = reftype;
		}

		// Token: 0x06002AE2 RID: 10978 RVA: 0x000712CC File Offset: 0x000702CC
		public void Add(string stName, Guid objectGuid, Guid messageGuid)
		{
			if (objectGuid == messageGuid)
			{
				messageGuid = Guid.Empty;
			}
			GuidClass guidClass;
			if (messageGuid != Guid.Empty)
			{
				guidClass = new GuidClassEx(messageGuid);
			}
			else
			{
				guidClass = new GuidClass(objectGuid);
			}
			GuidClass guidClass2;
			if (!this.m_htGuids.TryGetValue(guidClass, ref guidClass2))
			{
				guidClass2 = guidClass;
				this.m_htGuids[guidClass2] = guidClass2;
			}
			if (messageGuid != Guid.Empty)
			{
				GuidClassEx guidClassEx = guidClass2 as GuidClassEx;
				if (guidClassEx == null)
				{
					this.m_htGuids.Remove(guidClass2);
					guidClassEx = (GuidClassEx)guidClass;
					this.m_htGuids[guidClassEx] = guidClassEx;
				}
				guidClassEx.AddObjectGuid(objectGuid);
			}
			object obj;
			if (!this.m_htCrossreferences.TryGetValue(stName, ref obj))
			{
				this.m_htCrossreferences[stName] = guidClass2;
			}
			else if (obj is GuidClass)
			{
				if ((obj as GuidClass).guid != guidClass2.guid)
				{
					LList<GuidClass> llist = new LList<GuidClass>(2);
					llist.Add(obj as GuidClass);
					llist.Add(guidClass2);
					this.m_htCrossreferences[stName] = llist;
				}
			}
			else if (obj is LList<GuidClass>)
			{
				LList<GuidClass> llist2 = obj as LList<GuidClass>;
				bool flag = false;
				foreach (GuidClass guidClass3 in llist2)
				{
					if (guidClass2.guid == guidClass3.guid)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					if (llist2.Count > 10)
					{
						LDictionary<GuidClass, GuidClass> ldictionary = new LDictionary<GuidClass, GuidClass>(llist2.Count);
						foreach (GuidClass guidClass4 in llist2)
						{
							ldictionary.Add(guidClass4, guidClass4);
						}
						ldictionary.Add(guidClass2, guidClass2);
						this.m_htCrossreferences[stName] = ldictionary;
					}
					else
					{
						llist2.Add(guidClass2);
					}
				}
			}
			else if (obj is LDictionary<GuidClass, GuidClass>)
			{
				LDictionary<GuidClass, GuidClass> ldictionary2 = obj as LDictionary<GuidClass, GuidClass>;
				if (!ldictionary2.ContainsKey(guidClass2))
				{
					ldictionary2[guidClass2] = guidClass2;
				}
			}
			else
			{
				Debug.Assert(false);
			}
			CaseInsensitiveDictionary<object> caseInsensitiveDictionary = null;
			if (!this.m_htIds.TryGetValue(guidClass2, ref caseInsensitiveDictionary))
			{
				caseInsensitiveDictionary = new CaseInsensitiveDictionary<object>();
				this.m_htIds.Add(guidClass2, caseInsensitiveDictionary);
			}
			if (!caseInsensitiveDictionary.ContainsKey(stName))
			{
				caseInsensitiveDictionary.Add(stName, stName);
			}
		}

		// Token: 0x06002AE3 RID: 10979 RVA: 0x00071530 File Offset: 0x00070530
		public void Remove(Guid objectGuid)
		{
			GuidClass guidClass = new GuidClass(objectGuid);
			if (!this.m_htIds.ContainsKey(guidClass))
			{
				return;
			}
			CaseInsensitiveDictionary<object> caseInsensitiveDictionary = this.m_htIds[guidClass];
			if (caseInsensitiveDictionary == null)
			{
				return;
			}
			GuidClass guidClass2 = this.m_htGuids[guidClass];
			if (guidClass2 == null)
			{
				return;
			}
			this.m_htIds.Remove(guidClass);
			this.m_htGuids.Remove(guidClass);
			foreach (string text in caseInsensitiveDictionary.Keys)
			{
				object obj = null;
				this.m_htCrossreferences.TryGetValue(text, ref obj);
				if (obj == null || obj is GuidClass)
				{
					this.m_htCrossreferences.Remove(text);
				}
				else if (obj is LList<GuidClass>)
				{
					LList<GuidClass> llist = obj as LList<GuidClass>;
					llist.Remove(guidClass2);
					if (llist.Count == 0)
					{
						this.m_htCrossreferences.Remove(text);
					}
				}
				else if (obj is LDictionary<GuidClass, GuidClass>)
				{
					LDictionary<GuidClass, GuidClass> ldictionary = obj as LDictionary<GuidClass, GuidClass>;
					ldictionary.Remove(guidClass2);
					if (ldictionary.Count == 0)
					{
						this.m_htCrossreferences.Remove(text);
					}
				}
			}
		}

		// Token: 0x06002AE4 RID: 10980 RVA: 0x00071664 File Offset: 0x00070664
		internal LList<GuidClass> Get(string stName)
		{
			LList<GuidClass> llist = new LList<GuidClass>();
			stName = stName.Trim();
			string[] array;
			if (!stName.StartsWith("%") && stName.Contains("."))
			{
				array = stName.Split(new char[]
				{
					'.'
				});
			}
			else
			{
				array = new string[]
				{
					stName
				};
			}
			foreach (string text in array)
			{
				object obj = null;
				if (this.m_htCrossreferences.TryGetValue(text, ref obj))
				{
					if (obj is GuidClass)
					{
						llist.Add(obj as GuidClass);
					}
					else if (obj is LList<GuidClass>)
					{
						llist.AddRange(obj as LList<GuidClass>);
					}
					else if (obj is LDictionary<GuidClass, GuidClass>)
					{
						foreach (GuidClass guidClass in (obj as LDictionary<GuidClass, GuidClass>).Keys)
						{
							llist.Add(guidClass);
						}
					}
				}
			}
			HashSet<GuidClass> hashSet = new HashSet<GuidClass>();
			foreach (GuidClass guidClass2 in llist)
			{
				if (guidClass2 != null)
				{
					GuidClass guidClass3 = null;
					if (this.m_htGuids.TryGetValue(guidClass2, ref guidClass3))
					{
						if (guidClass3 != null)
						{
							hashSet.Add(guidClass3);
						}
						else if (guidClass3 is GuidClassEx)
						{
							foreach (Guid g in (guidClass3 as GuidClassEx).htObjectGuids.Keys)
							{
								hashSet.Add(new GuidClass(g));
							}
						}
					}
				}
			}
			llist.AddRange(hashSet);
			return llist;
		}

		// Token: 0x06002AE5 RID: 10981 RVA: 0x00071848 File Offset: 0x00070848
		internal LList<GuidClass> Get(Regex regex)
		{
			object obj = null;
			HashSet<GuidClass> hashSet = new HashSet<GuidClass>();
			foreach (string text in this.m_htCrossreferences.Keys)
			{
				if (regex.Match(text).Success)
				{
					this.m_htCrossreferences.TryGetValue(text, ref obj);
					if (obj is GuidClass)
					{
						GuidClass item = obj as GuidClass;
						if (!hashSet.Contains(item))
						{
							hashSet.Add(obj as GuidClass);
						}
					}
					else
					{
						if (obj is LList<GuidClass>)
						{
							using (IEnumerator<GuidClass> enumerator2 = (obj as LList<GuidClass>).GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									GuidClass item2 = enumerator2.Current;
									if (!hashSet.Contains(item2))
									{
										hashSet.Add(item2);
									}
								}
								continue;
							}
						}
						if (obj is LDictionary<GuidClass, GuidClass>)
						{
							foreach (GuidClass item3 in (obj as LDictionary<GuidClass, GuidClass>).Keys)
							{
								if (!hashSet.Contains(item3))
								{
									hashSet.Add(item3);
								}
							}
						}
					}
				}
			}
			LList<GuidClass> llist = new LList<GuidClass>();
			llist.AddRange(hashSet);
			return llist;
		}

		// Token: 0x06002AE6 RID: 10982 RVA: 0x000719DC File Offset: 0x000709DC
		public IList<IDirectVariableAccess> AllAccesses()
		{
			object obj = null;
			HashSet<Tuple<string, GuidClass>> hashSet = new HashSet<Tuple<string, GuidClass>>();
			foreach (string text in this.m_htCrossreferences.Keys)
			{
				this.m_htCrossreferences.TryGetValue(text, ref obj);
				if (obj is GuidClass)
				{
					GuidClass item = obj as GuidClass;
					Tuple<string, GuidClass> item2 = new Tuple<string, GuidClass>(text, item);
					if (!hashSet.Contains(item2))
					{
						hashSet.Add(item2);
					}
				}
				else
				{
					if (obj is LList<GuidClass>)
					{
						using (IEnumerator<GuidClass> enumerator2 = (obj as LList<GuidClass>).GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								GuidClass item3 = enumerator2.Current;
								Tuple<string, GuidClass> item4 = new Tuple<string, GuidClass>(text, item3);
								if (!hashSet.Contains(item4))
								{
									hashSet.Add(item4);
								}
							}
							continue;
						}
					}
					if (obj is LDictionary<GuidClass, GuidClass>)
					{
						foreach (GuidClass item5 in (obj as LDictionary<GuidClass, GuidClass>).Keys)
						{
							Tuple<string, GuidClass> item6 = new Tuple<string, GuidClass>(text, item5);
							if (!hashSet.Contains(item6))
							{
								hashSet.Add(item6);
							}
						}
					}
				}
			}
			LList<Tuple<string, GuidClass>> llist = new LList<Tuple<string, GuidClass>>();
			llist.AddRange(hashSet);
			IList<IDirectVariableAccess> list = new LList<IDirectVariableAccess>();
			foreach (Tuple<string, GuidClass> tuple in llist)
			{
				IList<IAccessInfo> list2 = new LList<IAccessInfo>();
				foreach (PreCompileContext preCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(true, true))
				{
					if (!preCompileContext.PrecompiledLibrary)
					{
						int nProjectHandle;
						if (preCompileContext.KindOf == KindOfContext.Library)
						{
							nProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(preCompileContext.LibraryId);
						}
						else
						{
							nProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
						}
						ISignature signature = preCompileContext[tuple.Item2.guid];
						CompiledPOU compiledPOU = preCompileContext.GetCompiledPOU(tuple.Item2.guid) as CompiledPOU;
						if (signature != null)
						{
							CompilerProxy.FindCrossReferencesPrecompile(list2, signature, tuple.Item1, this.m_reftype, nProjectHandle, preCompileContext);
						}
						if (compiledPOU != null)
						{
							CompilerProxy.FindCrossReferencesPrecompile(list2, compiledPOU, tuple.Item1, this.m_reftype, nProjectHandle, preCompileContext);
						}
						if (tuple.Item2 is GuidClassEx)
						{
							foreach (Guid objectGuid in (tuple.Item2 as GuidClassEx).htObjectGuids.Keys)
							{
								signature = preCompileContext[objectGuid];
								compiledPOU = (preCompileContext.GetCompiledPOU(objectGuid) as CompiledPOU);
								if (signature != null)
								{
									CompilerProxy.FindCrossReferencesPrecompile(list2, signature, tuple.Item1, this.m_reftype, nProjectHandle, preCompileContext);
								}
								if (compiledPOU != null)
								{
									CompilerProxy.FindCrossReferencesPrecompile(list2, compiledPOU, tuple.Item1, this.m_reftype, nProjectHandle, preCompileContext);
								}
							}
						}
						_IScanner iscanner = CompilerProxy.CreateScanner();
						iscanner.Initialize(tuple.Item1);
						DirectVariable directVariable = null;
						try
						{
							IToken token;
							if (iscanner.GetNext(out token) == TokenType.DirectVariable || token.Type == TokenType.IncompleteDirectVariable)
							{
								DirectVariableLocation location;
								DirectVariableSize size;
								int[] components;
								bool flag;
								iscanner.GetDirectVariable(token, out location, out size, out components, out flag);
								if (!flag)
								{
									directVariable = new DirectVariable(location, size, components);
								}
							}
						}
						catch
						{
						}
						if (directVariable != null)
						{
							foreach (IAccessInfo accessInfo in list2)
							{
								IAccessInfo2 accinfo = (IAccessInfo2)accessInfo;
								list.Add(new PreCompCrossReferences.DirectVariableAccess(directVariable, accinfo));
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x17000BF4 RID: 3060
		public IList<IAccessInfo> this[string stName]
		{
			get
			{
				LList<GuidClass> llist = this.Get(stName);
				IList<IAccessInfo> list = new LList<IAccessInfo>();
				foreach (GuidClass guidClass in llist)
				{
					foreach (PreCompileContext preCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(true, true))
					{
						if (!preCompileContext.PrecompiledLibrary)
						{
							int nProjectHandle;
							if (preCompileContext.KindOf == KindOfContext.Library)
							{
								nProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(preCompileContext.LibraryId);
							}
							else
							{
								nProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
							}
							ISignature signature = preCompileContext[guidClass.guid];
							CompiledPOU compiledPOU = preCompileContext.GetCompiledPOU(guidClass.guid) as CompiledPOU;
							if (signature != null)
							{
								CompilerProxy.FindCrossReferencesPrecompile(list, signature, stName, this.m_reftype, nProjectHandle, preCompileContext);
							}
							if (compiledPOU != null)
							{
								CompilerProxy.FindCrossReferencesPrecompile(list, compiledPOU, stName, this.m_reftype, nProjectHandle, preCompileContext);
							}
							if (guidClass is GuidClassEx)
							{
								foreach (Guid objectGuid in (guidClass as GuidClassEx).htObjectGuids.Keys)
								{
									signature = preCompileContext[objectGuid];
									compiledPOU = (preCompileContext.GetCompiledPOU(objectGuid) as CompiledPOU);
									if (signature != null)
									{
										CompilerProxy.FindCrossReferencesPrecompile(list, signature, stName, this.m_reftype, nProjectHandle, preCompileContext);
									}
									if (compiledPOU != null)
									{
										CompilerProxy.FindCrossReferencesPrecompile(list, compiledPOU, stName, this.m_reftype, nProjectHandle, preCompileContext);
									}
								}
							}
						}
					}
				}
				return list;
			}
		}

		// Token: 0x06002AE8 RID: 10984 RVA: 0x0007200C File Offset: 0x0007100C
		public void GetCrossReferences(Regex regex, IDictionary<string, IList<IAccessInfo>> ht)
		{
			LList<GuidClass> llist = this.Get(regex);
			int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			foreach (GuidClass guidClass in llist)
			{
				foreach (PreCompileContext preCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(true, false))
				{
					ISignature signature = preCompileContext[guidClass.guid];
					CompiledPOU compiledPOU = preCompileContext.GetCompiledPOU(guidClass.guid) as CompiledPOU;
					if (signature != null)
					{
						CompilerProxy.FindCrossReferencesPrecompile(ht, signature, regex, this.m_reftype, primaryProjectHandle, preCompileContext);
					}
					if (compiledPOU != null)
					{
						CompilerProxy.FindCrossReferencesPrecompile(ht, compiledPOU, regex, this.m_reftype, primaryProjectHandle, preCompileContext);
					}
					if (guidClass is GuidClassEx)
					{
						foreach (Guid objectGuid in (guidClass as GuidClassEx).htObjectGuids.Keys)
						{
							signature = preCompileContext[objectGuid];
							compiledPOU = (preCompileContext.GetCompiledPOU(objectGuid) as CompiledPOU);
							if (signature != null)
							{
								CompilerProxy.FindCrossReferencesPrecompile(ht, signature, regex, this.m_reftype, primaryProjectHandle, preCompileContext);
							}
							if (compiledPOU != null)
							{
								CompilerProxy.FindCrossReferencesPrecompile(ht, compiledPOU, regex, this.m_reftype, primaryProjectHandle, preCompileContext);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002AE9 RID: 10985 RVA: 0x0007219C File Offset: 0x0007119C
		public void Clear()
		{
			this.m_htCrossreferences.Clear();
			this.m_htIds.Clear();
			this.m_htGuids.Clear();
		}

		// Token: 0x06002AEA RID: 10986 RVA: 0x000721C0 File Offset: 0x000711C0
		public ITextualPreCompCrossReferencesSerializable AllAccessesSerializable()
		{
			LList<ITextualPreCompCrossReferenceSerializable> llist = new LList<ITextualPreCompCrossReferenceSerializable>(this.m_htCrossreferences.Keys.Count);
			SharedGuidTable shGdTbl = new SharedGuidTable();
			foreach (KeyValuePair<string, object> keyValuePair in this.m_htCrossreferences)
			{
				LDictionary<int, ICollection<int>> ldictionary = new LDictionary<int, ICollection<int>>();
				object value = keyValuePair.Value;
				GuidClass guidClass = value as GuidClass;
				if (guidClass != null)
				{
					this.CollectAccessingObjects(ldictionary, shGdTbl, guidClass);
				}
				else
				{
					LList<GuidClass> llist2 = value as LList<GuidClass>;
					if (llist2 != null)
					{
						using (IEnumerator<GuidClass> enumerator2 = llist2.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								GuidClass guidClass2 = enumerator2.Current;
								this.CollectAccessingObjects(ldictionary, shGdTbl, guidClass2);
							}
							goto IL_F0;
						}
					}
					if (value is LDictionary<GuidClass, GuidClass>)
					{
						foreach (GuidClass guidClass3 in (value as LDictionary<GuidClass, GuidClass>).Keys)
						{
							this.CollectAccessingObjects(ldictionary, shGdTbl, guidClass3);
						}
					}
				}
				IL_F0:
				llist.Add(new TextualPreCompCrossReferenceSerializable(keyValuePair.Key, ldictionary));
			}
			return new TextualPreCompCrossReferencesSerializable(shGdTbl, llist);
		}

		// Token: 0x06002AEB RID: 10987 RVA: 0x0007231C File Offset: 0x0007131C
		internal void CollectAccessingObjects(LDictionary<int, ICollection<int>> accObjByMsg, SharedGuidTable shGdTbl, GuidClass guidClass)
		{
			GuidClass guidClass2;
			if (this.m_htGuids.TryGetValue(guidClass, ref guidClass2))
			{
				GuidClassEx guidClassEx = guidClass2 as GuidClassEx;
				Guid gd = (guidClassEx != null) ? guidClassEx.guid : Guid.Empty;
				int num = shGdTbl.GuidToIdx(gd);
				ICollection<int> collection;
				if (!accObjByMsg.TryGetValue(num, ref collection))
				{
					collection = new LHashSet<int>();
					accObjByMsg[num] = collection;
				}
				if (guidClassEx != null)
				{
					using (LDictionary<Guid, object>.KeyCollection.Enumerator enumerator = guidClassEx.htObjectGuids.Keys.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Guid gd2 = enumerator.Current;
							collection.Add(shGdTbl.GuidToIdx(gd2));
						}
						return;
					}
				}
				collection.Add(shGdTbl.GuidToIdx(guidClass2.guid));
			}
		}

		// Token: 0x0400082C RID: 2092
		private CaseInsensitiveDictionary<object> m_htCrossreferences = new CaseInsensitiveDictionary<object>();

		// Token: 0x0400082D RID: 2093
		private LDictionary<GuidClass, CaseInsensitiveDictionary<object>> m_htIds = new LDictionary<GuidClass, CaseInsensitiveDictionary<object>>();

		// Token: 0x0400082E RID: 2094
		private LDictionary<GuidClass, GuidClass> m_htGuids = new LDictionary<GuidClass, GuidClass>();

		// Token: 0x0400082F RID: 2095
		private RefType m_reftype;

		// Token: 0x020002E3 RID: 739
		public class DirectVariableAccess : IDirectVariableAccess
		{
			// Token: 0x06002CAE RID: 11438 RVA: 0x0007525A File Offset: 0x0007425A
			public DirectVariableAccess(IDirectVariable dirvar, IAccessInfo2 accinfo)
			{
				this._dirvar = dirvar;
				this._accinfo = accinfo;
			}

			// Token: 0x17000C39 RID: 3129
			// (get) Token: 0x06002CAF RID: 11439 RVA: 0x00075270 File Offset: 0x00074270
			public IDirectVariable DirectVariable
			{
				get
				{
					return this._dirvar;
				}
			}

			// Token: 0x17000C3A RID: 3130
			// (get) Token: 0x06002CB0 RID: 11440 RVA: 0x00075278 File Offset: 0x00074278
			public IAccessInfo2 AccessInfo
			{
				get
				{
					return this._accinfo;
				}
			}

			// Token: 0x04000926 RID: 2342
			private IDirectVariable _dirvar;

			// Token: 0x04000927 RID: 2343
			private IAccessInfo2 _accinfo;
		}
	}
}
