using UnityEngine;

public class Motor {

	//A set of constants obtained from the datasheet of the motor.
	private float TorqueConstant;
	private float MotorConstant;
	private float ArmatureResistance;
	private float ArmatureInductance;
	
	//The current is stored in this variable between time steps.
	private float current;

	//Constructor class that is used to initialize all the constants.
	public Motor(float torqueConstant, float motorConstant, float armatureResistance, float armatureInductance) {
		TorqueConstant = torqueConstant;
		MotorConstant = motorConstant;
		ArmatureResistance = armatureResistance;
		ArmatureInductance = armatureInductance;

		//Initially, the motor consumes no current.
		current = 0.0F;
	}

	//ComputeTorque obtains the torque using the motor armature model.
	public float ComputeTorque(float voltageDifference, float angularVelocity) {
		//The new current is calculated with the derived equation.
		current = (voltageDifference * Time.fixedDeltaTime - MotorConstant * angularVelocity * Time.fixedDeltaTime + ArmatureInductance * current) / (ArmatureResistance * Time.fixedDeltaTime + ArmatureInductance);

		//The torque is calculated and returned.
		float torque = TorqueConstant * current;
		return torque;
	}

}