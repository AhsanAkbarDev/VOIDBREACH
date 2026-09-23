using UnityEngine;

public class SharedWeapon : MonoBehaviour
{
    [Header("Weapon")]
    public string weaponName;
    public Transform firePoint;
    public float fireRange;
    public float damage;
    public float fireRate;

    [Header("Shot Settings")]
    public int pelletCount = 1;
    public float spread = 0f;

    [Header("Ammo")]
    public int magazine;
    public int maxMagazineAmount;
    public int ammoAmount;
    public bool hasReloaded = true;

    [Header("Effects")]
    public ParticleSystem particle;
    public AudioSource audioSource;
    public AudioClip[] clips;

    private float nextTimeToShoot;

    private void Start()
    {
        ammoAmount = magazine;
    }

    public void TryShoot()
    {
        if (!hasReloaded)
            return;

        if (ammoAmount <= 0)
        {
            PlayEmptySound();
            return;
        }

        if (Time.time < nextTimeToShoot)
            return;

        ammoAmount--;

        Shoot();

        nextTimeToShoot =
            Time.time + (1f / fireRate);
    }

    private void Shoot()
    {
        particle.Play();
        audioSource.PlayOneShot(clips[0]);

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 direction =
                CalculateShotDirection();

            if (Physics.Raycast(
                firePoint.position,
                direction,
                out RaycastHit hit,
                fireRange))
            {
                ApplyDamage(hit);
            }
        }
    }

    private Vector3 CalculateShotDirection()
    {
        Vector3 direction = firePoint.forward;

        direction +=
            firePoint.right *
            Random.Range(-spread, spread) / 100f;

        direction +=
            firePoint.up *
            Random.Range(-spread, spread) / 100f;

        direction.Normalize();

        return direction;
    }

    private void ApplyDamage(RaycastHit hit)
    {
        Enemy enemy =
            hit.collider.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Harbinger harbinger =
            hit.collider.GetComponent<Harbinger>();

        if (harbinger != null)
        {
            harbinger.TakeDamage(damage);
        }
    }

    private void PlayEmptySound()
    {
        if (audioSource != null &&
            clips.Length > 1)
        {
            audioSource.PlayOneShot(clips[1]);
        }
    }

    public bool Reload()
    {
        if (ammoAmount >= magazine ||
            maxMagazineAmount <= 0)
        {
            return false;
        }

        hasReloaded = false;

        if (audioSource != null &&
            clips.Length > 2)
        {
            audioSource.PlayOneShot(clips[2]);
        }

        return true;
    }

    // Called when the player's reload animation finishes.
    public void Reloaded()
    {
        int ammoNeeded =
            magazine - ammoAmount;

        int ammoToReload =
            Mathf.Min(
                ammoNeeded,
                maxMagazineAmount
            );

        ammoAmount += ammoToReload;
        maxMagazineAmount -= ammoToReload;

        hasReloaded = true;
    }
}
