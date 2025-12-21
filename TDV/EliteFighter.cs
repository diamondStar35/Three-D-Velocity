/* This source is provided under the GNU AGPLv3  license. You are free to modify and distribute this source and any containing work (such as sound files) provided that:
* - You make available complete source code of modifications, even if the modifications are part of a larger project, and make the modified work available under the same license (GNU AGPLv3).
* - You include all copyright and license notices on the modified source.
* - You state which parts of this source were changed in your work
* Note that containing works (such as SharpDX) may be available under a different license.
* Copyright (C) Munawar Bijani
*/
using System;

namespace TDV
{
	public class EliteFighter : Aircraft
	{
		public EliteFighter(float x, float y)
			: base(0, 1800, "EliteFighter", true, new Track(Options.currentTrack))
		{
			this.x = x;
			this.y = y;
			maxProbability = 80;
			setDamagePoints(4000);
			accelerationSpeed = 40;
		}

		public EliteFighter()
			: this(0f, 0f)
		{ }
	}
}
