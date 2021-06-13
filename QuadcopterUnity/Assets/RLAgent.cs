using UnityEngine;
using Unity.MLAgents;

public class RLAgent : Agent {

	public Rigidbody Body;
	public GameObject Target;
	public bool AllowReverseFlight;
	public float MaxVoltage;

	float[] heuristicActions = {-1.0F, -1.0F, -1.0F, -1.0F};

	public Vector3 ComponentNormalize(Vector3 vec) {
		if(vec.magnitude == 0) {
			return vec;
		}
		return vec / Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
	}

	public override void OnEpisodeBegin() {
		gameObject.SendMessage("ResetSimulation");
	}

	public override void OnActionReceived(float[] vectorAction) {
		double[] voltages = new double[4];
		for(int i = 0; i < 4; i++) {
			if(AllowReverseFlight) {
				voltages[i] = ScaleAction(vectorAction[i], -MaxVoltage, MaxVoltage);
			} else {
				voltages[i] = ScaleAction(vectorAction[i], 0, MaxVoltage);
			}
		}
		gameObject.SendMessage("SetVoltages", voltages);
	}

	public override void Heuristic(float[] actionsOut) {
		for(int i = 0; i < 4; i++) {
			actionsOut[i] = heuristicActions[i];
		}
	}

	public void GetHeuristic(float[] actionsOut) {
		heuristicActions = actionsOut;
	}
	
}