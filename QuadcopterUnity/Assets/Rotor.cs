using UnityEngine;
using System;

public class Rotor {

	public double Mass, Diameter, Width, MomentOfInertia;
	public double TorqueConstant, MotorConstant, ArmatureResistance, ArmatureInductance, ViscousFriction;
	public double Current, Torque, AngularAcceleration, AngularVelocity, AngularDisplacement;

	public Rotor() {
		Mass = 0.161D;
		Diameter = 0.711D;
		Width = 0.044D;
		MomentOfInertia = 0.003408D;

		ArmatureResistance = 0.06D;
		ArmatureInductance = 0.0000455D;
		TorqueConstant = 0.079D;
		MotorConstant = TorqueConstant / Math.Sqrt(ArmatureResistance);
		ViscousFriction = 0.1D;
		
		ResetRotor();
	}

	public void ResetRotor() {
		Current = 0.0D;
		Torque = 0.0D;
		AngularAcceleration = 0.0D;
		AngularVelocity = 0.0D;
		AngularDisplacement = 0.0D;
	}

	public void UpdateMotor(double VoltageDifference) {
		Current = (VoltageDifference * Time.fixedDeltaTime - MotorConstant * AngularVelocity * Time.fixedDeltaTime + ArmatureInductance * Current) / (ArmatureResistance * Time.fixedDeltaTime + ArmatureInductance);
		Torque = TorqueConstant * Current;
		AngularVelocity = (Torque + MomentOfInertia * AngularVelocity / Time.fixedDeltaTime) / (MomentOfInertia / Time.fixedDeltaTime + ViscousFriction);
		AngularDisplacement += AngularVelocity * Time.fixedDeltaTime;
		int Turns = (int)(AngularDisplacement / (2 * Math.PI));
		AngularDisplacement -= Turns * 2 * Math.PI;
	}

}