using System;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0010
{
	// Token: 0x02000266 RID: 614
	internal static class \u0005
	{
		// Token: 0x06002794 RID: 10132 RVA: 0x00089010 File Offset: 0x00087210
		internal static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			if (!global::\u0010.\u0005.\u0001(\u0002, \u0003))
			{
				return null;
			}
			_IExpressionStatement iexpressionStatement = \u0019.\u0003.\u0001(\u0002);
			_IExpressionStatement iexpressionStatement2 = \u0019.\u0003.\u0001(global::\u0010.\u0005.\u0001(\u0002._LValue), global::\u0010.\u0005.\u0001(\u0002._RValue));
			_IStatement istatement = \u0019.\u0003.\u0001(new _IExpressionStatement[]
			{
				iexpressionStatement,
				iexpressionStatement2
			});
			\u0003.Generator.\u0001<_IStatement>(istatement, \u0003._Scope, \u0003.CompiledPOU);
			\u0003.Generator.DisableFlowBPForallExceptFirst(istatement, \u0002._Position);
			return istatement;
		}

		// Token: 0x06002795 RID: 10133 RVA: 0x00089090 File Offset: 0x00087290
		private static _IExpression \u0001(_IExpression \u0002)
		{
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				return \u0019.\u0003.\u0001(ivariableExpression.Name + "__Array__Info");
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				_ICompoAccessExpression icompoAccessExpression2 = \u0019.\u0003.\u0001();
				icompoAccessExpression2._Left = (_IExpression)icompoAccessExpression._Left.Duplicate();
				icompoAccessExpression2._Right = global::\u0010.\u0005.\u0001(icompoAccessExpression._Right);
				return icompoAccessExpression2;
			}
			return null;
		}

		// Token: 0x06002796 RID: 10134 RVA: 0x000890F8 File Offset: 0x000872F8
		internal static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			IVariable variable = \u0002._LValue.GetVariable(\u0003._Scope);
			IVariable variable2 = \u0002._RValue.GetVariable(\u0003._Scope);
			return variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY) && variable2 != null && variable2.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
		}
	}
}
