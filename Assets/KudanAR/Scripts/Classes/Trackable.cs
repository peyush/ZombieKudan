using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	public class Trackable
	{
		public string name;
		public int width;
		public int height;
		public bool isDetected;
		public Vector3 position;
		public Quaternion orientation;
		public int trackingMethod;
	}
};