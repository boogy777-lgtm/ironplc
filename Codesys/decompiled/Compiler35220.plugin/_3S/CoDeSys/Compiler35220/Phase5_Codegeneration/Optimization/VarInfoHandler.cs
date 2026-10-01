using System;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000267 RID: 615
	public static class VarInfoHandler
	{
		// Token: 0x06002797 RID: 10135 RVA: 0x0008914C File Offset: 0x0008734C
		internal static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			IOperatorExpression operatorExpression = \u0002._RValue as IOperatorExpression;
			if (operatorExpression != null && operatorExpression.Code == Operator.__VarInfo)
			{
				string u = VarInfoHandler.\u0001(\u0002, \u0003);
				_ISequenceStatement isequenceStatement = (_ISequenceStatement)\u0003.Generator.\u0001(u, \u0003._Scope, \u0003.CompiledPOU);
				\u0003.Generator.DisableFlowBPForallExceptFirst(isequenceStatement, \u0002._Position);
				return isequenceStatement;
			}
			return null;
		}

		// Token: 0x06002798 RID: 10136 RVA: 0x000891B8 File Offset: 0x000873B8
		internal static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			IOperatorExpression operatorExpression = \u0002._RValue as IOperatorExpression;
			return operatorExpression != null && operatorExpression.Code == Operator.__VarInfo;
		}

		// Token: 0x06002799 RID: 10137 RVA: 0x000891E4 File Offset: 0x000873E4
		private static string \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			_IExpression iexpression = ((_IOperatorExpression)\u0002._RValue)._OperandsList[0];
			IVariable variable = iexpression.GetVariable(\u0003._Scope);
			IDirectVariable directVariable = null;
			IDataLocation dataLocation = null;
			_IAddressExpression iaddressExpression = iexpression as _IAddressExpression;
			if (iaddressExpression != null)
			{
				directVariable = iaddressExpression.DirectAddress;
				IMessage message;
				bool flag;
				dataLocation = Locator.\u0001(\u0003.Comcon, out message, out flag, iaddressExpression.Position, directVariable, null);
			}
			else if (variable != null)
			{
				directVariable = variable.Address;
				dataLocation = variable.DataLocation;
			}
			string text = \u0002._LValue.ToString();
			lstringBuilder.Append(text + ".FB_INIT(FALSE, FALSE);");
			if (dataLocation != null)
			{
				if (!dataLocation.IsRelativ)
				{
					lstringBuilder.AppendFormat("{0}.ByteAddress := ADR({1});", new object[]
					{
						text,
						iexpression.ToString()
					});
				}
				lstringBuilder.AppendFormat("{0}.ByteOffset := {1};", new object[]
				{
					text,
					dataLocation.Offset
				});
				lstringBuilder.AppendFormat("{0}.Area := {1};", new object[]
				{
					text,
					dataLocation.Area
				});
				lstringBuilder.AppendFormat("{0}.BitNr := {1};", new object[]
				{
					text,
					dataLocation.BitNr
				});
			}
			if ((directVariable != null && directVariable.Size == DirectVariableSize.X) || (iexpression.Type != null && iexpression.Type.Class == TypeClass.Bit))
			{
				lstringBuilder.AppendFormat("{0}.BitAdress := BITADR({1});", new object[]
				{
					text,
					iexpression
				});
			}
			VarInfoHandler.\u0001(\u0003, dataLocation, lstringBuilder, text, variable, directVariable);
			VarInfoHandler.\u0001(\u0003, variable, lstringBuilder, text);
			lstringBuilder.AppendFormat("{0}.Symbol := '{1}';", new object[]
			{
				text,
				\u0084.\u0002.\u0001(iexpression.ToString(), false)
			});
			string text2 = "MEM_UNKNOWN";
			text2 = VarInfoHandler.\u0001(directVariable, text2, dataLocation);
			lstringBuilder.AppendFormat("{0}.MemoryArea := __SYSTEM.MEMORY_AREA.{1};", new object[]
			{
				text,
				text2
			});
			return lstringBuilder.ToString();
		}

		// Token: 0x0600279A RID: 10138 RVA: 0x000893D8 File Offset: 0x000875D8
		private static void \u0001(global::\u000E.\u0011 \u0002, IDataLocation \u0003, LStringBuilder \u0004, string \u0005, IVariable \u0006, IDirectVariable \u0007)
		{
			if (\u0003 != null && \u0003.IsBitLocation)
			{
				\u0004.AppendFormat("{0}.BitSize := 1;", new object[]
				{
					\u0005
				});
				return;
			}
			if (\u0006 != null)
			{
				\u0004.AppendFormat("{0}.BitSize := {1};", new object[]
				{
					\u0005,
					\u0006.CompiledType.Size(\u0002._Scope) * 8
				});
				return;
			}
			if (\u0007 != null)
			{
				\u0004.AppendFormat("{0}.BitSize := {1};", new object[]
				{
					\u0005,
					((_IDirectVariable)\u0007).BitSize
				});
			}
		}

		// Token: 0x0600279B RID: 10139 RVA: 0x00089470 File Offset: 0x00087670
		private static void \u0001(global::\u000E.\u0011 \u0002, IVariable \u0003, LStringBuilder \u0004, string \u0005)
		{
			if (\u0003 != null)
			{
				\u0004.AppendFormat("{0}.TypeClass := {1};", new object[]
				{
					\u0005,
					(int)\u0003.CompiledType.Class
				});
				\u0004.AppendFormat("{0}.TypeName := '{1}';", new object[]
				{
					\u0005,
					\u0084.\u0002.\u0001(\u0003.CompiledType.ToString(), false)
				});
				_IArrayType iarrayType = \u0003.CompiledType as _IArrayType;
				if (iarrayType != null)
				{
					\u0004.AppendFormat("{0}.NumElements := {1};", new object[]
					{
						\u0005,
						iarrayType.GetNumOfElements(\u0002._Scope)
					});
				}
				\u0004.AppendFormat("{0}.BaseTypeClass := {1};", new object[]
				{
					\u0005,
					(int)\u0003.CompiledType.BaseType.Class
				});
				\u0004.AppendFormat("{0}.ElemBitSize := {1};", new object[]
				{
					\u0005,
					\u0003.CompiledType.BaseType.Size(\u0002._Scope) * 8
				});
				\u0004.AppendFormat("{0}.Comment := '{1}';", new object[]
				{
					\u0005,
					\u0084.\u0002.\u0001(\u0003.Comment ?? "", false)
				});
			}
		}

		// Token: 0x0600279C RID: 10140 RVA: 0x000895A4 File Offset: 0x000877A4
		private static string \u0001(IDirectVariable \u0002, string \u0003, IDataLocation \u0004)
		{
			if (\u0002 != null)
			{
				switch (\u0002.Location)
				{
				case DirectVariableLocation.Input:
					\u0003 = "MEM_INPUT";
					break;
				case DirectVariableLocation.Output:
					\u0003 = "MEM_OUTPUT";
					break;
				case DirectVariableLocation.Memory:
					\u0003 = "MEM_MEMORY";
					break;
				}
			}
			else if (\u0004 != null)
			{
				\u0003 = (\u0004.IsRelativ ? "MEM_LOCAL" : "MEM_GLOBAL");
			}
			return \u0003;
		}
	}
}
