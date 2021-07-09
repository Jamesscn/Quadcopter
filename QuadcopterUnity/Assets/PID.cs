using System;
using UnityEngine;

/**
The PID class is used to simulate a discrete Proportional-Integral-Derivative controller with clamping anti-windup. The constructor initialises the previously stored errors to be zero, and the ComputeOutput function calculates the PID signal given a set-point or desired value and a measured value.
*/
public class PID {

    double Kp, Ki, Kd, LastError, CumulativeError, ClampingMinimum, ClampingMaximum;

    public PID(double Proportional, double Integral, double Derivative, double ClampingMin, double ClampingMax) {
        Kp = Proportional;
        Ki = Integral;
        Kd = Derivative;
        ClampingMinimum = ClampingMin;
        ClampingMaximum = ClampingMax;
        LastError = 0.0D;
        CumulativeError = 0.0D;
    }

    public double ComputeOutput(double DesiredValue, double CurrentValue) {
        double Error = DesiredValue - CurrentValue;
        double ProportionalValue = Error;
        double IntegralValue = CumulativeError + Error * Time.fixedDeltaTime;
        double DerivativeValue = (Error - LastError) / Time.fixedDeltaTime;
        double output = Kp * ProportionalValue + Ki * IntegralValue + Kd * DerivativeValue;
        if(output < ClampingMinimum || output > ClampingMaximum) {
            if(Math.Sign(Error) == Math.Sign(output)) {
                IntegralValue = CumulativeError;
            }
            if(output < ClampingMinimum) {
                output = ClampingMinimum;
            }
            if(output > ClampingMaximum) {
                output = ClampingMaximum;
            }
        }
        CumulativeError = IntegralValue;
        LastError = Error;
        return output;
    }

}
