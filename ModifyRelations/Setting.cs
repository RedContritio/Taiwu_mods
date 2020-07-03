using UnityModManagerNet;
using System.Collections.Generic;
using UnityEngine;

namespace ModifyRelations
{
	public class Setting : UnityModManager.ModSettings
	{
		public bool DebugMode = false;

        public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save(this, modEntry);
		}
	}
}
