using System;
using System.Collections;
using BandTogether.Quantum;
using BandTogether.TheDoor;
using BandTogether.Util;
using UnityEngine;

namespace BandTogether;

public class EndingController : MonoBehaviour
{
	[SerializeField] private EntrywayTrigger roomEntryway = null;
	[SerializeField] private Campfire fire = null;
	[SerializeField] private QuantumNPC doorkeeper = null;
	[SerializeField] private QuantumNPC[] nomai = null;
	[SerializeField] private QuantumNPC[] ghird = null;
	[SerializeField] private OWAudioSource doorkeeperInstrument = null;
	[SerializeField] private OWAudioSource[] nomaiInstruments = null;
	[SerializeField] private OWAudioSource[] ghirdInstruments = null;
	[SerializeField] private OWAudioSource pad = null;

	private bool _fireLit = false;
	private bool _nomaiMusicStarted = false;
	private bool _ghirdMusicStarted = false;
	private float _musicStartTime = 0; 

	private void Awake()
	{
		fire.OnCampfireStateChange += OnFireStateChanged;
		roomEntryway.OnEntry += OnRoomEntered;
		nomai.ForEach(npc => npc.OnPostCollapse += OnNomaiMove);
		ghird.ForEach(npc => npc.OnPostCollapse += OnGhirdMove);
		ModMain.AddDialogueConditionListener(EnableFire, "DOORKEEPER_TO_FIRE");
		
		doorkeeperInstrument.SetLocalVolume(0);
		nomaiInstruments.ForEach(instrument => instrument.SetLocalVolume(0));
		ghirdInstruments.ForEach(instrument => instrument.SetLocalVolume(0));
	}

	private void OnDestroy()
	{
		fire.OnCampfireStateChange -= OnFireStateChanged;
		roomEntryway.OnEntry -= OnRoomEntered;
		nomai.ForEach(npc => npc.OnPostCollapse -= OnNomaiMove);
		ghird.ForEach(npc => npc.OnPostCollapse -= OnGhirdMove);
		ModMain.RemoveDialogueConditionListener(EnableFire, "DOORKEEPER_TO_FIRE");
	}

	private void OnRoomEntered(GameObject enteringObject)
	{
		if (!enteringObject.CompareTag("PlayerDetector")) return;

		fire.SetInteractionEnabled(false);
		roomEntryway.OnEntry -= OnRoomEntered;
		roomEntryway.enabled = false;
		ModMain.SetCondition("SEARCHED_GREAT_DOOR", true);
	}

	private void EnableFire(string condition, bool value)
	{
		fire.SetInteractionEnabled(true);
		fire.SetState(Campfire.State.SMOLDERING);
	}

	private void OnNomaiMove(QuantumObject quantumObject, bool collapsed) =>
		StartCoroutine(OnNpcMove(quantumObject, collapsed, StartNomaiMusic));

	private void OnGhirdMove(QuantumObject quantumObject, bool collapsed) =>
		StartCoroutine(OnNpcMove(quantumObject, collapsed, StartGhirdMusic));

	private IEnumerator OnNpcMove(QuantumObject quantumObject, bool collapsed, Action action)
	{
		var npc = (QuantumNPC)quantumObject;
		if (!collapsed || npc.CurrentLocation != QuantumTarget.Fire) yield break;
		yield return new WaitUntil(npc.IsVisible);
		action();
	}

	private void StartDoorkeeperMusic()
	{
		_musicStartTime = Time.time;
		doorkeeperInstrument.FadeIn(1, fadeFromNothing: true);
		nomaiInstruments.ForEach(instrument => instrument.Play());
		ghirdInstruments.ForEach(instrument => instrument.Play());
		Invoke(nameof(NomaiToFire), 8);
	}

	private void StartNomaiMusic()
	{
		if (_nomaiMusicStarted) return;
		_nomaiMusicStarted = true;
		nomaiInstruments.ForEach(instrument => instrument.FadeIn(4, fadeFromNothing: true));
		Invoke(nameof(GhirdToFire), 8);
	}

	private void StartGhirdMusic()
	{
		if (_ghirdMusicStarted) return;
		_ghirdMusicStarted = true;
		ghirdInstruments.ForEach(instrument => instrument.FadeIn(4, fadeFromNothing: true));

		var timeSinceStart = Time.time - _musicStartTime;
		var padLength = pad.clip.length / 2; // audio doesn't actually start until half way through;
		var delayBeforeStart = timeSinceStart < 2 * padLength ? padLength : 0f;
		delayBeforeStart += padLength - (timeSinceStart % padLength);
		StartCoroutine(TheEnd(delayBeforeStart));
	}

	private void OnFireStateChanged(Campfire changedFire)
	{
		if (!_fireLit && fire.GetState() == Campfire.State.LIT) OnFireLit();
	}

	private void OnFireLit()
	{
		_fireLit = true;
		doorkeeper.SetInteractionEnabled(false);
		Invoke(nameof(StartDoorkeeperMusic), 5);
	}

	private void NomaiToFire()
	{
		ModMain.SetCondition("NOMAI_TO_FIRE", true);
	}

	private void GhirdToFire()
	{
		ModMain.SetCondition("GHIRD_TO_FIRE", true);
	}

	private IEnumerator TheEnd(float delayBeforeStart)
	{
		yield return new WaitForSeconds(delayBeforeStart);
		
		var padLength = pad.clip.length / 2; // audio doesn't actually start until half way through;
		pad.time = padLength;
		pad.loop = false;
		pad.Play();
		doorkeeperInstrument.loop = false;
		nomaiInstruments.ForEach(instrument => instrument.loop = false);
		ghirdInstruments.ForEach(instrument => instrument.loop = false);
		
		yield return new WaitForSeconds(padLength - 10);
		
		pad.FadeOut(10);
		doorkeeperInstrument.FadeOut(10);
		nomaiInstruments.ForEach(instrument => instrument.FadeOut(10));
		ghirdInstruments.ForEach(instrument => instrument.FadeOut(10));

		yield return new WaitForSeconds(2);

        ReferenceLocator.GetCreditsSong().FadeIn(1f, true, false, 1f);

        yield return new WaitForSeconds(7);
		
		ModMain.TriggerEnd();
	}
}