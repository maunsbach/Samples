using UnityEngine;

public class StaticSoundingObject : MonoBehaviour
{
    public KData objectData;

    public struct KData
    {
        public int len;
        public int maxDecayTime;

        public float[] A1x;
        public float[] A2x;
        public float[] A1v;
        public float[] A2v;

        public float[] bx;
        public float[] bv;

        public float sumbx;
        public float sumbv;

        public float normalizeAmplifier;

        public KData(int len, int maxDecayTime, float[] A1x, float[] A2x, float[] A1v, float[] A2v, float[] bx, float[] bv, float sumbx, float sumbv, float normalizeAmplifier)
        {
            this.len = len;
            this.maxDecayTime = maxDecayTime;

            this.A1x = A1x;
            this.A2x = A2x;
            this.A1v = A1v;
            this.A2v = A2v;

            this.bx = bx;
            this.bv = bv;
            this.sumbx = sumbx;
            this.sumbv = sumbv;

            this.normalizeAmplifier = normalizeAmplifier;
        }
    }

    public KData GetKData()
    {
        return new KData (len, maxDecayTime, A1x, A2x, A1v, A2v, bx, bv, sumbx, sumbv, normalizeAmplifier );
    }

    private int len;
    private int maxDecayTime;

    private float[] A1x;
    private float[] A2x;
    private float[] A1v;
    private float[] A2v;

    private float[] bx;
    private float[] bv;

    private float sumbx;
    private float sumbv;

    private float normalizeAmplifier;

    // MODEL PROPERTIES //
    public static void createSM(GameObject thisParent, Materials.springMassData SM)
    {
        StaticSoundingObject thisObj = thisParent.AddComponent<StaticSoundingObject>();

        // Get values and apply them to this object
        thisObj.len = SM.modes.Length;
        float sampleRate = AudioSettings.outputSampleRate;

        thisObj.normalizeAmplifier = SM.normalizeAmplifier;

        float alpha = 2f * sampleRate;
        float[] omega = new float[thisObj.len];
        float [] g = new float[thisObj.len];

        thisObj.A1x = new float[thisObj.len];
        thisObj.A2x = new float[thisObj.len];
        thisObj.A1v = new float[thisObj.len];
        thisObj.A2v = new float[thisObj.len];

        thisObj.bx = new float[thisObj.len];
        thisObj.bv = new float[thisObj.len];

        for (int i = 0; i < thisObj.len; i++)
        {
            omega[i] = 2f * Mathf.PI * SM.modes[i];
            g[i] = omega[i] / SM.q;
            float detTemp = 1 / (Mathf.Pow(alpha, 2) + alpha * g[i] + Mathf.Pow(omega[i], 2));
            thisObj.A1x[i] = detTemp * (Mathf.Pow(alpha, 2) + alpha * g[i] - Mathf.Pow(omega[i], 2));
            thisObj.A2x[i] = detTemp * 2f * alpha;
            thisObj.A1v[i] = detTemp * (-2f * alpha * Mathf.Pow(omega[i], 2));
            thisObj.A2v[i] = detTemp * (Mathf.Pow(alpha, 2) - alpha * g[i] - Mathf.Pow(omega[i], 2));

            thisObj.bx[i] = detTemp * 1 / SM.mass[i];
            thisObj.sumbx += thisObj.bx[i];
            thisObj.bv[i] = detTemp * (alpha / SM.mass[i]);
            thisObj.sumbv += thisObj.bv[i];
        }

        thisObj.maxDecayTime = SM.maxDecayTime;
    }

    void Awake()
    {
        gameObject.transform.tag = "StaticObject";

    }
}
