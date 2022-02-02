using System.IO;
using UnityEngine;

/**
The PIDTest class is used to test a PID controller that tries to fly a single rotor vertically. The main purpose of this is to generate a CSV file of outputs which can be graphed and used to compare the functionality to that of the same controller in another program (such as Matlab).
*/
public class PIDTest : MonoBehaviour {

    PID Controller;
    Rotor TestRotor;
    double StartTime = 0.0D;
    bool Finished = false;
    string CSVOutput = "Time, Input, Output\n";

    double Velocity = 0.0D;
    double Position = 0.0D;

    void Start() {
        Controller = new PID(4.0D, 3.0D, 1.0D, 0.0D, 44.4D);
        TestRotor = new Rotor();
        StartTime = Time.fixedTime;
    }

    void FixedUpdate() {
        //The following code simulates a step response and prints the time, input and output at each time step to the CSV file.
        double TimeElapsed = Time.fixedTime - StartTime;
        if(TimeElapsed < 20.0D) {
            double Input = 0.0D;
            if(TimeElapsed > 0.1D) {
                Input = 30.0D;
            }
            double Voltage = Controller.ComputeOutput(Input, Position);
            TestRotor.UpdateRotor(Clamp(Voltage, 0.0D, 44.4D));
            double Force = TestRotor.LiftForce;
            double Mass = 0.75D;
            double Acceleration = Force / Mass - 9.81D;
            Velocity += Acceleration * Time.fixedDeltaTime;
            Position += Velocity * Time.fixedDeltaTime;
            double Output = Position;
            CSVOutput += TimeElapsed.ToString() + ", " + Input.ToString() + ", " + Output.ToString() + "\n";
        } else {
            if(!Finished) {
                Finished = true;
                StreamWriter Writer = new StreamWriter("PIDTest.csv", false);
                Writer.Write(CSVOutput);
                Writer.Close();
                Debug.Log("Wrote to file");
            }
        }
    }

    double Clamp(double value, double min, double max) {
        if(value < min) {
            value = min;
        }
        if(value > max) {
            value = max;
        }
        return value;
    }
}
