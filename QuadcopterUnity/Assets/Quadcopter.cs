using UnityEngine;
using System;

public class Quadcopter : MonoBehaviour {

	public Rigidbody Body;
	public GameObject[] RotorObjects;
	public Vector3[] RotorPositions;

	Rotor[] Rotors = {
		new Rotor(),
		new Rotor(),
		new Rotor(),
		new Rotor()
	};

	public double ThrustCoefficient, TorqueCoefficient;
	
	double AirDensity = 1.2256D;

	double[] MotorVoltages = {0.0D, 0.0D, 0.0D, 0.0D};

	void Start() {
		Body.centerOfMass = Vector3.zero;
	}

	public void ResetSimulation() {
		for(int i = 0; i < 4; i++) {
			Rotors[i].ResetRotor();
		}
		Body.transform.localPosition = new Vector3(UnityEngine.Random.Range(-2.0F, 2.0F), 0.0F, UnityEngine.Random.Range(-2.0F, 2.0F));
		Body.transform.localRotation = Quaternion.identity;
		Body.velocity = Vector3.zero;
		Body.angularVelocity = Vector3.zero;
		MotorVoltages = new double[]{0.0D, 0.0D, 0.0D, 0.0D};
	}

	public void SetVoltages(double[] voltages) {
		MotorVoltages = voltages;
	}

	void FixedUpdate() {
		for(int i = 0; i < 4; i++) {
			Rotors[i].UpdateMotor(MotorVoltages[i]);
			double liftMagnitude = ThrustCoefficient * AirDensity * Math.Pow(Rotors[i].AngularVelocity, 2) * Math.Pow(Rotors[i].Diameter, 4);
			//ignore drag for now
			//double dragMagnitude = TorqueCoefficient * AirDensity * Math.Pow(Rotors[i].AngularVelocity, 2) * Math.Pow(Rotors[i].Diameter, 5);
			double torqueMagnitude = Rotors[i].Torque;

			float rotorDirection = (float)Math.Sign(Rotors[i].AngularVelocity);
			float torqueDirection = rotorDirection;
			if(i % 2 == 1) {
				torqueDirection = -rotorDirection;
			}

			Vector3 RelativeRotorPosition = Body.transform.TransformPoint(RotorPositions[i]);

			Body.AddForceAtPosition(rotorDirection * (float)liftMagnitude * Body.transform.up, RelativeRotorPosition);
			Body.AddTorque(torqueDirection * (float)torqueMagnitude * Body.transform.up);
			RotorObjects[i].transform.position = RelativeRotorPosition;
			RotorObjects[i].transform.rotation = Body.transform.rotation;
			RotorObjects[i].transform.RotateAround(RelativeRotorPosition, Body.transform.up, (float)(torqueDirection * Rotors[i].AngularDisplacement * 180.0D / Math.PI));
		}
    }

}