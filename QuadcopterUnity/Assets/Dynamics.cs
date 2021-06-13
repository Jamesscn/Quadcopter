using UnityEngine;
using System;

public class Dynamics : MonoBehaviour {

	public Rigidbody Body;
	public GameObject[] RotorObjects;
	public Vector3[] RotorPositions;
	public float SpawnRadius;
	public float MaxInitialVelocity;
	public float MaxInitialAngularVelocity;
	public bool StartRotated;
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

	public void ResetSimulation() {
		Body.useGravity = false;
		Body.isKinematic = true;
		Body.centerOfMass = Vector3.zero;
		for(int i = 0; i < 4; i++) {
			Rotors[i].ResetRotor();
		}
		if(StartRotated) {
			Body.transform.localRotation = UnityEngine.Random.rotationUniform;
		}
		Body.transform.localPosition = UnityEngine.Random.insideUnitSphere * SpawnRadius;
		Body.velocity = UnityEngine.Random.insideUnitSphere * MaxInitialVelocity;
		Body.angularVelocity = UnityEngine.Random.insideUnitSphere * MaxInitialAngularVelocity;
		MotorVoltages = new double[] {0.0D, 0.0D, 0.0D, 0.0D};
		Body.gameObject.SetActive(false);
		SimulationRestarted = false;
	}

	public void SetVoltages(double[] voltages) {
		MotorVoltages = voltages;
	}

	void Update() {
		for(int i = 0; i < 4; i++) {
			Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);
			RotorObjects[i].transform.position = WorldRotorPosition;
			RotorObjects[i].transform.rotation = Body.transform.rotation;
			RotorObjects[i].transform.RotateAround(WorldRotorPosition, Body.transform.up, (float)(TorqueDirections[i] * Rotors[i].AngularDisplacement * 180.0D / Math.PI));
		}
	}

	void FixedUpdate() {
		if(!SimulationRestarted) {
			Body.gameObject.SetActive(true);
			Body.useGravity = true;
			Body.isKinematic = false;
			SimulationRestarted = true;
		}
		double YawTorque = 0.0D;
		for(int i = 0; i < 4; i++) {
			Rotors[i].UpdateRotor(MotorVoltages[i]);
			Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);
			YawTorque += TorqueDirections[i] * Rotors[i].NetTorque;	
			Body.AddForceAtPosition((float)Rotors[i].LiftForce * Body.transform.up, WorldRotorPosition);
		}
		Body.AddTorque((float)YawTorque * Body.transform.up);
    }

	void OnDrawGizmos() {
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