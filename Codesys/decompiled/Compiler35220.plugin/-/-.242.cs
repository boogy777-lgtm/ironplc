using System;
using System.Collections.Generic;
using System.Globalization;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x0200029F RID: 671
	internal static class \u0010
	{
		// Token: 0x06002A3B RID: 10811 RVA: 0x00093758 File Offset: 0x00091958
		public static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			DirectVariableLocation u;
			return \u0010.\u0001(\u0010.\u0001(\u0010.\u0001(operandsList[0], \u0003), operandsList[0], \u0003, out u), u, \u0003, operandsList[0]);
		}

		// Token: 0x06002A3C RID: 10812 RVA: 0x000937A4 File Offset: 0x000919A4
		private static IDirectVariable \u0001(_IExpression \u0002, \u0011 \u0003)
		{
			IAddressExpression addressExpression = \u0002 as IAddressExpression;
			if (addressExpression != null)
			{
				return addressExpression.DirectAddress;
			}
			IVariable variable = \u0002.GetVariable(\u0003._Scope);
			if (variable == null)
			{
				return null;
			}
			return variable.Address;
		}

		// Token: 0x06002A3D RID: 10813 RVA: 0x000937DC File Offset: 0x000919DC
		private static uint \u0001(IDirectVariable \u0002, _IExpression \u0003, \u0011 \u0004, out DirectVariableLocation \u0005)
		{
			uint result;
			if (\u0002 != null)
			{
				result = (uint)(\u0002.Components[0] * 8 + \u0002.Components[1]);
				\u0005 = \u0002.Location;
			}
			else
			{
				IDataLocation dataLocation = \u0003.DataLocation(\u0004._Scope);
				result = (uint)(dataLocation.Offset * 8 + (int)dataLocation.BitNr);
				\u0005 = \u0010.\u0001(dataLocation, \u0004);
			}
			return result;
		}

		// Token: 0x06002A3E RID: 10814 RVA: 0x00093834 File Offset: 0x00091A34
		private static DirectVariableLocation \u0001(IDataLocation \u0002, \u0011 \u0003)
		{
			foreach (_IDataSegment idataSegment in \u0003.Comcon.DataManager._DataSegments)
			{
				if (!idataSegment.GetFlag(DataSegmentFlags.Area) && idataSegment.Area == \u0002.Area && idataSegment.Address <= \u0002.Offset && idataSegment.Address + idataSegment.Size > \u0002.Offset)
				{
					if (idataSegment.GetFlag(DataSegmentFlags.Memory))
					{
						return DirectVariableLocation.Memory;
					}
					if (idataSegment.GetFlag(DataSegmentFlags.Input))
					{
						return DirectVariableLocation.Input;
					}
					if (idataSegment.GetFlag(DataSegmentFlags.Output))
					{
						return DirectVariableLocation.Output;
					}
					break;
				}
			}
			return DirectVariableLocation.None;
		}

		// Token: 0x06002A3F RID: 10815 RVA: 0x000938F4 File Offset: 0x00091AF4
		private static _IExpression \u0001(uint \u0002, DirectVariableLocation \u0003, \u0011 \u0004, _IExpression \u0005)
		{
			switch (\u0003)
			{
			case DirectVariableLocation.Input:
				\u0002 |= 2147483648U;
				break;
			case DirectVariableLocation.Output:
				\u0002 |= 3221225472U;
				break;
			case DirectVariableLocation.Memory:
				\u0002 |= 1073741824U;
				break;
			}
			_IExpression iexpression = \u0004.Generator.GenerateExpression(\u0002.ToString(CultureInfo.InvariantCulture), \u0004._Scope, \u0004.CompiledPOU);
			\u0004.Generator.CopyPositionAndMessages(\u0005, iexpression);
			return iexpression;
		}
	}
}
