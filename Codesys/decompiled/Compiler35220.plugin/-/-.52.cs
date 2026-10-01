using System;
using System.Linq;
using \u0007;
using \u0018;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0081;

namespace \u001C
{
	// Token: 0x020000BF RID: 191
	internal sealed class \u0003
	{
		// Token: 0x06000E77 RID: 3703 RVA: 0x00027680 File Offset: 0x00025880
		public \u0003(\u0080.\u0005.\u0001 \u0098\u0004, \u0080.\u0005.\u0002 \u0016\u0005, int[] \u0017\u0005, _ICompileContext \u001C\u0004)
		{
			this.\u0001 = \u0098\u0004;
			this.\u0001 = \u0016\u0005;
			this.\u0001 = \u0017\u0005;
			this.\u0001 = \u001C\u0004;
			this.\u0001 = new \u0081.\u0003(\u001C\u0004, \u0098\u0004);
		}

		// Token: 0x06000E78 RID: 3704 RVA: 0x000276B4 File Offset: 0x000258B4
		internal InstancePathsContainer \u0001(_ISignature \u0002)
		{
			_ISignature isignature = null;
			if (\u0002.POUType == Operator.Method || \u0002.POUType == Operator.Action)
			{
				isignature = \u0002;
				\u0002 = this.\u0001[\u0002.ParentSignatureId];
			}
			InstancePathsContainer instancePathsContainer = this.\u0001(\u0002, isignature);
			if (isignature != null)
			{
				InstancePathsContainer instancePathsContainer2 = new InstancePathsContainer();
				foreach (InstancePathInformation instancePathInformation in instancePathsContainer.InstancePathInformations)
				{
					string text = instancePathInformation.Path;
					text = text + "." + isignature.OrgName;
					instancePathsContainer2.\u0001(text, instancePathInformation.DeclaredVariable, instancePathInformation.SignDeclarationLocation);
				}
				instancePathsContainer = instancePathsContainer2;
			}
			return instancePathsContainer;
		}

