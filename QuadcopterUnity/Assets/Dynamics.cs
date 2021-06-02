using UnityEngine;
using System;

public class Dynamics : MonoBehaviour {

	public Rigidbody Body;
	public GameObject[] RotorObjects;
	public Vector3[] RotorPositions;

	Rotor[] Rotors = {
		new Rotor(),
		new Rotor(),
		new Rotor(),
		new Rotor()
	};

	public double ThrustCoefficient;
	
	double AirDensity = 1.2256D;
	float SpawnRange = 6.0F;

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
		Body.transform.localPosition = new Vector3(UnityEngine.Random.Range(-SpawnRange, SpawnRange), UnityEngine.Random.Range(-SpawnRange, SpawnRange), UnityEngine.Random.Range(-SpawnRange, SpawnRange));
		//try random rotations
		Body.transform.localRotation = Quaternion.identity;
		Body.velocity = Vector3.zero;
		Body.angularVelocity = Vector3.zero;
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
		double yawTorque = 0.0D;
		for(int i = 0; i < 4; i++) {
			Rotors[i].UpdateMotor(MotorVoltages[i]);
			Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);
			double liftMagnitude = ThrustCoefficient * AirDensity * Math.Pow(Rotors[i].AngularVelocity, 2) * Math.Pow(Rotors[i].Diameter, 4);
			//ignore drag for now
			//double dragMagnitude = TorqueCoefficient * AirDensity * Math.Pow(Rotors[i].AngularVelocity, 2) * Math.Pow(Rotors[i].Diameter, 5);
			double torqueMagnitude = Rotors[i].Torque;

			double rotorDirection = 0.0D;
			if(Rotors[i].AngularVelocity > 0) {
				rotorDirection = 1.0D;
			} else if(Rotors[i].AngularVelocity < 0) {
				rotorDirection = -1.0D;
			}

			yawTorque += TorqueDirections[i] * torqueMagnitude;			
			Body.AddForceAtPosition((float)(rotorDirection * liftMagnitude) * Body.transform.up, WorldRotorPosition);
		}
		Body.AddTorque((float)yawTorque * Body.transform.up);
    }

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		double yawTorque = 0.0D;
		for(int i = 0; i < 4; i++) {
			double liftMagnitude = ThrustCoefficient * AirDensity * Math.Pow(Rotors[i].AngularVelocity, 2) * Math.Pow(Rotors[i].Diameter, 4);
			double torqueMagnitude = Rotors[i].Torque;

			double rotorDirection = 0.0D;
			if(Rotors[i].AngularVelocity > 0) {
				rotorDirection = 1.0D;
			} else if(Rotors[i].AngularVelocity < 0) {
				rotorDirection = -1.0D;
			}

			yawTorque += TorqueDirections[i] * torqueMagnitude;			
			Vector3 WorldRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);

			Gizmos.DrawRay(WorldRotorPosition, 0.1F * (float)(rotorDirection * liftMagnitude) * Body.transform.up);
		}
		Gizmos.color = Color.green;
		Gizmos.DrawRay(Body.transform.position, (float)yawTorque * Body.transform.up);
	}

}