using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SilencePlease;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInIncompatibility("org.bepinex.plugins.valheim_plus")]
public class SilencePlease : BaseUnityPlugin
{
	private const string ModName = "Silence Please";
	private const string ModVersion = "1.0.2";
	private const string ModGUID = "org.bepinex.plugins.silenceplease";

	private static ConfigEntry<string> muteSounds = null!;
	private static readonly HashSet<string> mutedSoundsList = new();
	private static ConfigEntry<bool> logSounds = null!;
	private static HashSet<string> previousMutedSoundsList = new();
	private static ConfigEntry<bool> silenceChickens = null!;
	private static ConfigEntry<float> wolfSilenceRange = null!;
	private static ConfigEntry<bool> silenceShieldGenerators = null!;
	private static ConfigEntry<WolfSilenceMode> wolfSilenceMode = null!;
	private static ConfigEntry<bool> muteEnabled = null!;
	private static ConfigEntry<bool> silenceAsksvin = null!;

	public enum WolfSilenceMode
	{
		Off,
		CubsInRange,
		On
	}

	private static SilencePlease selfReference = null!;
	private static ManualLogSource logger => selfReference.Logger;

	public void Awake()
	{
		selfReference = this;

		muteEnabled = Config.Bind("General", "Enable Mod", true, "Enable/Disable mod");
		muteSounds = Config.Bind("General", "Mute Sounds", "", "Comma separated list of the sound effects you want to mute");

		silenceChickens = Config.Bind("Quick Mute", "Silence Chickens", false, "If on, mutes the chicken sound effect (sfx_love).");
		silenceShieldGenerators = Config.Bind("Quick Mute", "Silence Shield Generators", false, "If on, mutes the shield generator sound effect (sfx_shieldgenerator_powered_loop).");
		wolfSilenceMode = Config.Bind("Quick Mute", "Silence Wolves", WolfSilenceMode.Off, "Controls if wolf howls are muted. 'CubsInRange' only mutes howls if a wolf cub is in range.");
		silenceAsksvin = Config.Bind("Quick Mute", "Silence Asksvin", false, "If on, mutes the asksvin sound effects (sfx_asksvin_footstep, sfx_asksvin_idle).");

		wolfSilenceRange = Config.Bind("Advanced", "Wolf Silence Range", 30f, "Range to check for nearby wolves when silencing howls.");
		logSounds = Config.Bind("Advanced", "Log Sounds", false, "When enabled, the name of sounds playedwill be logged to console");

		muteSounds.SettingChanged += UpdateMuteList;
		muteEnabled.SettingChanged += UpdateMuteList;
		silenceChickens.SettingChanged += UpdateMuteList;
		silenceShieldGenerators.SettingChanged += UpdateMuteList;
		wolfSilenceMode.SettingChanged += UpdateMuteList;
		wolfSilenceRange.SettingChanged += UpdateMuteList;
		silenceAsksvin.SettingChanged += UpdateMuteList;

		Assembly assembly = Assembly.GetExecutingAssembly();
		Harmony harmony = new(ModGUID);
		harmony.PatchAll(assembly);

		UpdateMuteList(null, null);
	}

	private static void UpdateMuteList(object? sender, EventArgs? e)
	{
		previousMutedSoundsList = new HashSet<string>(mutedSoundsList);

		mutedSoundsList.Clear();

		if (muteEnabled.Value)
		{
			foreach (string s in muteSounds.Value.Split(','))
			{
				string trimmed = s.Trim();
				if (!string.IsNullOrEmpty(trimmed))
				{
					mutedSoundsList.Add(trimmed);
				}
			}

			if (silenceChickens.Value)
			{
				mutedSoundsList.Add("sfx_love");
			}

			if (silenceShieldGenerators.Value)
			{
				mutedSoundsList.Add("sfx_shieldgenerator_powered_loop");
			}

			if (silenceAsksvin.Value)
			{
				mutedSoundsList.Add("sfx_asksvin_footstep");
				mutedSoundsList.Add("sfx_asksvin_idle");
			}
		}

		foreach (ZSFX zsfx in FindObjectsOfType<ZSFX>())
		{
			string prefabName = Utils.GetPrefabName(zsfx.gameObject);
			AudioSource zsfxAudioSource = zsfx.GetComponent<AudioSource>();
			if (zsfxAudioSource == null)
			{
				continue;
			}

			if (!muteEnabled.Value)
			{
				if (previousMutedSoundsList.Contains(prefabName))
				{
					zsfxAudioSource.mute = false;
					if (logSounds.Value)
					{
						logger.LogInfo($"Unmuted {prefabName} because muting is globally disabled.");
					}
				}
				continue;
			}

			if (mutedSoundsList.Contains(prefabName))
			{
				zsfxAudioSource.mute = true;
				if (logSounds.Value)
				{
					logger.LogInfo($"Muted already-active sound effect {prefabName} due to mute list update.");
				}
			}

			else if (previousMutedSoundsList.Contains(prefabName))
			{
				zsfxAudioSource.mute = false;
				if (logSounds.Value)
				{
					logger.LogInfo($"Unmuted already-active sound effect {prefabName} due to mute list update.");
				}
			}
		}
	}

	[HarmonyPatch(typeof(ZSFX), nameof(ZSFX.Awake))]
	private static class RemoveSounds
	{
		private static void Postfix(ZSFX __instance)
		{
			if (!muteEnabled.Value)
			{
				return;
			}

			string prefabName = Utils.GetPrefabName(__instance.gameObject);

			bool muted = false;

			if (mutedSoundsList.Contains(prefabName))
			{
				muted = true;
			}

			if (!muted && wolfSilenceMode.Value != WolfSilenceMode.Off && __instance.name.Contains("sfx_wolf_haul"))
			{
				List<Character> characters = new List<Character>();
				Character.GetCharactersInRange(__instance.transform.position, wolfSilenceRange.Value, characters);
				bool found = characters.Exists(c =>
					(c.name == "Wolf_cub(Clone)" || c.name == "Wolf_cub") ||
					(wolfSilenceMode.Value == WolfSilenceMode.On && (c.name == "Wolf(Clone)" || c.name == "Wolf"))
				);
				if (found)
				{
					muted = true;
				}
			}

			AudioSource instanceAudioSource = __instance.GetComponent<AudioSource>();
			if (instanceAudioSource != null)
			{
				instanceAudioSource.mute = muted;
				if (logSounds.Value)
				{
					if (muted)
					{
						logger.LogInfo($"Sound effect {prefabName} was muted.");
					}
					else
					{
						logger.LogInfo($"Sound effect {prefabName} played.");
					}
				}
			}
		}
	}
}
