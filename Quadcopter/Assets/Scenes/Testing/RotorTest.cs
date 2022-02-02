using System.IO;
using UnityEngine;

/**
The PIDTest class is used to test the Rotor class. The main purpose of this is to generate a CSV file of outputs which can be graphed and used to compare the functionality to that of the rotor in another program (such as Matlab).
*/
public class RotorTest : MonoBehaviour {

    Rotor TestRotor = new Rotor();
    double StartTime = 0.0D;
    bool Finished = false;
    string CSVOutput = "Time, Input Voltage, Net Torque, Lift Force\n";

    void Start() {
        StartTime = Time.fixedTime;
    }

    void FixedUpdate() {
        //The following code simulates a step response and prints the time, input and outputs at each time step to the CSV file.
        double TimeElapsed = Time.fixedTime - StartTime;
        if(TimeElapsed < 0.5D) {
            double Voltage = 0.0D;
            if(TimeElapsed > 0.1D) {
                Voltage = 1.0D;
            }
            TestRotor.UpdateRotor(Voltage);
            CSVOutput += TimeElapsed.ToString() + ", " + Voltage.ToString() + ", " + TestRotor.NetTorque.ToString() + ", " + TestRotor.LiftForce.ToString() + "\n";
        } else {
            if(!Finished) {
                Finished = true;
                StreamWriter Writer = new StreamWriter("RotorTest.csv", false);
                Writer.Write(CSVOutput);
                Writer.Close();
                Debug.Log("Wrote to file");
            }
        }
    }

}
