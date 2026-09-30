using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u0011;
using \u0014;
using \u0019;
using \u001A;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0082;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.OnlineChange
{
	// Token: 0x02000355 RID: 853
	internal sealed class InterfaceRelinkCode
	{
		// Token: 0x06003337 RID: 13111 RVA: 0x000C66FC File Offset: 0x000C48FC
		private InterfaceRelinkCode(_ICompileContext comconNew, _ICompileContext comconRef)
		{
			this.\u0001 = comconNew;
			this.\u0002 = comconRef;
			this.RelinkVarInstanceId = 0;
		}

		// Token: 0x06003338 RID: 13112 RVA: 0x000C6770 File Offset: 0x000C4970
		public InterfaceRelinkCode(_ICompileContext comconNew, _ICompileContext comconRef, Codegeneration codegeneration) : this(comconNew, comconRef)
		{
			this.\u0001 = codegeneration;
		}

		// Token: 0x1700086A RID: 2154
		// (get) Token: 0x06003339 RID: 13113 RVA: 0x000C6784 File Offset: 0x000C4984
		// (set) Token: 0x0600333A RID: 13114 RVA: 0x000C678C File Offset: 0x000C498C
		private int RelinkVarInstanceId { get; set; }

		// Token: 0x1700086B RID: 2155
		// (get) Token: 0x0600333B RID: 13115 RVA: 0x000C6798 File Offset: 0x000C4998
		// (set) Token: 0x0600333C RID: 13116 RVA: 0x000C67A0 File Offset: 0x000C49A0
		private int NumberOfCollissions { get; set; }

		// Token: 0x0600333D RID: 13117 RVA: 0x000C67AC File Offset: 0x000C49AC
		internal void \u0001(global::\u001A.\u0014 \u0002)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(\u0002.VariablePath);
			if (\u0002.\u0001())
			{
				return;
			}
			if (this.\u0001(\u0002))
			{
				return;
			}
			_IUserdefType iuserdefType = \u0002.VariableType as _IUserdefType;
			if (iuserdefType == null)
			{
				return;
			}
			_ISignature isignature = iuserdefType.GetSignature(\u0002.ScopeRef) as _ISignature;
			if (isignature == null)
			{
				return;
			}
			this.\u0001(\u0002, isignature);
		}

		// Token: 0x0600333E RID: 13118 RVA: 0x000C6814 File Offset: 0x000C4A14
		private void \u0001(global::\u001A.\u0014 \u0002, _ISignature \u0003)
		{
			if (!\u0002.\u0002() && !\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_NO_RELINK_CODE))
			{
				LDictionary<int, int> u = new LDictionary<int, int>();
				Helper.\u0001(\u0002.ScopeRef, \u0003, u, true);
				this.\u0001(\u0002, \u0003, u);
			}
			this.\u0002(\u0002, \u0003);
		}

		// Token: 0x0600333F RID: 13119 RVA: 0x000C685C File Offset: 0x000C4A5C
		private void \u0002(global::\u001A.\u0014 \u0002, _ISignature \u0003)
		{
			while (\u0003 != null)
			{
				foreach (_IVariable ivariable in \u0003.AllVariables)
				{
					if (!ivariable.IsProperty && \u0084.\u0004.\u0001(ivariable.CompiledType) is _IUserdefType)
					{
						global::\u001A.\u0014 u = \u0002.\u0001(\u0003, ivariable);
						this.\u0001(u);
					}
				}
				\u0003 = (\u0002.ScopeRef[\u0003.BaseSignatureId] as _ISignature);
			}
		}

		// Token: 0x06003340 RID: 13120 RVA: 0x000C68EC File Offset: 0x000C4AEC
		private void \u0001(global::\u001A.\u0014 \u0002, _ISignature \u0003, LDictionary<int, int> \u0004)
		{
			using (LDictionary<int, int>.KeyCollection.Enumerator enumerator = \u0004.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					InterfaceRelinkCode.\u0001 u = new InterfaceRelinkCode.\u0001();
					u.\u0001 = enumerator.Current;
					ISignature signature = \u0002.Scope[u.\u0001];
					if (signature != null && !signature.HasAttribute(CompileAttributes.ATTRIBUTE_NO_RELINK_CODE))
					{
						LList<_IVariable> llist = new LList<_IVariable>();
						LList<_ISignature> llist2 = new LList<_ISignature>();
						\u0080.\u0005.\u0001 u2 = new \u0080.\u0005.\u0001
						{
							\u0001 = true,
							\u0002 = false,
							\u0003 = false,
							\u0004 = false,
							\u0005 = true
						};
						LList<string> u3 = InstancePathService.\u0001(this.\u0001, signature as _ISignature, Array.Empty<int>(), this.\u0001, llist, llist2, u2);
						IInterfaceInfo[] u4 = \u0003.InterfaceHierarchy.Interfaces.Where(new Func<IInterfaceInfo, bool>(u.\u0001)).ToArray<IInterfaceInfo>();
						this.\u0001(\u0002, \u0003, u3, llist, llist2, u4);
					}
				}
			}
		}

		// Token: 0x06003341 RID: 13121 RVA: 0x000C6A00 File Offset: 0x000C4C00
		private void \u0001(global::\u001A.\u0014 \u0002, _ISignature \u0003, LList<string> \u0004, LList<_IVariable> \u0005, LList<_ISignature> \u0006, IInterfaceInfo[] \u0007)
		{
			for (int i = 0; i < \u0004.Count; i++)
			{
				if (APEnvironmentFacade.Instance.LanguageModelMgr.Progress.Callback.Aborting)
				{
					throw new CancelledByUserException();
				}
				if (!\u0005[i].HasAttribute(CompileAttributes.ATTRIBUTE_NO_RELINK_CODE) && !\u0005[i].HasFlag(VarFlag.OnlChangeInit))
				{
					string text = \u0004[i];
					if (!this.\u0001.ContainsKey(text))
					{
						this.\u0001.Add(text, new VariableInfo(\u0005[i].Id, \u0006[i].Id, VarFlag.None));
					}
					foreach (IInterfaceInfo ii in \u0007)
					{
						string uniqueInterfaceVariableName = \u0003.InterfaceHierarchy.GetUniqueInterfaceVariableName(ii);
						string text2 = \u0002.VariablePath + "." + uniqueInterfaceVariableName;
						if (!this.\u0001.ContainsKey(text2))
						{
							this.\u0001(text2, \u0002, uniqueInterfaceVariableName);
						}
					}
					this.\u0002(\u0002);
				}
			}
		}

		// Token: 0x06003342 RID: 13122 RVA: 0x000C6B14 File Offset: 0x000C4D14
		private void \u0002(global::\u001A.\u0014 \u0002)
		{
			if (!this.\u0002.ContainsKey(\u0002.VariablePath))
			{
				this.\u0002.Add(\u0002.VariablePath, new VariableInfo(\u0002.VarId, \u0002.SignId, VarFlag.None));
			}
		}

		// Token: 0x06003343 RID: 13123 RVA: 0x000C6B50 File Offset: 0x000C4D50
		private bool \u0001(global::\u001A.\u0014 \u0002)
		{
			if (\u0002.VariableType.Class != TypeClass.Array)
			{
				return false;
			}
			_IUserdefType iuserdefType = \u0084.\u0004.\u0001(\u0002.VariableType) as _IUserdefType;
			if (iuserdefType == null)
			{
				return true;
			}
			if (!InterfaceRelinkCode.\u0001(iuserdefType, \u0002.ScopeRef))
			{
				return true;
			}
			bool flag;
			string[] array = this.\u0002.SubElements(string.Empty, \u0002.VariablePath, out flag);
			if (!flag || array == null)
			{
				return true;
			}
			string[] array2 = this.\u0001.SubElements(string.Empty, \u0002.VariablePath, out flag) ?? Array.Empty<string>();
			if (!flag)
			{
				return true;
			}
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			for (int i = 0; i < array2.Length; i++)
			{
				dictionary.Add(array2[i], i);
			}
			for (int j = 0; j < array.Length; j++)
			{
				string text = array[j];
				int value;
				int? u = dictionary.TryGetValue(text, out value) ? new int?(value) : null;
				global::\u001A.\u0014 u2 = new global::\u001A.\u0014(\u0002.Scope, \u0002.ScopeRef)
				{
					VariablePath = text,
					MovedVariableRef = \u0002.MovedVariableRef,
					VariableType = iuserdefType,
					ContainingSignatureRef = \u0002.ContainingSignatureRef,
					ParentVar = \u0002.ParentVar,
					ArrayIndexOld = j,
					ArrayIndexNew = u
				};
				this.\u0001(u2);
			}
			return true;
		}

		// Token: 0x06003344 RID: 13124 RVA: 0x000C6CA4 File Offset: 0x000C4EA4
		private static bool \u0001(_IUserdefType \u0002, IScope5 \u0003)
		{
			LDictionary<int, int> ldictionary = new LDictionary<int, int>();
			_ISignature isignature = \u0002.GetSignature(\u0003) as _ISignature;
			if (isignature == null)
			{
				return false;
			}
			Helper.\u0001(\u0003, isignature, ldictionary, true);
			if (ldictionary.Keys.Any<int>())
			{
				return true;
			}
			foreach (_IVariable ivariable in isignature.AllVariables)
			{
				if (!ivariable.IsProperty)
				{
					_IUserdefType iuserdefType = \u0084.\u0004.\u0001(ivariable.CompiledType) as _IUserdefType;
					if (iuserdefType != null && InterfaceRelinkCode.\u0001(iuserdefType, \u0003))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06003345 RID: 13125 RVA: 0x000C6D4C File Offset: 0x000C4F4C
		internal string \u0001()
		{
			if (!this.\u0001())
			{
				return "";
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in this.\u0001.Keys)
			{
				lstringBuilder.AppendFormat("__RELINK_ITF(ADR({0}.__Interface));\n", new object[]
				{
					text
				});
				lstringBuilder.AppendLine();
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x06003346 RID: 13126 RVA: 0x000C6DD4 File Offset: 0x000C4FD4
		private bool \u0001()
		{
			if (this.\u0001.Count == 0)
			{
				return false;
			}
			this.\u0002();
			if (this.\u0001 == null || this.\u0001.Length == 0)
			{
				return false;
			}
			this.\u0001();
			_ISignature isignature = this.\u0001(ParserHelper.\u0001("\r\nTYPE __INTERFACE_MOVE_INFO :\r\nSTRUCT\r\n\tusiAreaOld: UINT;\r\n\tudiOffsetOld: __UXINT;\r\n\r\n\tusiAreaNew : UINT;\r\n\tudiOffsetNew : __UXINT;\r\n\tpNext : POINTER TO __INTERFACE_MOVE_INFO;\r\nEND_STRUCT\r\nEND_TYPE\r\n", true));
			Locator.\u0001(isignature, null, this.\u0001, this.\u0002);
			string u = this.\u0002();
			_ISignature u2 = this.\u0001(ParserHelper.\u0001("__RELINK_AREA_INFO", u, true));
			this.\u0001(u2);
			string u3 = this.\u0003();
			_ISignature isignature2 = this.\u0001(ParserHelper.\u0001("__RELINK_HASH_TABLE", u3, true));
			Locator.\u0001(isignature2, null, this.\u0001, this.\u0002);
			\u0082.\u000F u000F = new \u0082.\u000F(this.\u0001, this.\u0001.CreateGlobalIScope() as IScope5, this.\u0001.Codegenerator);
			foreach (IVariable variable in isignature2.AllForInitCode)
			{
				u000F.\u0001(variable as _IVariable, isignature, isignature2, true);
			}
			if (APEnvironmentFacade.Instance.LanguageModelMgr.Progress.Callback.Aborting)
			{
				throw new CancelledByUserException();
			}
			_ISignature isignature3 = this.\u0001(ParserHelper.\u0001(string.Format("\r\nFUNCTION __RELINK_ITF : BOOL\r\nVAR_INPUT\r\n\t__itf : POINTER TO POINTER TO BYTE;\r\nEND_VAR\r\nVAR\r\n\t__uiArea : UDINT;\r\n\t__udiOffset : __UXINT;\r\n\t__key : __UXINT;\r\n    __areaAdr : __XWORD;\r\n\t__pCanditateInfo : POINTER TO __INTERFACE_MOVE_INFO;\r\n\t__pMoveInfo : POINTER TO __INTERFACE_MOVE_INFO;\r\n\t__uiCnt : UDINT;\r\nEND_VAR\r\nVAR CONSTANT\r\n    {{attribute 'const_replaced'}}\r\n\t__R : UINT := {0};\r\n    {{attribute 'const_replaced'}}\r\n\t__M : UINT := {1};\r\nEND_VAR\r\n", 31, this.\u0004), true));
			Locator.\u0001(this.\u0001, isignature3, null);
			Locator.\u0001(isignature3, null, this.\u0001, this.\u0002);
			CodeInitGenerator.\u0001(this.\u0001, isignature3, this.\u0001[IdentifierConstants.GlobalImplicitFunctionPointers], null);
			this.\u0001(isignature3, "\r\nIF __itf^ = 0 THEN\r\n\tRETURN;\r\nEND_IF\r\n\r\n// Set all locals to safe initial values, not done by the compiler for this super special function\r\n__areaAdr := 0;\r\n__pMoveInfo := 0;\r\n\r\n// Find <area,offset> for given address\r\nFOR __uiCnt := 0 TO (__areaCountCpy - 1) BY 1 DO\r\n\tIF __itf^ >= __areaStartAdrCpy[__uiCnt] AND_THEN __itf^ <= (__areaStartAdrCpy[__uiCnt] + __areaSizesCpy[__uiCnt]) THEN\r\n\t\t__areaAdr := __areaStartAdrCpy[__uiCnt];\r\n\t\t__uiArea := __uiCnt;\r\n\t\t__udiOffset := __itf^ - __areaAdr;\r\n\tEND_IF\r\nEND_FOR;\r\nIF __areaAdr = 0 THEN\r\n\tRETURN;\r\nEND_IF\r\n\r\n__key := (((__R * __uiArea) MOD __M) + __udiOffset) MOD __M;\r\n__pCanditateInfo := __itfLocMapping[__key];\r\n\r\n// Handle collisions\r\nWHILE __pCanditateInfo <> 0 DO\r\n\tIF __pCanditateInfo^.usiAreaOld = __uiArea AND_THEN __pCanditateInfo^.udiOffsetOld = __udiOffset THEN\r\n\t\t__pMoveInfo := __pCanditateInfo;\r\n\t\tEXIT;\r\n\tEND_IF\r\n\t__pCanditateInfo := __pCanditateInfo^.pNext;\r\nEND_WHILE;\r\n\r\n// no entry found in hash table, no relink needs to be done, return\r\nIF __pMoveInfo = 0 THEN\r\n\tRETURN;\r\nEND_IF\r\n\r\n// found a matching entry, adapt the pointer\r\n__areaAdr := __areaStartAdrNew[__pMoveInfo^.usiAreaNew];\r\n__itf^ := __areaAdr + __pMoveInfo^.udiOffsetNew;\r\n");
			return true;
		}

		// Token: 0x06003347 RID: 13127 RVA: 0x000C6F98 File Offset: 0x000C5198
		private void \u0001()
		{
			this.\u0004 = InterfaceRelinkCode.\u0001.First<int>();
			int count = this.\u0001.Keys.Count;
			int num = 0;
			while (num < InterfaceRelinkCode.\u0001.Length && count >= InterfaceRelinkCode.\u0001[num])
			{
				this.\u0004 = InterfaceRelinkCode.\u0001[num];
				num++;
			}
			foreach (InterfaceRelinkCode.HashTableEntry hashTableEntry in this.\u0001.Values)
			{
				hashTableEntry.\u0001(31, this.\u0004);
			}
			IEnumerable<InterfaceRelinkCode.HashTableEntry> enumerable = this.\u0001.Values.OrderBy(new Func<InterfaceRelinkCode.HashTableEntry, int>(InterfaceRelinkCode.<>c.<>9.\u0001));
			InterfaceRelinkCode.HashTableEntry hashTableEntry2 = null;
			foreach (InterfaceRelinkCode.HashTableEntry hashTableEntry3 in enumerable)
			{
				if (hashTableEntry2 != null && hashTableEntry2.HashKey == hashTableEntry3.HashKey)
				{
					int num2 = this.NumberOfCollissions;
					this.NumberOfCollissions = num2 + 1;
					hashTableEntry3.Collision = true;
					hashTableEntry2.NextEntry = hashTableEntry3;
				}
				hashTableEntry2 = hashTableEntry3;
			}
		}

		// Token: 0x06003348 RID: 13128 RVA: 0x000C70DC File Offset: 0x000C52DC
		private void \u0001(_ISignature \u0002)
		{
			foreach (_IArea iarea in this.\u0001.OfType<_IArea>())
			{
				_IVariable ivariable = \u0002[string.Format("__areaCpy{0}StartDummy", iarea.Index)] as _IVariable;
				if (ivariable != null)
				{
					ivariable.DataLocation = global::\u0019.\u0003.\u0001((ushort)iarea.Index, 0);
					ivariable.SetFlag(VarFlag.Absolut | VarFlag.NoInit, true);
				}
			}
			foreach (_IArea iarea2 in this.\u0002.OfType<_IArea>())
			{
				_IVariable ivariable2 = \u0002[string.Format("__areaNew{0}StartDummy", iarea2.Index)] as _IVariable;
				if (ivariable2 != null)
				{
					ivariable2.DataLocation = global::\u0019.\u0003.\u0001((ushort)iarea2.Index, 0);
					ivariable2.SetFlag(VarFlag.Absolut | VarFlag.NoInit, true);
				}
			}
		}

		// Token: 0x06003349 RID: 13129 RVA: 0x000C71F0 File Offset: 0x000C53F0
		private void \u0002()
		{
			this.\u0001 = MemoryCompiler.\u0001(this.\u0002.DataManager);
			this.\u0002 = this.\u0001.DataManager.Areas.Where(new Func<IArea, bool>(InterfaceRelinkCode.<>c.<>9.\u0001)).ToArray<IArea>();
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				this.\u0001[this.\u0001[i].Index] = i;
			}
			for (int j = 0; j < this.\u0002.Length; j++)
			{
				this.\u0002[this.\u0002[j].Index] = j;
			}
		}

		// Token: 0x0600334A RID: 13130 RVA: 0x000C72AC File Offset: 0x000C54AC
		private string \u0002()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (_IArea iarea in this.\u0001.OfType<_IArea>())
			{
				lstringBuilder.AppendFormat("{{attribute 'no_init'}}\n __areaCpy{0}StartDummy : BYTE;", new object[]
				{
					iarea.Index
				});
				lstringBuilder.AppendLine();
			}
			LStringBuilder lstringBuilder2 = new LStringBuilder();
			foreach (_IArea iarea2 in this.\u0002.OfType<_IArea>())
			{
				lstringBuilder2.AppendFormat("{{attribute 'no_init'}}\n __areaNew{0}StartDummy : BYTE;", new object[]
				{
					iarea2.Index
				});
				lstringBuilder2.AppendLine();
			}
			LStringBuilder lstringBuilder3 = new LStringBuilder();
			lstringBuilder3.AppendFormat("\r\nVAR_GLOBAL\r\n\t{0}\r\n\t{1}\r\nEND_VAR\r\n", new object[]
			{
				lstringBuilder.ToString(),
				lstringBuilder2.ToString()
			});
			return lstringBuilder3.ToString();
		}

		// Token: 0x0600334B RID: 13131 RVA: 0x000C73C0 File Offset: 0x000C55C0
		private string \u0003()
		{
			bool flag = true;
			LStringBuilder lstringBuilder = new LStringBuilder();
			LStringBuilder lstringBuilder2 = new LStringBuilder();
			foreach (_IArea iarea in this.\u0001.OfType<_IArea>())
			{
				if (!flag)
				{
					lstringBuilder.Append(" ,");
				}
				lstringBuilder.AppendFormat("ADR(__areaCpy{0}StartDummy)", new object[]
				{
					iarea.Index
				});
				if (!flag)
				{
					lstringBuilder2.Append(" ,");
				}
				lstringBuilder2.AppendFormat("{0}", new object[]
				{
					iarea.Size
				});
				flag = false;
			}
			LStringBuilder lstringBuilder3 = new LStringBuilder();
			LStringBuilder lstringBuilder4 = new LStringBuilder();
			flag = true;
			foreach (_IArea iarea2 in this.\u0002.OfType<_IArea>())
			{
				if (!flag)
				{
					lstringBuilder3.Append(" ,");
				}
				lstringBuilder3.AppendFormat("ADR(__areaNew{0}StartDummy)", new object[]
				{
					iarea2.Index
				});
				if (!flag)
				{
					lstringBuilder4.Append(" ,");
				}
				lstringBuilder4.AppendFormat("{0}", new object[]
				{
					iarea2.Size
				});
				flag = false;
			}
			lstringBuilder3.AppendFormat(", 0", Array.Empty<object>());
			lstringBuilder4.AppendFormat(", 0", Array.Empty<object>());
			if (APEnvironmentFacade.Instance.LanguageModelMgr.Progress.Callback.Aborting)
			{
				throw new CancelledByUserException();
			}
			LStringBuilder lstringBuilder5 = new LStringBuilder();
			foreach (InterfaceRelinkCode.HashTableEntry hashTableEntry in this.\u0001.Values)
			{
				lstringBuilder5.Append(hashTableEntry.\u0001());
				lstringBuilder5.AppendLine();
			}
			if (APEnvironmentFacade.Instance.LanguageModelMgr.Progress.Callback.Aborting)
			{
				throw new CancelledByUserException();
			}
			LStringBuilder lstringBuilder6 = this.\u0001();
			LStringBuilder lstringBuilder7 = new LStringBuilder();
			lstringBuilder7.AppendFormat("\r\nVAR_GLOBAL\r\n\t// here come all INTERFACE_MOVE_INFO instances, which are pointed to by the above table\r\n\t{0}\r\n\t{{attribute 'no_init'}}\r\n\t__itfLocMapping : ARRAY[0..{8}] OF POINTER TO __INTERFACE_MOVE_INFO := [{1}];\r\n\t// implict vars to derive area start addresses from\r\n\t{{attribute 'no_init'}}\r\n\t__areaStartAdrCpy : ARRAY[0..({2}-1)] OF POINTER TO BYTE := [{3}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaSizesCpy : ARRAY[0..({2}-1)] OF __UXINT := [{4}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaCountCpy : UDINT := {2};\r\n\t{{attribute 'no_init'}}\r\n\t__areaStartAdrNew : ARRAY[0..({5}-1)] OF POINTER TO BYTE := [{6}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaSizesNew : ARRAY[0..({5}-1)] OF __UXINT := [{7}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaCountNew : UDINT := {5};\r\nEND_VAR\r\n", new object[]
			{
				lstringBuilder5.ToString(),
				lstringBuilder6.ToString(),
				this.\u0001.Length,
				lstringBuilder.ToString(),
				lstringBuilder2.ToString(),
				this.\u0002.Length + 1,
				lstringBuilder3.ToString(),
				lstringBuilder4.ToString(),
				this.\u0004 - 1
			});
			return lstringBuilder7.ToString();
		}

		// Token: 0x0600334C RID: 13132 RVA: 0x000C7690 File Offset: 0x000C5890
		private LStringBuilder \u0001()
		{
			bool flag = true;
			LStringBuilder lstringBuilder = new LStringBuilder();
			IEnumerator<InterfaceRelinkCode.HashTableEntry> enumerator = this.\u0001.Values.OrderBy(new Func<InterfaceRelinkCode.HashTableEntry, int>(InterfaceRelinkCode.<>c.<>9.\u0002)).GetEnumerator();
			InterfaceRelinkCode.HashTableEntry hashTableEntry = enumerator.MoveNext() ? enumerator.Current : null;
			for (int i = 0; i < this.\u0004; i++)
			{
				if (!flag)
				{
					lstringBuilder.Append(" ,");
				}
				else
				{
					flag = false;
				}
				if (hashTableEntry != null && hashTableEntry.HashKey == i)
				{
					InterfaceRelinkCode.\u0001(lstringBuilder, enumerator, ref hashTableEntry);
				}
				else
				{
					lstringBuilder.Append("0");
				}
			}
			return lstringBuilder;
		}

		// Token: 0x0600334D RID: 13133 RVA: 0x000C773C File Offset: 0x000C593C
		private static void \u0001(LStringBuilder \u0002, IEnumerator<InterfaceRelinkCode.HashTableEntry> \u0003, ref InterfaceRelinkCode.HashTableEntry \u0004)
		{
			\u0002.AppendFormat("ADR({0})", new object[]
			{
				\u0004.VariableName
			});
			do
			{
				\u0004 = (\u0003.MoveNext() ? \u0003.Current : null);
			}
			while (\u0004 != null && \u0004.Collision);
		}

		// Token: 0x0600334E RID: 13134 RVA: 0x000C7788 File Offset: 0x000C5988
		private void \u0001(string \u0002, global::\u001A.\u0014 \u0003, string \u0004)
		{
			IDataLocation dataLocation;
			IDataLocation dataLocation2;
			\u0003.\u0001(out dataLocation, out dataLocation2);
			_IVariable ivariable = \u0003.MovedVariableRef as _IVariable;
			if (ivariable == null)
			{
				return;
			}
			_IUserdefType iuserdefType = \u0084.\u0004.\u0001(ivariable.CompiledType) as _IUserdefType;
			if (iuserdefType == null)
			{
				return;
			}
			_ISignature sign = this.\u0002[iuserdefType.SignatureId];
			_IScope iscope = ((_IScope)\u0003.ScopeRef).CreateLocalScope(sign) as _IScope;
			if (iscope == null)
			{
				return;
			}
			ISignature signature;
			_IVariable ivariable2 = iscope.FindVariableLocal(\u0004, out signature) as _IVariable;
			if (ivariable2 == null)
			{
				return;
			}
			_IDataLocation oldDataLoc = (_IDataLocation)global::\u0019.\u0003.\u0001(dataLocation.Area, dataLocation.Offset + ivariable2.DataLocation.Offset);
			_IDataLocation newDataLoc = null;
			if (dataLocation2 != null)
			{
				_ISignature sign2 = this.\u0001[((_IUserdefType)\u0003.VariableType).SignatureId];
				_IScope iscope2 = ((_IScope)\u0003.Scope).CreateLocalScope(sign2) as _IScope;
				_IVariable ivariable3 = ((iscope2 != null) ? iscope2.FindVariableLocal(\u0004, out signature) : null) as _IVariable;
				if (ivariable3 != null)
				{
					newDataLoc = (_IDataLocation)global::\u0019.\u0003.\u0001(dataLocation2.Area, dataLocation2.Offset + ivariable3.DataLocation.Offset);
				}
			}
			LDictionary<string, InterfaceRelinkCode.HashTableEntry> u = this.\u0001;
			string format = "__rimi__{0}";
			int num = this.RelinkVarInstanceId;
			this.RelinkVarInstanceId = num + 1;
			u[\u0002] = new InterfaceRelinkCode.HashTableEntry(string.Format(format, num), oldDataLoc, newDataLoc, this.\u0001, this.\u0002);
		}

		// Token: 0x0600334F RID: 13135 RVA: 0x000C78F0 File Offset: 0x000C5AF0
		private _ISignature \u0001(_ISignature \u0002)
		{
			\u0002 = \u0002.CreateCompiledSignature(null, this.\u0001.HasByteSupport());
			\u0002.SetFlag(SignatureFlag.Generated, true);
			\u0002.SetFlag(SignatureFlag.ToRemoveAfterDownload, true);
			this.\u0001.AddSignature(\u0002, null, this.\u0002, true);
			IScope5 u = global::\u0007.\u0005.\u0001(this.\u0001, \u0002.Id);
			global::\u0014.\u0013.\u0002(\u0002, u, this.\u0001);
			global::\u0014.\u0013.\u0001(\u0002, u, this.\u0001);
			return \u0002;
		}

		// Token: 0x06003350 RID: 13136 RVA: 0x000C7970 File Offset: 0x000C5B70
		private void \u0001(_ISignature \u0002, string \u0003)
		{
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(\u0002.Name);
			icompiledPOU.SignatureId = \u0002.Id;
			_IStatement istatement = new global::\u0011.\u0006(\u0003, true).\u0001();
			IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, \u0002.Id);
			istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, this.\u0001, false, icompiledPOU)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			istatement.Accept(new TypeCheckerVisitor(scope, this.\u0001, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			int num = 0;
			foreach (IMessage message in errorVisitor.MessageList)
			{
				if (message.Severity == Severity.Error || message.Severity == Severity.FatalError)
				{
					num++;
				}
			}
			Debug.\u0001(num == 0);
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			isequenceStatement.Add(istatement);
			icompiledPOU.SetParseTree(isequenceStatement);
			this.\u0001.\u0001(icompiledPOU, null, \u0002);
			ushort u = 255;
			int u2 = -1;
			if (!MemoryCompiler.\u0003(this.\u0001.DataManager, ref u, ref u2, this.\u0001.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, this.\u0001.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU.Name,
					icompiledPOU.CompiledCode.CodeSize
				});
				_ICompilerMessage message2 = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(messageCategory, message2);
			}
			else
			{
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
			}
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.Generated, true);
			this.\u0001.AddCompiledPOU(icompiledPOU, \u0002, true, null);
		}

		// Token: 0x06003351 RID: 13137 RVA: 0x000C7B88 File Offset: 0x000C5D88
		internal IDictionary<string, IVariableInfo> \u0001()
		{
			LDictionary<string, IVariableInfo> ldictionary = new LDictionary<string, IVariableInfo>();
			foreach (KeyValuePair<string, IVariableInfo> keyValuePair in this.\u0002)
			{
				ldictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
			return ldictionary;
		}

		// Token: 0x06003352 RID: 13138 RVA: 0x000C7BF0 File Offset: 0x000C5DF0
		internal IDictionary<string, IVariableInfo> \u0002()
		{
			LDictionary<string, IVariableInfo> ldictionary = new LDictionary<string, IVariableInfo>();
			foreach (KeyValuePair<string, IVariableInfo> keyValuePair in this.\u0001)
			{
				ldictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
			return ldictionary;
		}

		// Token: 0x040009B5 RID: 2485
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040009B6 RID: 2486
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x040009B7 RID: 2487
		private const string \u0001 = "\r\nTYPE __INTERFACE_MOVE_INFO :\r\nSTRUCT\r\n\tusiAreaOld: UINT;\r\n\tudiOffsetOld: __UXINT;\r\n\r\n\tusiAreaNew : UINT;\r\n\tudiOffsetNew : __UXINT;\r\n\tpNext : POINTER TO __INTERFACE_MOVE_INFO;\r\nEND_STRUCT\r\nEND_TYPE\r\n";

		// Token: 0x040009B8 RID: 2488
		private const string \u0002 = "\r\nVAR_GLOBAL\r\n\t{0}\r\n\t{1}\r\nEND_VAR\r\n";

		// Token: 0x040009B9 RID: 2489
		private const string \u0003 = "\r\nVAR_GLOBAL\r\n\t// here come all INTERFACE_MOVE_INFO instances, which are pointed to by the above table\r\n\t{0}\r\n\t{{attribute 'no_init'}}\r\n\t__itfLocMapping : ARRAY[0..{8}] OF POINTER TO __INTERFACE_MOVE_INFO := [{1}];\r\n\t// implict vars to derive area start addresses from\r\n\t{{attribute 'no_init'}}\r\n\t__areaStartAdrCpy : ARRAY[0..({2}-1)] OF POINTER TO BYTE := [{3}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaSizesCpy : ARRAY[0..({2}-1)] OF __UXINT := [{4}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaCountCpy : UDINT := {2};\r\n\t{{attribute 'no_init'}}\r\n\t__areaStartAdrNew : ARRAY[0..({5}-1)] OF POINTER TO BYTE := [{6}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaSizesNew : ARRAY[0..({5}-1)] OF __UXINT := [{7}];\r\n\t{{attribute 'no_init'}}\r\n\t__areaCountNew : UDINT := {5};\r\nEND_VAR\r\n";

		// Token: 0x040009BA RID: 2490
		private const string \u0004 = "\r\nFUNCTION __RELINK_ITF : BOOL\r\nVAR_INPUT\r\n\t__itf : POINTER TO POINTER TO BYTE;\r\nEND_VAR\r\nVAR\r\n\t__uiArea : UDINT;\r\n\t__udiOffset : __UXINT;\r\n\t__key : __UXINT;\r\n    __areaAdr : __XWORD;\r\n\t__pCanditateInfo : POINTER TO __INTERFACE_MOVE_INFO;\r\n\t__pMoveInfo : POINTER TO __INTERFACE_MOVE_INFO;\r\n\t__uiCnt : UDINT;\r\nEND_VAR\r\nVAR CONSTANT\r\n    {{attribute 'const_replaced'}}\r\n\t__R : UINT := {0};\r\n    {{attribute 'const_replaced'}}\r\n\t__M : UINT := {1};\r\nEND_VAR\r\n";

		// Token: 0x040009BB RID: 2491
		private const string \u0005 = "\r\nIF __itf^ = 0 THEN\r\n\tRETURN;\r\nEND_IF\r\n\r\n// Set all locals to safe initial values, not done by the compiler for this super special function\r\n__areaAdr := 0;\r\n__pMoveInfo := 0;\r\n\r\n// Find <area,offset> for given address\r\nFOR __uiCnt := 0 TO (__areaCountCpy - 1) BY 1 DO\r\n\tIF __itf^ >= __areaStartAdrCpy[__uiCnt] AND_THEN __itf^ <= (__areaStartAdrCpy[__uiCnt] + __areaSizesCpy[__uiCnt]) THEN\r\n\t\t__areaAdr := __areaStartAdrCpy[__uiCnt];\r\n\t\t__uiArea := __uiCnt;\r\n\t\t__udiOffset := __itf^ - __areaAdr;\r\n\tEND_IF\r\nEND_FOR;\r\nIF __areaAdr = 0 THEN\r\n\tRETURN;\r\nEND_IF\r\n\r\n__key := (((__R * __uiArea) MOD __M) + __udiOffset) MOD __M;\r\n__pCanditateInfo := __itfLocMapping[__key];\r\n\r\n// Handle collisions\r\nWHILE __pCanditateInfo <> 0 DO\r\n\tIF __pCanditateInfo^.usiAreaOld = __uiArea AND_THEN __pCanditateInfo^.udiOffsetOld = __udiOffset THEN\r\n\t\t__pMoveInfo := __pCanditateInfo;\r\n\t\tEXIT;\r\n\tEND_IF\r\n\t__pCanditateInfo := __pCanditateInfo^.pNext;\r\nEND_WHILE;\r\n\r\n// no entry found in hash table, no relink needs to be done, return\r\nIF __pMoveInfo = 0 THEN\r\n\tRETURN;\r\nEND_IF\r\n\r\n// found a matching entry, adapt the pointer\r\n__areaAdr := __areaStartAdrNew[__pMoveInfo^.usiAreaNew];\r\n__itf^ := __areaAdr + __pMoveInfo^.udiOffsetNew;\r\n";

		// Token: 0x040009BC RID: 2492
		private readonly _ICompileContext \u0001;

		// Token: 0x040009BD RID: 2493
		private readonly _ICompileContext \u0002;

		// Token: 0x040009BE RID: 2494
		private readonly Codegeneration \u0001;

		// Token: 0x040009BF RID: 2495
		private IArea[] \u0001;

		// Token: 0x040009C0 RID: 2496
		private IArea[] \u0002;

		// Token: 0x040009C1 RID: 2497
		private readonly LDictionary<string, InterfaceRelinkCode.HashTableEntry> \u0001 = new LDictionary<string, InterfaceRelinkCode.HashTableEntry>();

		// Token: 0x040009C2 RID: 2498
		private readonly LDictionary<string, IVariableInfo> \u0001 = new LDictionary<string, IVariableInfo>();

		// Token: 0x040009C3 RID: 2499
		private readonly LDictionary<string, IVariableInfo> \u0002 = new LDictionary<string, IVariableInfo>();

		// Token: 0x040009C4 RID: 2500
		private readonly \u0080.\u0005.\u0002 \u0001 = \u0080.\u0005.\u0001();

		// Token: 0x040009C5 RID: 2501
		private static readonly int[] \u0001 = new int[]
		{
			249,
			509,
			1009,
			5003,
			10007,
			20011
		};

		// Token: 0x040009C6 RID: 2502
		private const int \u0003 = 31;

		// Token: 0x040009C7 RID: 2503
		private int \u0004 = 2;

		// Token: 0x040009C8 RID: 2504
		private readonly LDictionary<int, int> \u0001 = new LDictionary<int, int>();

		// Token: 0x040009C9 RID: 2505
		private readonly LDictionary<int, int> \u0002 = new LDictionary<int, int>();

		// Token: 0x02000356 RID: 854
		private sealed class HashTableEntry
		{
			// Token: 0x06003354 RID: 13140 RVA: 0x000C7C70 File Offset: 0x000C5E70
			public HashTableEntry(string stVarName, IDataLocation oldDataLoc, IDataLocation newDataLoc, IDictionary<int, int> _areaCpyIdxToNormalizedIdx, IDictionary<int, int> _areaNewIdxToNormalizedIdx)
			{
				this.\u0002 = oldDataLoc;
				this.\u0001 = newDataLoc;
				this.\u0001 = stVarName;
				this.\u0001 = _areaCpyIdxToNormalizedIdx;
				this.\u0002 = _areaNewIdxToNormalizedIdx;
			}

			// Token: 0x06003355 RID: 13141 RVA: 0x000C7CA0 File Offset: 0x000C5EA0
			internal string \u0001()
			{
				string format = "{0} : __INTERFACE_MOVE_INFO := (usiAreaOld := {1}, udiOffsetOld := {2}, usiAreaNew := {3}, udiOffsetNew := {4}, pNext := {5});";
				object[] array = new object[6];
				array[0] = this.\u0001;
				array[1] = this.\u0001[(int)this.\u0002.Area];
				array[2] = this.\u0002.Offset;
				array[3] = ((this.\u0001 != null) ? this.\u0002[(int)this.\u0001.Area] : this.\u0002.Count);
				int num = 4;
				IDataLocation u = this.\u0001;
				array[num] = ((u != null) ? u.Offset : 0);
				array[5] = ((this.NextEntry != null) ? string.Format("ADR({0})", this.NextEntry.VariableName) : "0");
				return string.Format(format, array);
			}

			// Token: 0x06003356 RID: 13142 RVA: 0x000C7D6C File Offset: 0x000C5F6C
			internal void \u0001(int \u0002, int \u0003)
			{
				this.HashKey = (\u0002 * this.\u0001[(int)this.\u0002.Area] % \u0003 + this.\u0002.Offset) % \u0003;
			}

			// Token: 0x1700086C RID: 2156
			// (get) Token: 0x06003357 RID: 13143 RVA: 0x000C7D9C File Offset: 0x000C5F9C
			internal string VariableName
			{
				get
				{
					return this.\u0001;
				}
			}

			// Token: 0x1700086D RID: 2157
			// (get) Token: 0x06003358 RID: 13144 RVA: 0x000C7DA4 File Offset: 0x000C5FA4
			// (set) Token: 0x06003359 RID: 13145 RVA: 0x000C7DAC File Offset: 0x000C5FAC
			internal int HashKey { get; set; }

			// Token: 0x1700086E RID: 2158
			// (get) Token: 0x0600335A RID: 13146 RVA: 0x000C7DB8 File Offset: 0x000C5FB8
			// (set) Token: 0x0600335B RID: 13147 RVA: 0x000C7DC0 File Offset: 0x000C5FC0
			internal InterfaceRelinkCode.HashTableEntry NextEntry { get; set; }

			// Token: 0x1700086F RID: 2159
			// (get) Token: 0x0600335C RID: 13148 RVA: 0x000C7DCC File Offset: 0x000C5FCC
			// (set) Token: 0x0600335D RID: 13149 RVA: 0x000C7DD4 File Offset: 0x000C5FD4
			internal bool Collision { get; set; }

			// Token: 0x040009CA RID: 2506
			private readonly IDataLocation \u0001;

			// Token: 0x040009CB RID: 2507
			private readonly IDataLocation \u0002;

			// Token: 0x040009CC RID: 2508
			private readonly string \u0001;

			// Token: 0x040009CD RID: 2509
			private readonly IDictionary<int, int> \u0001;

			// Token: 0x040009CE RID: 2510
			private readonly IDictionary<int, int> \u0002;

			// Token: 0x040009CF RID: 2511
			[CompilerGenerated]
			private int \u0001;

			// Token: 0x040009D0 RID: 2512
			[CompilerGenerated]
			private InterfaceRelinkCode.HashTableEntry \u0001;

			// Token: 0x040009D1 RID: 2513
			[CompilerGenerated]
			private bool \u0001;
		}

		// Token: 0x02000357 RID: 855
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x0600335F RID: 13151 RVA: 0x000C7DE8 File Offset: 0x000C5FE8
			internal bool \u0001(IInterfaceInfo \u0002)
			{
				return \u0002.InterfaceId == this.\u0001;
			}

			// Token: 0x040009D2 RID: 2514
			public int \u0001;
		}
	}
}
