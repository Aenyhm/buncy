namespace Module2.Toolbox;

public delegate float Easing(float x);

// https://easings.net/
// https://animapix.github.io/Tweening-Cheat-Sheet/
public static class Easings {
    public static float InOutQuad(float x) => (float)(x < 0.5f ? 2*x*x : 1 - Math.Pow(-2*x + 2, 2)/2);
    public static float InOutSine(float x) => (float)-(Math.Cos(Math.PI * x) - 1)/2;
    public static  float InBounce(float t) {
      if (t == 0) return 0;
      if (Math.Abs(t - 1) < float.Epsilon) return 1;
      return (float)(Math.Pow(2, 6*(t - 1))*Math.Abs(Math.Cos((t - 1f)*3.5f*Math.PI)));
    }
    
    public static float InElastic(float x) {
        const double c4 = 2*Math.PI/3;
        return (float)(x == 0 ? 0 : Math.Abs(x - 1) < double.Epsilon ? 1 : -Math.Pow(2, 10*x - 10)*Math.Sin((x*10 - 10.75)*c4));
    }
    public static float InOutBack(float t) {
        if (t < 0.5f) {
            var a = 2 * t;
            return (float)(a * a * a - a * Math.Sin(a * Math.PI))/2;
        } else {
            var a = -2 * t + 2;
            return (float)(1 - (a * a * a - a * Math.Sin(a * Math.PI))/2);
        }
    }
    public static float InOutExpo(float t) {
        if (t == 0) return 0;
        if (Math.Abs(t - 1) < float.Epsilon) return 1;
        return (t < 0.5f)
            ? (float)(Math.Pow(2, 10 * (2 * t - 1)) / 2)
            : (float)(1 - Math.Pow(2, -10 * (2 * t - 1)) / 2);
    }
}
