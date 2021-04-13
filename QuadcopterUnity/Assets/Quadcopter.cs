using UnityEngine;

public class Quadcopter : MonoBehaviour {

	//This is an array of rigidbodies of all of the rotors of the quadcopter.
	//The rotors are stored in order of front left, front right, back right and back left.
	public Rigidbody[] Rotors;

	//The body of the quadcopter.
	public Rigidbody Body;

	//This array holds four instances of the Motor class, one for each motor in the same order as the Rotors array.
	Motor[] Motors = {
		new Motor(),
		new Motor(),
		new Motor(),
		new Motor()
	};

	//These are experimentally determined proportionality constants, obtained by trial and error.
	float ThrustCoefficient = 0.01F;
	float TorqueCoefficient = 0.0001F;

	//This is the typical density of air.
	float AirDensity = 1.2256F;

	//This is the diameter of all four rotors.
	float RotorDiameter = 0.5094F;

	//The following variable stores the voltage differences for each of the motors.
	float[] MotorVoltages = {0.0F, 0.0F, 0.0F, 0.0F};

	//This function resets the position, rotation, velocity and angular velocity of the quadcopter.
	public void ResetSimulation() {
		Body.transform.SetPositionAndRotation(new Vector3(0.0F, 0.1F, 0.0F), Quaternion.identity);
		Body.velocity = Vector3.zero;
		Body.angularVelocity = Vector3.zero;
		Body.Sleep();
		for(int i = 0; i < 4; i++) {
			Motors[i].ResetMotor();
			Rotors[i].transform.SetPositionAndRotation(new Vector3(0.0F, 0.1F, 0.0F), Quaternion.identity);
			Rotors[i].velocity = Vector3.zero;
			Rotors[i].angularVelocity = Vector3.zero;
			Rotors[i].Sleep();
		}
	}

	//This function obtains the voltage differences for each of the motors from the controller, in this case the reinforcement learning agent.
	public void SetVoltages(float[] voltages) {
		MotorVoltages = voltages;
	}

	//This function is called every time the physics engine updates.
	void FixedUpdate() {
		//This loop iterates over all four motors and rotors.
		for(int i = 0; i < 4; i++) {
			//The magnitude and unit vector of the angular velocity of the current rotor is found.
			float angularVelocity = Rotors[i].angularVelocity.magnitude;
			Vector3 angularVector = Rotors[i].angularVelocity.normalized;

			//The torque, lift and drag of the rotor are calculated.
			float torqueMagnitude = Motors[i].ComputeTorque(MotorVoltages[i], angularVelocity);
			float liftMagnitude = ComputeLift(angularVelocity);
			float dragMagnitude = ComputeDrag(angularVelocity);

			//The following variables are unit vectors that point in the directions of the torque, lift and drag of the rotor. Drag goes against the angular velocity of the rotor, while lift points in the same direction as the angular velocity vector.
			Vector3 torqueVector = Rotors[i].transform.up;
			Vector3 liftVector = angularVector;
			Vector3 dragVector = -angularVector;

			//If the rotor rotates clockwise, then the torque and lift vectors must point in the other direction.
			if(i % 2 == 1) {
				torqueVector = -torqueVector;
				liftVector = -angularVector;
			}

			//The unit vectors are scaled up by their corresponding magnitudes, and applied onto the rotors with the help of the physics engine.
			Rotors[i].AddTorque(torqueMagnitude * torqueVector);
			Rotors[i].AddTorque(dragMagnitude * dragVector);
			Rotors[i].AddForce(liftMagnitude * liftVector);

			//The counteracting torque due to Newton's third law has to be added to the body.
			Body.AddTorque(-torqueMagnitude * torqueVector);
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