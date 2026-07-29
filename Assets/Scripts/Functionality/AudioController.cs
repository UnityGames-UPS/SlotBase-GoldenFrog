using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
  [SerializeField] private AudioSource bg_adudio;
  [SerializeField] private AudioSource audioPlayer_wl;
  [SerializeField] private AudioSource audioPlayer_button;
  [SerializeField] private AudioSource audioPlayer_Spin;

  [Header("clips")]
  [SerializeField] private AudioClip SpinButtonClip;
  [SerializeField] private AudioClip SpinClip;
  [SerializeField] private AudioClip Button;
  [SerializeField] private AudioClip Win_Audio;
  [SerializeField] private AudioClip NormalBg_Audio;

  private List<AudioSource> focusManagedSources;
  private readonly Dictionary<AudioSource, bool> preFocusMuteState = new Dictionary<AudioSource, bool>();
  private bool isForceMuted = false;

  private void Awake()
  {
    playBgAudio();
    focusManagedSources = new List<AudioSource> { bg_adudio, audioPlayer_wl, audioPlayer_button, audioPlayer_Spin };
  }

  internal void PlayWLAudio(string type)
  {

    switch (type)
    {
      case "win":
        audioPlayer_wl.clip = Win_Audio;
        break;
    }
    StopWLAudio();
    audioPlayer_wl.Play();
  }

  internal void PlaySpinAudio()
  {
    if (audioPlayer_Spin)
    {
      audioPlayer_Spin.clip = SpinClip;
      audioPlayer_Spin.Play();
    }
  }

  internal void StopSpinAudio()
  {
    if (audioPlayer_Spin) audioPlayer_Spin.Stop();
  }

  internal void CheckFocusFunction(bool focus, bool IsSpinning)
  {
    SetMuteAll(!focus);
    if (focus && !IsSpinning)
    {
      StopWLAudio();
      audioPlayer_Spin.Stop();
    }
  }

  // Focus-driven — called from both the WebGL/JS OnFocusChanged path and OnApplicationFocus.
  // Never force-unmutes: on regained focus, each source is restored to whatever .mute it had
  // right before it was force-muted (i.e. the user's real setting), never a hardcoded false.
  internal void SetMuteAll(bool forceMute)
  {
    if (forceMute == isForceMuted) return;
    isForceMuted = forceMute;

    foreach (var source in focusManagedSources)
    {
      if (source == null) continue;
      if (forceMute)
      {
        preFocusMuteState[source] = source.mute;
        source.mute = true;
      }
      else
      {
        source.mute = preFocusMuteState.TryGetValue(source, out bool prevMuted) ? prevMuted : source.mute;
      }
    }
  }



  internal void playBgAudio()
  {
    if (bg_adudio)
    {
      bg_adudio.clip = NormalBg_Audio;
      bg_adudio.Play();
    }
  }

  internal void PlayButtonAudio(string type = "default")
  {
    if (type == "spin")
      audioPlayer_button.clip = SpinButtonClip;
    else
      audioPlayer_button.clip = Button;
    audioPlayer_button.Play();
  }

  internal void StopWLAudio()
  {
    audioPlayer_wl.Stop();
    audioPlayer_wl.loop = false;
  }

  internal void StopButtonAudio()
  {
    audioPlayer_button.Stop();
  }

  internal void StopBgAudio()
  {
    bg_adudio.Stop();
  }

  internal void ToggleMute(bool toggle, string type = "all")
  {
    switch (type)
    {
      case "bg":
        SetSourceMute(bg_adudio, toggle);
        break;
      case "button":
        SetSourceMute(audioPlayer_button, toggle);
        SetSourceMute(audioPlayer_Spin, toggle);
        break;
      case "wl":
        SetSourceMute(audioPlayer_wl, toggle);
        break;
      case "all":
        SetSourceMute(audioPlayer_wl, toggle);
        SetSourceMute(bg_adudio, toggle);
        SetSourceMute(audioPlayer_button, toggle);
        break;
    }
  }

  // The user's own mute/unmute control always wins immediately: it writes .mute directly
  // (not layered behind isForceMuted), and also updates the stored pre-focus value so a
  // later focus-regain restore doesn't clobber this newer choice.
  private void SetSourceMute(AudioSource source, bool mute)
  {
    if (source == null) return;
    source.mute = mute;
    if (isForceMuted) preFocusMuteState[source] = mute;
  }
}
