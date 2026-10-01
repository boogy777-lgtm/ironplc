using System;
using System.Linq;
using \u0007;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0004
{
	// Token: 0x02000338 RID: 824
	internal static class \u0012
	{
		// Token: 0x060031C6 RID: 12742 RVA: 0x000C04B8 File Offset: 0x000BE6B8
		public static IScope5 \u0001(IScope5 \u0002, _ICompileContext \u0003, ISignature \u0004, bool \u0005)
		{
			bool copyScope = false;
			if (\u0002.CopyScope)
			{
				copyScope = true;
				\u0003 = (\u0002.ApplicationContext as _ICompileContext);
			}
			_ISignature isignature = \u0002[\u0004.Id] as _ISignature;
			IScope5 scope = null;
			if (isignature != null)
			{
				if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && !\u0005)
				{
					_IVariable ivariable = isignature["__Interface"] as _IVariable;
					if (ivariable != null)
					{
						_IUserdefType iuserdefType = ivariable._Type.BaseType as _IUserdefType;
						Debug.\u0001(iuserdefType != null);
						isignature = (iuserdefType.GetSignature(\u0002) as _ISignature);
					}
				}
				if (isignature != null && isignature.POUType == Operator.Action)
				{
					isignature = (\u0002[isignature.ParentSignatureId] as _ISignature);
				}
				if (isignature != null)
				{
					scope = \u0005.\u0001(\u0003, isignature.Id, false);
					if (scope != null)
					{
						scope.CopyScope = copyScope;
						scope.LocalScope = true;
					}
					if (isignature.POUType == Operator.FunctionBlock)
					{
						ISignature subSignature = isignature.GetSubSignature("__MAIN");
						if (scope != null)
						{
							scope.MethodSignature = (subSignature as _ISignature);
						}
					}
				}
			}
			return scope;
		}

		// Token: 0x060031C7 RID: 12743 RVA: 0x000C05AC File Offset: 0x000BE7AC
		public static IScope5 \u0001(IScope5 \u0002, _ICompileContext \u0003, _IUserdefType \u0004, bool \u0005)
		{
			bool copyScope = false;
			if (\u0002.CopyScope)
			{
				\u0003 = (\u0002.ApplicationContext as _ICompileContext);
				copyScope = true;
			}
			ISignature[] array = null;
			IScope5 scope = null;
			IScope5 scope2;
			if (\u0004.ScopeId != Helper.InvalidId)
			{
				scope2 = (\u0002.GetScopeById(\u0004.ScopeId) as IScope5);
				if (scope2 != null)
				{
					return scope2;
				}
			}
			_ISignature isignature = \u0002[\u0004.SignatureId] as _ISignature;
			if (isignature == null)
			{
				array = \u0002.FindSignature(\u0004.NameExpression);
				if (array != null && array.Length != 0)
				{
					isignature = (array[0] as _ISignature);
				}
				else
				{
					scope = (\u0002.FindScope(\u0004.NameExpression) as IScope5);
				}
			}
			if (array != null && array.Length != 0)
			{
				isignature = (array[0] as _ISignature);
			}
			if (isignature != null)
			{
				if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && !\u0005)
				{
					_IUserdefType iuserdefType = (isignature["__Interface"] as _IVariable)._Type.BaseType as _IUserdefType;
					Debug.\u0001(iuserdefType != null);
					isignature = (iuserdefType.GetSignature(\u0002) as _ISignature);
				}
				scope = \u0005.\u0001(\u0003, isignature.Id, false);
				if (scope != null)
				{
					scope.LocalScope = true;
					scope.CopyScope = copyScope;
				}
			}
			scope2 = scope;
			ISignature signature;
			if (scope2 != null && isignature != null)
			{
				if (isignature.POUType == Operator.FunctionBlock)
				{
					ISignature subSignature = isignature.GetSubSignature("__MAIN");
					scope2.MethodSignature = (subSignature as _ISignature);
				}
				else if (isignature.POUType == Operator.Type && isignature.GetFlag(SignatureFlag.Alias) && isignature.AllVariables.First<_IVariable>().CompiledType.Class == TypeClass.Enum)
				{
					_IEnumType ienumType = isignature.AllVariables.First<_IVariable>().CompiledType as _IEnumType;
					ISignature localSignature = \u0002[ienumType.SignatureId];
					scope2.LocalSignature = localSignature;
				}
			}
			else if ((signature = \u0004.GetSignature(\u0002)) != null)
			{
				scope2 = \u0012.\u0001(\u0002, \u0003, signature, \u0005);
			}
			return scope2;
		}
	}
}
