using GameData;
using UnityEngine;
using System.Collections.Generic;

namespace ModifyRelations
{
	public class Relation
	{
		public int key;
		public string text;
		public bool only;
		public bool net;
		public bool enterable {
			get{ return net || !only; }
		}

		public bool addable {
			get{ return !net;}
		}

		public Relation(int _key, string _text, bool isOnly = false, bool isNet = false) {
			key = _key;
			text = _text;
			only = isOnly;
			net = isNet;
		}
	}
}
