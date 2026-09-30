using System;
using \u0002;
using \u000E;
using \u0011;
using \u0013;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0016
{
	// Token: 0x02000317 RID: 791
	internal static class \u0013
	{
		// Token: 0x06002F8D RID: 12173 RVA: 0x000B2DBC File Offset: 0x000B0FBC
		private static void \u0001(_ISignature \u0002, ISourcePosition \u0003, Severity \u0004, MessageId \u0005)
		{
			string stError = \u0018.\u0001(\u0005);
			\u0002.AddMessage(\u0002.CreateCompilerMessage(\u0003, stError, Severity.Error, (int)\u0005));
		}

		// Token: 0x06002F8E RID: 12174 RVA: 0x000B2DE0 File Offset: 0x000B0FE0
		private static void \u0001(_ISignature \u0002, ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0002.GetFlag(SignatureFlag.Abstract) && \u0003 != null && \u0003.POUType == Operator.FunctionBlock && !\u0003.GetFlag(SignatureFlag.Abstract))
			{
				(\u0003 as _ISignature).AddMessage(Severity.Error, MessageId.Err_AbstractMethodOnlyInAbstractFunctionblock, new object[]
				{
					\u0002.OrgName,
					\u0003.OrgName
				});
			}
		}

		// Token: 0x06002F8F RID: 12175 RVA: 0x000B2E48 File Offset: 0x000B1048
		private static void \u0002(_ISignature \u0002, ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0002.GetFlag(SignatureFlag.Abstract) && \u0003 != null && Helper.InvalidId != \u0002.Id)
			{
				_ICompiledPOU icompiledPOU = \u0004.GetCompiledPOUById(\u0002.Id) as _ICompiledPOU;
				if (icompiledPOU != null && icompiledPOU.ParseTree != null)
				{
					global::\u0002.\u0001 u = new global::\u0002.\u0001();
					IStatementTraverser ivisit = new global::\u0013.\u0001(u);
					(icompiledPOU.ParseTree as _IStatement).Accept(ivisit);
					if (!u.\u0001())
					{
						\u0002.AddMessage(Severity.Error, MessageId.Err_AbstractMethodMustNotContainAnyStatements, new object[]
						{
							\u0003.OrgName,
							\u0002.OrgName
						});
					}
				}
			}
		}

		// Token: 0x06002F90 RID: 12176 RVA: 0x000B2EDC File Offset: 0x000B10DC
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, IScope5 \u0004, _ICompileContext \u0005)
		{
			foreach (_ISignature isignature in \u0003.SubSignatures)
			{
				if (!(\u0002.GetSubSignature(isignature.Name) is _ISignature) && isignature.GetFlag(SignatureFlag.Abstract) && !\u0002.GetFlag(SignatureFlag.Abstract))
				{
					ISignature signature = \u0004.FindSignatureLocal(isignature.Name);
					if (\u0005.GetSignatureById(signature.ParentSignatureId).Id == \u0003.Id)
					{
						if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
						{
							string text = null;
							string orgName = isignature.OrgName;
							string text2 = null;
							if (orgName.StartsWith("__get"))
							{
								text = global::\u0011.\u0001.Getter;
								text2 = orgName.Substring("__get".Length);
							}
							else if (orgName.StartsWith("__set"))
							{
								text = global::\u0011.\u0001.Setter;
								text2 = orgName.Substring("__set".Length);
							}
							if (!string.IsNullOrEmpty(text))
							{
								\u0002.AddMessage(Severity.Error, MessageId.Err_AbstractPropertyNotImplemented, new object[]
								{
									text,
									text2,
									\u0003.OrgName
								});
							}
						}
						else
						{
							\u0002.AddMessage(Severity.Error, MessageId.Err_AbstractMethodNotImplemented, new object[]
							{
								isignature.OrgName,
								\u0003.OrgName
							});
						}
						_ISourcePosition sourcepos = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid, 0L, 0, 0);
						\u0003.AddMessage(sourcepos, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
					}
				}
			}
		}

		// Token: 0x06002F91 RID: 12177 RVA: 0x000B307C File Offset: 0x000B127C
		private static void \u0001(_ISignature \u0002, ISignature \u0003)
		{
			if (\u0002.GetFlag(SignatureFlag.Abstract) && Operator.Method == \u0002.POUType)
			{
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					if (!ivariable.IsProperty && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE) && !ivariable.HasFlag(VarFlag.Input | VarFlag.Output | VarFlag.Inout))
					{
						bool flag = false;
						if (ivariable.HasFlag(VarFlag.Local) && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
						{
							string text = "__get" + ivariable.OrgName;
							flag = (("__set" + ivariable.OrgName).Equals(\u0002.OrgName) || text.Equals(\u0002.OrgName));
						}
						if (!flag)
						{
							global::\u0016.\u0013.\u0001(\u0002, ivariable.SourcePosition, Severity.Error, MessageId.Err_AbstractWrongVarInMethod);
							break;
						}
						break;
					}
				}
			}
		}

		// Token: 0x06002F92 RID: 12178 RVA: 0x000B317C File Offset: 0x000B137C
		private static void \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			if (\u0002.HasFlag(SignatureFlag.Abstract))
			{
				foreach (int nId in \u0002.DeclarerIds)
				{
					ISignature signatureById = \u0003.GetSignatureById(nId);
					foreach (IVariable variable in signatureById.All)
					{
						\u001F.\u0002 typvis = new \u001F.\u0002(\u0003, signatureById, variable.SourcePosition);
						((_IType)variable.Type).Accept(typvis);
					}
				}
				foreach (int nId2 in \u0002.ReferencerIds)
				{
					ISignature signatureById2 = \u0003.GetSignatureById(nId2);
					if (signatureById2 != null)
					{
						foreach (IVariable variable2 in signatureById2.All)
						{
							\u001F.\u0002 typvis2 = new \u001F.\u0002(\u0003, signatureById2, variable2.SourcePosition);
							((_IType)variable2.Type).Accept(typvis2);
						}
					}
				}
			}
		}

		// Token: 0x06002F93 RID: 12179 RVA: 0x000B3274 File Offset: 0x000B1474
		internal static void \u0001(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			Operator poutype = \u0002.POUType;
			if (poutype != Operator.FunctionBlock)
			{
				if (poutype == Operator.Method)
				{
					ISignature u = \u0003[\u0002.ParentSignatureId] as _ISignature;
					global::\u0016.\u0013.\u0001(\u0002, u, \u0004);
					global::\u0016.\u0013.\u0002(\u0002, u, \u0004);
					global::\u0016.\u0013.\u0001(\u0002, u);
					return;
				}
			}
			else
			{
				_ISignature isignature;
				for (int baseSignatureId = \u0002.BaseSignatureId; baseSignatureId != Helper.InvalidId; baseSignatureId = isignature.BaseSignatureId)
				{
					isignature = (\u0003[baseSignatureId] as _ISignature);
					if (isignature == null)
					{
						break;
					}
					global::\u0016.\u0013.\u0001(\u0002, isignature, \u0003, \u0004);
				}
				global::\u0016.\u0013.\u0001(\u0002, \u0004);
			}
		}
	}
}
