using System;
using System.Linq;
using \u0007;
using \u0011;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.OnlineChange.FastOnlinechange.Steps
{
	// Token: 0x0200037B RID: 891
	internal static class SignatureChangesInspector
	{
		// Token: 0x0600345B RID: 13403 RVA: 0x000CE1D4 File Offset: 0x000CC3D4
		internal static global::\u0011.\u0014 \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			global::\u0011.\u0014 u = new global::\u0011.\u0014
			{
				PrecompileSignature = \u0004,
				CompiledSignature = \u0003,
				\u0001 = (SignatureChangesInspector.\u0001(\u0003) != \u0004.Name)
			};
			IScope5 u2 = global::\u0007.\u0005.\u0001(\u0002, \u0003.Id);
			foreach (_IVariable ivariable in \u0004.AllVariables)
			{
				_IVariable ivariable2 = \u0003[ivariable.OrgName] as _IVariable;
				if (!SignatureChangesInspector.\u0001(\u0004, u, u2, ivariable, ivariable2))
				{
					SignatureChangesInspector.\u0001(u, ivariable, ivariable2);
				}
			}
			SignatureChangesInspector.\u0001(\u0003, \u0004, u);
			SignatureChangesInspector.\u0002(\u0003, \u0004, u);
			return u;
		}

		// Token: 0x0600345C RID: 13404 RVA: 0x000CE28C File Offset: 0x000CC48C
		private static bool \u0001(_ISignature \u0002, global::\u0011.\u0014 \u0003, IScope5 \u0004, _IVariable \u0005, _IVariable \u0006)
		{
			if (\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINIT) || (\u0006 != null && \u0006.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINIT)))
			{
				\u0003.\u0013 = true;
			}
			if (((\u0006 != null) ? \u0006._Initial : null) is IArrayInitialization && ArrayInitialisationCodeGenerator.CanPerformArrayInitMemCopy(\u0004, \u0006))
			{
				\u0003.\u0014 = true;
			}
			if (\u0006 != null)
			{
				return false;
			}
			if (\u0005.HasFlag(VarFlag.ReplacedConstant))
			{
				return true;
			}
			if (SignatureChangesInspector.\u0001(\u0002, \u0005))
			{
				\u0003.\u0003 = true;
			}
			else if (SignatureChangesInspector.\u0002(\u0002, \u0005))
			{
				\u0003.\u0006 = true;
			}
			else if (SignatureChangesInspector.\u0001(\u0005))
			{
				\u0003.\u000E = true;
			}
			if (\u0005.Address != null && \u0005.Address.Incomplete)
			{
				\u0003.\u0012 = true;
			}
			return true;
		}

		// Token: 0x0600345D RID: 13405 RVA: 0x000CE348 File Offset: 0x000CC548
		private static void \u0001(global::\u0011.\u0014 \u0002, _IVariable \u0003, _IVariable \u0004)
		{
			if (!VariableComparer.InitialValueEquals(\u0004, \u0003, false))
			{
				if (\u0004.GetFlag(VarFlag.ReplacedConstant))
				{
					\u0002.\u0002 = true;
				}
				else if (\u0004.GetFlag(VarFlag.Constant))
				{
					\u0002.\u0002 = true;
				}
				else if (\u0004.GetFlag(VarFlag.Absolut))
				{
					\u0002.\u0010 = true;
				}
				else if (\u0004.GetFlag(VarFlag.RelativeInstance))
				{
					\u0002.\u0008 = true;
				}
				else
				{
					\u0002.\u0005 = true;
				}
				if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE))
				{
					\u0002.\u0011 = true;
				}
			}
		}

		// Token: 0x0600345E RID: 13406 RVA: 0x000CE3D0 File Offset: 0x000CC5D0
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, global::\u0011.\u0014 \u0004)
		{
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (!ivariable.GetFlag(VarFlag.Implicit) && !(\u0003[ivariable.Name] is _IVariable))
				{
					if (SignatureChangesInspector.\u0001(\u0002, ivariable))
					{
						\u0004.\u0004 = true;
					}
					else if (SignatureChangesInspector.\u0002(\u0002, ivariable))
					{
						\u0004.\u0007 = true;
					}
					else if (SignatureChangesInspector.\u0001(ivariable))
					{
						\u0004.\u000F = true;
					}
					\u0004.DeletedVariables.Add(ivariable);
				}
			}
		}

		// Token: 0x0600345F RID: 13407 RVA: 0x000CE47C File Offset: 0x000CC67C
		private static string \u0001(_ISignature \u0002)
		{
			if (\u0002.Name.Contains('<'))
			{
				return \u0002.Name.Substring(0, \u0002.Name.IndexOf('<'));
			}
			return \u0002.Name;
		}

		// Token: 0x06003460 RID: 13408 RVA: 0x000CE4B0 File Offset: 0x000CC6B0
		private static void \u0002(_ISignature \u0002, _ISignature \u0003, global::\u0011.\u0014 \u0004)
		{
			if (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method)
			{
				IVariable[] u = \u0002.Inputs.Where(new Func<IVariable, bool>(SignatureChangesInspector.<>c.<>9.\u0001)).ToArray<IVariable>();
				SignatureChangesInspector.\u0001(\u0004, u, \u0003.Inputs);
				IVariable[] u2 = \u0002.Outputs.Where(new Func<IVariable, bool>(SignatureChangesInspector.<>c.<>9.\u0002)).ToArray<IVariable>();
				SignatureChangesInspector.\u0001(\u0004, u2, \u0003.Outputs);
			}
		}

		// Token: 0x06003461 RID: 13409 RVA: 0x000CE54C File Offset: 0x000CC74C
		private static void \u0001(global::\u0011.\u0014 \u0002, IVariable[] \u0003, IVariable[] \u0004)
		{
			for (int i = 0; i < \u0003.Length; i++)
			{
				if (\u0003[i].Name != \u0004[i].Name)
				{
					\u0002.\u0015 = true;
				}
			}
		}

		// Token: 0x06003462 RID: 13410 RVA: 0x000CE588 File Offset: 0x000CC788
		private static bool \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003.HasFlag(VarFlag.IsCompiled))
			{
				return \u0003.HasFlag(VarFlag.RelativeStack);
			}
			if (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method)
			{
				return \u0003.HasFlag(VarFlag.Local | VarFlag.Input | VarFlag.Output);
			}
			return \u0003.HasFlag(VarFlag.Temp);
		}

		// Token: 0x06003463 RID: 13411 RVA: 0x000CE5DC File Offset: 0x000CC7DC
		private static bool \u0002(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003.HasFlag(VarFlag.IsCompiled))
			{
				return \u0003.HasFlag(VarFlag.RelativeInstance);
			}
			return \u0002.POUType == Operator.FunctionBlock && \u0003.HasFlag(VarFlag.Local | VarFlag.Input | VarFlag.Output);
		}

		// Token: 0x06003464 RID: 13412 RVA: 0x000CE610 File Offset: 0x000CC810
		private static bool \u0001(_IVariable \u0002)
		{
			return \u0002.HasFlag(VarFlag.Absolut);
		}
	}
}
