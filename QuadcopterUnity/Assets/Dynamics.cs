using UnityEngine;
using System;

/**
The Dynamics class is in charge of simulating the dynamics of the quadcopter. It does this by first creating four instances of the Rotor class to simulate torque and lift on each rotor, then adding these forces onto the body of the quadcopter.

This class also sets the initial parameters of the quadcopter (position, rotation, velocity and angular velocity) at the beginning of each episode, creates the illusion that the rotors are spinning in the simulation and displays several Gizmos that show the magnitude of the forces and yaw while debugging.
*/
public class Dynamics : MonoBehaviour {

	public Rigidbody Body;
	public GameObject[] RotorObjects;
	public Vector3[] RotorPositions;
	public bool RandomStart;
	public bool PIDStart;
	public bool ShowForces;
	Rotor[] Rotors = {
		new Rotor(),
		new Rotor(),
		new Rotor(),
		new Rotor()
	};

	double GizmoScale = 0.1D;
	double[] MotorVoltages = {0.0D, 0.0D, 0.0D, 0.0D};
	double[] TorqueDirections = {1.0D, -1.0D, 1.0D, -1.0D};
	bool SimulationRestarted = false;

	//This function resets the simulation and gives the quadcopter an initial position, rotation, velocity and angular velocity.
	public void ResetSimulation() {
		Body.useGravity = false;
		Body.isKinematic = true;
		Body.centerOfMass = Vector3.zero;
		for(int i = 0; i < 4; i++) {
			Rotors[i].ResetRotor();
		}
		if(RandomStart) {
			float InitialYaw = UnityEngine.Random.Range(-90.0F, 90.0F);
			float InitialPitch = UnityEngine.Random.Range(-45.0F, 45.0F);
			float InitialRoll = UnityEngine.Random.Range(-45.0F, 45.0F);
			Body.transform.localRotation = Quaternion.Euler(InitialPitch, InitialYaw, InitialRoll);
			Body.transform.localPosition = UnityEngine.Random.insideUnitSphere * 5.0F;
		} else {
			if(PIDStart) {
				Body.transform.localRotation = Quaternion.Euler(45, 90, -45);
				Body.transform.localPosition = new Vector3(1, 1, 1).normalized * 5.0F;
			} else {
				Body.transform.localRotation = Quaternion.identity;
				Body.transform.localPosition = Vector3.zero;
			}
		}
		MotorVoltages = new double[] {0.0D, 0.0D, 0.0D, 0.0D};
		Body.gameObject.SetActive(false);
		SimulationRestarted = false;
	}

	//This function is called outside of this class by the controller, with a parameter that sets the voltage differences across each of the motors.
	public void SetVoltages(double[] voltages) {
		MotorVoltages = voltages;
	}

	//This function runs everytime the viewport is updated, and is used to create the illusion that the rotor models are spinning.
	void Update() {
		for(int i = 0; i < 4; i++) {
			Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);
			RotorObjects[i].transform.position = WorldRotorPosition;
			RotorObjects[i].transform.rotation = Body.transform.rotation;
			RotorObjects[i].transform.RotateAround(WorldRotorPosition, Body.transform.up, (float)(TorqueDirections[i] * Rotors[i].AngularDisplacement * 180.0D / Math.PI));
		}
	}

	//This function runs at the frequency of the physics engine, and is used to calculate and apply the forces and torques on the body.
	void FixedUpdate() {
		//The following lines of code are a continuation of ResetSimulation, and are needed to avoid undesired effects such as the velocity of the frame before resetting from being applied.
		if(!SimulationRestarted) {
			Body.gameObject.SetActive(true);
			Body.useGravity = true;
			Body.isKinematic = false;
			SimulationRestarted = true;
			//Body.velocity = UnityEngine.Random.insideUnitSphere * MaxInitialVelocity;
			//Body.angularVelocity = UnityEngine.Random.insideUnitSphere * MaxInitialAngularVelocity;
		}
		//The net yaw torque over the entire body and the forces on each of the rotors are applied to the body of the quadcopter.
		double YawTorque = 0.0D;
		for(int i = 0; i < 4; i++) {
			Rotors[i].UpdateRotor(MotorVoltages[i]);
			Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);
			YawTorque += TorqueDirections[i] * Rotors[i].NetTorque;	
			Body.AddForceAtPosition((float)Rotors[i].LiftForce * Body.transform.up, WorldRotorPosition);
		}
		Body.AddTorque((float)YawTorque * Body.transform.up);
    }

	//The following function is used to show the forces and yaw torque while debugging.
	void OnDrawGizmos() {
		if(ShowForces) {
			Gizmos.color = Color.red;
			double YawTorque = 0.0D;
			for(int i = 0; i < 4; i++) {
				Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);
				YawTorque += TorqueDirections[i] * Rotors[i].NetTorque;			
				Gizmos.DrawRay(WorldRotorPosition, (float)(GizmoScale * Rotors[i].LiftForce) * Body.transform.up);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawRay(Body.transform.position, (float)(GizmoScale * YawTorque) * Body.transform.up);
		}
	}

}