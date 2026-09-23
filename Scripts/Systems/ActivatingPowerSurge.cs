using System.Collections;
using UnityEngine;

public class ActivatingPowerSurge : MonoBehaviour
{
    [Header("Effects")]
    public ParticleSystem particle;
    public AudioSource audioSource;

    [Header("Activation Settings")]
    public float spawnerDelay = 3f;
    public float audioFullVolumeDuration = 7f;
    public float audioFadeDuration = 4f;

    private Movement player;
    private bool hasActivated;

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated ||
            !other.CompareTag("Player"))
        {
            return;
        }

        player = other.GetComponent<Movement>();

        if (player == null)
            return;

        hasActivated = true;

        StartCoroutine(
            ActivatePowerSurge()
        );
    }

    private IEnumerator ActivatePowerSurge()
    {
        if (player.powerSurgeActivated)
            yield break;

        PlayEffects();

        yield return new WaitForSeconds(
            spawnerDelay
        );

        player.powerSurgeActivated = true;
    }

    private void PlayEffects()
    {
        if (particle != null)
        {
            particle.Play();
        }

        if (audioSource != null)
        {
            StartCoroutine(
                PlayAndFadeAudio()
            );
        }
    }

    private IEnumerator PlayAndFadeAudio()
    {
        audioSource.volume = 1f;
        audioSource.Play();

        yield return new WaitForSeconds(
            audioFullVolumeDuration
        );

        float timer = 0f;

        while (timer < audioFadeDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume =
                Mathf.Lerp(
                    1f,
                    0f,
                    timer / audioFadeDuration
                );

            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();

        if (particle != null)
        {
            particle.Stop();
        }
    }

    public void ResetPowerSurge()
    {
        StopAllCoroutines();

        hasActivated = false;

        if (player != null)
        {
            player.powerSurgeActivated = false;
        }

        if (particle != null)
        {
            particle.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 1f;
        }
    }
}
