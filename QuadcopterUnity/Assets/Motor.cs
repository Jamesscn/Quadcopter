using UnityEngine;

public class Motor {

	//A set of constants obtained from the datasheet of the motor.
	float TorqueConstant;
	float MotorConstant;
	float ArmatureResistance;
	float ArmatureInductance;
	
	//The current is stored in this variable between time steps.
	float Current;

	//Constructor class that is used to initialize all the constants.
	public Motor() {
		//Constants obtained from the Pittman 6216 6V motor datasheet.
		TorqueConstant = 0.007F;
		MotorConstant = 0.005F;
		ArmatureResistance = 1.9F;
		ArmatureInductance = 0.00101F;

		//Initially, the motor consumes no current.
		Current = 0.0F;
	}

	//ComputeTorque obtains the torque using the motor armature model.
	public float ComputeTorque(float VoltageDifference, float AngularVelocity) {
		//The new current is calculated with the derived equation.
		Current = (VoltageDifference * Time.fixedDeltaTime - MotorConstant * AngularVelocity * Time.fixedDeltaTime + ArmatureInductance * Current) / (ArmatureResistance * Time.fixedDeltaTime + ArmatureInductance);

		//The torque is calculated and returned.
		float Torque = TorqueConstant * Current;
		return Torque;
	}

	//Resets the motor to its initial conditions.
	public void ResetMotor() {
		Current = 0.0F;
	}

}