using UnityEngine;

public class CharacterAiming : MonoBehaviour 
{
    public float turnSpeed = 15;
    public float aimDuration = 0.3f;
    public Transform cameraLookAt;
    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    public bool isAiming;

    Animator animator;
    ActiveWeapon activeWeapon;
    int isAimingParam = Animator.StringToHash("isAiming");
    Vector3 cameraRotation;

    // Start is called before the first frame update
    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        activeWeapon = GetComponent<ActiveWeapon>();
    }

    void Update() {
        isAiming = Input.GetMouseButton(1);
        animator.SetBool(isAimingParam, isAiming);

        var weapon = activeWeapon.GetActiveWeapon();
        if (weapon) {
            weapon.recoil.recoilModifier = isAiming ? 0.3f : 1.0f;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        xAxis.Update(Time.fixedDeltaTime);
        yAxis.Update(Time.fixedDeltaTime);

        cameraRotation.x = yAxis.Value;
        cameraRotation.y = xAxis.Value;
        cameraLookAt.eulerAngles = cameraRotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, cameraRotation.y, 0), turnSpeed * Time.fixedDeltaTime);
    }
}
