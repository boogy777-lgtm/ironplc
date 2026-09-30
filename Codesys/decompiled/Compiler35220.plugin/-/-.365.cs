using System;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u000F
{
	// Token: 0x020003B3 RID: 947
	internal static class \u0018
	{
		// Token: 0x0600368D RID: 13965 RVA: 0x000DCD84 File Offset: 0x000DAF84
		internal static void \u0001(_IVirtualFunctionTable \u0002, _ISequenceStatement \u0003, _ICompileContext \u0004)
		{
			for (int i = 0; i < \u0002._Entries.Count; i++)
			{
				_IFunctionPointerEntry ifunctionPointerEntry = \u0002._Entries[i] as _IFunctionPointerEntry;
				if (ifunctionPointerEntry != null)
				{
					_IIndexAccessExpression iindexAccessExpression = \u0003.\u0001();
					iindexAccessExpression._Var = \u0003.\u0001(IdentifierConstants.GetVFTableVarName(\u0002.Signature.Id));
					iindexAccessExpression.AddAccess(\u0003.\u0001((long)i, TypeClass.Int));
					_IAssignmentExpression iassignmentExpression = \u0003.\u0001(iindexAccessExpression);
					if (\u0004.NewVFTable)
					{
						if (\u0004.GetCodegeneratorProperty(CodegeneratorProperties.Thumb2))
						{
							iassignmentExpression._RValue = \u0003.\u0001(Operator.Or, \u0003.\u0001(IdentifierConstants.GetFPAddressVarName(ifunctionPointerEntry.Id)), \u0003.\u0001(1L, TypeClass.DWord));
						}
						else
						{
							iassignmentExpression._RValue = \u0003.\u0001(IdentifierConstants.GetFPAddressVarName(ifunctionPointerEntry.Id));
						}
					}
					else
					{
						_IOperatorExpression ioperatorExpression = \u0003.\u0001(Operator.Adr);
						ioperatorExpression.AddOperand(\u0003.\u0001(IdentifierConstants.GetFPAddressVarName(ifunctionPointerEntry.Id)));
						iassignmentExpression._RValue = ioperatorExpression;
					}
					\u0003.Add(\u0003.\u0001(iassignmentExpression, Token.Empty));
				}
				else
				{
					_IInterfaceOffsetEntry iinterfaceOffsetEntry = \u0002._Entries[i] as _IInterfaceOffsetEntry;
					Debug.\u0001(iinterfaceOffsetEntry != null);
					_IIndexAccessExpression iindexAccessExpression2 = \u0003.\u0001(\u0003.\u0001(IdentifierConstants.GetVFTableVarName(\u0002.Signature.Id)), Token.Empty);
					iindexAccessExpression2.AddAccess(\u0003.\u0001((long)i, TypeClass.Int));
					_IAssignmentExpression iassignmentExpression2 = \u0003.\u0001(iindexAccessExpression2);
					int num = iinterfaceOffsetEntry.InstancePointerOffset;
					if (num == 0)
					{
						num = \u0002.GetInterfaceOffsetInInstance(iinterfaceOffsetEntry.Id, \u0004);
					}
					Debug.\u0001(num != 0);
					iassignmentExpression2._RValue = \u0003.\u0001((ulong)((long)num), TypeClass.DWord);
					\u0003.Add(\u0003.\u0001(iassignmentExpression2, Token.Empty));
				}
			}
		}

		// Token: 0x0600368E RID: 13966 RVA: 0x000DCF24 File Offset: 0x000DB124
		internal static void \u0001(_IVirtualFunctionTable \u0002, LStringBuilder \u0003, _ICompileContext \u0004)
		{
			for (int i = 0; i < \u0002._Entries.Count; i++)
			{
				_IFunctionPointerEntry ifunctionPointerEntry = \u0002._Entries[i] as _IFunctionPointerEntry;
				string vftableVarName = IdentifierConstants.GetVFTableVarName(\u0002.Signature.Id);
				if (ifunctionPointerEntry != null)
				{
					string fpaddressVarName = IdentifierConstants.GetFPAddressVarName(ifunctionPointerEntry.Id);
					if (\u0004.NewVFTable)
					{
						\u0003.AppendLine(string.Format("{0}[{1}] := {2};", vftableVarName, i, fpaddressVarName));
					}
					else
					{
						\u0003.AppendLine(string.Format("{0}[{1}] := ADR({2});", vftableVarName, i, fpaddressVarName));
					}
				}
				else
				{
					_IInterfaceOffsetEntry iinterfaceOffsetEntry = (_IInterfaceOffsetEntry)\u0002._Entries[i];
					int interfaceOffsetInInstance = \u0002.GetInterfaceOffsetInInstance(iinterfaceOffsetEntry.Id, \u0004);
					\u0003.AppendLine(string.Format("{0}[{1}] := {2};", vftableVarName, i, interfaceOffsetInInstance));
				}
			}
		}
	}
}
