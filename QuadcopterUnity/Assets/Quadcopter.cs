using UnityEngine;

public class Quadcopter : MonoBehaviour {

	//This is an array of rigidbodies of all of the rotors of the quadcopter
	//The order of the rotors is Front Left, Front Right, Back Right and Back Left
	public Rigidbody[] Rotors;

	//The rigidbody for the frame or chassis of the quadcopter is stored in this variable
	public Rigidbody Frame;

	//This variable is the slider for the voltage difference
	[Range(0.0F, 6.0F)]
	public float UserVoltage;

	//This array holds the Front Left, Front Right, Back Right and Back Left motor classes in that order
	//Each motor has the same characteristics and constants
	private Motor[] Motors = {
		new Motor(0.007F, 0.005F, 1.9F),
		new Motor(0.007F, 0.005F, 1.9F),
		new Motor(0.007F, 0.005F, 1.9F),
		new Motor(0.007F, 0.005F, 1.9F)
	};

	//This is a proportionality value for the lift, determined experimentally
	private float ThrustCoefficient = 0.0001F;

	//This is the typical density of air at 15 degrees celcius
	private float AirDensity = 1.225F;

	//This is the diameter of the rotors
	private float RotorDiameter = 0.5094F;

	//This function runs at the frequency of the physics engine
    void FixedUpdate() {
		//This loop iterates over all four motors and rotors
		for(int i = 0; i < 4; i++) {
			//The angular velocity of the current rotor is found
			float angularVelocity = Rotors[i].angularVelocity.magnitude;

			//The torque of the motor and the thrust of the rotor are calculated
			Motors[i].Update(UserVoltage, angularVelocity);
			float torqueMagnitude = Motors[i].GetTorque();
			float thrustMagnitude = CalculateThrust(angularVelocity);

			//These vectors represent the directions of the torque and the thrust of the rotor
			Vector3 thrustVector = Rotors[i].transform.up;
			Vector3 torqueVector = Rotors[i].transform.up;

			//If the rotor rotates clockwise, then the torque vector must point downwards instead of up
			if(i % 2 == 1) {
				torqueVector = -Rotors[i].transform.up;
			}

			//The torque and thrust vectors are sent to the physics engine
			Rotors[i].AddTorque(torqueMagnitude * torqueVector);
			Rotors[i].AddForce(thrustMagnitude * thrustVector);
		}
    }

	//This function applies the aerodynamic lift equation of a propeller to calculate thrust
	float CalculateThrust(float angularVelocity) {
		return ThrustCoefficient * AirDensity * Mathf.Pow(angularVelocity, 2) * Mathf.Pow(RotorDiameter, 4);
	}

}
