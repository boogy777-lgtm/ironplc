using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0006;
using \u0007;
using \u000E;
using \u0017;
using \u0019;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.CompilerPhases
{
	// Token: 0x020003F8 RID: 1016
	internal sealed class CompilerPhase2_AfterTypification
	{
		// Token: 0x17000959 RID: 2393
		// (get) Token: 0x06003853 RID: 14419 RVA: 0x000E6E10 File Offset: 0x000E5010
		// (set) Token: 0x06003854 RID: 14420 RVA: 0x000E6E18 File Offset: 0x000E5018
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x1700095A RID: 2394
		// (get) Token: 0x06003855 RID: 14421 RVA: 0x000E6E24 File Offset: 0x000E5024
		private bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x1700095B RID: 2395
		// (get) Token: 0x06003856 RID: 14422 RVA: 0x000E6E34 File Offset: 0x000E5034
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x1700095C RID: 2396
		// (get) Token: 0x06003857 RID: 14423 RVA: 0x000E6E44 File Offset: 0x000E5044
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x06003858 RID: 14424 RVA: 0x000E6E54 File Offset: 0x000E5054
		internal CompilerPhase2_AfterTypification(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
		}

		// Token: 0x06003859 RID: 14425 RVA: 0x000E6E64 File Offset: 0x000E5064
		public bool \u0001(bool \u0002)
		{
			if (!\u0002)
			{
				global::\u0006.\u000F.\u0001(this.ComconNew, this.ComconOld);
				global::\u0006.\u000F.\u0002(this.ComconNew, this.ComconOld);
			}
			\u0002 = (\u0002 || !this.\u0002());
			global::\u0006.\u000F.\u0003(this.ComconNew, this.ComconOld);
			this.\u0001();
			if (!\u0002)
			{
				global::\u0017.\u0018.\u0001(this.ComconNew, this.ComconOld);
				this.\u0001(this.ComconNew);
				global::\u0003.\u0002.\u0001(this.ComconNew);
				PersistentCodegenerator.\u0001(this.ComconNew);
			}
			this.\u0002();
			return \u0002;
		}

		// Token: 0x0600385A RID: 14426 RVA: 0x000E6EFC File Offset: 0x000E50FC
		private bool \u0002()
		{
			bool result = true;
			IEnumerable<_ISignature> allSignatureList = this.ComconNew.AllSignatureList;
			IScope scope = this.ComconNew.CreateGlobalIScope();
			foreach (_ISignature isignature in allSignatureList)
			{
				if (isignature.GetFlag(SignatureFlag.PoolSignature) && (isignature.GetFlag(SignatureFlag.Enum) || isignature.POUType == Operator.VarGlobal) && !isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
				{
					foreach (IVariable variable in isignature.AllVariables)
					{
						ISignature[] array2;
						IVariable[] array = scope.FindVariable(variable.OrgName, out array2);
						if (array2 != null && array2.Length != 0)
						{
							IEnumerable<ISignature> source = array2.Where(new Func<ISignature, bool>(CompilerPhase2_AfterTypification.<>c.<>9.\u0001));
							if (array.Count<IVariable>() > 1 && source.Count<ISignature>() > 0)
							{
								string u = global::\u0003.\u0006.\u0001(MessageId.Err_Ambiguity, new object[]
								{
									variable.OrgName
								});
								_ICompilerMessage message = global::\u0019.\u0003.\u0001(variable.SourcePosition, u, Severity.Error, MessageId.Err_Ambiguity);
								isignature.AddMessage(message);
								for (int i = 0; i < array2.Length; i++)
								{
									if (array2[i].Id != isignature.Id && !array2[i].GetFlag(SignatureFlag.PoolSignature) && string.IsNullOrEmpty(array2[i].LibraryPath))
									{
										_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001(array[i].SourcePosition.ProjectHandle, array[i].SourcePosition.ObjectGuid, array[i].SourcePosition.Position, array[i].SourcePosition.PositionOffset, array[i].SourcePosition.Length);
										isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(array2[i].LibraryPath), array2[i].ObjectGuid);
										u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
										isignature.AddMessage(global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
										result = false;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600385B RID: 14427 RVA: 0x000E7190 File Offset: 0x000E5390
		internal static bool \u0001(_ISignature \u0002, LStack<_ISignature> \u0003)
		{
			bool result = true;
			int num = 0;
			if (\u0002 == null)
			{
				return true;
			}
			foreach (_ISignature isignature in \u0003)
			{
				if (isignature.Id == \u0002.Id)
				{
					string text = " -> " + \u0002.Name;
					object[] array = \u0003.ToArray();
					object[] array2 = array;
					for (int i = 0; i <= num; i++)
					{
						_ISignature isignature2 = array2[i] as _ISignature;
						if (num == i)
						{
							text = isignature2.Name + text;
						}
						else
						{
							text = " -> " + isignature2.Name + text;
						}
					}
					isignature.AddMessage(Severity.Warning, MessageId.Wrn_CallRecursion, new object[]
					{
						text
					});
					result = false;
					break;
				}
				num++;
			}
			return result;
		}

		// Token: 0x0600385C RID: 14428 RVA: 0x000E7280 File Offset: 0x000E5480
		private bool \u0001(_ISignature \u0002, LStack<_ISignature> \u0003, LDictionary<int, int> \u0004)
		{
			if (\u0002 == null)
			{
				return true;
			}
			if (!CompilerPhase2_AfterTypification.\u0001(\u0002, \u0003))
			{
				return false;
			}
			if (\u0004.ContainsKey(\u0002.Id))
			{
				return true;
			}
			if (\u0002.POUType == Operator.Method)
			{
				return true;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(this.ComconNew);
			\u0003.Push(\u0002);
			foreach (int nId in \u0002.CalleeIds)
			{
				_ISignature u = scope[nId] as _ISignature;
				if (!this.\u0001(u, \u0003, \u0004))
				{
					return false;
				}
			}
			\u0003.Pop();
			\u0004[\u0002.Id] = \u0002.Id;
			return true;
		}

		// Token: 0x0600385D RID: 14429 RVA: 0x000E731C File Offset: 0x000E551C
		private void \u0001()
		{
			LStack<_ISignature> lstack = new LStack<_ISignature>();
			LDictionary<int, int> ldictionary = new LDictionary<int, int>();
			LHashSet<int> lhashSet = new LHashSet<int>();
			foreach (_ISignature u in this.ComconNew.SlotPOUs.GetAllTaskPOUs(this.ComconNew))
			{
				lstack.Clear();
				ldictionary.Clear();
				if (!this.\u0001(u, lstack, ldictionary))
				{
					return;
				}
				lhashSet.UnionWith(ldictionary.Keys);
			}
			foreach (_ISignature isignature in this.ComconNew.AllSignatureList)
			{
				if ((isignature.POUType == Operator.Program || isignature.POUType == Operator.Function) && !lhashSet.Contains(isignature.Id))
				{
					lstack.Clear();
					ldictionary.Clear();
					if (!this.\u0001(isignature, lstack, ldictionary))
					{
						break;
					}
				}
			}
		}

		// Token: 0x0600385E RID: 14430 RVA: 0x000E7414 File Offset: 0x000E5614
		private void \u0001(_ISignature \u0002, byte \u0003)
		{
			if (\u0002 == null || (int)\u0003 >= this.ComconNew.TaskList.Count || \u0002.IsReferencedByTask(\u0003))
			{
				return;
			}
			\u0002.AddTaskReference(\u0003);
			this.\u0001(global::\u0007.\u0005.\u0001(this.ComconNew)[\u0002.BaseSignatureId] as _ISignature, \u0003);
			foreach (object obj in \u0002._SubSignatures)
			{
				_ISignature u = (_ISignature)obj;
				this.\u0001(u, \u0003);
			}
			foreach (int nId in \u0002.CalleeIds)
			{
				this.\u0001(this.ComconNew[nId], \u0003);
			}
			foreach (IVariable variable in \u0002.AllVariables)
			{
				_IUserdefType iuserdefType = null;
				TypeClass @class = variable.CompiledType.DeRefType.Class;
				if (@class != TypeClass.Array)
				{
					if (@class != TypeClass.Userdef)
					{
						continue;
					}
					iuserdefType = (variable.CompiledType as _IUserdefType);
				}
				else
				{
					ICompiledType compiledType = \u0084.\u0004.\u0001(variable.CompiledType.DeRefType as _IArrayType);
					if (compiledType.Class == TypeClass.Userdef)
					{
						iuserdefType = (compiledType as _IUserdefType);
					}
				}
				if (iuserdefType != null)
				{
					this.\u0001(this.ComconNew[iuserdefType.SignatureId], \u0003);
				}
			}
		}

		// Token: 0x0600385F RID: 14431 RVA: 0x000E75A8 File Offset: 0x000E57A8
		private bool \u0001(_ICompileContext \u0002)
		{
			foreach (_ISignature isignature in \u0002.POUSignatures)
			{
				foreach (Guid guidTask in \u0002.SlotPOUs.GetAllTasksForObjectGuid(isignature.ObjectGuid))
				{
					this.\u0001(isignature, \u0002.TaskList.GetTaskIndexByGuid(guidTask));
				}
			}
			foreach (_ISignature isignature2 in \u0002.AllSignatureList)
			{
				if (isignature2.HasAttribute("task"))
				{
					string attributeValue = isignature2.GetAttributeValue("task");
					for (int j = 0; j < \u0002.TaskList.Count; j++)
					{
						if (\u0002.TaskList[j].TaskName.ToUpperInvariant() == attributeValue.ToUpperInvariant())
						{
							isignature2.AddTaskReference((byte)j);
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06003860 RID: 14432 RVA: 0x000E76CC File Offset: 0x000E58CC
		private void \u0002()
		{
			foreach (_ISignature isignature in this.ComconNew.AllSignatures.OfType<_ISignature>())
			{
				ISignature signature = this.ComconNew[isignature.BaseSignatureId];
				if (signature != null && signature.HasAttribute(CompileAttributes.ATTRIBUTE_HASANYTYPE))
				{
					isignature.AddAttribute(CompileAttributes.ATTRIBUTE_HASANYTYPE, null);
				}
			}
		}

		// Token: 0x04000B3A RID: 2874
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;
	}
}
