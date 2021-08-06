

    using Unity.Entities;
    using Unity.Mathematics;

    public static class Extension {

        public static void Reverse(this DynamicBuffer<float3> a) {
            var size = a.Length >> 1;
            for (int i = 0; i < size ; i++) {
                var tmp = a[i];
                a[i] = a[size - i - 1];
                a[size - i - 1] = tmp;
            }
        }
    }

