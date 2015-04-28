using UnityEngine;
using System.Collections;


//main New Atlantis engine class
public static class NA
{
	public static AudioListener listener = null;
	//public static Camera currentcam = null;
	public static bool bAugmentAudioSources = false;

	//public static bool bDisplayAudioSourceName = true;





	public static void DecorateAudioSource(AudioSource s)
	{
		NAReverbEffector eff = s.gameObject.GetComponent<NAReverbEffector>();
		if (eff == null)
			s.gameObject.AddComponent<NAReverbEffector>();
			
		AudioReverbFilter arf = s.gameObject.GetComponent<AudioReverbFilter>();
		if (arf == null)
			s.gameObject.AddComponent<AudioReverbFilter>();
		
		
		NAAudioSource src = s.gameObject.GetComponent<NAAudioSource>();
		if (src == null)
			s.gameObject.AddComponent<NAAudioSource>();
		
		if (NA.bAugmentAudioSources)
		{

			
			NAOcclusionFX occ = s.gameObject.GetComponent<NAOcclusionFX>();
			if (occ == null)
			{
				s.gameObject.AddComponent<NAOcclusionFX>();
				//workaround
				NAOcclusionFX occ2 = s.gameObject.AddComponent<NAOcclusionFX>();
				NAOcclusionFX.Destroy(occ2);
			}

		}
	}
}
