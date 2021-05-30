using UnityEngine;
using System;

public class Rotor {

	public double Mass, Diameter, Width, MomentOfInertia;
	public double TorqueConstant, MotorConstant, ArmatureResistance, ArmatureInductance;
	
	public double Current, Torque, AngularAcceleration, AngularVelocity, AngularDisplacement;

	public Rotor() {
		Mass = 0.161D;
		Diameter = 0.711D;
		Width = 0.044D;
		MomentOfInertia = Mass * (Math.Pow(Diameter, 2) + Math.Pow(Width, 2)) / 12.0D;

		ArmatureResistance = 0.06D;
		ArmatureInductance = 0.0000455D;
		TorqueConstant = 0.079D;
		MotorConstant = TorqueConstant / Math.Sqrt(ArmatureResistance);
		
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
		AngularAcceleration = Torque / MomentOfInertia;
		AngularVelocity += AngularAcceleration * Time.fixedDeltaTime;
		AngularDisplacement += AngularVelocity * Time.fixedDeltaTime;
		int Turns = (int)(AngularDisplacement / (2 * Math.PI));
		AngularDisplacement -= Turns * 2 * Math.PI;
	}

}