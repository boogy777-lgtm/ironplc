using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200024A RID: 586
	public class MonitoringService : ILMMonitoringService
	{
		// Token: 0x06002718 RID: 10008 RVA: 0x00062440 File Offset: 0x00061440
		public bool CanConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder)
		{
			CompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(guidApplication) as CompileContext;
			if (compileContext == null || typeVarRef == null)
			{
				return false;
			}
			IScope5 scope = compileContext.CreateGlobalScope();
			ICompiledType4 compiledType = typeVarRef as ICompiledType4;
			if (compiledType != null && compiledType.Class == TypeClass.Reference)
			{
				if (compiledType.BaseType.Class == TypeClass.Enum)
				{
					compiledType = (compiledType.BaseType as ICompiledType4);
				}
				else
				{
					compiledType = (compiledType.DeRefType as ICompiledType4);
				}
			}
			return compiledType != null && compiledType.CanConvertRaw(raw, byteOrder, scope);
		}

		// Token: 0x06002719 RID: 10009 RVA: 0x000624C0 File Offset: 0x000614C0
		public object ConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder)
		{
			IScope5 scope;
			if (guidApplication != Guid.Empty)
			{
				_ICompileContext referenceContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(guidApplication);
				if (referenceContext == null)
				{
					return false;
				}
				scope = CompilerProxy.CreateGlobalScope(referenceContext);
			}
			else
			{
				scope = null;
			}
			ICompiledType4 compiledType = typeVarRef as ICompiledType4;
			if (compiledType != null && compiledType.Class == TypeClass.Reference)
			{
				if (compiledType.BaseType.Class == TypeClass.Enum)
				{
					compiledType = (compiledType.BaseType as ICompiledType4);
				}
				else
				{
					compiledType = (compiledType.DeRefType as ICompiledType4);
				}
			}
			if (compiledType == null)
			{
				return false;
			}
			return compiledType.ConvertRaw(raw, byteOrder, scope);
		}

		// Token: 0x0600271A RID: 10010 RVA: 0x00062554 File Offset: 0x00061554
		public byte[] ConvertToRaw(object value, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder)
		{
			IScope5 scope;
			if (guidApplication != Guid.Empty)
			{
				_ICompileContext referenceContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(guidApplication);
				if (referenceContext == null)
				{
					throw new Exception("No compilecontext");
				}
				scope = CompilerProxy.CreateGlobalScope(referenceContext);
			}
			else
			{
				scope = null;
			}
			if (typeVarRef.Class == TypeClass.Reference)
			{
				return ((ICompiledType4)((ICompiledType4)typeVarRef).BaseType).ConvertToRaw(value, byteOrder, scope);
			}
			return ((ICompiledType4)typeVarRef).ConvertToRaw(value, byteOrder, scope);
		}

		// Token: 0x0600271B RID: 10011 RVA: 0x000625C9 File Offset: 0x000615C9
		public IAddressInfo GetAddressInfo(Guid guidApplication, IExpression exp, IScope scope)
		{
			return VarReferenceCreator.GetAddressInfo(guidApplication, exp, scope);
		}

		// Token: 0x0600271C RID: 10012 RVA: 0x000625D3 File Offset: 0x000615D3
		public IEnumerable<IVarRef> GetAllVarReferences(string stInstance, long[] alPositionsOfInterest)
		{
			return VarReferenceCreator.GetAllVarReferences(Guid.Empty, Guid.Empty, stInstance, alPositionsOfInterest);
		}

		// Token: 0x0600271D RID: 10013 RVA: 0x000625E6 File Offset: 0x000615E6
		public IEnumerable<IVarRef> GetAllVarReferences(Guid objectguid, string stInstance, long[] alPositionsOfInterest)
		{
			return VarReferenceCreator.GetAllVarReferences(objectguid, Guid.Empty, stInstance, alPositionsOfInterest);
		}

		// Token: 0x0600271E RID: 10014 RVA: 0x000625F5 File Offset: 0x000615F5
		public IEnumerable<IVarRef> GetAllVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest)
		{
			return VarReferenceCreator.GetAllVarReferences(objectguid, guidExplicitApplicationGuid, stInstance, alPositionsOfInterest);
		}

		// Token: 0x0600271F RID: 10015 RVA: 0x00062601 File Offset: 0x00061601
		public IConverterFromIEC GetConverterFromIEC()
		{
			return new ConvertFromIEC();
		}

		// Token: 0x06002720 RID: 10016 RVA: 0x00062608 File Offset: 0x00061608
		public IConverterToIEC GetConverterToIEC(bool bOmitPrefixesWherePossible, bool bUseShortPrefixes, DisplayMode displayMode)
		{
			return new ConvertToIEC(bOmitPrefixesWherePossible, bUseShortPrefixes, displayMode);
		}

		// Token: 0x06002721 RID: 10017 RVA: 0x00062612 File Offset: 0x00061612
		public IVarRef GetVarReference(string stExpression)
		{
			return VarReferenceCreator.GetVarReference(stExpression);
		}

		// Token: 0x06002722 RID: 10018 RVA: 0x0006261A File Offset: 0x0006161A
		public IVarRef GetVarReference(Guid guidApplication, string stExpression)
		{
			return VarReferenceCreator.GetVarReference(guidApplication, stExpression, false);
		}

		// Token: 0x06002723 RID: 10019 RVA: 0x00062624 File Offset: 0x00061624
		public IVarRef GetVarReference(string stExpression, string stInstancePath)
		{
			return VarReferenceCreator.GetVarReference(stExpression, stInstancePath);
		}

		// Token: 0x06002724 RID: 10020 RVA: 0x0006262D File Offset: 0x0006162D
		public IVarRef GetVarReference(Guid guidApplication, string stPOUName, string stExpression)
		{
			return VarReferenceCreator.GetVarReference(guidApplication, stPOUName, stExpression);
		}

		// Token: 0x06002725 RID: 10021 RVA: 0x00062637 File Offset: 0x00061637
		public IVarRef GetVarReference(Guid guidApplication, string stExpression, bool bAllowShortExpressions)
		{
			return VarReferenceCreator.GetVarReference(guidApplication, stExpression, bAllowShortExpressions);
		}

		// Token: 0x06002726 RID: 10022 RVA: 0x00062641 File Offset: 0x00061641
		public IVarRef GetVarReference(Guid guidApplication, string stInstancePath, string stExpression, int nProjectHandle, Guid guidObject)
		{
			return VarReferenceCreator.GetVarReference(guidApplication, stInstancePath, stExpression, nProjectHandle, guidObject);
		}
	}
}
