using System;
using System.Collections;
using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public enum WeaponSlot {
        Primary = 0,
        Secondary = 1
    }

    public Transform crossHairTarget;
    public Animator rigController;
    public Transform[] weaponSlots;
    public CharacterAiming characterAiming;
    public AmmoWidget ammoWidget;
    public bool isChangingWeapon;

    RaycastWeapon[] equipped_weapons = new RaycastWeapon[2];
    int activeWeaponIndex;
    bool isHolstered = false;

    // Start is called before the first frame update
    void Start()
    {
        RaycastWeapon existingWeapon = GetComponentInChildren<RaycastWeapon>();
        if (existingWeapon) {
            Equip(existingWeapon);
        }
    }

    public bool IsFiring() {
        RaycastWeapon currentWeapon = GetActiveWeapon();
        if (!currentWeapon) {
            return false;
        }
        return currentWeapon.isFiring;
    }

    public RaycastWeapon GetActiveWeapon() {
        return GetWeapon(activeWeaponIndex);
    }

    RaycastWeapon GetWeapon(int index) {
        if (index < 0 || index >= equipped_weapons.Length) {
            return null;
        }
        return equipped_weapons[index];
    }

    // Update is called once per frame
    void Update()
    {
        var weapon = GetWeapon(activeWeaponIndex);
        bool notSprinting = rigController.GetCurrentAnimatorStateInfo(2).shortNameHash == Animator.StringToHash("notSprinting");
        if (weapon && !isHolstered && notSprinting) {
            weapon.UpdateWeapon(Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            ToggleActiveWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SetActiveWeapon(WeaponSlot.Primary);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SetActiveWeapon(WeaponSlot.Secondary);
        }
    }

    public void Equip(RaycastWeapon newWeapon) {
        int weaponSlotIndex = (int)newWeapon.weaponSlot;
        var weapon = GetWeapon(weaponSlotIndex);
        if (weapon) {
            Destroy(weapon.gameObject);
        }
        weapon = newWeapon;
        weapon.raycastDestination = crossHairTarget;
        weapon.recoil.characterAiming = characterAiming;
        weapon.recoil.rigController = rigController;
        weapon.transform.SetParent(weaponSlots[weaponSlotIndex], false);
        equipped_weapons[weaponSlotIndex] = weapon;

        SetActiveWeapon(newWeapon.weaponSlot);

        ammoWidget.Refresh(weapon.ammoCount);
    }

    void ToggleActiveWeapon() {
        bool isHolstered = rigController.GetBool("holster_weapon");
        if (isHolstered) {
            StartCoroutine(ActivateWeapon(activeWeaponIndex));
        } else {
            StartCoroutine(HolsterWeapon(activeWeaponIndex));
        }
    }

    void SetActiveWeapon(WeaponSlot weaponSlot) {
        int holsterIndex = activeWeaponIndex;
        int activateIndex = (int)weaponSlot;

        if (holsterIndex == activateIndex) {
            holsterIndex = -1;
        }

        StartCoroutine(SwitchWeapon(holsterIndex, activateIndex));
    }

    IEnumerator SwitchWeapon(int holsterIndex, int activateIndex) {
        rigController.SetInteger("weapon_index", activateIndex);
        yield return StartCoroutine(HolsterWeapon(holsterIndex));
        yield return StartCoroutine(ActivateWeapon(activateIndex));
        activeWeaponIndex = activateIndex;
    }

    IEnumerator HolsterWeapon(int index) {
        isChangingWeapon = true;
        isHolstered = true;
        var weapon = GetWeapon(index);
        if (weapon) {
            rigController.SetBool("holster_weapon", true);
            do {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }
        isChangingWeapon = false;
    }

    IEnumerator ActivateWeapon(int index) {
        isChangingWeapon = true;
        var weapon = GetWeapon(index);
        if (weapon) {
            rigController.SetBool("holster_weapon", false);
            rigController.Play("equip_" + weapon.weaponName);
            do {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
            isHolstered = false;
        }
        isChangingWeapon = false;
    }
}
