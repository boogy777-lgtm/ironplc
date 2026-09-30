using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200001A RID: 26
	[ReleasedInterface]
	public interface IProject
	{
		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000070 RID: 112
		bool Dirty { get; }

		// Token: 0x06000071 RID: 113
		void SetDirty();

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000072 RID: 114
		int Handle { get; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000073 RID: 115
		// (set) Token: 0x06000074 RID: 116
		string Path { get; set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000075 RID: 117
		// (set) Token: 0x06000076 RID: 118
		string Id { get; set; }

		// Token: 0x06000077 RID: 119
		bool Close();

		// Token: 0x06000078 RID: 120
		bool Save();

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000079 RID: 121
		bool Primary { get; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600007A RID: 122
		bool Library { get; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x0600007B RID: 123
		Guid[] Attributes { get; }

		// Token: 0x0600007C RID: 124
		bool HasAttribute(Guid projectAttr);

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600007D RID: 125
		// (set) Token: 0x0600007E RID: 126
		ISVNode[] SelectedSVNodes { get; set; }

		// Token: 0x0600007F RID: 127
		void SelectSVNode(ISVNode svNode, bool bAddToSelection);

		// Token: 0x06000080 RID: 128
		void SelectSVNodes(ISVNode[] svNodes, bool bAddToSelection);

		// Token: 0x06000081 RID: 129
		void DeselectSVNode(ISVNode svNode);

		// Token: 0x06000082 RID: 130
		void DeselectSVNodes(ISVNode[] svNodes);

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000083 RID: 131
		// (set) Token: 0x06000084 RID: 132
		Guid ActiveApplication { get; set; }

		// Token: 0x06000085 RID: 133
		void GetInstancePathInfo(string stInstancePath, out Guid appObjectGuid, out bool bExisting, out bool bOnline);

		// Token: 0x06000086 RID: 134
		bool GetProfile(out Profile profile, out string stProfileName);

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000087 RID: 135
		// (remove) Token: 0x06000088 RID: 136
		event ProjectChangedEventHandler DirtyChanged;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000089 RID: 137
		// (remove) Token: 0x0600008A RID: 138
		event ProjectChangedEventHandler PathChanged;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x0600008B RID: 139
		// (remove) Token: 0x0600008C RID: 140
		event ProjectChangedEventHandler SelectionChanged;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x0600008D RID: 141
		// (remove) Token: 0x0600008E RID: 142
		event ProjectChangedEventHandler ActiveApplicationChanged;
	}
}
