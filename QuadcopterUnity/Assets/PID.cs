using UnityEngine;

/**
The PID class is used to simulate a discrete Proportional-Integral-Derivative controller. The constructor initialises the previously stored errors to be zero, and the ComputeOutput function calculates the PID signal given a set-point or desired value and a measured value.
*/
public class PID {

    double Kp, Ki, Kd;
    public double LastError, IntegralValue;

    public PID(double Proportional, double Integral, double Derivative) {
        Kp = Proportional;
        Ki = Integral;
        Kd = Derivative;
        LastError = 0.0D;
        IntegralValue = 0.0D;
    }

    public double ComputeOutput(double DesiredValue, double CurrentValue) {
        double Error = DesiredValue - CurrentValue;
        double ProportionalValue = Error;
        IntegralValue += Error * Time.fixedDeltaTime;
        double DerivativeValue = (Error - LastError) / Time.fixedDeltaTime;
        double output = Kp * ProportionalValue + Ki * IntegralValue + Kd * DerivativeValue;
        LastError = Error;
        return output;
    }

}
