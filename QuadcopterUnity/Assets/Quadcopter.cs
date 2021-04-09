using UnityEngine;

public class Quadcopter : MonoBehaviour {

	//This is an array of rigidbodies of all of the rotors of the quadcopter.
	//The order of the rotors in this array is Front Left, Front Right, Back Right and Back Left.
	public Rigidbody[] Rotors;

	//The body of the quadcopter.
	public Rigidbody Body;

	//This variable is a debugging slider for the voltage difference across both terminals of the motor.
	[Range(0.0F, 6.0F)]
	public float VoltageSlider;

	//This array holds four instances of the Motor class, one for each motor in the same order as the Rotors array.
	//Each motor has the same characteristics and constants, obtained from the Pittman 6216 6V motor datasheet.
	private Motor[] Motors = {
		new Motor(0.007F, 0.005F, 1.9F, 0.00101F),
		new Motor(0.007F, 0.005F, 1.9F, 0.00101F),
		new Motor(0.007F, 0.005F, 1.9F, 0.00101F),
		new Motor(0.007F, 0.005F, 1.9F, 0.00101F)
	};

	//These are experimentally determined proportionality constants.
	private float ThrustCoefficient = 0.01F;
	private float TorqueCoefficient = 0.0001F;

	//This is the typical density of air.
	private float AirDensity = 1.2256F;

	//This is the diameter of all four rotors.
	private float RotorDiameter = 0.5094F;

	//This function is called every time the physics engine updates.
	void FixedUpdate() {
		//This loop iterates over all four motors and rotors.
		for(int i = 0; i < 4; i++) {
			//The angular velocity of the current rotor is found.
			float angularVelocity = Rotors[i].angularVelocity.magnitude;

			//The torque, lift and drag of the rotor are calculated.
			//All four motors are given the voltage of the slider.
			float torqueMagnitude = Motors[i].ComputeTorque(VoltageSlider, angularVelocity);
			float liftMagnitude = ComputeLift(angularVelocity);
			float dragMagnitude = ComputeDrag(angularVelocity);

			//The following variables are unit vectors that point in the directions of the torque, thrust and drag of the rotor. Drag goes against the angular velocity of the rotor.
			Vector3 torqueVector = Rotors[i].transform.up;
			Vector3 liftVector = Rotors[i].transform.up;
			Vector3 dragVector = -Rotors[i].angularVelocity.normalized;

			//If the rotor rotates clockwise, then the torque vector must point downwards instead of up.
			if(i % 2 == 1) {
				torqueVector = -Rotors[i].transform.up;
			}

			//The unit vectors are scaled up by their corresponding magnitudes, and applied onto the rotors with the help of the physics engine.
			Rotors[i].AddTorque(torqueMagnitude * torqueVector);
			Rotors[i].AddTorque(dragMagnitude * dragVector);
			Rotors[i].AddForce(liftMagnitude * liftVector);

			//The counteracting torques due to Newton's third law have to be added to the body.
			Body.AddTorque(-torqueMagnitude * torqueVector);
			Body.AddTorque(-dragMagnitude * dragVector);
		}
    }

	//This function applies the aerodynamic lift equation of a propeller.
	float ComputeLift(float angularVelocity) {
		return ThrustCoefficient * AirDensity * Mathf.Pow(angularVelocity, 2) * Mathf.Pow(RotorDiameter, 4);
	}

	//This function applies the aerodynamic drag equation of a propeller.
	float ComputeDrag(float angularVelocity) {
		return TorqueCoefficient * AirDensity * Mathf.Pow(angularVelocity, 2) * Mathf.Pow(RotorDiameter, 5);
	}

}