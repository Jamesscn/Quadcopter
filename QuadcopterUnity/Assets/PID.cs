using UnityEngine;

public class PID {

    double Kp, Ki, Kd;
    double lastError, cumulativeError;

    public PID(double Proportional, double Integral, double Derivative) {
        Kp = Proportional;
        Ki = Integral;
        Kd = Derivative;
        lastError = 0.0D;
        cumulativeError = 0.0D;
    }

    public double ComputeOutput(double DesiredValue, double CurrentValue) {
        double error = DesiredValue - CurrentValue;
        cumulativeError += error;
        double output = Kp * error + Ki * cumulativeError + Kd * (error - lastError);
        lastError = error;
        return output;
    }

}
