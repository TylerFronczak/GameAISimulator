//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource interactionAudioSource;
    [SerializeField] AudioSource musicAudioSource;

    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip cellEdit;
    [SerializeField] AudioClip music;
    [SerializeField] AudioClip sliderTick;

    public void PlayClip_ButtonClick()
    {
        interactionAudioSource.PlayOneShot(buttonClick);
    }

    public void PlayClip_CellEdit()
    {
        interactionAudioSource.PlayOneShot(cellEdit);
    }

    public void PlayClip_SliderTick()
    {
        interactionAudioSource.PlayOneShot(sliderTick);
    }
}
