using System.IO;
using UnityEngine;

public class MotorTest : MonoBehaviour {

    Rotor TestRotor = new Rotor();
    double StartTime = 0.0D;
    bool Finished = false;
    string CSVOutput = "Time, Input Voltage, Current, Angular Velocity, Lift Force\n";

    void Start() {
        StartTime = Time.fixedTime;
    }

    void FixedUpdate() {
        double TimeElapsed = Time.fixedTime - StartTime;
        if(TimeElapsed < 0.3D) {
            double Voltage = 0.0D;
            if(TimeElapsed > 0.1D) {
                Voltage = 45.0D;
            }
            TestRotor.UpdateRotor(Voltage);
            CSVOutput += TimeElapsed.ToString() + ", " + Voltage.ToString() + ", " + TestRotor.Current.ToString() + ", " + TestRotor.AngularVelocity.ToString() + ", " + TestRotor.LiftForce.ToString() + "\n";
        } else {
            if(!Finished) {
                Finished = true;
                StreamWriter Writer = new StreamWriter("MotorTest.csv", false);
                Writer.Write(CSVOutput);
                Writer.Close();
                Debug.Log("Wrote to file");
            }
        }
    }

}
