using UnityEngine;
using System;

public class Rotor {

	public double Diameter, MomentOfInertia, TorqueConstant, BackEMFConstant, ArmatureResistance, ArmatureInductance, ViscousFriction, LiftCoefficient, DragCoefficient, AirDensity;
	public double ElectromotiveForce, Current, ElectromagneticTorque, NetTorque, LiftForce, DragTorque, AngularVelocity, AngularDisplacement;

	public Rotor() {
		Diameter = 0.711D;
		MomentOfInertia = 0.003408D;
		TorqueConstant = 0.079D;
		BackEMFConstant = 0.0796D;
		ArmatureResistance = 0.06D;
		ArmatureInductance = 0.0000455D;
		ViscousFriction = 0.0001D;
		LiftCoefficient = 0.0015D;
		DragCoefficient = 0.00004D;
		AirDensity = 1.2256D;
		ResetRotor();
	}

	public void ResetRotor() {
		ElectromotiveForce = 0.0D;
		Current = 0.0D;
		ElectromagneticTorque = 0.0D;
		NetTorque = 0.0D;
		LiftForce = 0.0D;
		DragTorque = 0.0D;
		AngularVelocity = 0.0D;
		AngularDisplacement = 0.0D;
	}

	public void UpdateRotor(double VoltageDifference) {
		double DeltaTime = Time.fixedDeltaTime;
		Current += (VoltageDifference - ElectromotiveForce - ArmatureResistance * Current) * DeltaTime / ArmatureInductance;
		ElectromagneticTorque = TorqueConstant * Current;
		NetTorque = ElectromagneticTorque - DragTorque;
		AngularVelocity += (NetTorque - ViscousFriction * AngularVelocity) * DeltaTime / MomentOfInertia;
		ElectromotiveForce = BackEMFConstant * AngularVelocity;
		LiftForce = LiftCoefficient * AirDensity * Math.Pow(AngularVelocity, 2) * Math.Pow(Diameter, 4);
		DragTorque = DragCoefficient * AirDensity * Math.Pow(AngularVelocity, 2) * Math.Pow(Diameter, 5);
		if(AngularVelocity < 0) {
			DragTorque = -DragTorque;
			LiftForce = -LiftForce;
		}

		AngularDisplacement += AngularVelocity * DeltaTime;
		int Turns = (int)(AngularDisplacement / (2 * Math.PI));
		AngularDisplacement -= Turns * 2 * Math.PI;
	}

}