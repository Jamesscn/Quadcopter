using UnityEngine;

public class Motor {

	private float TorqueConstant, MotorConstant, Resistance;

	public Motor(float torqueConstant, float motorConstant, float resistance) {
		TorqueConstant = torqueConstant;
		MotorConstant = motorConstant;
		Resistance = resistance;
	}

	public float CalculateTorque(float voltage, float angularVelocity) {
		if(voltage <= 0.0F) {
			return 0.0F;
		}
		return TorqueConstant / Resistance * (voltage - MotorConstant * angularVelocity);
	}

}

public class Quadcopter : MonoBehaviour {

	//Front Left, Front Right, Back Right, Back Left
	public Rigidbody[] Blades;
	public Rigidbody Frame;

	[Range(0.0F, 6.0F)]
	public float UserVoltage;

	private float[] voltages = {0.0F, 0.0F, 0.0F, 0.0F};
	
	private Motor PittmanMotor = new Motor(0.007F, 0.005F, 1.9F);
	private float ThrustCoefficient = 0.0000005F;

    void Update() {
		voltages = new float[] {UserVoltage, UserVoltage, UserVoltage, UserVoltage};
		Motor[] motors = {PittmanMotor, PittmanMotor, PittmanMotor, PittmanMotor};
		ApplyDynamics(voltages, motors);
    }

	void ApplyDynamics(float[] voltages, Motor[] motors) {
		for(int i = 0; i < 4; i++) {
			float angularVelocity = 10 * Blades[i].angularVelocity.magnitude;
			float torque = motors[i].CalculateTorque(voltages[i], angularVelocity);
			float thrust = ThrustCoefficient * Mathf.Pow(angularVelocity, 2);
			Vector3 thrustVector = Blades[i].transform.up;
			Vector3 torqueVector = Blades[i].transform.up;
			if(i % 2 == 1) {
				torqueVector = -Blades[i].transform.up;
			}
			Blades[i].AddTorque(torque * torqueVector);
			Blades[i].AddForce(thrust * thrustVector);
		}
	}

}