		// Token: 0x06000E79 RID: 3705 RVA: 0x00027770 File Offset: 0x00025970
		private InstancePathsContainer \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			if (!\u0002.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants) || global::\u0018.\u0003.\u0001(\u0002))
			{
				return this.\u0002(\u0002, \u0003);
			}
			InstancePathsContainer instancePathsContainer = new InstancePathsContainer();
			foreach (_ISignature u in global::\u0018.\u0003.\u0001(this.\u0001, \u0002))
			{
				instancePathsContainer.\u0001(this.\u0002(u, \u0003).InstancePathInformations);
			}
			return instancePathsContainer;
		}

		// Token: 0x06000E7A RID: 3706 RVA: 0x000277F8 File Offset: 0x000259F8
		private InstancePathsContainer \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			if (\u0002 == null)
			{
				return new InstancePathsContainer();
			}
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				if (\u0002.Id == this.\u0001[i])
				{
					return new InstancePathsContainer();
				}
			}
			\u0080.\u0005.\u0004 u;
			InstancePathsContainer result;
			if (this.\u0001(\u0002, \u0003, out u, out result))
			{
				return result;
			}
			string u2;
			if (this.\u0001(\u0002, out u2))
			{
				InstancePathsContainer instancePathsContainer = new InstancePathsContainer();
				instancePathsContainer.\u0001(u2, null, null);
				return instancePathsContainer;
			}
			InstancePathsContainer instancePathsContainer2 = new InstancePathsContainer();
			this.\u0001(\u0002, instancePathsContainer2);
			this.\u0001.Cache[u] = new \u0080.\u0005.\u0005(instancePathsContainer2);
			return instancePathsContainer2;
		}

		// Token: 0x06000E7B RID: 3707 RVA: 0x0002788C File Offset: 0x00025A8C
		private bool \u0001(_ISignature \u0002)
		{
			return \u0002.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants) && !global::\u0018.\u0003.\u0001(\u0002);
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x000278A8 File Offset: 0x00025AA8
		private void \u0001(_ISignature \u0002, InstancePathsContainer \u0003)
		{
			int[] declarerIds = \u0002.DeclarerIds;
			IScope scope = this.\u0001.CreateGlobalIScope();
			foreach (int nId in declarerIds)
			{
				_ISignature isignature = scope[nId] as _ISignature;
				bool u;
				if (isignature != null && !this.\u0001(isignature) && !this.\u0001(\u0002, isignature, out u))
				{
					int[] array2 = new int[this.\u0001.Length + 1];
					this.\u0001.CopyTo(array2, 0);
					array2[this.\u0001.Length] = \u0002.Id;
					this.\u0001(\u0002, \u0003, isignature, array2);
					InstancePathsContainer u2;
					if (!this.\u0001(\u0003, isignature, array2) && !this.\u0002(isignature) && this.\u0001.\u0001(\u0002, isignature) && !this.\u0001(u, isignature, array2, out u2))
					{
						this.\u0001.\u0001(\u0002, \u0003, isignature, u2);
					}
				}
			}
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x00027998 File Offset: 0x00025B98
		private bool \u0002(_ISignature \u0002)
		{
			return (\u0002.POUType == Operator.Method || \u0002.POUType == Operator.Function) && !this.\u0001.\u0002;
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x000279C0 File Offset: 0x00025BC0
		private bool \u0001(_ISignature \u0002, _ISignature \u0003, out bool \u0004)
		{
			\u0004 = this.\u0001.\u0003;
			if (this.\u0001.\u0005)
			{
				\u0004 = true;
				if (\u0003.POUType == Operator.Interface && \u0003.BaseSignatureId == \u0002.Id)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x000279FC File Offset: 0x00025BFC
		private bool \u0001(bool \u0002, _ISignature \u0003, int[] \u0004, out InstancePathsContainer \u0005)
		{
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001(this.\u0001)
			{
				\u0003 = \u0002
			};
			InstancePathsContainer instancePathsContainer = InstancePathService.\u0001(this.\u0001, \u0003, \u0004, this.\u0001, u);
			\u0005 = new InstancePathsContainer();
			\u0005.\u0001(instancePathsContainer.InstancePathInformations);
			return !\u0005.InstancePaths.Any<string>();
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x00027A58 File Offset: 0x00025C58
		private bool \u0001(InstancePathsContainer \u0002, _ISignature \u0003, int[] \u0004)
		{
			if (\u0003.GetFlag(SignatureFlag.Alias))
			{
				IType type = \u0003.AllVariables[0].Type;
				if (type.Class == TypeClass.Array)
				{
					LList<_IVariable> llist = new LList<_IVariable>();
					LList<_ISignature> llist2 = new LList<_ISignature>();
					LList<string> llist3 = InstancePathService.\u0001(this.\u0001, \u0003, \u0004, this.\u0001, llist, llist2, this.\u0001);
					_IArrayType u = type as _IArrayType;
					IScope5 u2 = global::\u0007.\u0005.\u0001(this.\u0001, \u0003.Id);
					LList<string> llist4 = InstancePathService.\u0001(u, u2);
					for (int i = 0; i < llist3.Count; i++)
					{
						foreach (string str in llist4)
						{
							string u3 = llist3[i] + str;
							\u0002.\u0001(u3, llist[i], llist2[i]);
						}
					}
				}
				else
				{
					InstancePathsContainer instancePathsContainer = InstancePathService.\u0001(this.\u0001, \u0003, \u0004, this.\u0001, this.\u0001);
					\u0002.\u0001(instancePathsContainer.InstancePathInformations);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x00027B84 File Offset: 0x00025D84
		private void \u0001(_ISignature \u0002, InstancePathsContainer \u0003, _ISignature \u0004, int[] \u0005)
		{
			if (\u0004.BaseSignatureId == \u0002.Id && this.\u0001.\u0003)
			{
				InstancePathsContainer instancePathsContainer = InstancePathService.\u0001(this.\u0001, \u0004, \u0005, this.\u0001, this.\u0001);
				\u0003.\u0001(instancePathsContainer.InstancePathInformations);
			}
		}

		// Token: 0x06000E82 RID: 3714 RVA: 0x00027BD4 File Offset: 0x00025DD4
		public static bool \u0001(\u0080.\u0005.\u0001 \u0002, _ICompileContext \u0003, _ISignature \u0004, out string \u0005)
		{
			if (\u0004.POUType != Operator.VarGlobal && \u0004.POUType != Operator.Program && \u0004.POUType != Operator.Function)
			{
				\u0005 = null;
				return false;
			}
			string text;
			if (\u0004.GetFlag(SignatureFlag.PoolSignature) && \u0002.\u0006)
			{
				text = "__POOL." + \u0004.OrgName;
			}
			else
			{
				text = \u0004.OrgName;
			}
			if (!string.IsNullOrEmpty(\u0004.LibraryPath) && \u0002.\u0001)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0004.LibraryPath);
				if (libraryContext != null)
				{
					IExpression expression = Helper.\u0001(\u0003, libraryContext);
					if (expression != null)
					{
						text = string.Format("{0}#{1}", expression, text);
					}
				}
			}
			\u0005 = text;
			return true;
		}

		// Token: 0x06000E83 RID: 3715 RVA: 0x00027C84 File Offset: 0x00025E84
		private bool \u0001(_ISignature \u0002, out string \u0003)
		{
			return \u001C.\u0003.\u0001(this.\u0001, this.\u0001, \u0002, out \u0003);
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x00027C9C File Offset: 0x00025E9C
		private bool \u0001(_ISignature \u0002, _ISignature \u0003, out \u0080.\u0005.\u0004 \u0004, out InstancePathsContainer \u0005)
		{
			\u0005 = null;
			int u0097_u = -1;
			if (this.\u0001.Length != 0)
			{
				u0097_u = this.\u0001.Last<int>();
			}
			\u0004 = new \u0080.\u0005.\u0004(\u0002.Id, (\u0003 != null) ? \u0003.Id : -1, u0097_u, this.\u0001);
			if (this.\u0001.Cache.ContainsKey(\u0004))
			{
				\u0080.\u0005.\u0005 u = this.\u0001.Cache[\u0004];
				\u0005 = u.\u0001;
				return true;
			}
			return false;
		}

		// Token: 0x04000277 RID: 631
		private readonly _ICompileContext \u0001;

		// Token: 0x04000278 RID: 632
		private readonly int[] \u0001;

		// Token: 0x04000279 RID: 633
		private readonly \u0080.\u0005.\u0002 \u0001;

		// Token: 0x0400027A RID: 634
		private readonly \u0080.\u0005.\u0001 \u0001;

		// Token: 0x0400027B RID: 635
		private readonly \u0081.\u0003 \u0001;
	}
}
