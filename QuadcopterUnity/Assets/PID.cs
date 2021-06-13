public class PID {

    double Kp, Ki, Kd;
    double LastError, CumulativeError;

    public PID(double Proportional, double Integral, double Derivative) {
        Kp = Proportional;
        Ki = Integral;
        Kd = Derivative;
        LastError = 0.0D;
        CumulativeError = 0.0D;
    }

    public double ComputeOutput(double DesiredValue, double CurrentValue) {
        double Error = DesiredValue - CurrentValue;
        CumulativeError += Error;
        double output = Kp * Error + Ki * CumulativeError + Kd * (Error - LastError);
        LastError = Error;
        return output;
    }

}
