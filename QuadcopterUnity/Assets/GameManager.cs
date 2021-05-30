using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject Quadcopter;
	public Camera GroundCamera, BodyCamera, ChaseCamera;
	public Vector3 ChaseOffset, BodyOffset;
	private int CameraActiveIndex;

    void Start() {
		CameraActiveIndex = 2;
		GroundCamera.gameObject.SetActive(false);
		BodyCamera.gameObject.SetActive(false);
		ChaseCamera.gameObject.SetActive(true);
    }

    void Update() {
		if(Input.GetButtonDown("Camera")) {
			CameraActiveIndex = (CameraActiveIndex + 1) % 3;
		}
		GroundCamera.gameObject.SetActive(CameraActiveIndex == 0);
		BodyCamera.gameObject.SetActive(CameraActiveIndex == 1);
		ChaseCamera.gameObject.SetActive(CameraActiveIndex == 2);

        GroundCamera.transform.LookAt(Quadcopter.transform.position);
		ChaseCamera.transform.position = Quadcopter.transform.position + ChaseOffset;
		BodyCamera.transform.SetPositionAndRotation(Quadcopter.transform.TransformPoint(BodyOffset), Quadcopter.transform.rotation);
    }
	
}
