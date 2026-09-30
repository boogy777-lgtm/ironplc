using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000307 RID: 775
	internal sealed class FBInitParameterService : \u001A
	{
		// Token: 0x06002F10 RID: 12048 RVA: 0x000B1268 File Offset: 0x000AF468
		public FBInitParameterService(IAssignmentExpression[] variableInputAssignments)
		{
			this.\u0001 = variableInputAssignments;
		}

		// Token: 0x06002F11 RID: 12049 RVA: 0x000B1278 File Offset: 0x000AF478
		public IVariable \u0001(_ISignature \u0002, int \u0003)
		{
			if (this.\u0001[\u0003].LValue is INullExpression)
			{
				return \u0002.AllInputs[\u0003 + 2];
			}
			return \u0002[this.\u0001[\u0003].LValue.ToString()];
		}

		// Token: 0x06002F12 RID: 12050 RVA: 0x000B12B4 File Offset: 0x000AF4B4
		public IEnumerable<_IExpression> \u0001()
		{
			return this.\u0001.OfType<_IAssignmentExpression>().Select(new Func<_IAssignmentExpression, _IExpression>(FBInitParameterService.<>c.<>9.\u0001));
		}

		// Token: 0x06002F13 RID: 12051 RVA: 0x000B12E8 File Offset: 0x000AF4E8
		public bool \u0001(CaseInsensitiveDictionary<IVariable> \u0002, _ISignature \u0003)
		{
			bool result = false;
			foreach (IVariable variable in \u0003.AllInputs)
			{
				if (!(variable.Name == "__INSTANCEPOINTER") && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INPUT) && !string.Equals(variable.Name, "bInitRetains", StringComparison.OrdinalIgnoreCase) && !string.Equals(variable.Name, "bInCopyCode", StringComparison.OrdinalIgnoreCase) && variable.Initial == null && !\u0002.ContainsKey(variable.Name))
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x040008F5 RID: 2293
		private readonly IAssignmentExpression[] \u0001;
	}
}
