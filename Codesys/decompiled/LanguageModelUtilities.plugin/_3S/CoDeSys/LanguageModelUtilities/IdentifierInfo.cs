using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class IdentifierInfo : IIdentifierInfo2, IIdentifierInfo
	{
		private string m_stName = string.Empty;

		private string m_stComment = string.Empty;

		private IdentifierInfoFlag m_flags;

		private IType m_type;

		private IVariable m_var;

		private ISignature m_sign;

		private readonly ISignature m_containingSign;

		public string Name
		{
			get
			{
				return m_stName;
			}
			set
			{
				m_stName = value;
			}
		}

		public string Comment
		{
			get
			{
				if (m_stComment == null)
				{
					return string.Empty;
				}
				return m_stComment;
			}
			set
			{
				m_stComment = value;
			}
		}

		public IdentifierInfoFlag Flags
		{
			get
			{
				return m_flags;
			}
			set
			{
				m_flags = value;
			}
		}

		public IType Type
		{
			get
			{
				return m_type;
			}
			set
			{
				m_type = value;
			}
		}

		public IVariable Variable
		{
			get
			{
				return m_var;
			}
			set
			{
				m_var = value;
			}
		}

		public ISignature Signature
		{
			get
			{
				return m_sign;
			}
			set
			{
				m_sign = value;
			}
		}

		public IScope Scope => null;

		public ISignature ContainingSignature => m_containingSign;

		public IdentifierInfo(string stName, string stComment, IdentifierInfoFlag flags, IType type)
		{
			m_flags = flags;
			m_stName = stName;
			m_stComment = stComment;
			m_type = type;
		}

		public IdentifierInfo(IVariable var, ISignature containingSign, string stName, string stComment, IdentifierInfoFlag flags, IType type)
		{
			m_flags = flags;
			m_stName = stName;
			m_stComment = stComment;
			m_type = type;
			m_var = var;
			m_containingSign = containingSign;
		}

		public IdentifierInfo(ISignature sign, string stName, string stComment, IdentifierInfoFlag flags, IType type)
		{
			m_flags = flags;
			m_stName = stName;
			m_stComment = stComment;
			m_type = type;
			m_sign = sign;
		}
	}
}
