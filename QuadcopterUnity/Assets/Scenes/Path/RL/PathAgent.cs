using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PathAgent : RLAgent {

    public GameObject Start, End;
    
    Vector3 CurrentTarget;
    
    public override void CollectObservations(VectorSensor sensor) {

    }

    void FixedUpdate() {
        CurrentTarget = Vector3.Lerp(Start.transform.position, End.transform.position, StepCount / MaxStep);
        Vector3 differenceVector = CurrentTarget - Body.transform.position;
        float distance = differenceVector.magnitude;
        float maxDistance = 14.0F;
		float distancePower = 0.5F;
        float spinImportance = 0.02F;
        float distanceReward = Mathf.Max(0.0F, 1.0F - Mathf.Pow(distance / maxDistance, distancePower));
        float spinPenalty = -spinImportance * Body.angularVelocity.magnitude;
		AddReward((distanceReward - spinPenalty) / Mathf.Max(1, MaxStep));
		Academy.Instance.StatsRecorder.Add("Average Distance", distance, StatAggregationMethod.Average);
        if(StepCount == MaxStep - 1 || distance > maxDistance) {
			EndEpisode();
		}
    }
}
