using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Weapon")]
    public SharedWeapon equippedWeapon;

    [Header("Components")]
    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f ||
            equippedWeapon == null)
        {
            return;
        }

        HandleShooting();
        HandleReloading();
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButton(0))
        {
            equippedWeapon.TryShoot();
        }
    }

    private void HandleReloading()
    {
        if (!Input.GetKeyDown(KeyCode.R))
            return;

        bool reloadStarted =
            equippedWeapon.Reload();

        if (reloadStarted)
        {
            animator.SetTrigger("Reload");
        }
    }

    // Called by an animation event when
    // the reload animation finishes.
    public void FinishedReloading()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.Reloaded();
        }
    }

    public void EquipWeapon(SharedWeapon newWeapon)
    {
        equippedWeapon = newWeapon;
    }
}
