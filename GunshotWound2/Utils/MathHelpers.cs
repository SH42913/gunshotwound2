namespace GunshotWound2.Utils {
    public static class MathHelpers {
        public static int Clamp(int value, int min, int max) {
            if (value >= max) {
                return max;
            } else if (value <= min) {
                return min;
            } else {
                return value;
            }
        }

        public static float Clamp(float value, float min, float max) {
            if (value >= max) {
                return max;
            } else if (value <= min) {
                return min;
            } else {
                return value;
            }
        }
    }
}