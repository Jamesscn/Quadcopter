using UnityEngine;
using System;

/**
The Rotor class simulates both the electrical effects of the M10 motor and the aerodynamic effects of the E5000 rotor by DJI.
*/
public class Rotor {

	public double Diameter, MomentOfInertia, TorqueConstant, BackEMFConstant, ArmatureResistance, ArmatureInductance, ViscousFriction, LiftCoefficient, DragCoefficient, AirDensity;
	public double ElectromotiveForce, Current, ElectromagneticTorque, NetTorque, LiftForce, DragTorque, AngularVelocity, AngularDisplacement;
	public int EulerSteps;

	//The constructor sets the following values, which are mostly parameters obtained either from datasheets or by experimenting with the simulation.
	public Rotor() {
		Diameter = 0.711D;
		MomentOfInertia = 0.003408D;
		TorqueConstant = 0.079D;
		BackEMFConstant = 0.0796D;
		ArmatureResistance = 0.06D;
		ArmatureInductance = 0.0000455D;
		ViscousFriction = 0.03408D;
		LiftCoefficient = 0.0026D;
		DragCoefficient = 0.00004D;
		AirDensity = 1.2256D;
		EulerSteps = 100;
		ResetRotor();
	}

	//The following function resets all of the variables of the simulated rotor to be zero.
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

	//The following function simulates the changes in the rotor since the last time it was called. This is meant to be called at the frequency of the physics engine.
	public void UpdateRotor(double InputVoltage) {
		//The following condition stops the rotor if a negative voltage is given, because the quadcopter cannot fly in reverse.
		if(InputVoltage < 0.0D) {
			InputVoltage = 0.0D;
		}
		//Due to precision errors which can lead to undesirable results, Euler's method has to be used to calculate the change in the model since the previous time step. The number of steps taken by Euler's method is defined by EulerSteps.
		//As a sidenote, decreasing the time step of the physics engine leads to slower training from the reinforcement learning agents, so this is done instead.
		double DeltaTime = Time.fixedDeltaTime / EulerSteps;
		for(int i = 0; i < EulerSteps; i++) {
			Current += (InputVoltage - ElectromotiveForce - ArmatureResistance * Current) * DeltaTime / ArmatureInductance;
			ElectromagneticTorque = TorqueConstant * Current;
			NetTorque = ElectromagneticTorque - DragTorque;
			AngularVelocity += (NetTorque - ViscousFriction * AngularVelocity) * DeltaTime / MomentOfInertia;
			ElectromotiveForce = BackEMFConstant * AngularVelocity;
			LiftForce = LiftCoefficient * AirDensity * Math.Pow(AngularVelocity, 2) * Math.Pow(Diameter, 4);
			DragTorque = DragCoefficient * AirDensity * Math.Pow(AngularVelocity, 2) * Math.Pow(Diameter, 5);
		}
		//The following lines of code are used to determine the current angle of the rotor, which is used while displaying it in the viewport.
		AngularDisplacement += AngularVelocity * DeltaTime * EulerSteps;
		int Turns = (int)(AngularDisplacement / (2 * Math.PI));
		AngularDisplacement -= Turns * 2 * Math.PI;
	}

}