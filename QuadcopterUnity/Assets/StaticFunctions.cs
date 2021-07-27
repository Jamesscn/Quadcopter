/**
The StaticFunctions class provides a set of helpful functions that can be used by any other class. While some of these functions already exist, they are not compatible with doubles, while these ones are.
*/
public static class StaticFunctions {

    public static double Rescale(double value, double fromMin, double fromMax, double toMin, double toMax) {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    public static double Clamp(double value, double min, double max) {
        if(value < min) {
            value = min;
        }
        if(value > max) {
            value = max;
        }
        return value;
    }

    //WIP
    public static double AddNoise(double value, double strength, double seed) {
        return value;
    }

}
