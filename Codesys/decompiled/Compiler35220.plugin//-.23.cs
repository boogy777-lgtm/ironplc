using System;
using System.Linq;
using \u0011;
using \u0018;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x02000321 RID: 801
	internal static class \u001C
	{
		// Token: 0x06002FE8 RID: 12264 RVA: 0x000B50B0 File Offset: 0x000B32B0
		internal static bool \u0001(_IPreCompileContext \u0002, _ISignature \u0003, _ICompiledPOU \u0004)
		{
			bool result;
			try
			{
				if (!\u0003.GetFlag((SignatureFlag)((ulong)-2147483648)))
				{
					result = true;
				}
				else
				{
					_ISignature isignature = \u0003.Duplicate() as _ISignature;
					LList<IVariable> llist = isignature.\u0001();
					if (llist.Count == 0)
					{
						result = true;
					}
					else
					{
						int num = int.MaxValue;
						while (llist.Count != 0 && num > llist.Count && !APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.DelayedLoaderWorking)
						{
							num = llist.Count;
							UnknownIdentVisitor unknownIdentVisitor = new UnknownIdentVisitor(new CheckerScope(isignature, \u0002, APEnvironmentFacade.Instance.LanguageModelMgr.Pool)
							{
								ApplicationGuid = \u0002.ApplicationGuid
							}, true, null, llist);
							\u0004.Accept(unknownIdentVisitor);
							\u0018.\u0001[] u = unknownIdentVisitor.DeclarationInfo;
							\u001C.\u0001(llist, unknownIdentVisitor, u);
							llist = isignature.\u0001();
						}
						foreach (_IVariable ivariable in \u0003.\u0001().OfType<_IVariable>())
						{
							IVariable variable = isignature[ivariable.Name];
							if (variable != null && variable.Type != null && TypeTable.IsConcreteType(variable.Type.Class))
							{
								ivariable.AddAttribute("inferredtype", variable.Type.ToString());
							}
						}
						result = !isignature.AllLazy.Any<IVariable>();
					}
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06002FE9 RID: 12265 RVA: 0x000B5244 File Offset: 0x000B3444
		private static void \u0001(LList<IVariable> \u0002, UnknownIdentVisitor \u0003, \u0018.\u0001[] \u0004)
		{
			foreach (_IVariable ivariable in \u0002.OfType<_IVariable>())
			{
				foreach (\u0018.\u0001 u in \u0004)
				{
					if (string.Compare(u.Name, ivariable.VersionedName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						IType type = u.DerivedType;
						if (\u0003.\u0001(type))
						{
							ivariable._Type = (type as _IType).Duplicate;
							if (ivariable._Type.Class == TypeClass.Bit)
							{
								ivariable._Type = TypeTable.Bool;
								break;
							}
							break;
						}
					}
				}
			}
		}
	}
}
