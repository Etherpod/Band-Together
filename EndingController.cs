using System;
using System.Collections;
using BandTogether.Quantum;
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
	private bool _endingInProgress = false;
	private bool _musicWasPlaying = false;
	private float _musicStartTime = 0;
	private float _musicPauseTime = 0;
	private float _durationToStartFinale = float.MaxValue;
	private float _durationToEndFinale = float.MaxValue;
	private FinaleState _finaleState = FinaleState.Waiting;

	private void Awake()
	{
		fire.OnCampfireStateChange += OnFireStateChanged;
		roomEntryway.OnEntry += OnRoomEntered;
		nomai.ForEach(npc => npc.OnPostCollapse += OnNomaiMove);
		ghird.ForEach(npc => npc.OnPostCollapse += OnGhirdMove);
		ModMain.AddDialogueConditionListener(EnableFire, "BT_DOORKEEPER_TO_FIRE");

		doorkeeperInstrument.SetLocalVolume(0);
		nomaiInstruments.ForEach(instrument => instrument.SetLocalVolume(0));
		ghirdInstruments.ForEach(instrument => instrument.SetLocalVolume(0));

		enabled = false;
	}

	private void OnDestroy()
	{
		fire.OnCampfireStateChange -= OnFireStateChanged;
		roomEntryway.OnEntry -= OnRoomEntered;
		nomai.ForEach(npc => npc.OnPostCollapse -= OnNomaiMove);
		ghird.ForEach(npc => npc.OnPostCollapse -= OnGhirdMove);
		ModMain.RemoveDialogueConditionListener(EnableFire, "BT_DOORKEEPER_TO_FIRE");
	}

	private void Update()
	{
		if (!_endingInProgress)
		{
			enabled = false;
			ModMain.WriteDebugMessage("ending not in progress. disabling updates");
			return;
		}

		if (_musicWasPlaying != doorkeeperInstrument.isPlaying)
		{
			if (_musicWasPlaying)
			{
				_musicPauseTime = Time.time;
				ModMain.WriteDebugMessage("pausing music");
			}
			else
			{
				_musicStartTime += Time.time - _musicPauseTime;
				ModMain.WriteDebugMessage($"unpausing music after {Time.time - _musicPauseTime}s");
			}

			_musicWasPlaying = !_musicWasPlaying;
			return;
		}

		var timeSinceMusicStart = Time.time - _musicStartTime;
		if (timeSinceMusicStart < _durationToStartFinale) return;

		switch (_finaleState)
		{
			case FinaleState.Waiting:
				pad.loop = false;
				doorkeeperInstrument.loop = false;
				nomaiInstruments.ForEach(instrument => instrument.loop = false);
				ghirdInstruments.ForEach(instrument => instrument.loop = false);
				pad.time = doorkeeperInstrument.time;
				pad.Play();

				ModMain.WriteDebugMessage($"finale started. fading out in {_durationToEndFinale - timeSinceMusicStart}s");
				_finaleState = FinaleState.Start;
				break;

			case FinaleState.Start:
				if (timeSinceMusicStart < _durationToEndFinale - 5) return;

				pad.FadeOut(5);
				doorkeeperInstrument.FadeOut(5);
				nomaiInstruments.ForEach(instrument => instrument.FadeOut(5));
				ghirdInstruments.ForEach(instrument => instrument.FadeOut(5));

                ReferenceLocator.GetCreditsSong().FadeIn(1f, true, false, 1f);

                ModMain.WriteDebugMessage($"finale fading out");
				_finaleState = FinaleState.FadeOut;
				break;

			case FinaleState.FadeOut:
				if (timeSinceMusicStart < _durationToEndFinale + 2) return;

				ModMain.TriggerEnd();

				ModMain.WriteDebugMessage($"triggered credits");
				_finaleState = FinaleState.Credits;
				break;

			case FinaleState.Credits:
				enabled = false;
				ModMain.WriteDebugMessage($"disabling ending updates");
				break;
		}
	}

	private void OnRoomEntered(GameObject enteringObject)
	{
		if (!enteringObject.CompareTag("PlayerDetector")) return;

		fire.SetInteractionEnabled(false);
		roomEntryway.OnEntry -= OnRoomEntered;
		roomEntryway.enabled = false;
		ModMain.SetCondition("BT_SEARCHED_GREAT_DOOR", true);
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
		doorkeeperInstrument.FadeIn(8, fadeFromNothing: true);
		_musicWasPlaying = true;
		// nomaiInstruments.ForEach(instrument => instrument.Play());
		// ghirdInstruments.ForEach(instrument => instrument.Play());
		Invoke(nameof(NomaiToFire), 8);
	}

	private void StartNomaiMusic()
	{
		if (_nomaiMusicStarted) return;
		_nomaiMusicStarted = true;
		nomaiInstruments.ForEach(instrument =>
		{
			instrument.time = doorkeeperInstrument.time;
			instrument.FadeIn(4, fadeFromNothing: true);
		});
		Invoke(nameof(GhirdToFire), 8);
	}

	private void StartGhirdMusic()
	{
		if (_ghirdMusicStarted) return;
		_ghirdMusicStarted = true;
		ghirdInstruments.ForEach(instrument =>
		{
			instrument.time = doorkeeperInstrument.time;
			instrument.FadeIn(4, fadeFromNothing: true);
		});

		var timeSinceStart = Time.time - _musicStartTime;
		var padLength = pad.clip.length;

		// ensure we wait until at least one loop has completed
		_durationToStartFinale = Math.Max(1f, (float)Math.Floor(timeSinceStart / padLength));
		// end the finale at the end of the next loop
		_durationToEndFinale = _durationToStartFinale + 1f;
		// we should start in the middle of a loop, as that is when the pad begins
		// and this way we guarantee that no instruments are cut off at the boundary
		_durationToStartFinale += 0.5f;
		// multiply by pad length to compute the actual final duration to wait
		_durationToStartFinale *= padLength;
		_durationToEndFinale *= padLength;
		
		ModMain.WriteDebugMessage($"starting finale in {_durationToStartFinale - timeSinceStart}s");
	}

	private void OnFireStateChanged(Campfire changedFire)
	{
		if (!_fireLit && fire.GetState() == Campfire.State.LIT) OnFireLit();
	}

	private void OnFireLit()
	{
		_fireLit = true;
		doorkeeper.SetInteractionEnabled(false);
		_endingInProgress = true;
		enabled = true;
		Invoke(nameof(StartDoorkeeperMusic), 5f);
	}

	private void NomaiToFire()
	{
		ModMain.SetCondition("NOMAI_TO_FIRE", true);
	}

	private void GhirdToFire()
	{
		ModMain.SetCondition("GHIRD_TO_FIRE", true);
	}

	private enum FinaleState
	{
		Waiting,
		Start,
		FadeOut,
		Credits,
	}
}