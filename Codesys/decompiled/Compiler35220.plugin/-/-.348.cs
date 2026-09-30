using System;
using \u0003;
using \u0007;
using \u0018;
using \u001C;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x0200038E RID: 910
	internal sealed class \u0017 : \u001C.\u0012
	{
		// Token: 0x060034B6 RID: 13494 RVA: 0x000CFEE8 File Offset: 0x000CE0E8
		public bool \u0001(\u0016 \u0002)
		{
			if (!\u0002.signChanges.VariableDeleted)
			{
				return true;
			}
			foreach (_IVariable ivariable in \u0002.signChanges.DeletedVariables)
			{
				if (\u0017.\u0001(\u0002.sign, \u0002.focContext.ComconNew, ivariable))
				{
					return false;
				}
				foreach (ICrossReference u in ivariable.CrossReferences)
				{
					if (!\u0017.\u0001(\u0002.focContext, u))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060034B7 RID: 13495 RVA: 0x000CFF94 File Offset: 0x000CE194
		private static bool \u0001(\u0018.\u0010 \u0002, ICrossReference \u0003)
		{
			if (\u0002.\u0001.Contains(\u0003.CodeId))
			{
				return true;
			}
			\u0002.\u0001.Add(\u0003.CodeId);
			IScope5 scope = null;
			if (!\u0017.\u0001(\u0002, \u0003, ref scope))
			{
				return false;
			}
			_ICompiledPOU icompiledPOU = \u0002.ComconNew.GetCompiledPOUById(\u0003.CodeId) as _ICompiledPOU;
			if (icompiledPOU == null)
			{
				return true;
			}
			_ICompiledPOU icompiledPOU2 = Helper.\u0001(\u0002.ComconNew, icompiledPOU, \u0002.Precomp, \u0002.PrecompPool);
			if (icompiledPOU2 == null)
			{
				return true;
			}
			scope = global::\u0007.\u0005.\u0001(\u0002.ComconNew, \u0003.CodeId);
			_ICompiledPOU icompiledPOU3 = icompiledPOU2.CreateCompiledPOU();
			icompiledPOU3.SignatureId = icompiledPOU.SignatureId;
			icompiledPOU3.DuplicateParseTreeForCompilation();
			ExpressionTypifierWithSpecialTasks visitor = new ExpressionTypifierWithSpecialTasks(scope, \u0002.ComconNew, null, true, false, icompiledPOU3);
			icompiledPOU3.Accept(visitor);
			TypeCheckerVisitor visitor2 = new TypeCheckerVisitor(scope, \u0002.ComconNew, false, icompiledPOU3, false);
			icompiledPOU3.Accept(visitor2);
			return !\u0002.\u0001(icompiledPOU3, new ErrorVisitor());
		}

		// Token: 0x060034B8 RID: 13496 RVA: 0x000D0080 File Offset: 0x000CE280
		private static bool \u0001(\u0018.\u0010 \u0002, ICrossReference \u0003, ref IScope5 \u0004)
		{
			_ISignature isignature = \u0002.ComconNew.GetSignatureById(\u0003.CodeId) as _ISignature;
			if (isignature == null)
			{
				return true;
			}
			ISignature[] array;
			_IPreCompileContext ipreCompileContext;
			_ISignature isignature2 = Helper.\u0001(\u0002.ComconNew, isignature, \u0002.Precomp, \u0002.PrecompPool, out array, out ipreCompileContext);
			if (isignature2 == null)
			{
				return true;
			}
			_ISignature isignature3 = isignature2.CreateCompiledSignature(isignature, \u0002.ComconNew, \u0002.ComconOld, \u0002.ComconNew.HasByteSupport());
			\u0004 = global::\u0007.\u0005.\u0001(\u0002.ComconNew, isignature3.Id);
			\u0004.LocalSignature = isignature3;
			\u0002.CompileInformation.InterfaceCompiler.\u0003(isignature3, \u0004);
			\u0002.CompileInformation.InterfaceCompiler.\u0001(isignature3, \u0004);
			return !\u0002.\u0001(isignature3);
		}

		// Token: 0x060034B9 RID: 13497 RVA: 0x000D0140 File Offset: 0x000CE340
		private static bool \u0001(_ISignature \u0002, _ICompileContext \u0003, _IVariable \u0004)
		{
			IVariable[] array;
			ISignature[] array2;
			IScope scope;
			return (global::\u0007.\u0005.\u0001(\u0003, \u0002.Id) as _IScope).FindDeclaration(\u0004.Name, out array, out array2, out scope);
		}
	}
}
