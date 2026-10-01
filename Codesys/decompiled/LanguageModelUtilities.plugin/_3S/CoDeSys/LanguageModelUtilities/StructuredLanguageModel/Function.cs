using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	[TypeGuid("{07E014F2-9635-4FA4-A68F-918434DEC92E}")]
	public sealed class Function : AbstractPOUWithReturnValue, IFunctionBuilder2, IFunctionBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		private int m_iSlot = -1;

		private Guid m_gdTaskGuid = Guid.Empty;

		public int Slot
		{
			get
			{
				return m_iSlot;
			}
			set
			{
				m_iSlot = value;
			}
		}

		public Guid TaskGuid
		{
			get
			{
				return m_gdTaskGuid;
			}
			set
			{
				m_gdTaskGuid = value;
			}
		}

		protected override Operator PouType => Operator.Function;

		protected override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel, ILMPOU pou, IPOUDeclarationStatement pouDeclaration)
		{
			if (m_iSlot != -1 && m_gdTaskGuid != Guid.Empty)
			{
				pou.Slot = m_iSlot;
				pou.TaskReference = m_gdTaskGuid;
			}
			base.AddToLanguageModel(pos, languageModel, pou, pouDeclaration);
		}
	}
}
