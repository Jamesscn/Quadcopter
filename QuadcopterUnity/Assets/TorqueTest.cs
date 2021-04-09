using UnityEngine;

public class TorqueTest : MonoBehaviour {

	public Rigidbody Arm;
	public Rigidbody LeftSpinner;
	public Rigidbody RightSpinner;

    void Update() {
        LeftSpinner.AddTorque(Vector3.up);
		Arm.AddTorque(-2.0F * Vector3.up);
		RightSpinner.AddTorque(Vector3.up);
    }
}
